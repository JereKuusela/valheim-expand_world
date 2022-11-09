using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace ExpandWorld;

public class LocationManager {
  public static string FileName = "expand_locations.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.ConfigPath, FileName);
  public static string Pattern = "expand_locations*.yaml";
  public static Dictionary<string, ZDO> ZDO = new();
  public static ZoneSystem.ZoneLocation FromData(LocationData data) {
    var loc = new ZoneSystem.ZoneLocation();
    if (data.data != "") {
      ZPackage pkg = new(data.data);
      ZDO zdo = new();
      Data.Deserialize(zdo, pkg);
      ZDO[data.prefab] = zdo;
    }
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
    if (data.minDistance > 1f)
      data.minDistance /= 10000f;
    data.maxDistance = loc.m_maxDistance;
    if (data.maxDistance > 1f)
      data.maxDistance /= 10000f;
    data.minAltitude = loc.m_minAltitude;
    data.maxAltitude = loc.m_maxAltitude;
    return data;
  }
  public static bool IsValid(ZoneSystem.ZoneLocation loc) => loc.m_prefab;

  private static void ToFile() {
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(ZoneSystem.instance.m_locations.Where(IsValid).Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }

  public static void Load() {
    if (Helper.IsClient()) return;
    if (Configuration.DataLocation && File.Exists(FilePath))
      SetLocations(FromFile(Data.Read(Pattern)));
    else
      SetLocations(DefaultItems);
    ToFile();
  }
  public static List<ZoneSystem.ZoneLocation> DefaultItems = new();
  private static List<ZoneSystem.ZoneLocation> FromFile(string yaml) {
    try {
      ZDO.Clear();
      var data = Data.Deserialize<LocationData>(yaml, FileName)
        .Select(FromData).ToList();
      ExpandWorld.Log.LogInfo($"Reloading {data.Count} location data.");
      foreach (var list in LocationList.m_allLocationLists)
        list.m_locations.Clear();
      return data;
    } catch (Exception e) {
      ExpandWorld.Log.LogError(e.StackTrace);
    }
    return new();
  }
  ///<summary>Copies setup from locations.</summary>
  private static ZoneSystem.ZoneLocation Setup(ZoneSystem.ZoneLocation item) {
    var prefabName = item.m_prefabName.Split(':')[0];
    if (!ZoneLocations.TryGetValue(prefabName, out var zoneLocation)) {
      // Don't warn on the default data since it has missing stuff.
      if (File.Exists(FilePath))
        ExpandWorld.Log.LogWarning($"Location prefab {prefabName} not found!");
      return item;
    }
    item.m_prefab = zoneLocation.m_prefab;
    item.m_location = zoneLocation.m_location;
    item.m_interiorRadius = zoneLocation.m_interiorRadius;
    item.m_exteriorRadius = zoneLocation.m_exteriorRadius;
    item.m_interiorPosition = zoneLocation.m_interiorPosition;
    item.m_generatorPosition = zoneLocation.m_generatorPosition;
    item.m_hash = zoneLocation.m_hash;
    item.m_netViews = zoneLocation.m_netViews;
    item.m_randomSpawns = zoneLocation.m_randomSpawns;
    return item;
  }
  ///<summary>Sets zone location entries (ensures that all locations have an entry).</summary>
  public static void SetLocations(List<ZoneSystem.ZoneLocation> items) {
    var zs = ZoneSystem.instance;
    var missingLocations = ZoneLocations.Keys.ToHashSet();
    foreach (var item in items) {
      Setup(item);
      missingLocations.Remove(item.m_prefabName);
    }
    zs.m_locations = items;
    zs.m_locations.AddRange(missingLocations.Select(name => ZoneLocations[name]));
    ExpandWorld.Log.LogDebug($"Loaded {zs.m_locations.Count} zone locations.");
    UpdateHashes();
  }
  private static Dictionary<string, ZoneSystem.ZoneLocation> ZoneLocations = new();
  public static void SetupLocations(List<ZoneSystem.ZoneLocation> initialized) {
    ZoneLocations = initialized.ToDictionary(kvp => kvp.m_prefabName, kvp => kvp);
    var array = Resources.FindObjectsOfTypeAll<GameObject>();
    foreach (var obj in array) {
      if (obj.name != "_Locations") continue;
      var locations = obj.GetComponentsInChildren<Location>(true);
      foreach (var location in locations)
        SetupLocation(location);
    }
    ExpandWorld.Log.LogDebug($"Loaded {ZoneLocations.Count} locations.");
  }

  private static void UpdateHashes() {
    var zs = ZoneSystem.instance;
    foreach (ZoneSystem.ZoneLocation zoneLocation in zs.m_locations)
      zs.m_locationsByHash[zoneLocation.m_hash] = zoneLocation;
    ExpandWorld.Log.LogDebug($"Loaded {zs.m_locationsByHash.Count} zone hashes.");
  }
  ///<summary>Initializes a zone location.</summary>
  private static void SetupLocation(Location location) {
    ZoneSystem.ZoneLocation zoneLocation = new();
    zoneLocation.m_enable = false;
    zoneLocation.m_maxTerrainDelta = 10f;
    zoneLocation.m_randomRotation = false;
    zoneLocation.m_prefab = location.gameObject;
    zoneLocation.m_prefabName = zoneLocation.m_prefab.name;
    zoneLocation.m_hash = zoneLocation.m_prefabName.GetStableHashCode();
    zoneLocation.m_location = location;
    zoneLocation.m_interiorRadius = (location.m_hasInterior ? location.m_interiorRadius : 0f);
    zoneLocation.m_exteriorRadius = location.m_exteriorRadius;
    if (location.m_interiorTransform && location.m_generator) {
      zoneLocation.m_interiorPosition = location.m_interiorTransform.localPosition;
      zoneLocation.m_generatorPosition = location.m_generator.transform.localPosition;
    }
    ZoneSystem.PrepareNetViews(zoneLocation.m_prefab, zoneLocation.m_netViews);
    ZoneSystem.PrepareRandomSpawns(zoneLocation.m_prefab, zoneLocation.m_randomSpawns);
    ZoneLocations[location.gameObject.name] = zoneLocation;
  }

  public static void SetupWatcher() {
    Data.SetupWatcher(Pattern, Load);
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.CreateLocationProxy))]
public class LocationZDO {
  static void Prefix(ZoneSystem __instance, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rotation, ZoneSystem.SpawnMode mode) {
    if (!LocationManager.ZDO.TryGetValue(location.m_prefabName, out var data)) return;
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(pos);
    Data.CopyData(data.Clone(), ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = rotation;
    if (__instance.m_locationProxyPrefab.GetComponent<ZNetView>() is { } view) {
      ZNetView.m_initZDO.m_type = view.m_type;
      ZNetView.m_initZDO.m_distant = view.m_distant;
      ZNetView.m_initZDO.m_persistent = view.m_persistent;
      ZNetView.m_initZDO.m_prefab = view.GetPrefabName().GetStableHashCode();
    }
    ZNetView.m_initZDO.m_dataRevision = 1;

  }
}
[HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.SetLocation))]
public class FixGhostInit {
  static void Prefix(LocationProxy __instance, ref bool spawnNow) {
    if (ZNetView.m_ghostInit) {
      spawnNow = false;
      __instance.m_nview.m_ghost = true;
      ZNetScene.instance.m_instances.Remove(__instance.m_nview.GetZDO());
    }
  }
}