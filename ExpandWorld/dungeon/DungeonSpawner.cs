using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld.Dungeon;

[HarmonyPatch(typeof(DungeonGenerator))]
public class Spawner
{
  ///<summary>Dungeon doesn't know its location so it must be tracked manually.</summary>
  public static string LocationName = "";

  public static Dictionary<string, FakeDungeonGenerator> Generators = new();
  public static string RoomName = "";
  public static string DungeonName = "";

  public static bool TryGetSwap(string dungeon, string prefab, out string swapped)
  {
    swapped = "";
    if (!Generators.TryGetValue(dungeon, out var gen)) return false;
    if (!gen.m_objectSwaps.TryGetValue(prefab, out var swaps)) return false;
    swapped = Spawn.RandomizeSwap(swaps);
    return true;
  }

  public static ZDO? DataOverride(string dungeon, string prefab)
  {
    if (!Generators.TryGetValue(dungeon, out var gen)) return null;
    if (!gen.m_objectData.TryGetValue(prefab, out var data)) return null;
    return data;
  }

  ///<summary>Implements object data and swapping from location data.</summary>
  static GameObject CustomObject(GameObject prefab, Vector3 position, Quaternion rotation, DungeonGenerator dg)
  {
    // Some mods cause client side dungeon reloading. In this case, no data is available.
    // Revert to the default behaviour as a fail safe.
    if (Helper.IsClient()) return UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    BlueprintObject bpo = new(Utils.GetPrefabName(prefab), position, rotation, prefab.transform.localScale, "", null, 1f);
    if (TryGetSwap(dg.name, bpo.Prefab, out var objName))
      bpo.Prefab = objName;
    if (RoomSpawning.TryGetSwap(RoomName, bpo.Prefab, out objName))
      bpo.Prefab = objName;

    var obj = Spawn.BPO(dg.name, bpo, DataOverride, null);
    return obj ?? LocationSpawning.DummySpawn;
  }

  static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
  {
    var instantiator = typeof(UnityEngine.Object).GetMethods().First(m => m.Name == nameof(UnityEngine.Object.Instantiate) && m.IsGenericMethodDefinition && m.GetParameters().Skip(1).Select(p => p.ParameterType).SequenceEqual(new[] { typeof(Vector3), typeof(Quaternion) })).MakeGenericMethod(typeof(GameObject));
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Call, instantiator))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(CustomObject).operand)
      .InstructionEnumeration();
  }

  // Room objects in the dungeon.
  [HarmonyPatch(nameof(DungeonGenerator.PlaceRoom), typeof(DungeonDB.RoomData), typeof(Vector3), typeof(Quaternion), typeof(RoomConnection), typeof(ZoneSystem.SpawnMode)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DungeonObjects(IEnumerable<CodeInstruction> instructions) => Transpile(instructions);

  // Doors in the dungeon.
  [HarmonyPatch(nameof(DungeonGenerator.PlaceDoors)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DungeonDoors(IEnumerable<CodeInstruction> instructions) => Transpile(instructions);


  [HarmonyPatch(nameof(DungeonGenerator.Generate), typeof(ZoneSystem.SpawnMode)), HarmonyPrefix]
  static void Generate(DungeonGenerator __instance)
  {
    if (LocationName == "") return;
    var dungeonName = Utils.GetPrefabName(__instance.gameObject);
    if (LocationLoading.LocationData.TryGetValue(LocationName, out var data) && data.dungeon != "")
      dungeonName = data.dungeon;
    Override(__instance, dungeonName);
  }


  // The dungeon prefab is only used for generating, so the properties can be just overwritten.
  public static void Override(DungeonGenerator dg, string name)
  {
    if (!Generators.TryGetValue(name, out var data)) return;
    dg.name = name;
    dg.m_algorithm = data.m_algorithm;
    dg.m_zoneSize = data.m_zoneSize;
    dg.m_alternativeFunctionality = data.m_alternativeFunctionality;
    dg.m_campRadiusMax = data.m_campRadiusMax;
    dg.m_campRadiusMin = data.m_campRadiusMin;
    dg.m_doorChance = data.m_doorChance;
    dg.m_doorTypes = data.m_doorTypes;
    dg.m_maxRooms = data.m_maxRooms;
    dg.m_minRooms = data.m_minRooms;
    dg.m_maxTilt = data.m_maxTilt;
    dg.m_minAltitude = data.m_minAltitude;
    dg.m_minRequiredRooms = data.m_minRequiredRooms;
    dg.m_requiredRooms = data.m_requiredRooms;
    dg.m_themes = Data.ToEnum<Room.Theme>(data.m_themes);
    dg.m_gridSize = data.m_gridSize;
    dg.m_tileWidth = data.m_tileWidth;
    dg.m_spawnChance = data.m_spawnChance;
    dg.m_perimeterSections = data.m_perimeterSections;
    dg.m_perimeterBuffer = data.m_perimeterBuffer;
    dg.m_useCustomInteriorTransform = data.m_useCustomInteriorTransform;
  }
  [HarmonyPatch(nameof(DungeonGenerator.SetupAvailableRooms)), HarmonyPostfix]
  public static void SetupAvailableRooms(DungeonGenerator __instance)
  {
    var name = Utils.GetPrefabName(__instance.gameObject);
    if (!Generators.TryGetValue(name, out var gen)) return;
    if (gen.m_excludedRooms.Count == 0) return;
    DungeonGenerator.m_availableRooms = DungeonGenerator.m_availableRooms.Where(room => !gen.m_excludedRooms.Contains(room.m_room.name)).ToList();

  }

}

// Client side tweak to make the dungeon environment box extend to the whole dungeon.
[HarmonyPatch]
public class EnvironmentBox
{

  // Not fully sure if the generator or location loads first.
  public static Dictionary<Vector2i, Vector3> Cache = new();


  private static void TryScale(Location loc)
  {
    var zone = ZoneSystem.instance.GetZone(loc.transform.position);
    if (!Cache.TryGetValue(zone, out var size)) return;
    // Only interior locations can have the environment box.
    if (!loc.m_hasInterior) return;
    var envZone = loc.GetComponentInChildren<EnvZone>();
    if (!envZone) return;
    // Don't shrink from the default so that people can build there more easily.
    // Otherwise for small dungeons the box would be very small.
    var origSize = envZone.transform.localScale;
    // Also leave some margins just in case so that the box doesn't clip through the walls.
    size.x = Mathf.Max(size.x + 10f, origSize.x);
    size.y = Mathf.Max(size.y + 10f, origSize.y);
    size.z = Mathf.Max(size.z + 10f, origSize.z);
    envZone.transform.localScale = size;
  }

  [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Load)), HarmonyPostfix]
  static void ScaleEnvironmentBox1(DungeonGenerator __instance)
  {
    var pos = __instance.transform.position;
    var zone = ZoneSystem.instance.GetZone(pos);
    var colliders = __instance.GetComponentsInChildren<Collider>();
    Bounds bounds = new();
    foreach (var collider in colliders)
      bounds.Encapsulate(collider.bounds);
    var size = bounds.size;
    Cache[zone] = size;
    var locsInZone = Location.m_allLocations.Where(loc => ZoneSystem.instance.GetZone(loc.transform.position) == zone).ToArray();
    foreach (var loc in locsInZone)
      TryScale(loc);
  }

  [HarmonyPatch(typeof(Location), nameof(Location.Awake)), HarmonyPostfix]
  static void ScaleEnvironmentBox2(Location __instance)
  {
    TryScale(__instance);
  }
}