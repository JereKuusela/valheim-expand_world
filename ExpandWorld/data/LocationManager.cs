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
  public static Dictionary<string, ZDO> ZDO = new();
  public static Dictionary<string, Dictionary<string, List<Tuple<float, string>>>> ObjectSwaps = new();
  public static Dictionary<string, Dictionary<string, ZDO>> ObjectData = new();
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
        Parse.AngleYXZ(split, 4)
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
      ExpandWorld.Log.LogWarning(total);
      return Tuple.Create(weight, s[0]);
    }).ToList().Select(t => Tuple.Create(t.Item1 / total, t.Item2)).ToList();
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
    if (loc.m_location)
    {
      data.randomDamage = loc.m_location.m_applyRandomDamage;
      data.exteriorRadius = loc.m_location.m_exteriorRadius;
      data.clearArea = loc.m_location.m_clearArea;
      data.noBuild = loc.m_location.m_noBuild;
    }
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
    item.m_location.m_exteriorRadius = data.exteriorRadius;
    item.m_exteriorRadius = item.m_location.m_exteriorRadius;
    if (radius.HasValue && item.m_exteriorRadius == 0)
      item.m_exteriorRadius = radius.Value;
    item.m_location.m_applyRandomDamage = data.randomDamage;
    item.m_location.m_clearArea = data.clearArea;
    item.m_location.m_noBuild = data.noBuild;
  }
  ///<summary>Copies setup from locations.</summary>
  private static void Setup(ZoneSystem.ZoneLocation item)
  {
    var prefabName = item.m_prefabName.Split(':')[0];
    item.m_hash = item.m_prefabName.GetStableHashCode();
    if (!ZoneLocations.TryGetValue(prefabName, out var zoneLocation))
    {
      if (Blueprints.TryGetBluePrint(prefabName, out var bp))
      {
        BlueprintFiles[prefabName] = bp;
        item.m_prefab = new();
        if (!BlueprintLocations.TryGetValue(item.m_prefabName, out item.m_location))
        {
          var obj = new GameObject();
          item.m_location = obj.AddComponent<Location>();
          BlueprintLocations.Add(item.m_prefabName, item.m_location);
        }
        ApplyLocationData(item, bp.Radius + 5);
        item.m_netViews = new();
        item.m_randomSpawns = new();
        return;
      }
      // Don't warn on the default data since it has missing stuff.
      if (File.Exists(FilePath))
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
    return !LocationManager.BlueprintFiles.ContainsKey(Location);
  }
  static void SetData(GameObject prefab, Vector3 position, Quaternion rotation, ZDO? data = null)
  {
    if (data == null)
    {
      ExpandWorld.Log.LogWarning(Location);
      if (!LocationManager.ObjectData.TryGetValue(Location, out var objectData)) return;
      ExpandWorld.Log.LogWarning("FOUND DATAS");
      if (!objectData.TryGetValue(Utils.GetPrefabName(prefab), out data)) return;
      ExpandWorld.Log.LogWarning("FOUND DATA");
    }
    if (data == null) return;
    if (!prefab.TryGetComponent<ZNetView>(out var view)) return;
    ExpandWorld.Log.LogWarning("INIT DATA");
    Data.InitZDO(position, rotation, data, view);
  }
  static string RandomizeSwap(List<Tuple<float, string>> swaps)
  {
    if (swaps.Count == 0)
      return "";
    if (swaps.Count == 1)
      return swaps[0].Item2;
    var rng = UnityEngine.Random.value;
    ExpandWorld.Log.LogWarning(rng);
    foreach (var swap in swaps)
    {
      rng -= swap.Item1;
      ExpandWorld.Log.LogWarning(swap.Item1);
      if (rng <= 0f) return swap.Item2;
    }
    return swaps[swaps.Count - 1].Item2;
  }
  static GameObject Swap(GameObject prefab)
  {
    if (!LocationManager.ObjectSwaps.TryGetValue(Location, out var objectSwaps)) return prefab;
    if (!objectSwaps.TryGetValue(Utils.GetPrefabName(prefab), out var swaps)) return prefab;
    var swap = RandomizeSwap(swaps);
    return ZNetScene.instance.GetPrefab(swap) ?? prefab;
  }
  public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    prefab = Swap(prefab);
    return InstantiateWithData(prefab, position, rotation);
  }
  public static GameObject InstantiateWithData(GameObject prefab, Vector3 position, Quaternion rotation, ZDO? data = null)
  {
    SetData(prefab, position, rotation, data);
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    if (obj.TryGetComponent<DungeonGenerator>(out var dg))
    {
      if (LocationManager.Dungeons.TryGetValue(Location, out var dungeon))
        DungeonManager.Override(dg, dungeon);
      else
        DungeonManager.Override(dg, Utils.GetPrefabName(obj));
    }
    Data.CleanGhostInit(obj);
    return obj;
  }

  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZNetView), nameof(ZNetView.StartGhostInit))))
      .Advance(5)
      .Set(OpCodes.Call, Transpilers.EmitDelegate(Instantiate).operand)
      .InstructionEnumeration();
  }

  static GameObject SpawnBPO(ZoneSystem __instance, bool flag, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects, BlueprintObject obj)
  {
    var objPos = pos + rot * obj.Pos;
    var objRot = rot * obj.Rot;
    if (mode == ZoneSystem.SpawnMode.Ghost)
    {
      ZNetView.StartGhostInit();
    }
    var prefab = ZNetScene.instance.GetPrefab(obj.Prefab);
    var go = InstantiateWithData(prefab, objPos, objRot, obj.Data);
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
    return go;
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
    if (!LocationManager.BlueprintFiles.TryGetValue(location.m_prefabName, out var bp)) return;
    if (LocationManager.LocationData.TryGetValue(location.m_prefabName, out var data))
    {
      if (data.levelArea)
      {
        var radius = location.m_exteriorRadius;
        ExpandWorld.Log.LogDebug("Leveling " + radius);
        var compilers = Terrain.GetCompilers(pos, radius);
        var indicer = Terrain.CreateIndexer(pos, radius);
        var compilerIndices = Terrain.GetIndices(compilers, indicer);
        Terrain.ResetTerrain(compilerIndices, pos, radius);
        Terrain.LevelTerrain(compilerIndices, pos, radius, 0.5f, pos.y);
      }
      pos.y += data.offset;
    }
    var loc = location.m_location;
    WearNTear.m_randomInitialDamage = loc.m_applyRandomDamage;
    var flag = loc && loc.m_useCustomInteriorTransform && loc.m_interiorTransform && loc.m_generator;
    foreach (var obj in bp.Objects)
      SpawnBPO(__instance, flag, location, pos, rot, mode, spawnedGhostObjects, obj);
    WearNTear.m_randomInitialDamage = false;
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
      .Set(OpCodes.Call, Transpilers.EmitDelegate(LocationObjectDataAndSwap.Instantiate).operand)
      .InstructionEnumeration();
  }
}