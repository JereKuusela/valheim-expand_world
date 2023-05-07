using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

[HarmonyPatch(typeof(DungeonGenerator))]
public class DungeonSpawning
{
  public static ZoneSystem.ZoneLocation? Location = null;
  public static GameObject Object(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    var go = ObjectSub(prefab, position, rotation);
    Data.CleanGhostInit(go);
    return go;
  }
  static GameObject ObjectSub(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    // Some mods cause client side dungeon reloading.
    // In this case location information is not available.
    // Revert to the default behaviour as fail safe.
    if (Location == null) return UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    var locName = Location.m_prefabName;
    if (LocationSpawning.TryGetSwap(locName, prefab, out var objName))
      prefab = ZNetScene.instance.GetPrefab(objName);

    if (prefab)
    {
      LocationSpawning.SetData(locName, prefab, position, rotation, prefab.transform.localScale);
      return UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    }

    if (BlueprintManager.TryGet(objName, out var bp))
    {
      LocationSpawning.Blueprint(bp, Location, position, rotation, null);
      return LocationSpawning.DummySpawn;
    }

    if (objName != "")
      ExpandWorld.Log.LogWarning($"Swapped prefab {objName} not found!");
    return LocationSpawning.DummySpawn;
  }
  static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions, int count)
  {
    var instantiator = typeof(UnityEngine.Object).GetMethods().First(m => m.Name == nameof(UnityEngine.Object.Instantiate) && m.IsGenericMethodDefinition && m.GetParameters().Skip(1).Select(p => p.ParameterType).SequenceEqual(new[] { typeof(Vector3), typeof(Quaternion) })).MakeGenericMethod(typeof(GameObject));
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Call, instantiator))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(Object).operand)
      .InstructionEnumeration();
  }
  [HarmonyPatch(nameof(DungeonGenerator.PlaceRoom), typeof(DungeonDB.RoomData), typeof(Vector3), typeof(Quaternion), typeof(RoomConnection), typeof(ZoneSystem.SpawnMode)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DungeonObjects(IEnumerable<CodeInstruction> instructions) => Transpile(instructions, 5);
  [HarmonyPatch(nameof(DungeonGenerator.PlaceDoors)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DungeonDoors(IEnumerable<CodeInstruction> instructions) => Transpile(instructions, 1);




  public static void BPO(Vector3 pos, Quaternion rot, BlueprintObject obj)
  {
    pos += rot * obj.Pos;
    rot = rot * obj.Rot;
    var prefab = ZNetScene.instance.GetPrefab(obj.Prefab);
    if (!prefab)
    {
      if (BlueprintManager.TryGet(obj.Prefab, out var bp))
      {
        Blueprint(bp, pos, rot);
        return;
      }
      ExpandWorld.Log.LogWarning($"Blueprint prefab {obj.Prefab} not found!");
      return;
    }
    var locName = Location?.m_prefabName ?? "";
    LocationSpawning.SetData(locName, prefab, pos, rot, obj.Scale, obj.Data);
    var go = UnityEngine.Object.Instantiate<GameObject>(prefab, pos, rot);
    Data.CleanGhostInit(go);
    if (ZNetView.m_ghostInit)
      UnityEngine.Object.Destroy(go);
  }
  public static void Blueprint(Blueprint bp, Vector3 pos, Quaternion rot)
  {
    foreach (var obj in bp.Objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      BPO(pos, rot, obj);
    }
  }
}