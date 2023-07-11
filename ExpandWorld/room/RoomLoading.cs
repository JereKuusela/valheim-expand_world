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


  // For finding rooms with wrong case.
  private static Dictionary<string, string> RoomNames = new();
  public static List<string> ParseRooms(string names) => DataManager.ToList(names).Select(s => RoomLoading.RoomNames.TryGetValue(s, out var name) ? name : s).ToList();
  public static void Initialize()
  {
    DefaultEntries.Clear();
    RoomSpawning.Prefabs.Clear();
    if (Helper.IsServer())
      SetDefaultEntries();
    Load();
  }
  private static void ToFile()
  {
    var yaml = DataManager.Serializer().Serialize(DefaultEntries.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }

  private static readonly Dictionary<string, Room.Theme> DefaultNameToTheme = new() {
    {"Crypt", Room.Theme.Crypt},
    {"SunkenCrypt", Room.Theme.SunkenCrypt},
    {"Cave", Room.Theme.Cave},
    {"ForestCrypt", Room.Theme.ForestCrypt},
    {"GoblinCamp", Room.Theme.GoblinCamp},
    {"MeadowsVillage", Room.Theme.MeadowsVillage},
    {"MeadowsFarm", Room.Theme.MeadowsFarm},
    {"DvergerTown", Room.Theme.DvergerTown},
    {"DvergerBoss", Room.Theme.DvergerBoss}
  };
  // For extra custom room themes.
  public static Dictionary<string, Room.Theme> NameToTheme = DefaultNameToTheme.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);
  public static Dictionary<Room.Theme, string> ThemeToName = DefaultNameToTheme.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
  public static bool TryGetTheme(string name, out Room.Theme theme) => NameToTheme.TryGetValue(name.ToLower(), out theme);

  public static void Load()
  {
    RoomSpawning.Data.Clear();
    RoomSpawning.Objects.Clear();
    RoomSpawning.ObjectSwaps.Clear();
    RoomSpawning.ObjectData.Clear();
    NameToTheme = DefaultNameToTheme.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);
    ThemeToName = DefaultNameToTheme.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    if (Helper.IsClient()) return;
    if (!Configuration.DataRooms)
    {
      ExpandWorld.Log.LogInfo($"Reloading default room data ({DefaultEntries.Count} entries).");
      return;
    }
    if (!File.Exists(FilePath))
    {
      ToFile();
      return; // Watcher triggers reload.
    }
    var data = FromFile();
    if (data.Count == 0)
    {
      ExpandWorld.Log.LogWarning($"Failed to load any room data.");
      ExpandWorld.Log.LogInfo($"Reloading default room data ({DefaultEntries.Count} entries).");
      return;
    }
    if (Configuration.DataMigration && AddMissingEntries(data))
    {
      // Watcher triggers reload.
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading room themes ({NameToTheme.Count} entries).");
    ExpandWorld.Log.LogInfo($"Reloading room data ({data.Count} entries).");

    RoomNames = data.ToDictionary(room => room.m_room.name.ToLowerInvariant(), room => room.m_room.name);
    DungeonDB.instance.m_rooms = data;
  }

  public static DungeonDB.RoomData CreateProxy(string name, string centerPiece, string[] snapPieces)
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
    else if (BlueprintManager.Load(name, centerPiece, snapPieces))
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
    var roomData = CreateProxy(data.name, data.centerPiece, snapPieces);
    var room = roomData.m_room;
    var missingThemes = DataManager.ToList(data.theme).Where(s => !NameToTheme.ContainsKey(s.ToLowerInvariant())).ToArray();
    foreach (var theme in missingThemes)
    {
      var nextValue = (Room.Theme)(2 * (int)NameToTheme.Values.Max());
      NameToTheme[theme.ToLowerInvariant()] = nextValue;
      ThemeToName[nextValue] = theme;
    }
    room.m_theme = DataManager.ToEnum<Room.Theme>(data.theme);
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
    if (data.objectSwap != null)
      RoomSpawning.ObjectSwaps[data.name] = Spawn.LoadSwaps(data.objectSwap);
    if (data.objectData != null)
      RoomSpawning.ObjectData[data.name] = Spawn.LoadData(data.objectData);
    return roomData;
  }
  private static RoomData ToData(DungeonDB.RoomData roomData)
  {
    var room = roomData.m_room;
    RoomData data = new()
    {
      name = room.gameObject.name,
      theme = DataManager.FromEnum(room.m_theme),
      entrance = room.m_entrance,
      endCap = room.m_endCap,
      divider = room.m_divider,
      enabled = room.m_enabled,
      size = $"{room.m_size.x},{room.m_size.z},{room.m_size.y}",
      minPlaceOrder = room.m_minPlaceOrder,
      weight = room.m_weight,
      faceCenter = room.m_faceCenter,
      perimeter = room.m_perimeter,
      endCapPriority = room.m_endCapPrio,
      connections = room.GetConnections().Select(connection => new RoomConnectionData
      {
        position = $"{Helper.Print(connection.transform.localPosition)},{Helper.Print(connection.transform.localRotation)}",
        type = connection.m_type,
        entrance = connection.m_entrance,
        door = connection.m_allowDoor ? "true" : connection.m_doorOnlyIfOtherAlsoAllowsDoor ? "other" : "false"
      }).ToArray()
    };
    return data;
  }

  private static List<DungeonDB.RoomData> FromFile()
  {
    try
    {
      var yaml = DataManager.Read(Pattern);
      return DataManager.Deserialize<RoomData>(yaml, FileName).Select(FromData).Where(room => room.m_room).ToList();
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
    var data = DataManager.Serializer().Serialize(missing.Select(ToData));
    // Directly appending is risky but necessary to keep comments, etc.
    yaml += "\n" + data;
    File.WriteAllText(FilePath, yaml);
    return true;
  }
  public static void SetupWatcher()
  {
    DataManager.SetupWatcher(Pattern, Load);
  }
}
