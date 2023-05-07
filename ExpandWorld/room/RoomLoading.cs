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
    RoomSpawning.Objects.Clear();
    Load();
  }
  public static void Load()
  {
    if (!ZNet.instance.IsServer()) return;
    if (DefaultEntries.Count == 0) SetDefaultEntries();
    RoomSpawning.Objects.Clear();
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

  public static DungeonDB.RoomData CreateProxy(string name)
  {
    DungeonDB.RoomData roomData = new();
    roomData.m_room = new GameObject(name).AddComponent<Room>();
    if (!RoomSpawning.Prefabs.ContainsKey(Parse.Name(name)))
      BlueprintManager.Load(name, "");
    return roomData;
  }
  private static DungeonDB.RoomData FromData(RoomData data)
  {
    var roomData = CreateProxy(data.name);
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
    room.m_roomConnections = data.connections.Select(item =>
      {
        var conn = new GameObject().AddComponent<RoomConnection>();
        conn.transform.parent = room.transform;
        conn.transform.localPosition = Parse.VectorXZY(item.position);
        conn.m_type = item.type;
        conn.m_entrance = item.entrance;
        conn.m_allowDoor = item.door == "true";
        conn.m_doorOnlyIfOtherAlsoAllowsDoor = item.door == "other";
        return conn;
      }).ToArray();
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
      position = Helper.Print(connection.transform.localPosition),
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
