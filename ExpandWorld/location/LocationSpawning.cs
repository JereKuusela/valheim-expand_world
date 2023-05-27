using System.Collections.Generic;
using UnityEngine;

namespace ExpandWorld;

public class LocationSpawning
{

  // Random damage should override preset health.
  private static readonly int HealthHash = "health".GetStableHashCode();
  private static ZDO RemoveHealth(ZDO zdo, string location)
  {
    if (!LocationLoading.LocationData.TryGetValue(location, out var data)) return zdo;
    if (data.randomDamage && zdo.m_floats != null) zdo.m_floats.Remove(HealthHash);
    return zdo;
  }
  public static ZDO? DataOverride(ZDO? zdo, string location, string prefab)
  {
    if (zdo == null)
    {
      if (!LocationLoading.ObjectData.TryGetValue(location, out var objectData)) return null;
      if (!objectData.TryGetValue(prefab, out var data)) return null;
      zdo = Spawn.RandomizeData(data);
      ExpandWorld.Log.LogDebug($"Replaced data for {prefab} in {location}");
    }
    if (zdo == null) return null;
    return RemoveHealth(zdo, location);
  }
  public static string PrefabOverride(string location, string prefab)
  {
    if (!LocationLoading.ObjectSwaps.TryGetValue(location, out var objectSwaps)) return prefab;
    if (!objectSwaps.TryGetValue(prefab, out var swaps)) return prefab;
    return Spawn.RandomizeSwap(swaps);
  }
  static readonly string DummyObj = "vfx_auto_pickup";
  public static GameObject DummySpawn => UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab(DummyObj), Vector3.zero, Quaternion.identity);
  public static GameObject Object(GameObject prefab, Vector3 pos, Quaternion rot, List<GameObject> spawnedGhostObjects, ZoneSystem.ZoneLocation location)
  {
    BlueprintObject bpo = new(Utils.GetPrefabName(prefab), pos, rot, prefab.transform.localScale, "", null, 1f);
    var locName = location.m_prefabName;
    var obj = Spawn.BPO(locName, bpo, DataOverride, PrefabOverride, spawnedGhostObjects);
    return obj ?? DummySpawn;
  }


  public static void CustomObjects(ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, List<GameObject> spawnedGhostObjects)
  {
    if (!LocationLoading.Objects.TryGetValue(location.m_prefabName, out var objects)) return;
    ExpandWorld.Log.LogDebug($"Spawning {objects.Count} custom objects in {location.m_prefabName}");
    foreach (var obj in objects)
    {
      if (obj.Chance < 1f && Random.value > obj.Chance) continue;
      Spawn.BPO(location.m_prefabName, obj, pos, rot, DataOverride, PrefabOverride, spawnedGhostObjects);
    }
  }
}
