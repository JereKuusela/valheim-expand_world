using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ExpandWorld;

public class VegetationLoading
{
  private static string FileName = "expand_vegetation.yaml";
  private static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  private static string Pattern = "expand_vegetation*.yaml";


  // Default items are stored to track missing entries.
  private static List<ZoneSystem.ZoneVegetation> DefaultEntries = new();
  public static void Initialize()
  {
    DefaultEntries.Clear();
    DefaultKeys.Clear();
    if (Helper.IsServer())
      SetDefaultEntries();
    Load();
  }
  public static void Load()
  {
    VegetationSpawning.Scale.Clear();
    VegetationSpawning.ZDO.Clear();
    if (Helper.IsClient()) return;
    ZoneSystem.instance.m_vegetation = DefaultEntries;
    if (!Configuration.DataVegetation)
    {
      ExpandWorld.Log.LogInfo($"Reloading default vegetation data ({DefaultEntries.Count} entries).");
      return;
    }
    if (!File.Exists(FilePath))
    {
      var yaml = Data.Serializer().Serialize(DefaultEntries.Select(ToData).ToList());
      File.WriteAllText(FilePath, yaml);
      // Watcher triggers reload.
      return;
    }

    var data = FromFile();
    if (data.Count == 0)
    {
      ExpandWorld.Log.LogWarning($"Failed to load any vegetation data.");
      ExpandWorld.Log.LogInfo($"Reloading default vegetation data ({DefaultEntries.Count} entries).");
      return;
    }
    if (AddMissingEntries(data))
    {
      // Watcher triggers reload.
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading vegetation data ({data.Count} entries).");
    ZoneSystem.instance.m_vegetation = data;

  }
  ///<summary>Loads all yaml files returning the deserialized vegetation entries.</summary>
  private static List<ZoneSystem.ZoneVegetation> FromFile()
  {
    try
    {
      var yaml = Data.Read(Pattern);
      return Data.Deserialize<VegetationData>(yaml, FileName).Select(FromData)
        .Where(veg => veg.m_prefab).ToList();
    }
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.Message);
      ExpandWorld.Log.LogError(e.StackTrace);
    }
    return new();
  }
  ///<summary>Cleans up default vegetation data and stores it to track missing entries.</summary>
  private static void SetDefaultEntries()
  {
    ZoneSystem.instance.m_vegetation = ZoneSystem.instance.m_vegetation
      .Where(veg => veg.m_prefab)
      .Where(veg => ZNetScene.instance.m_namedPrefabs.ContainsKey(veg.m_prefab.name.GetStableHashCode()))
      .Where(veg => veg.m_enable && veg.m_biome != 0 && veg.m_max > 0f).ToList();
    DefaultEntries = ZoneSystem.instance.m_vegetation;
    DefaultKeys = Helper.ToSet(DefaultEntries, veg => veg.m_prefab.name);
  }
  // Used to optimize missing entries check (to avoid n^2 loop).
  private static HashSet<string> DefaultKeys = new();

  ///<summary>Detects missing entries and adds them back to the main yaml file. Returns true if anything was added.</summary>
  // Note: This is needed people add new content mods and then complain that Expand World doesn't spawn them.
  private static bool AddMissingEntries(List<ZoneSystem.ZoneVegetation> entries)
  {
    var missingKeys = DefaultKeys.ToHashSet();
    // Some mods override prefabs so the m_prefab.name is not reliable.
    foreach (var entry in entries)
      missingKeys.Remove(entry.m_name);
    if (missingKeys.Count == 0) return false;
    // But don't use m_name because it can be anything for original items.
    var missing = DefaultEntries.Where(veg => missingKeys.Contains(veg.m_prefab.name)).ToList();
    ExpandWorld.Log.LogWarning($"Adding {missing.Count} missing vegetation to the expand_vegetation.yaml file.");
    foreach (var veg in missing)
      ExpandWorld.Log.LogWarning(veg.m_prefab.name);
    var yaml = File.ReadAllText(FilePath);
    var data = Data.Deserialize<VegetationData>(yaml, FileName).ToList();
    data.AddRange(missing.Select(ToData));
    // Directly appending is risky if something goes wrong (like missing a linebreak).
    File.WriteAllText(FilePath, Data.Serializer().Serialize(data));
    return true;
  }


  public static ZoneSystem.ZoneVegetation FromData(VegetationData data)
  {
    var veg = new ZoneSystem.ZoneVegetation();
    veg.m_name = data.prefab;
    var hash = data.prefab.GetStableHashCode();
    Range<Vector3> scale = new(Parse.Scale(data.scaleMin), Parse.Scale(data.scaleMax));
    scale.Uniform = data.scaleUniform;
    // Minor optimization to skip RNG calls if there is nothing to randomize.
    if (Helper.IsMultiAxis(scale))
      VegetationSpawning.Scale[veg] = scale;
    if (data.data != "")
    {
      ZPackage pkg = new(data.data);
      ZDO zdo = new();
      Data.Deserialize(zdo, pkg);
      VegetationSpawning.ZDO[veg] = zdo;
    }
    if (ZNetScene.instance.m_namedPrefabs.TryGetValue(hash, out var obj))
    {
      veg.m_prefab = obj;
    }
    else if (BlueprintManager.Load(data.prefab, data.centerPiece))
    {
      veg.m_prefab = new GameObject(data.prefab);
    }
    veg.m_enable = data.enabled;
    veg.m_min = data.min;
    veg.m_max = data.max;
    veg.m_forcePlacement = data.forcePlacement;
    veg.m_scaleMin = Parse.Scale(data.scaleMin).x;
    veg.m_scaleMax = Parse.Scale(data.scaleMax).x;
    veg.m_randTilt = data.randTilt;
    veg.m_chanceToUseGroundTilt = data.chanceToUseGroundTilt;
    veg.m_biome = Data.ToBiomes(data.biome);
    veg.m_biomeArea = Data.ToBiomeAreas(data.biomeArea);
    veg.m_blockCheck = data.blockCheck;
    veg.m_minAltitude = data.minAltitude;
    veg.m_maxAltitude = data.maxAltitude;
    veg.m_minOceanDepth = data.minOceanDepth;
    veg.m_maxOceanDepth = data.maxOceanDepth;
    veg.m_minVegetation = data.minVegetation;
    veg.m_maxVegetation = data.maxVegetation;
    veg.m_minTilt = data.minTilt;
    veg.m_maxTilt = data.maxTilt;
    veg.m_terrainDeltaRadius = data.terrainDeltaRadius;
    veg.m_maxTerrainDelta = data.maxTerrainDelta;
    veg.m_minTerrainDelta = data.minTerrainDelta;
    veg.m_snapToWater = data.snapToWater;
    veg.m_snapToStaticSolid = data.snapToStaticSolid;
    veg.m_groundOffset = data.groundOffset;
    veg.m_groupSizeMin = data.groupSizeMin;
    veg.m_groupSizeMax = data.groupSizeMax;
    veg.m_groupRadius = data.groupRadius;
    veg.m_inForest = data.inForest;
    veg.m_forestTresholdMin = data.forestTresholdMin;
    veg.m_forestTresholdMax = data.forestTresholdMax;
    return veg;
  }
  public static VegetationData ToData(ZoneSystem.ZoneVegetation veg)
  {
    VegetationData data = new();
    data.enabled = veg.m_enable;
    data.prefab = veg.m_prefab.name;
    data.min = veg.m_min;
    data.max = veg.m_max;
    data.forcePlacement = veg.m_forcePlacement;
    data.scaleMin = veg.m_scaleMin.ToString(CultureInfo.InvariantCulture);
    data.scaleMax = veg.m_scaleMax.ToString(CultureInfo.InvariantCulture);
    data.randTilt = veg.m_randTilt;
    data.chanceToUseGroundTilt = veg.m_chanceToUseGroundTilt;
    data.biome = Data.FromBiomes(veg.m_biome);
    data.biomeArea = Data.FromBiomeAreas(veg.m_biomeArea);
    data.blockCheck = veg.m_blockCheck;
    data.minAltitude = veg.m_minAltitude;
    data.maxAltitude = veg.m_maxAltitude;
    data.minOceanDepth = veg.m_minOceanDepth;
    data.maxOceanDepth = veg.m_maxOceanDepth;
    data.minVegetation = veg.m_minVegetation;
    data.maxVegetation = veg.m_maxVegetation;
    data.minTilt = veg.m_minTilt;
    data.maxTilt = veg.m_maxTilt;
    data.terrainDeltaRadius = veg.m_terrainDeltaRadius;
    data.maxTerrainDelta = veg.m_maxTerrainDelta;
    data.minTerrainDelta = veg.m_minTerrainDelta;
    data.snapToWater = veg.m_snapToWater;
    data.snapToStaticSolid = veg.m_snapToStaticSolid;
    data.groundOffset = veg.m_groundOffset;
    data.groupSizeMin = veg.m_groupSizeMin;
    data.groupSizeMax = veg.m_groupSizeMax;
    data.groupRadius = veg.m_groupRadius;
    data.inForest = veg.m_inForest;
    data.forestTresholdMin = veg.m_forestTresholdMin;
    data.forestTresholdMax = veg.m_forestTresholdMax;
    return data;
  }

  public static void SetupWatcher()
  {
    Data.SetupWatcher(Pattern, Load);
  }
}
