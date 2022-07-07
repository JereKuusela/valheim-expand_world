using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ExpandWorld;

public class LocationManager {
  public static string FileName = Path.Combine(ExpandWorld.ConfigPath, "expand_locations.yaml");
  public static ZoneSystem.ZoneLocation FromData(LocationData data) {
    var loc = new ZoneSystem.ZoneLocation();
    loc.m_prefabName = data.prefab;
    loc.m_enable = data.enabled;
    loc.m_biome = Data.ToBiomes(data.biome);
    loc.m_biomeArea = Data.ToBiomeAreas(data.biomeArea);
    loc.m_quantity = data.quantity;
    loc.m_prioritized = data.prioritized;
    loc.m_centerFirst = data.centerFirst;
    loc.m_unique = data.unique;
    loc.m_group = data.group;
    loc.m_minDistanceFromSimilar = data.minDistanceFromSimilar;
    loc.m_iconAlways = data.iconAlways;
    loc.m_iconPlaced = data.iconPlaced;
    loc.m_randomRotation = data.randomRotation;
    loc.m_slopeRotation = data.slopeRotation;
    loc.m_snapToWater = data.snapToWater;
    loc.m_minTerrainDelta = data.minTerrainDelta;
    loc.m_maxTerrainDelta = data.maxTerrainDelta;
    loc.m_inForest = data.inForest;
    loc.m_forestTresholdMin = data.forestTresholdMin;
    loc.m_forestTresholdMax = data.forestTresholdMax;
    loc.m_minDistance = data.minDistance;
    loc.m_maxDistance = data.maxDistance;
    loc.m_minAltitude = data.minAltitude;
    loc.m_maxAltitude = data.maxAltitude;
    return loc;
  }
  public static LocationData ToData(ZoneSystem.ZoneLocation loc) {
    LocationData data = new();
    data.prefab = loc.m_prefab.name;
    data.enabled = loc.m_enable;
    data.biome = Data.FromBiomes(loc.m_biome);
    data.biomeArea = Data.FromBiomeAreas(loc.m_biomeArea);
    data.quantity = loc.m_quantity;
    data.prioritized = loc.m_prioritized;
    data.centerFirst = loc.m_centerFirst;
    data.unique = loc.m_unique;
    data.group = loc.m_group;
    data.minDistanceFromSimilar = loc.m_minDistanceFromSimilar;
    data.iconAlways = loc.m_iconAlways;
    data.iconPlaced = loc.m_iconPlaced;
    data.randomRotation = loc.m_randomRotation;
    data.slopeRotation = loc.m_slopeRotation;
    data.snapToWater = loc.m_snapToWater;
    data.maxTerrainDelta = loc.m_maxTerrainDelta;
    data.minTerrainDelta = loc.m_minTerrainDelta;
    data.inForest = loc.m_inForest;
    data.forestTresholdMin = loc.m_forestTresholdMin;
    data.forestTresholdMax = loc.m_forestTresholdMax;
    data.minDistance = loc.m_minDistance;
    data.maxDistance = loc.m_maxDistance;
    data.minAltitude = loc.m_minAltitude;
    data.maxAltitude = loc.m_maxAltitude;
    return data;
  }
  public static bool IsValid(ZoneSystem.ZoneLocation loc) => loc.m_prefab;

  public static void ToFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataLocation) return;
    if (File.Exists(FileName)) return;
    var yaml = Data.Serializer().Serialize(ZoneSystem.instance.m_locations.Where(IsValid).Select(ToData).ToList());
    File.WriteAllText(FileName, yaml);
  }
  public static void FromFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataLocation) return;
    if (!File.Exists(FileName)) return;
    var raw = File.ReadAllText(FileName);
    Configuration.configInternalDataLocations.Value = raw;
    if (Data.IsLoading) Set(raw);
  }
  public static void FromSetting(string raw) {
    if (!Data.IsLoading) Set(raw);
  }
  private static void Set(string raw) {
    if (raw == "" || !Configuration.DataLocation) return;
    var data = Data.Deserializer().Deserialize<List<LocationData>>(raw)
      .Select(FromData).ToList();
    if (data.Count == 0) {
      ExpandWorld.Log.LogWarning($"Failed to load any location data.");
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading {data.Count} location data.");
    foreach (var list in LocationList.m_allLocationLists)
      list.m_locations.Clear();
    ZoneSystem.instance.m_locations = data;
    if (!Data.IsLoading)
      Setup();
  }
  private static void Setup() {
    GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
    List<Location> list = new List<Location>();
    foreach (GameObject gameObject in array) {
      if (gameObject.name == "_Locations") {
        Location[] componentsInChildren = gameObject.GetComponentsInChildren<Location>(true);
        list.AddRange(componentsInChildren);
      }
    }
    var zs = ZoneSystem.instance;
    foreach (ZoneSystem.ZoneLocation zoneLocation in zs.m_locations) {
      Transform? transform = null;
      foreach (Location location in list) {
        if (location.gameObject.name == zoneLocation.m_prefabName) {
          transform = location.transform;
          break;
        }
      }
      if (transform == null) {
        ExpandWorld.Log.LogWarning($"Location prefab {zoneLocation.m_prefabName} not found!");
        continue;
      }
      zoneLocation.m_prefab = transform.gameObject;
      zoneLocation.m_hash = zoneLocation.m_prefab.name.GetStableHashCode();
      Location componentInChildren = zoneLocation.m_prefab.GetComponentInChildren<Location>();
      zoneLocation.m_location = componentInChildren;
      zoneLocation.m_interiorRadius = (componentInChildren.m_hasInterior ? componentInChildren.m_interiorRadius : 0f);
      zoneLocation.m_exteriorRadius = componentInChildren.m_exteriorRadius;
      if (componentInChildren.m_interiorTransform && componentInChildren.m_generator) {
        zoneLocation.m_interiorPosition = componentInChildren.m_interiorTransform.localPosition;
        zoneLocation.m_generatorPosition = componentInChildren.m_generator.transform.localPosition;
      }
      ZoneSystem.PrepareNetViews(zoneLocation.m_prefab, zoneLocation.m_netViews);
      ZoneSystem.PrepareRandomSpawns(zoneLocation.m_prefab, zoneLocation.m_randomSpawns);
      if (!zs.m_locationsByHash.ContainsKey(zoneLocation.m_hash)) {
        zs.m_locationsByHash.Add(zoneLocation.m_hash, zoneLocation);
      }

    }
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(FileName, FromFile);
  }
}
