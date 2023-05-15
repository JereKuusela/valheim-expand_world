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
  public static string PrefabOverride(string location, string prefab)
  {
    if (!LocationLoading.ObjectSwaps.TryGetValue(location, out var objectSwaps)) return prefab;
    if (!objectSwaps.TryGetValue(prefab, out var swaps)) return prefab;
    return Spawn.RandomizeSwap(swaps);
  }
  static string DummyObj = "vfx_auto_pickup";
  public static GameObject DummySpawn => UnityEngine.Object.Instantiate<GameObject>(ZNetScene.instance.GetPrefab(DummyObj), Vector3.zero, Quaternion.identity);
  public static GameObject Object(GameObject prefab, Vector3 position, Quaternion rotation, List<GameObject> spawnedGhostObjects, ZoneSystem.ZoneLocation location)
  {
    BlueprintObject bpo = new(Utils.GetPrefabName(prefab), position, rotation, prefab.transform.localScale, "", null, 1f);
    var locName = location.m_prefabName;
    var obj = Spawn.BPO(locName, bpo, DataOverride, PrefabOverride, spawnedGhostObjects);
    return obj ?? LocationSpawning.DummySpawn;
  }


  public static void CustomObjects(ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, List<GameObject> spawnedGhostObjects)
  {
    if (!LocationLoading.Objects.TryGetValue(location.m_prefabName, out var objects)) return;
    foreach (var obj in objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      Spawn.BPO(location.m_prefabName, obj, DataOverride, PrefabOverride, spawnedGhostObjects);
    }
  }
}
