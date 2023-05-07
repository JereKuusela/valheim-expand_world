using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExpandWorld;

public class LocationSpawning
{
  public static void SetData(string location, GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, ZDO? data = null)
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
  public static GameObject Object(GameObject prefab, Vector3 position, Quaternion rotation, List<GameObject> spawnedGhostObjects, ZoneSystem.ZoneLocation location)
  {
    var locName = location.m_prefabName;
    if (LocationSpawning.TryGetSwap(locName, prefab, out var objName))
      prefab = ZNetScene.instance.GetPrefab(objName);

    if (prefab)
      return LocationSpawning.InstantiateWithData(locName, prefab, position, rotation, prefab.transform.localScale);

    if (BlueprintManager.TryGet(objName, out var bp))
    {
      LocationSpawning.Blueprint(bp, location, position, rotation, spawnedGhostObjects);
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
  public static void BPO(ZoneSystem __instance, bool flag, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, List<GameObject>? spawnedGhostObjects, BlueprintObject obj)
  {
    var objPos = pos + rot * obj.Pos;
    var objRot = rot * obj.Rot;
    var prefab = ZNetScene.instance.GetPrefab(obj.Prefab);
    if (!prefab)
    {
      if (BlueprintManager.TryGet(obj.Prefab, out var bp))
      {
        Blueprint(bp, location, objPos, objRot, spawnedGhostObjects);
        return;
      }
      ExpandWorld.Log.LogWarning($"Blueprint prefab {obj.Prefab} not found!");
      return;
    }
    var go = InstantiateWithData(location.m_prefabName, prefab, objPos, objRot, obj.Scale, obj.Data);
    go.GetComponent<ZNetView>().GetZDO().SetPGWVersion(__instance.m_pgwVersion);
    if (go.TryGetComponent<DungeonGenerator>(out var dg))
    {
      if (flag)
        dg.m_originalPosition = location.m_generatorPosition;
      dg.Generate(ZNetView.m_ghostInit ? ZoneSystem.SpawnMode.Ghost : ZoneSystem.SpawnMode.Full);
    }
    if (ZNetView.m_ghostInit)
    {
      if (spawnedGhostObjects == null)
        UnityEngine.Object.Destroy(go);
      else
        spawnedGhostObjects.Add(go);
    }
  }

  public static void Blueprint(Blueprint bp, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, List<GameObject>? spawnedGhostObjects)
  {
    var zs = ZoneSystem.instance;
    var loc = location.m_location;
    WearNTear.m_randomInitialDamage = loc.m_applyRandomDamage;
    var flag = loc && loc.m_useCustomInteriorTransform && loc.m_interiorTransform && loc.m_generator;
    foreach (var obj in bp.Objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      BPO(zs, flag, location, pos, rot, spawnedGhostObjects, obj);
    }
    WearNTear.m_randomInitialDamage = false;
  }

  public static void CustomObjects(ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, List<GameObject> spawnedGhostObjects)
  {
    var zs = ZoneSystem.instance;
    if (!LocationLoading.Objects.TryGetValue(location.m_prefabName, out var objects)) return;
    var loc = location.m_location;
    var flag = loc && loc.m_useCustomInteriorTransform && loc.m_interiorTransform && loc.m_generator;
    foreach (var obj in objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      LocationSpawning.BPO(zs, flag, location, pos, rot, spawnedGhostObjects, obj);
    }
  }
}
