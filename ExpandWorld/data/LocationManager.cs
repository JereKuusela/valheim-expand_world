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
  public static string FilePath = Path.Combine(ExpandWorld.ConfigPath, FileName);
  public static string Pattern = "expand_locations*.yaml";
  public static Dictionary<string, ZDO> ZDO = new();
  public static Dictionary<string, Dictionary<string, string>> ObjectSwaps = new();
  public static Dictionary<string, Dictionary<string, ZDO>> ObjectData = new();
  public static Dictionary<string, List<BlueprintObject>> Objects = new();
  public static Dictionary<string, float> ClearAreas = new();
  public static HashSet<string> BlueprintNames = new();
  public static Dictionary<string, Location> BlueprintLocations = new();
  public static ZoneSystem.ZoneLocation FromData(LocationData data)
  {
    var loc = new ZoneSystem.ZoneLocation();
    if (data.clearRadius != 0f)
      ClearAreas[data.prefab] = data.clearRadius;
    if (data.data != "")
      ZDO[data.prefab] = Data.ToZDO(data.data);
    if (data.objectSwap != null)
      ObjectSwaps[data.prefab] = data.objectSwap;
    if (data.objectData != null)
      ObjectData[data.prefab] = data.objectData.ToDictionary(kvp => kvp.Key, kvp => Data.ToZDO(kvp.Value));
    if (data.objects != null)
    {
      Objects[data.prefab] = data.objects.Select(kvp => new BlueprintObject(
        kvp.Key,
        Parse.VectorXZY(kvp.Value.Split(','), 0),
        Parse.AngleYXZ(kvp.Value.Split(','), 3)
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
  public static LocationData ToData(ZoneSystem.ZoneLocation loc)
  {
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

  private static void ToFile()
  {
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(ZoneSystem.instance.m_locations.Where(IsValid).Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }

  public static void Load()
  {
    if (Helper.IsClient()) return;
    if (Configuration.DataLocation && File.Exists(FilePath))
      SetLocations(FromFile(Data.Read(Pattern)));
    else
      SetLocations(DefaultItems);
    ToFile();
  }
  public static List<ZoneSystem.ZoneLocation> DefaultItems = new();
  private static List<ZoneSystem.ZoneLocation> FromFile(string yaml)
  {
    try
    {
      ZDO.Clear();
      ObjectSwaps.Clear();
      ObjectData.Clear();
      Objects.Clear();
      ClearAreas.Clear();
      BlueprintNames.Clear();
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
  ///<summary>Copies setup from locations.</summary>
  private static ZoneSystem.ZoneLocation Setup(ZoneSystem.ZoneLocation item)
  {
    var prefabName = item.m_prefabName.Split(':')[0];
    if (!ZoneLocations.TryGetValue(prefabName, out var zoneLocation))
    {
      if (Blueprints.Exists(prefabName))
      {
        BlueprintNames.Add(prefabName);
        item.m_hash = item.m_prefabName.GetStableHashCode();
        item.m_prefab = new();
        if (!BlueprintLocations.TryGetValue(item.m_prefabName, out item.m_location))
        {
          var obj = new GameObject();
          item.m_location = obj.AddComponent<Location>();
          BlueprintLocations.Add(item.m_prefabName, item.m_location);
        }
        if (ClearAreas.TryGetValue(item.m_prefabName, out var radius))
        {
          item.m_location.m_clearArea = true;
          item.m_exteriorRadius = radius;
          item.m_interiorRadius = radius;
        }
        item.m_netViews = new();
        item.m_randomSpawns = new();
        return item;
      }
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
  public static void SetLocations(List<ZoneSystem.ZoneLocation> items)
  {
    var zs = ZoneSystem.instance;
    var missingLocations = ZoneLocations.Keys.ToHashSet();
    foreach (var item in items)
    {
      Setup(item);
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

  private static void UpdateHashes()
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
    Data.InitZDO(pos, rotation, data, view);
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
    Location = location.m_prefabName;
    return !LocationManager.BlueprintNames.Contains(Location);
  }
  static void SetData(GameObject prefab, Vector3 position, Quaternion rotation, ZDO? data = null)
  {
    if (data == null)
    {
      if (!LocationManager.ObjectData.TryGetValue(Location, out var objectData)) return;
      if (!objectData.TryGetValue(Utils.GetPrefabName(prefab), out data)) return;
    }
    if (data == null) return;
    if (!prefab.TryGetComponent<ZNetView>(out var view)) return;
    Data.InitZDO(position, rotation, data, view);
  }
  static GameObject Swap(GameObject prefab)
  {
    if (!LocationManager.ObjectSwaps.TryGetValue(Location, out var objectSwaps)) return prefab;
    if (!objectSwaps.TryGetValue(Utils.GetPrefabName(prefab), out var swap)) return prefab;
    return ZNetScene.instance.GetPrefab(swap) ?? prefab;
  }
  public static GameObject InstantiateWithSwap(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    prefab = Swap(prefab);
    return Instantiate(prefab, position, rotation);
  }
  public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation, ZDO? data = null)
  {
    SetData(prefab, position, rotation, data);
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    Data.CleanGhostInit(obj);
    return obj;
  }

  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZNetView), nameof(ZNetView.StartGhostInit))))
      .Advance(5)
      .Set(OpCodes.Call, Transpilers.EmitDelegate(InstantiateWithSwap).operand)
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
    var go = Instantiate(prefab, objPos, objRot, obj.Data);
    go.GetComponent<ZNetView>().GetZDO().SetPGWVersion(__instance.m_pgwVersion);
    var dg = go.GetComponent<DungeonGenerator>();
    if (dg)
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
      SpawnBPO(__instance, flag, location, pos, rot, mode, spawnedGhostObjects, obj);
    }
  }

  [HarmonyPostfix]
  static void SpawnBlueprint(ZoneSystem __instance, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects)
  {
    if (mode == ZoneSystem.SpawnMode.Client) return;
    if (!LocationManager.BlueprintNames.Contains(location.m_prefabName)) return;
    if (location.m_location.m_clearArea && location.m_exteriorRadius != 0f)
    {
      ExpandWorld.Log.LogWarning("EDITING");
      var compilers = Terrain.GetCompilers(pos, location.m_exteriorRadius);
      var indicer = Terrain.CreateIndexer(pos, location.m_exteriorRadius);
      var compilerIndices = Terrain.GetIndices(compilers, indicer);
      Terrain.ResetTerrain(compilerIndices, pos, location.m_exteriorRadius);
      Terrain.LevelTerrain(compilerIndices, pos, location.m_exteriorRadius, 0.5f, pos.y);
    }
    var bp = Blueprints.GetBluePrint(location.m_prefabName);
    if (bp == null) return;
    var loc = location.m_location;
    var flag = loc && loc.m_useCustomInteriorTransform && loc.m_interiorTransform && loc.m_generator;
    foreach (var obj in bp.Objects)
    {
      SpawnBPO(__instance, flag, location, pos, rot, mode, spawnedGhostObjects, obj);
    }
  }
}

[HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.PlaceRoom), typeof(DungeonDB.RoomData), typeof(Vector3), typeof(Quaternion), typeof(RoomConnection), typeof(ZoneSystem.SpawnMode))]
public class DungeonDataAndSwap
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZNetView), nameof(ZNetView.GetZDO))))
      .Advance(-6)
      .Set(OpCodes.Call, Transpilers.EmitDelegate(LocationObjectDataAndSwap.InstantiateWithSwap).operand)
      .InstructionEnumeration();
  }
}