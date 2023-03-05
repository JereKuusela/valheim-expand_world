using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace ExpandWorld;

public class LocationManager
{
  public static string FileName = "expand_locations.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  public static string Pattern = "expand_locations*.yaml";
  public static Dictionary<string, ZDO?> ZDO = new();
  public static Dictionary<string, Dictionary<string, List<Tuple<float, string>>>> ObjectSwaps = new();
  public static Dictionary<string, Dictionary<string, ZDO?>> ObjectData = new();
  public static Dictionary<string, List<BlueprintObject>> Objects = new();
  public static Dictionary<string, LocationData> LocationData = new();
  public static Dictionary<string, string> Dungeons = new();
  public static Dictionary<string, Blueprint> BlueprintFiles = new();
  public static Dictionary<string, Location> BlueprintLocations = new();
  public static ZoneSystem.ZoneLocation FromData(LocationData data)
  {
    var loc = new ZoneSystem.ZoneLocation();
    LocationData[data.prefab] = data;
    if (data.data != "")
      ZDO[data.prefab] = Data.ToZDO(data.data);
    if (data.dungeon != "")
      Dungeons[data.prefab] = data.dungeon;
    if (data.objectSwap != null)
    {
      ObjectSwaps[data.prefab] = data.objectSwap.Select(Data.ToList)
        .ToDictionary(arr => arr[0], arr => ParseTuple(arr.Skip(1)));
    }
    if (data.objectData != null)
      ObjectData[data.prefab] = data.objectData.Select(Data.ToList).ToDictionary(arr => arr[0], arr => Data.ToZDO(arr[1]));
    if (data.objects != null)
    {
      Objects[data.prefab] = data.objects.Select(s => s.Split(',')).Select(split => new BlueprintObject(
        split[0],
        Parse.VectorXZY(split, 1),
        Parse.AngleYXZ(split, 4),
        Parse.VectorXZY(split, 7, Vector3.one),
        "",
        Data.ToZDO(split.Length > 10 ? split[11] : ""),
        Parse.Float(split, 10, 1f)
      )).ToList();
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
  private static List<Tuple<float, string>> ParseTuple(IEnumerable<string> items)
  {
    var total = 0f;
    return items.Select(s => s.Split(':')).Select(s =>
    {
      var weight = Parse.Float(s, 1, 1f);
      total += weight;
      return Tuple.Create(weight, s[0]);
    }).ToList().Select(t => Tuple.Create(t.Item1 / total, t.Item2)).ToList();
  }
  public static LocationData ToData(ZoneSystem.ZoneLocation loc)
  {
    LocationData data = new();
    // For migrations, ensures that old data is preserved.
    if (LocationData.TryGetValue(loc.m_prefabName, out var existing))
      data = existing;
    data.prefab = loc.m_prefabName;
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
    if (loc.m_maxAltitude == 1000f)
      data.maxAltitude = 10000f;
    else
      data.maxAltitude = loc.m_maxAltitude;
    if (loc.m_location)
    {
      data.randomDamage = loc.m_location.m_applyRandomDamage;
      data.exteriorRadius = loc.m_location.m_exteriorRadius;
      data.clearArea = loc.m_location.m_clearArea;
      data.noBuild = loc.m_location.m_noBuild ? "true" : "";
    }
    return data;
  }
  public static bool IsValid(ZoneSystem.ZoneLocation loc) => loc.m_prefab;

  private static void ToFile(bool force = false)
  {
    if (!force && File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(ZoneSystem.instance.m_locations.Where(IsValid).Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }

  public static void Load()
  {
    if (Helper.IsClient()) return;
    if (Configuration.DataLocation && File.Exists(FilePath))
      SetLocations(FromFile(Data.Read(Pattern)), true);
    else
      SetLocations(DefaultItems, false);
    UpdateInstances();
    NoBuildManager.UpdateData();
    var force = LocationData.Values.Any(value => value.exteriorRadius == 0f && !BlueprintLocations.ContainsKey(value.prefab));
    ToFile(force);
  }
  private static void UpdateInstances()
  {
    var zs = ZoneSystem.m_instance;
    var instances = zs.m_locationInstances;
    foreach (var zone in instances.Keys.ToArray())
    {
      var value = instances[zone];
      var location = zs.GetLocation(value.m_location.m_prefabName);
      value.m_location = location;
      instances[zone] = value;
    }
  }
  public static List<ZoneSystem.ZoneLocation> DefaultItems = new();
  private static List<ZoneSystem.ZoneLocation> FromFile(string yaml)
  {
    try
    {
      ZDO.Clear();
      ObjectSwaps.Clear();
      ObjectData.Clear();
      Dungeons.Clear();
      Objects.Clear();
      LocationData.Clear();
      BlueprintFiles.Clear();
      var data = Data.Deserialize<LocationData>(yaml, FileName)
        .Select(FromData).ToList();
      ExpandWorld.Log.LogInfo($"Reloading {data.Count} location data.");
      foreach (var list in LocationList.m_allLocationLists)
        list.m_locations.Clear();
      return data;
    }
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.StackTrace);
    }
    return new();
  }
  private static void ApplyLocationData(ZoneSystem.ZoneLocation item, float? radius = null)
  {
    if (!LocationData.TryGetValue(item.m_prefabName, out var data)) return;
    // Old config won't have exterior radius so don't set anything.
    if (data.exteriorRadius == 0f && radius == null) return;
    item.m_location.m_exteriorRadius = data.exteriorRadius;
    item.m_exteriorRadius = item.m_location.m_exteriorRadius;
    if (radius.HasValue && item.m_exteriorRadius == 0)
      item.m_exteriorRadius = radius.Value;
    item.m_location.m_applyRandomDamage = data.randomDamage;
    item.m_location.m_clearArea = data.clearArea;
    item.m_location.m_noBuild = data.noBuild != "" && data.noBuild != "0" && data.noBuild != "false";
  }
  public static Location GetBluePrintLocation(string prefab)
  {
    if (!LocationManager.BlueprintLocations.TryGetValue(prefab, out var location))
    {
      var obj = new GameObject();
      location = obj.AddComponent<Location>();
      LocationManager.BlueprintLocations.Add(prefab, location);
    }
    return location;
  }
  ///<summary>Copies setup from locations.</summary>
  private static void Setup(ZoneSystem.ZoneLocation item, bool showWarnings)
  {
    var prefabName = item.m_prefabName.Split(':')[0];
    item.m_hash = item.m_prefabName.GetStableHashCode();
    if (!ZoneLocations.TryGetValue(prefabName, out var zoneLocation) || zoneLocation.m_prefab == null || zoneLocation.m_location == null)
    {
      if (Blueprints.TryGetBluePrint(prefabName, out var bp))
      {
        float? offset = null;
        Vector2? center = null;
        if (LocationData.TryGetValue(item.m_prefabName, out var data))
        {
          offset = data.offset;
          center = Parse.VectorXY(data.center);
        }
        bp.Center(center);
        bp.Offset(offset);
        BlueprintFiles[prefabName] = bp;
        item.m_prefab = new();
        item.m_location = GetBluePrintLocation(item.m_prefabName);
        ApplyLocationData(item, bp.Radius + 5);
        item.m_netViews = new();
        item.m_randomSpawns = new();
        return;
      }
      if (showWarnings)
        ExpandWorld.Log.LogWarning($"Location prefab {prefabName} not found!");
      return;
    }
    item.m_prefab = zoneLocation.m_prefab;
    item.m_location = zoneLocation.m_location;
    item.m_interiorRadius = zoneLocation.m_interiorRadius;
    item.m_exteriorRadius = zoneLocation.m_exteriorRadius;
    item.m_interiorPosition = zoneLocation.m_interiorPosition;
    item.m_generatorPosition = zoneLocation.m_generatorPosition;
    ApplyLocationData(item);
    item.m_netViews = zoneLocation.m_netViews;
    item.m_randomSpawns = zoneLocation.m_randomSpawns;
  }
  ///<summary>Sets zone location entries (ensures that all locations have an entry).</summary>
  public static void SetLocations(List<ZoneSystem.ZoneLocation> items, bool showWarnings)
  {
    var zs = ZoneSystem.instance;
    var missingLocations = ZoneLocations.Keys.ToHashSet();
    foreach (var item in items)
    {
      Setup(item, showWarnings);
      missingLocations.Remove(item.m_prefabName);
    }
    zs.m_locations = items;
    zs.m_locations.AddRange(missingLocations.Select(name => ZoneLocations[name]));
    ExpandWorld.Log.LogDebug($"Loaded {zs.m_locations.Count} zone locations.");
    UpdateHashes();
  }
  private static Dictionary<string, ZoneSystem.ZoneLocation> ZoneLocations = new();
  public static void SetupLocations(List<ZoneSystem.ZoneLocation> initialized)
  {
    ZoneLocations = initialized.ToDictionary(kvp => kvp.m_prefabName, kvp => kvp);
    var array = Resources.FindObjectsOfTypeAll<GameObject>();
    foreach (var obj in array)
    {
      if (obj.name != "_Locations") continue;
      var locations = obj.GetComponentsInChildren<Location>(true);
      foreach (var location in locations)
        SetupLocation(location);
    }
    ExpandWorld.Log.LogDebug($"Loaded {ZoneLocations.Count} locations.");
  }

  public static void UpdateHashes()
  {
    var zs = ZoneSystem.instance;
    foreach (ZoneSystem.ZoneLocation zoneLocation in zs.m_locations)
      zs.m_locationsByHash[zoneLocation.m_hash] = zoneLocation;
    ExpandWorld.Log.LogDebug($"Loaded {zs.m_locationsByHash.Count} zone hashes.");
  }
  ///<summary>Initializes a zone location.</summary>
  private static void SetupLocation(Location location)
  {
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
    if (location.m_interiorTransform && location.m_generator)
    {
      zoneLocation.m_interiorPosition = location.m_interiorTransform.localPosition;
      zoneLocation.m_generatorPosition = location.m_generator.transform.localPosition;
    }
    ZoneSystem.PrepareNetViews(zoneLocation.m_prefab, zoneLocation.m_netViews);
    ZoneSystem.PrepareRandomSpawns(zoneLocation.m_prefab, zoneLocation.m_randomSpawns);
    ZoneLocations[location.gameObject.name] = zoneLocation;
  }

  public static void SetupWatcher()
  {
    Data.SetupWatcher(Pattern, Load);
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.CreateLocationProxy))]
public class LocationZDO
{
  static void Prefix(ZoneSystem __instance, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rotation, ZoneSystem.SpawnMode mode)
  {
    if (!LocationManager.ZDO.TryGetValue(location.m_prefabName, out var data)) return;
    if (!__instance.m_locationProxyPrefab.TryGetComponent<ZNetView>(out var view)) return;
    if (data != null) Data.InitZDO(pos, rotation, data, view);
  }
}
[HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.SetLocation))]
public class FixGhostInit
{
  static void Prefix(LocationProxy __instance, ref bool spawnNow)
  {
    if (ZNetView.m_ghostInit)
    {
      spawnNow = false;
      Data.CleanGhostInit(__instance.m_nview);
    }
  }
}


[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
public class LocationObjectDataAndSwap
{
  private static string Location = "";
  static bool Prefix(ZoneSystem.ZoneLocation location, ZoneSystem.SpawnMode mode)
  {
    if (mode != ZoneSystem.SpawnMode.Client)
      Location = location.m_prefabName;
    return !LocationManager.BlueprintFiles.ContainsKey(location.m_prefabName);
  }
  static void SetData(string location, GameObject prefab, Vector3 position, Quaternion rotation, ZDO? data = null)
  {
    if (data == null)
    {
      if (!LocationManager.ObjectData.TryGetValue(location, out var objectData)) return;
      if (!objectData.TryGetValue(Utils.GetPrefabName(prefab), out data)) return;
    }
    if (data == null) return;
    if (!prefab.TryGetComponent<ZNetView>(out var view)) return;
    Data.InitZDO(position, rotation, data, view);
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
  static GameObject Swap(string location, GameObject prefab)
  {
    if (!LocationManager.ObjectSwaps.TryGetValue(location, out var objectSwaps)) return prefab;
    if (!objectSwaps.TryGetValue(Utils.GetPrefabName(prefab), out var swaps)) return prefab;
    var swap = RandomizeSwap(swaps);
    return ZNetScene.instance.GetPrefab(swap) ?? prefab;
  }
  public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    prefab = Swap(Location, prefab);
    return InstantiateWithData(Location, prefab, position, rotation);
  }
  public static GameObject InstantiateWithData(string location, GameObject prefab, Vector3 position, Quaternion rotation, ZDO? data = null)
  {
    SetData(location, prefab, position, rotation, data);
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    if (obj.TryGetComponent<DungeonGenerator>(out var dg))
    {
      if (LocationManager.Dungeons.TryGetValue(location, out var dungeon))
        DungeonManager.Override(dg, dungeon);
      else
        DungeonManager.Override(dg, Utils.GetPrefabName(obj));
    }
    Data.CleanGhostInit(obj);
    return obj;
  }

  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var instantiator = typeof(UnityEngine.Object).GetMethods().First(m => m.Name == nameof(UnityEngine.Object.Instantiate) && m.IsGenericMethodDefinition && m.GetParameters().Skip(1).Select(p => p.ParameterType).SequenceEqual(new[] { typeof(Vector3), typeof(Quaternion) })).MakeGenericMethod(typeof(GameObject));
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Call, instantiator))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(Instantiate).operand)
      .InstructionEnumeration();
  }

  static void SpawnBPO(ZoneSystem __instance, bool flag, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects, BlueprintObject obj)
  {
    var objPos = pos + rot * obj.Pos;
    var objRot = rot * obj.Rot;
    if (mode == ZoneSystem.SpawnMode.Ghost)
    {
      ZNetView.StartGhostInit();
    }
    var prefab = ZNetScene.instance.GetPrefab(obj.Prefab);
    if (!prefab)
    {
      ExpandWorld.Log.LogWarning($"Blueprint prefab {obj.Prefab} not found!");
      ZNetView.FinishGhostInit();
      return;
    }
    var go = InstantiateWithData(location.m_prefabName, prefab, objPos, objRot, obj.Data);
    go.GetComponent<ZNetView>().GetZDO().SetPGWVersion(__instance.m_pgwVersion);
    if (go.TryGetComponent<DungeonGenerator>(out var dg))
    {
      if (flag)
        dg.m_originalPosition = location.m_generatorPosition;
      dg.Generate(mode);
    }
    if (mode == ZoneSystem.SpawnMode.Ghost)
    {
      spawnedGhostObjects.Add(go);
      ZNetView.FinishGhostInit();
    }
  }

  [HarmonyPostfix, HarmonyPriority(Priority.LowerThanNormal)]
  static void SpawnCustomObjects(ZoneSystem __instance, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects)
  {
    if (mode == ZoneSystem.SpawnMode.Client) return;
    if (!LocationManager.Objects.TryGetValue(location.m_prefabName, out var objects)) return;
    var loc = location.m_location;
    var flag = loc && loc.m_useCustomInteriorTransform && loc.m_interiorTransform && loc.m_generator;
    foreach (var obj in objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      SpawnBPO(__instance, flag, location, pos, rot, mode, spawnedGhostObjects, obj);
    }
  }
  [HarmonyPostfix]
  static void SpawnBlueprint(ZoneSystem __instance, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects)
  {
    if (mode == ZoneSystem.SpawnMode.Client) return;
    if (!LocationManager.BlueprintFiles.TryGetValue(location.m_prefabName, out var bp)) return;
    if (LocationManager.LocationData.TryGetValue(location.m_prefabName, out var data))
    {
      if (data.levelArea != 0f)
        Terrain.Level(pos, location.m_exteriorRadius, 1f - data.levelArea);
    }
    var loc = location.m_location;
    WearNTear.m_randomInitialDamage = loc.m_applyRandomDamage;
    var flag = loc && loc.m_useCustomInteriorTransform && loc.m_interiorTransform && loc.m_generator;
    foreach (var obj in bp.Objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      SpawnBPO(__instance, flag, location, pos, rot, mode, spawnedGhostObjects, obj);
    }
    WearNTear.m_randomInitialDamage = false;
  }

  public static IEnumerable<CodeInstruction> TranspileInstantiate(IEnumerable<CodeInstruction> instructions)
  {
    var instantiator = typeof(UnityEngine.Object).GetMethods().First(m => m.Name == nameof(UnityEngine.Object.Instantiate) && m.IsGenericMethodDefinition && m.GetParameters().Skip(1).Select(p => p.ParameterType).SequenceEqual(new[] { typeof(Vector3), typeof(Quaternion) })).MakeGenericMethod(typeof(GameObject));
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Call, instantiator)).Set(OpCodes.Call, Transpilers.EmitDelegate(LocationObjectDataAndSwap.Instantiate).operand)
      .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.PlaceRoom), typeof(DungeonDB.RoomData), typeof(Vector3), typeof(Quaternion), typeof(RoomConnection), typeof(ZoneSystem.SpawnMode))]
public class DungeonDataAndSwap
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => LocationObjectDataAndSwap.TranspileInstantiate(instructions);
}
[HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.PlaceDoors))]
public class DungeonDoorDataAndSwap
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => LocationObjectDataAndSwap.TranspileInstantiate(instructions);
}
