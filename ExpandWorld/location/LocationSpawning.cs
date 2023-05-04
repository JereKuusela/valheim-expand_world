using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

public class LocationSpawning
{
  static void SetData(string location, GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, ZDO? data = null)
  {
    if (data == null)
    {
      if (!LocationLoading.ObjectData.TryGetValue(location, out var objectData)) return;
      if (!objectData.TryGetValue(Utils.GetPrefabName(prefab), out data)) return;
    }
    if (data == null) return;
    if (!prefab.TryGetComponent<ZNetView>(out var view)) return;
    Data.InitZDO(position, rotation, scale, data, view);
  }
  static string RandomizeSwap(List<Tuple<float, string>> swaps)
  {
    if (swaps.Count == 0)
      return "";
    if (swaps.Count == 1)
      return swaps[0].Item2;
    var rng = UnityEngine.Random.value;
    foreach (var swap in swaps)
    {
      rng -= swap.Item1;
      if (rng <= 0f) return swap.Item2;
    }
    return swaps[swaps.Count - 1].Item2;
  }
  public static bool TryGetSwap(string location, GameObject prefab, out string swapped)
  {
    swapped = "";
    if (!LocationLoading.ObjectSwaps.TryGetValue(location, out var objectSwaps)) return false;
    if (!objectSwaps.TryGetValue(Utils.GetPrefabName(prefab), out var swaps)) return false;
    swapped = RandomizeSwap(swaps);
    return true;
  }
  static string DummyObj = "vfx_auto_pickup";
  public static GameObject DummySpawn => UnityEngine.Object.Instantiate<GameObject>(ZNetScene.instance.GetPrefab(DummyObj), Vector3.zero, Quaternion.identity);
  public static GameObject Object(GameObject prefab, Vector3 position, Quaternion rotation, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects, ZoneSystem.ZoneLocation location)
  {
    var locName = location.m_prefabName;
    if (LocationSpawning.TryGetSwap(locName, prefab, out var objName))
      prefab = ZNetScene.instance.GetPrefab(objName);

    if (prefab)
      return LocationSpawning.InstantiateWithData(locName, prefab, position, rotation, prefab.transform.localScale);

    if (BlueprintManager.TryGet(objName, out var bp))
    {
      LocationSpawning.Blueprint(bp, location, position, rotation, mode, spawnedGhostObjects);
      return LocationSpawning.DummySpawn;
    }

    if (objName != "")
      ExpandWorld.Log.LogWarning($"Swapped prefab {objName} not found!");
    return LocationSpawning.DummySpawn;
  }
  public static GameObject InstantiateWithData(string location, GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, ZDO? data = null)
  {
    SetData(location, prefab, position, rotation, scale, data);
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    if (obj.TryGetComponent<DungeonGenerator>(out var dg))
    {
      if (LocationLoading.Dungeons.TryGetValue(location, out var dungeon))
        DungeonManager.Override(dg, dungeon);
      else
        DungeonManager.Override(dg, Utils.GetPrefabName(obj));
    }
    Data.CleanGhostInit(obj);
    return obj;
  }
  public static void BPO(ZoneSystem __instance, bool flag, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject>? spawnedGhostObjects, BlueprintObject obj)
  {
    var objPos = pos + rot * obj.Pos;
    var objRot = rot * obj.Rot;
    var prefab = ZNetScene.instance.GetPrefab(obj.Prefab);
    if (!prefab)
    {
      if (BlueprintManager.TryGet(obj.Prefab, out var bp))
      {
        Blueprint(bp, location, objPos, objRot, mode, spawnedGhostObjects);
        return;
      }
      ExpandWorld.Log.LogWarning($"Blueprint prefab {obj.Prefab} not found!");
      return;
    }
    if (mode == ZoneSystem.SpawnMode.Ghost)
    {
      ZNetView.StartGhostInit();
    }
    var go = InstantiateWithData(location.m_prefabName, prefab, objPos, objRot, obj.Scale, obj.Data);
    go.GetComponent<ZNetView>().GetZDO().SetPGWVersion(__instance.m_pgwVersion);
    if (go.TryGetComponent<DungeonGenerator>(out var dg))
    {
      if (flag)
        dg.m_originalPosition = location.m_generatorPosition;
      dg.Generate(mode);
    }
    if (mode == ZoneSystem.SpawnMode.Ghost)
    {
      if (spawnedGhostObjects == null)
        UnityEngine.Object.Destroy(go);
      else
        spawnedGhostObjects.Add(go);
      ZNetView.FinishGhostInit();
    }
  }

  public static void Blueprint(Blueprint bp, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject>? spawnedGhostObjects)
  {
    var zs = ZoneSystem.instance;
    var loc = location.m_location;
    WearNTear.m_randomInitialDamage = loc.m_applyRandomDamage;
    var flag = loc && loc.m_useCustomInteriorTransform && loc.m_interiorTransform && loc.m_generator;
    foreach (var obj in bp.Objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      BPO(zs, flag, location, pos, rot, mode, spawnedGhostObjects, obj);
    }
    WearNTear.m_randomInitialDamage = false;
  }

  public static void CustomObjects(ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects)
  {
    var zs = ZoneSystem.instance;
    if (!LocationLoading.Objects.TryGetValue(location.m_prefabName, out var objects)) return;
    var loc = location.m_location;
    var flag = loc && loc.m_useCustomInteriorTransform && loc.m_interiorTransform && loc.m_generator;
    foreach (var obj in objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      LocationSpawning.BPO(zs, flag, location, pos, rot, mode, spawnedGhostObjects, obj);
    }
  }
}

[HarmonyPatch(typeof(DungeonGenerator))]
public class DungeonSpawning
{
  public static ZoneSystem.ZoneLocation? Location = null;
  public static GameObject Object(GameObject prefab, Vector3 position, Quaternion rotation, ZoneSystem.SpawnMode mode)
  {
    // Some mods cause client side dungeon reloading.
    // In this case location information is not available.
    // Revert to the default behaviour as fail safe.
    if (Location == null) return UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    var locName = Location.m_prefabName;
    if (LocationSpawning.TryGetSwap(locName, prefab, out var objName))
      prefab = ZNetScene.instance.GetPrefab(objName);

    if (prefab)
      return LocationSpawning.InstantiateWithData(locName, prefab, position, rotation, prefab.transform.localScale);

    if (BlueprintManager.TryGet(objName, out var bp))
    {
      LocationSpawning.Blueprint(bp, Location, position, rotation, mode, null);
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
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, count))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(Object).operand)
      .InstructionEnumeration();
  }
  [HarmonyPatch(nameof(DungeonGenerator.PlaceRoom), typeof(DungeonDB.RoomData), typeof(Vector3), typeof(Quaternion), typeof(RoomConnection), typeof(ZoneSystem.SpawnMode)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DungeonObjects(IEnumerable<CodeInstruction> instructions) => Transpile(instructions, 5);
  [HarmonyPatch(nameof(DungeonGenerator.PlaceDoors)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DungeonDoors(IEnumerable<CodeInstruction> instructions) => Transpile(instructions, 1);

  [HarmonyPatch(nameof(DungeonGenerator.PlaceRoom), typeof(DungeonDB.RoomData), typeof(Vector3), typeof(Quaternion), typeof(RoomConnection), typeof(ZoneSystem.SpawnMode)), HarmonyPostfix]
  static void DungeonCustomObjects(DungeonDB.RoomData room, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode)
  {
    if (mode == ZoneSystem.SpawnMode.Client) return;
    if (!RoomSpawning.Objects.TryGetValue(room.m_room.name, out var objects)) return;
    int seed = (int)pos.x * 4271 + (int)pos.y * 9187 + (int)pos.z * 2134;
    UnityEngine.Random.State state = UnityEngine.Random.state;
    UnityEngine.Random.InitState(seed);
    foreach (var obj in objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      BPO(pos, rot, mode, obj);
    }
    UnityEngine.Random.state = state;
  }


  static void BPO(Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, BlueprintObject obj)
  {
    var objPos = pos + rot * obj.Pos;
    var objRot = rot * obj.Rot;
    var prefab = ZNetScene.instance.GetPrefab(obj.Prefab);
    if (!prefab)
    {
      if (BlueprintManager.TryGet(obj.Prefab, out var bp))
      {
        Blueprint(bp, objPos, objRot, mode);
        return;
      }
      ExpandWorld.Log.LogWarning($"Blueprint prefab {obj.Prefab} not found!");
      return;
    }
    if (mode == ZoneSystem.SpawnMode.Ghost)
    {
      ZNetView.StartGhostInit();
    }
    var go = UnityEngine.Object.Instantiate<GameObject>(prefab, pos, rot);
    if (mode == ZoneSystem.SpawnMode.Ghost)
    {
      UnityEngine.Object.Destroy(go);
      ZNetView.FinishGhostInit();
    }
  }
  static void Blueprint(Blueprint bp, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode)
  {
    foreach (var obj in bp.Objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      BPO(pos, rot, mode, obj);
    }
  }
}
