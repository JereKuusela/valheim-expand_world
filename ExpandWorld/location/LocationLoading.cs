using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace ExpandWorld;

public class LocationLoading
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
  public static Dictionary<string, Location> BlueprintLocations = new();
  public static ZoneSystem.ZoneLocation FromData(LocationData data)
  {
    var loc = new ZoneSystem.ZoneLocation();
    LocationData[data.prefab] = data;
    loc.m_prefabName = data.prefab;
    if (!Locations.ContainsKey(Parse.Name(data.prefab)))
    {
      if (!BlueprintManager.Load(data.prefab, data.centerPiece))
        loc.m_prefabName = "";
    }

    if (data.data != "")
      ZDO[data.prefab] = Data.ToZDO(data.data);
    if (data.dungeon != "")
      Dungeons[data.prefab] = data.dungeon;
    if (data.objectSwap != null)
      ObjectSwaps[data.prefab] = Spawn.LoadSwaps(data.objectSwap);
    if (data.objectData != null)
      ObjectData[data.prefab] = Spawn.LoadData(data.objectData);
    if (data.objects != null)
      Objects[data.prefab] = Parse.Objects(data.objects);

    loc.m_enable = data.enabled;
    loc.m_biome = Data.ToBiomes(data.biome);
    loc.m_biomeArea = Data.ToBiomeAreas(data.biomeArea);
    loc.m_quantity = data.quantity;
    loc.m_prioritized = data.prioritized;
    loc.m_centerFirst = data.centerFirst;
    loc.m_unique = data.unique;
    loc.m_group = data.group;
    loc.m_minDistanceFromSimilar = data.minDistanceFromSimilar;
    loc.m_iconAlways = data.iconAlways != "" && data.iconAlways != "false";
    loc.m_iconPlaced = data.iconPlaced != "" && data.iconPlaced != "false";
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
    data.iconAlways = loc.m_iconAlways ? loc.m_prefabName : "";
    data.iconPlaced = loc.m_iconPlaced ? loc.m_prefabName : "";
    data.randomRotation = loc.m_randomRotation;
    data.slopeRotation = loc.m_slopeRotation;
    data.snapToWater = loc.m_snapToWater;
    data.maxTerrainDelta = loc.m_maxTerrainDelta;
    data.minTerrainDelta = loc.m_minTerrainDelta;
    data.inForest = loc.m_inForest;
    data.forestTresholdMin = loc.m_forestTresholdMin;
    data.forestTresholdMax = loc.m_forestTresholdMax;
    data.minDistance = loc.m_minDistance;
    if (data.minDistance > 2f)
      data.minDistance /= 10000f;
    data.maxDistance = loc.m_maxDistance;
    if (data.maxDistance > 2f)
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

  private static void ToFile()
  {
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(ZoneSystem.instance.m_locations.Where(IsValid).Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }
  private static void AddMissing(HashSet<string> missingLocations)
  {
    ExpandWorld.Log.LogWarning($"Adding {missingLocations.Count} missing locations to the expand_locations.yaml file.");
    foreach (var loc in missingLocations)
      ExpandWorld.Log.LogWarning(loc);
    var yaml = File.ReadAllText(FilePath);
    var data = Data.Deserialize<LocationData>(yaml, FileName).ToList();
    data.AddRange(ZoneSystem.instance.m_locations.Where(loc => missingLocations.Contains(loc.m_prefabName)).Select(ToData));
    // Directly appending is risky if something goes wrong (like missing a linebreak).
    File.WriteAllText(FilePath, Data.Serializer().Serialize(data));
  }
  public static void Initialize()
  {
    DefaultEntries.Clear();
    Locations.Clear();
    if (Helper.IsServer())
    {
      DefaultEntries = ZoneSystem.instance.m_locations;
      Locations = Helper.ToDict(DefaultEntries, l => l.m_prefabName, l => l);
    }
    Load();
  }
  public static void Load()
  {
    ZDO.Clear();
    ObjectSwaps.Clear();
    ObjectData.Clear();
    Dungeons.Clear();
    Objects.Clear();
    LocationData.Clear();
    if (Helper.IsClient()) return;
    ZoneSystem.instance.m_locations = DefaultEntries;
    if (Configuration.DataLocation)
    {
      if (!File.Exists(FilePath))
      {
        ToFile();
        // Watcher triggers reload.
        return;
      }
      var data = FromFile();
      if (data.Count == 0)
      {
        ExpandWorld.Log.LogWarning($"Failed to load any location data.");
        ExpandWorld.Log.LogInfo($"Reloading default location data ({DefaultEntries.Count} entries).");
      }
      else
      {
        if (AddMissingEntries(data))
        {
          // Watcher triggers reload.
          return;
        }
        ZoneSystem.instance.m_locations = data;
        ExpandWorld.Log.LogInfo($"Reloading location data ({data.Count} entries).");
      }
    }
    else
      ExpandWorld.Log.LogInfo($"Reloading default location data ({DefaultEntries.Count} entries).");
    foreach (var item in ZoneSystem.instance.m_locations) Setup(item);
    LocationSetup.UpdateHashes();
    UpdateInstances();
    NoBuildManager.UpdateData();
    ZoneSystem.instance.SendLocationIcons(ZRoutedRpc.Everybody);
    CleanMap();
  }
  private static void CleanMap()
  {
    var mm = Minimap.instance;
    if (!mm) return;
    foreach (var pin in mm.m_locationPins)
      mm.RemovePin(pin.Value);
    mm.m_locationPins.Clear();
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
  // Dictionary can't be used because some mods might have multiple entries for the same location.
  private static List<ZoneSystem.ZoneLocation> DefaultEntries = new();
  // Used to optimize missing entries check (to avoid n^2 loop).
  // Also used to quickly check if a location is blueprint or not.
  private static Dictionary<string, ZoneSystem.ZoneLocation> Locations = new();
  private static bool AddMissingEntries(List<ZoneSystem.ZoneLocation> items)
  {
    var missingKeys = Locations.Keys.ToHashSet();
    foreach (var item in items)
      missingKeys.Remove(item.m_prefabName);
    if (missingKeys.Count == 0) return false;
    var missing = DefaultEntries.Where(loc => missingKeys.Contains(loc.m_prefabName)).ToList();
    ExpandWorld.Log.LogWarning($"Adding {missing.Count} missing locations to the expand_locations.yaml file.");
    foreach (var item in missing)
      ExpandWorld.Log.LogWarning(item);
    var yaml = File.ReadAllText(FilePath);
    var data = Data.Deserialize<LocationData>(yaml, FileName).ToList();
    data.AddRange(missing.Select(ToData));
    // Directly appending is risky if something goes wrong (like missing a linebreak).
    File.WriteAllText(FilePath, Data.Serializer().Serialize(data));
    return true;
  }
  private static List<ZoneSystem.ZoneLocation> FromFile()
  {
    try
    {
      var yaml = Data.Read(Pattern);
      return Data.Deserialize<LocationData>(yaml, FileName).Select(FromData)
        .Where(loc => loc.m_prefabName != "").ToList();
    }
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.Message);
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
    if (!LocationLoading.BlueprintLocations.TryGetValue(prefab, out var location))
    {
      var obj = new GameObject();
      obj.name = "Blueprint";
      location = obj.AddComponent<Location>();
      LocationLoading.BlueprintLocations.Add(prefab, location);
    }
    return location;
  }


  private static bool SetupBlueprint(ZoneSystem.ZoneLocation location)
  {
    if (!BlueprintManager.TryGet(location.m_prefabName, out var bp)) return false;
    location.m_prefab = new();
    location.m_location = GetBluePrintLocation(location.m_prefabName);
    ApplyLocationData(location, bp.Radius + 5);
    location.m_netViews = new();
    location.m_randomSpawns = new();
    return true;
  }
  ///<summary>Copies setup from locations.</summary>
  private static void Setup(ZoneSystem.ZoneLocation item)
  {
    var prefabName = Parse.Name(item.m_prefabName);
    item.m_hash = item.m_prefabName.GetStableHashCode();
    if (!Locations.TryGetValue(prefabName, out var zoneLocation) || zoneLocation.m_prefab == null || zoneLocation.m_location == null)
    {
      if (SetupBlueprint(item)) return;
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
    if (!LocationLoading.ZDO.TryGetValue(location.m_prefabName, out var data)) return;
    if (!__instance.m_locationProxyPrefab.TryGetComponent<ZNetView>(out var view)) return;
    if (data != null) Data.InitZDO(pos, rotation, Vector3.one, data, view);
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
  static bool Prefix(ZoneSystem.ZoneLocation location, ZoneSystem.SpawnMode mode, ref Vector3 pos)
  {
    var loc = location.m_location;
    var flag = loc && loc.m_useCustomInteriorTransform && loc.m_interiorTransform && loc.m_generator;
    if (flag)
      Spawn.DungeonGeneratorPos = location.m_generatorPosition;
    Dungeon.Spawner.Location = null;
    if (mode != ZoneSystem.SpawnMode.Client)
    {
      Dungeon.Spawner.Location = location;
      if (LocationLoading.LocationData.TryGetValue(location.m_prefabName, out var data))
        pos.y += data.groundOffset;
    }
    // Blueprints won't have any znetviews to spawn or other logic to handle.
    return !BlueprintManager.Has(location.m_prefabName);
  }

  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var instantiator = typeof(UnityEngine.Object).GetMethods().First(m => m.Name == nameof(UnityEngine.Object.Instantiate) && m.IsGenericMethodDefinition && m.GetParameters().Skip(1).Select(p => p.ParameterType).SequenceEqual(new[] { typeof(Vector3), typeof(Quaternion) })).MakeGenericMethod(typeof(GameObject));
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Call, instantiator))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 6))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(LocationSpawning.Object).operand)
      .InstructionEnumeration();
  }


  static void Postfix(ZoneSystem __instance, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects)
  {
    if (mode != ZoneSystem.SpawnMode.Client)
    {
      var isBluePrint = BlueprintManager.TryGet(location.m_prefabName, out var bp);
      if (LocationLoading.LocationData.TryGetValue(location.m_prefabName, out var data))
      {
        // Remove the applied offset.
        var surface = pos with { y = pos.y - data.groundOffset };
        HandleTerrain(surface, location.m_exteriorRadius, isBluePrint, data);
      }
      if (mode == ZoneSystem.SpawnMode.Ghost)
        ZNetView.StartGhostInit();
      if (isBluePrint)
        Spawn.Blueprint(location.m_prefabName, bp, pos, rot, LocationSpawning.DataOverride, spawnedGhostObjects);
      LocationSpawning.CustomObjects(location, pos, rot, spawnedGhostObjects);
      if (mode == ZoneSystem.SpawnMode.Ghost)
        ZNetView.FinishGhostInit();
    }
    Dungeon.Spawner.Location = null;
    Spawn.DungeonGeneratorPos = null;
  }

  static void HandleTerrain(Vector3 pos, float radius, bool isBlueprint, LocationData data)
  {
    var level = false;
    if (data.levelArea == "") level = isBlueprint;
    else if (data.levelArea == "false") level = false;
    else level = true;
    if (!level && data.paint == "") return;

    Terrain.ChangeTerrain(pos, compiler =>
    {
      if (level)
      {
        var levelRadius = data.levelRadius;
        var levelBorder = data.levelBorder;
        if (levelRadius == 0f && levelBorder == 0f)
        {
          var multiplier = Parse.Float(data.levelArea, 0.5f);
          levelRadius = multiplier * radius;
          levelBorder = (1 - multiplier) * radius;
        }
        Terrain.Level(compiler, pos, levelRadius, levelBorder);
      }
      if (data.paint != "")
      {
        var paintRadius = data.paintRadius ?? radius;
        var paintBorder = data.paintBorder ?? 5f;
        Terrain.Paint(compiler, pos, data.paint, paintRadius, paintBorder);
      }
    });
  }



}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetLocationIcons))]
public class LocationIcons
{
  static bool Prefix(ZoneSystem __instance, Dictionary<Vector3, string> icons)
  {
    if (!Configuration.DataLocation) return true;
    if (!ZNet.instance.IsServer()) return true;
    foreach (var kvp in __instance.m_locationInstances)
    {
      var loc = kvp.Value.m_location;
      var pos = kvp.Value.m_position;
      if (loc == null) continue;
      if (LocationLoading.LocationData.TryGetValue(loc.m_prefabName, out var data))
      {
        var placed = data.iconPlaced == "true" ? loc.m_prefabName : data.iconPlaced == "false" ? "" : data.iconPlaced;
        if (kvp.Value.m_placed && placed != "")
        {
          icons[pos] = placed;
        }
        else
        {
          pos.y += 0.00001f; // Trivial amount for a different key.
          var always = data.iconAlways == "true" ? loc.m_prefabName : data.iconAlways == "false" ? "" : data.iconAlways;
          if (always != "") icons[pos] = always;
        }
      }
      // Jewelcrafting has dynamic locations so use the vanilla code as a fallback.
      else if (loc.m_iconAlways || (loc.m_iconPlaced && kvp.Value.m_placed))
      {
        icons[pos] = loc.m_prefabName;
      }
    }
    return false;
  }
}


[HarmonyPatch(typeof(Minimap), nameof(Minimap.GetLocationIcon))]
public class NewLocationIcons
{
  static Sprite Postfix(Sprite result, string name)
  {
    if (result != null) return result;
    if (Enum.TryParse<Minimap.PinType>(name, true, out var icon))
      return Minimap.instance.GetSprite(icon);
    return null!;
  }
}