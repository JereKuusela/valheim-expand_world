using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ExpandWorld;

public class RoomLoading
{
  public static string FileName = "expand_rooms.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  public static string Pattern = "expand_rooms*.yaml";

  private static List<DungeonDB.RoomData> DefaultEntries = new();

  public static void Initialize()
  {
    DefaultEntries.Clear();
    RoomSpawning.Prefabs.Clear();
    RoomSpawning.Data.Clear();
    RoomSpawning.Objects.Clear();
    Load();
  }
  public static void Load()
  {
    if (!ZNet.instance.IsServer()) return;
    if (DefaultEntries.Count == 0) SetDefaultEntries();
    RoomSpawning.Objects.Clear();
    RoomSpawning.Data.Clear();
    DungeonDB.instance.m_rooms = DefaultEntries;
    if (!Configuration.DataRooms)
    {
      ExpandWorld.Log.LogInfo($"Reloading default room data ({DefaultEntries.Count} entries).");
      return;
    }
    if (!File.Exists(FilePath))
    {
      var yaml = Data.Serializer().Serialize(DefaultEntries.Select(ToData).ToList());
      File.WriteAllText(FilePath, yaml);
      // Watcher triggers reload.
      return;
    }
    var data = FromFile();
    if (data.Count == 0)
    {
      ExpandWorld.Log.LogWarning($"Failed to load any room data.");
      ExpandWorld.Log.LogInfo($"Reloading default room data ({DefaultEntries.Count} entries).");
      return;
    }
    if (AddMissingEntries(data))
    {
      // Watcher triggers reload.
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading room data ({data.Count} entries).");
    DungeonDB.instance.m_rooms = data;
  }

  public static DungeonDB.RoomData CreateProxy(string name, string[] snapPieces)
  {
    DungeonDB.RoomData roomData = new();
    var room = new GameObject(name).AddComponent<Room>();
    roomData.m_room = room;
    if (RoomSpawning.Prefabs.TryGetValue(Parse.Name(name), out var baseRoom))
    {
      var connections = baseRoom.m_room.GetConnections();
      if (connections.Length != snapPieces.Length)
        ExpandWorld.Log.LogWarning($"Room {name} has {snapPieces.Length} connections, but base room {baseRoom.m_room.name} has {connections.Length} connections.");
      // Initialize with base connections to allow data missing from the yaml.
      room.m_roomConnections = connections.Select(c =>
        {
          var conn = new GameObject(c.name).AddComponent<RoomConnection>();
          conn.transform.parent = room.transform;
          conn.transform.localPosition = c.transform.localPosition;
          conn.transform.localRotation = c.transform.localRotation;
          conn.m_type = c.m_type;
          conn.m_entrance = c.m_entrance;
          conn.m_allowDoor = c.m_allowDoor;
          conn.m_doorOnlyIfOtherAlsoAllowsDoor = c.m_doorOnlyIfOtherAlsoAllowsDoor;
          return conn;
        }).ToArray();
    }
    else if (BlueprintManager.Load(name, "", snapPieces))
    {
      room.m_roomConnections = snapPieces.Select(c =>
        {
          var conn = new GameObject("").AddComponent<RoomConnection>();
          conn.transform.parent = room.transform;
          return conn;
        }).ToArray();
    }
    return roomData;
  }
  private static void UpdateConnections(Room room, RoomConnectionData[] data)
  {
    for (var i = 0; i < data.Length; ++i)
    {
      var connData = data[i];
      if (i >= room.m_roomConnections.Length)
      {
        var newConn = new GameObject(connData.type).AddComponent<RoomConnection>();
        newConn.transform.parent = room.transform;
        room.m_roomConnections = room.m_roomConnections.Append(newConn).ToArray();
      }
      var conn = room.m_roomConnections[i];
      if (conn.name == "")
        conn.name = connData.type;
      conn.m_type = connData.type;
      conn.m_entrance = connData.entrance;
      conn.m_allowDoor = connData.door == "true";
      conn.m_doorOnlyIfOtherAlsoAllowsDoor = connData.door == "other";
      // Old yamls won't have rotation set so this is important to keep them working.
      // Also it doesn't hurt that the position can be missing as well.
      var split = Parse.Split(connData.position);
      if (split.Length > 2)
        conn.transform.localPosition = Parse.VectorXZY(split, 0);
      if (split.Length > 3)
        conn.transform.localRotation = Parse.AngleYXZ(split, 3);
    }
  }
  private static DungeonDB.RoomData FromData(RoomData data)
  {
    RoomSpawning.Data[data.name] = data;
    var snapPieces = data.connections.Select(c => c.position).ToArray();
    var roomData = CreateProxy(data.name, snapPieces);
    var room = roomData.m_room;
    room.m_theme = Data.ToEnum<Room.Theme>(data.theme);
    room.m_entrance = data.entrance;
    room.m_endCap = data.endCap;
    room.m_divider = data.divider;
    room.m_enabled = data.enabled;
    var size = Parse.VectorXZY(data.size);
    room.m_size = new((int)size.x, (int)size.y, (int)size.z);
    room.m_minPlaceOrder = data.minPlaceOrder;
    room.m_weight = data.weight;
    room.m_faceCenter = data.faceCenter;
    room.m_perimeter = data.perimeter;
    room.m_endCapPrio = data.endCapPriority;
    UpdateConnections(room, data.connections);
    if (data.objects != null)
      RoomSpawning.Objects[data.name] = Parse.Objects(data.objects);
    return roomData;
  }
  private static RoomData ToData(DungeonDB.RoomData roomData)
  {
    var room = roomData.m_room;
    RoomData data = new();
    data.name = room.gameObject.name;
    data.theme = Data.FromEnum(room.m_theme);
    data.entrance = room.m_entrance;
    data.endCap = room.m_endCap;
    data.divider = room.m_divider;
    data.enabled = room.m_enabled;
    data.size = $"{room.m_size.x},{room.m_size.z},{room.m_size.y}";
    data.minPlaceOrder = room.m_minPlaceOrder;
    data.weight = room.m_weight;
    data.faceCenter = room.m_faceCenter;
    data.perimeter = room.m_perimeter;
    data.endCapPriority = room.m_endCapPrio;
    data.connections = room.GetConnections().Select(connection => new RoomConnectionData
    {
      position = $"{Helper.Print(connection.transform.localPosition)},{Helper.Print(connection.transform.localRotation)}",
      type = connection.m_type,
      entrance = connection.m_entrance,
      door = connection.m_allowDoor ? "true" : connection.m_doorOnlyIfOtherAlsoAllowsDoor ? "other" : "false"
    }).ToArray();
    return data;
  }

  private static List<DungeonDB.RoomData> FromFile()
  {
    try
    {
      var yaml = Data.Read(Pattern);
      return Data.Deserialize<RoomData>(yaml, FileName).Select(FromData).Where(room => room.m_room).ToList();
    }
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.Message);
      ExpandWorld.Log.LogError(e.StackTrace);
    }
    return new();
  }
  private static void SetDefaultEntries()
  {
    DefaultEntries = DungeonDB.instance.m_rooms.Where(room => room.m_room).ToList();
    RoomSpawning.Prefabs = DefaultEntries.ToDictionary(entry => entry.m_room.name, entry => entry);
  }
  private static bool AddMissingEntries(List<DungeonDB.RoomData> entries)
  {
    var missingKeys = DefaultEntries.Select(entry => entry.m_room.name).Distinct().ToHashSet();
    foreach (var entry in entries)
      missingKeys.Remove(entry.m_room.name);
    if (missingKeys.Count == 0) return false;
    var missing = DefaultEntries.Where(entry => missingKeys.Contains(entry.m_room.name)).ToList();
    ExpandWorld.Log.LogWarning($"Adding {missing.Count} missing rooms to the expand_rooms.yaml file.");
    foreach (var entry in missing)
      ExpandWorld.Log.LogWarning(entry.m_room.name);
    var yaml = File.ReadAllText(FilePath);
    var data = Data.Deserialize<RoomData>(yaml, FileName).ToList();
    data.AddRange(missing.Select(ToData));
    // Directly appending is risky if something goes wrong (like missing a linebreak).
    File.WriteAllText(FilePath, Data.Serializer().Serialize(data));
    return true;
  }
  public static void SetupWatcher()
  {
    Data.SetupWatcher(Pattern, Load);
  }
}
