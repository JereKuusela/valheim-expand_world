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
        connection.transform.parent = room.transform;
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
    var connFrom = from.GetConnections();
    var connTo = to.GetConnections();
    for (var i = 0; i < connFrom.Length && i < connTo.Length; i++)
    {
      var cFrom = connFrom[i];
      var cTo = connTo[i];
      cTo.transform.localPosition = cFrom.transform.localPosition;
      cTo.m_type = cFrom.m_type;
      cTo.m_entrance = cFrom.m_entrance;
      cTo.m_allowDoor = cFrom.m_allowDoor;
      cTo.m_doorOnlyIfOtherAlsoAllowsDoor = cFrom.m_doorOnlyIfOtherAlsoAllowsDoor;
    }
    return to;
  }

  [HarmonyPatch(nameof(DungeonGenerator.PlaceRoom), typeof(DungeonDB.RoomData), typeof(Vector3), typeof(Quaternion), typeof(RoomConnection), typeof(ZoneSystem.SpawnMode)), HarmonyPrefix]
  static void ReplaceRoom(ref DungeonDB.RoomData room)
  {
    if (!Configuration.DataRooms) return;
    if (room.m_netViews.Count > 0) return;
    var parameters = room.m_room;
    var baseName = Parse.Name(parameters.name);
    // Combine the base room prefab and the room parameters.
    if (RoomPrefabs.TryGetValue(baseName, out var roomData))
    {
      // The proxy shouldn't be modified, or entries will get reference to the same room.
      room = new();
      room.m_room = OverrideParameters(parameters, roomData.m_room);
      room.m_netViews = roomData.m_netViews;
      room.m_randomSpawns = roomData.m_randomSpawns;
    }
  }
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