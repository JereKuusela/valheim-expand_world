using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

// Room variants are tricky to implement because the room prefab and room parameters are in the same component.
// New entries can't be directly created because the room prefab can't be copied.
// So the idea is to separate the prefab and parameters by creating a room proxy for each room.
// When the room is selected, the actual room component is built from the base room prefab and the proxy parameters.
[HarmonyPatch(typeof(DungeonGenerator))]
public class RoomSpawning
{
  // Note: OverrideRoomData will change the parameters of the room prefab. Don't use them for anything.
  public static Dictionary<string, DungeonDB.RoomData> RoomPrefabs = new();

  public static Dictionary<string, List<BlueprintObject>> Objects = new();
  // Proxy should be created even for the default rooms because the prefab parameters can't be trusted.
  public static Room CreateRoomProxy(string name)
  {
    var room = new GameObject(name).AddComponent<Room>();
    if (RoomPrefabs.TryGetValue(Parse.Name(name), out var baseRoom))
    {
      room.transform.localPosition = baseRoom.m_room.transform.localPosition;
      // New connection objects are needed to override them separately for each room.
      // The data itself can be anything because of OverrideRoomData.
      room.m_roomConnections = baseRoom.m_room.GetConnections().Select(c =>
      {
        var connection = new GameObject(c.name).AddComponent<RoomConnection>();
        connection.transform.localPosition = c.transform.localPosition;
        return connection;
      }).ToArray();
    }
    return room;
  }


  private static Room OverrideParameters(Room from, Room to)
  {
    // The name must be changed to allow Objects field to work.
    // The hash is used to save the room and handled with RoomSaving patch.
    to.name = from.name;
    to.m_theme = from.m_theme;
    to.m_entrance = from.m_entrance;
    to.m_endCap = from.m_endCap;
    to.m_divider = from.m_divider;
    to.m_enabled = from.m_enabled;
    to.m_size = from.m_size;
    to.m_minPlaceOrder = from.m_minPlaceOrder;
    to.m_weight = from.m_weight;
    to.m_faceCenter = from.m_faceCenter;
    to.m_perimeter = from.m_perimeter;
    to.m_endCapPrio = from.m_endCapPrio;
    to.m_perimeter = from.m_perimeter;
    to.m_roomConnections = from.m_roomConnections;
    return to;
  }

  static DungeonDB.RoomData OverrideRoomData(DungeonDB.RoomData result)
  {
    if (!Configuration.DataRooms) return result;
#nullable disable
    if (result == null) return result;
#nullable enable
    var room = result.m_room;
    var name = result.m_room.name;
    var baseName = Parse.Name(name);
    // Combine the base room prefab and the room parameters.
    if (RoomPrefabs.TryGetValue(baseName, out var roomData))
    {
      // The proxy shouldn't be modified, or entries will get reference to the same room.
      result = new();
      result.m_room = OverrideParameters(roomData.m_room, room);
      result.m_netViews = roomData.m_netViews;
      result.m_randomSpawns = roomData.m_randomSpawns;
    }
    return result;
  }
  [HarmonyPatch(nameof(DungeonGenerator.FindEndCap)), HarmonyPostfix]
  static DungeonDB.RoomData FindEndCap(DungeonDB.RoomData result) => OverrideRoomData(result);

  [HarmonyPatch(nameof(DungeonGenerator.GetRandomWeightedRoom), typeof(bool)), HarmonyPostfix]
  static DungeonDB.RoomData GetRandomWeightedRoom1(DungeonDB.RoomData result) => OverrideRoomData(result);


  [HarmonyPatch(nameof(DungeonGenerator.GetRandomWeightedRoom), typeof(RoomConnection)), HarmonyPostfix]
  static DungeonDB.RoomData GetRandomWeightedRoom2(DungeonDB.RoomData result) => OverrideRoomData(result);


  [HarmonyPatch(nameof(DungeonGenerator.GetWeightedRoom)), HarmonyPostfix]
  static DungeonDB.RoomData GetWeightedRoom(DungeonDB.RoomData result) => OverrideRoomData(result);


  [HarmonyPatch(nameof(DungeonGenerator.GetRandomRoom)), HarmonyPostfix]
  static DungeonDB.RoomData GetRandomRoom(DungeonDB.RoomData result) => OverrideRoomData(result);

  [HarmonyPatch(nameof(DungeonGenerator.FindStartRoom)), HarmonyPostfix]
  static DungeonDB.RoomData FindStartRoom(DungeonDB.RoomData result) => OverrideRoomData(result);
}

// Hash is used to save the generated room.
// Use the base room name to allow it work for vanilla clients.
[HarmonyPatch(typeof(Room), nameof(Room.GetHash))]
public class RoomSaving
{
  static int Postfix(int result, Room __instance)
  {
    return Parse.Name(Utils.GetPrefabName(__instance.gameObject)).GetStableHashCode();
  }
}