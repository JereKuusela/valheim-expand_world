using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExpandWorld;

public class LocationSpawning
{
  public static ZDO? DataOverride(string location, string prefab)
  {
    if (!LocationLoading.ObjectData.TryGetValue(location, out var objectData)) return null;
    if (!objectData.TryGetValue(prefab, out var data)) return null;
    return data;
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
  public static bool TryGetSwap(string location, string prefab, out string swapped)
  {
    swapped = "";
    if (!LocationLoading.ObjectSwaps.TryGetValue(location, out var objectSwaps)) return false;
    if (!objectSwaps.TryGetValue(prefab, out var swaps)) return false;
    swapped = RandomizeSwap(swaps);
    return true;
  }
  static string DummyObj = "vfx_auto_pickup";
  public static GameObject DummySpawn => UnityEngine.Object.Instantiate<GameObject>(ZNetScene.instance.GetPrefab(DummyObj), Vector3.zero, Quaternion.identity);
  public static GameObject Object(GameObject prefab, Vector3 position, Quaternion rotation, List<GameObject> spawnedGhostObjects, ZoneSystem.ZoneLocation location)
  {
    BlueprintObject bpo = new(Utils.GetPrefabName(prefab), position, rotation, prefab.transform.localScale, "", null, 1f);
    var locName = location.m_prefabName;
    if (LocationSpawning.TryGetSwap(locName, bpo.Prefab, out var objName))
      bpo.Prefab = objName;
    var obj = Spawn.BPO(locName, bpo, DataOverride, spawnedGhostObjects);
    return obj ?? LocationSpawning.DummySpawn;
  }


  public static void CustomObjects(ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, List<GameObject> spawnedGhostObjects)
  {
    if (!LocationLoading.Objects.TryGetValue(location.m_prefabName, out var objects)) return;
    foreach (var obj in objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      Spawn.BPO(location.m_prefabName, obj, DataOverride, spawnedGhostObjects);
    }
  }
}
