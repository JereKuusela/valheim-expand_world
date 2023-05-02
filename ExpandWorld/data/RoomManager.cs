using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ExpandWorld;

public class RoomManager
{
  public static string FileName = "expand_rooms.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  public static string Pattern = "expand_rooms*.yaml";

  private static Dictionary<string, DungeonDB.RoomData> Rooms = new();
  public static Dictionary<string, List<BlueprintObject>> Objects = new();

  private static DungeonDB.RoomData Clone(DungeonDB.RoomData roomData)
  {
    var room = roomData.m_room;
    var clone = new DungeonDB.RoomData();
    clone.m_netViews = roomData.m_netViews;
    clone.m_randomSpawns = roomData.m_randomSpawns;
    // Vanilla name should be used so that clients can find the room.
    var go = new GameObject(room.name);
    clone.m_room = go.AddComponent<Room>();
    clone.m_room.m_theme = room.m_theme;
    clone.m_room.m_entrance = room.m_entrance;
    clone.m_room.m_endCap = room.m_endCap;
    clone.m_room.m_divider = room.m_divider;
    clone.m_room.m_enabled = room.m_enabled;
    clone.m_room.m_size = room.m_size;
    return clone;
  }
  public static DungeonDB.RoomData FromData(RoomData data)
  {
    var hash = Parse.Name(data.name).GetStableHashCode();
    var rooms = DungeonDB.instance.m_roomByHash;
    if (!rooms.TryGetValue(hash, out var roomData))
    {
      ExpandWorld.Log.LogWarning($"Failed to find dungeon room {data.name}.");
      return new();
    }
    if (roomData.m_room.name != data.name)
      roomData = Clone(roomData);
    var room = roomData.m_room;
    room.gameObject.name = data.name;
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
    var connections = room.GetConnections();
    for (var i = 0; i < connections.Length && i < data.connections.Length; i++)
    {
      var connection = connections[i];
      var dataConnection = data.connections[i];
      connection.transform.localPosition = Parse.VectorXZY(dataConnection.position);
      connection.m_type = dataConnection.type;
      connection.m_entrance = dataConnection.entrance;
      connection.m_allowDoor = dataConnection.door == "true";
      connection.m_doorOnlyIfOtherAlsoAllowsDoor = dataConnection.door == "other";
    }
    if (data.objects != null)
      Objects[data.name] = Parse.Objects(data.objects);
    return roomData;
  }
  public static RoomData ToData(Room room)
  {
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

  public static void ToFile()
  {
    if (!Helper.IsServer() || !Configuration.DataRooms) return;
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(DungeonDB.instance.m_rooms.Select(room => ToData(room.m_room)).ToList());
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    if (!Helper.IsServer()) return;
    var yaml = Configuration.DataRooms ? Data.Read(Pattern) : "";
    Set(yaml);
  }
  private static void Set(string yaml)
  {
    if (Rooms.Count > 0)
      DungeonDB.instance.m_rooms = Rooms.Values.ToList();
    Objects.Clear();
    if (yaml == "" || !Configuration.DataRooms) return;
    try
    {
      if (Rooms.Count == 0)
        Rooms = Helper.ToDict(DungeonDB.instance.m_rooms, room => room.m_room.name, room => room);
      var rooms = Data.Deserialize<RoomData>(yaml, FileName).Select(FromData).ToList();
      DungeonDB.instance.m_rooms = rooms;
      // No need to generate the hashlist because no actual rooms can be added.
      ExpandWorld.Log.LogInfo($"Reloading room data ({rooms.Count} entries).");
    }
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.StackTrace);
    }
  }
  public static void SetupWatcher()
  {
    Data.SetupWatcher(Pattern, FromFile);
  }
}
