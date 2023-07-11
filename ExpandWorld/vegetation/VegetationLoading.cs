using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Service;
using UnityEngine;

namespace ExpandWorld;

public class VegetationLoading
{
  private static readonly string FileName = "expand_vegetation.yaml";
  private static readonly string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  private static readonly string Pattern = "expand_vegetation*.yaml";


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
    VegetationSpawning.Extra.Clear();
    if (Helper.IsClient()) return;
    ZoneSystem.instance.m_vegetation = DefaultEntries;
    if (!Configuration.DataVegetation)
    {
      ExpandWorld.Log.LogInfo($"Reloading default vegetation data ({DefaultEntries.Count} entries).");
      return;
    }
    if (!File.Exists(FilePath))
    {
      var yaml = DataManager.Serializer().Serialize(DefaultEntries.Select(ToData).ToList());
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
    if (Configuration.DataMigration && AddMissingEntries(data))
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
      var yaml = DataManager.Read(Pattern);
      return DataManager.Deserialize<VegetationData>(yaml, FileName).Select(FromData)
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
    var data = DataManager.Serializer().Serialize(missing.Select(ToData));
    // Directly appending is risky but necessary to keep comments, etc.
    yaml += "\n" + data;
    File.WriteAllText(FilePath, yaml);
    return true;
  }


  public static ZoneSystem.ZoneVegetation FromData(VegetationData data)
  {
    var veg = new ZoneSystem.ZoneVegetation
    {
      m_name = data.prefab,
      m_enable = data.enabled,
      m_min = data.min,
      m_max = data.max,
      m_forcePlacement = data.forcePlacement,
      m_scaleMin = Parse.Scale(data.scaleMin).x,
      m_scaleMax = Parse.Scale(data.scaleMax).x,
      m_randTilt = data.randTilt,
      m_chanceToUseGroundTilt = data.chanceToUseGroundTilt,
      m_biome = DataManager.ToBiomes(data.biome),
      m_biomeArea = DataManager.ToBiomeAreas(data.biomeArea),
      m_blockCheck = data.blockCheck,
      m_minAltitude = data.minAltitude,
      m_maxAltitude = data.maxAltitude,
      m_minOceanDepth = data.minOceanDepth,
      m_maxOceanDepth = data.maxOceanDepth,
      m_minVegetation = data.minVegetation,
      m_maxVegetation = data.maxVegetation,
      m_minTilt = data.minTilt,
      m_maxTilt = data.maxTilt,
      m_terrainDeltaRadius = data.terrainDeltaRadius,
      m_maxTerrainDelta = data.maxTerrainDelta,
      m_minTerrainDelta = data.minTerrainDelta,
      m_snapToWater = data.snapToWater,
      m_snapToStaticSolid = data.snapToStaticSolid,
      m_groundOffset = data.groundOffset,
      m_groupSizeMin = data.groupSizeMin,
      m_groupSizeMax = data.groupSizeMax,
      m_groupRadius = data.groupRadius,
      m_inForest = data.inForest,
      m_forestTresholdMin = data.forestTresholdMin,
      m_forestTresholdMax = data.forestTresholdMax
    };
    var hash = data.prefab.GetStableHashCode();
    Range<Vector3> scale = new(Parse.Scale(data.scaleMin), Parse.Scale(data.scaleMax))
    {
      Uniform = data.scaleUniform
    };
    VegetationExtra extra = new()
    {
      clearRadius = data.clearRadius
    };
    // Minor optimization to skip RNG calls if there is nothing to randomize.
    if (Helper.IsMultiAxis(scale))
      extra.scale = scale;
    if (data.data != "")
      extra.data = DataHelper.Deserialize(data.data);

    if (ZNetScene.instance.m_namedPrefabs.TryGetValue(hash, out var obj))
    {
      veg.m_prefab = obj;
    }
    else if (BlueprintManager.Load(data.prefab, data.centerPiece))
    {
      veg.m_prefab = new GameObject(data.prefab);
    }
    if (veg.m_enable && data.requiredGlobalKey != "")
      extra.requiredGlobalKeys = DataManager.ToList(data.requiredGlobalKey);
    if (veg.m_enable && data.forbiddenGlobalKey != "")
      extra.forbiddenGlobalKeys = DataManager.ToList(data.forbiddenGlobalKey);
    if (extra.IsValid())
      VegetationSpawning.Extra.Add(veg, extra);
    return veg;
  }
  public static VegetationData ToData(ZoneSystem.ZoneVegetation veg)
  {
    VegetationData data = new()
    {
      enabled = veg.m_enable,
      prefab = veg.m_prefab.name,
      min = veg.m_min,
      max = veg.m_max,
      forcePlacement = veg.m_forcePlacement,
      scaleMin = veg.m_scaleMin.ToString(CultureInfo.InvariantCulture),
      scaleMax = veg.m_scaleMax.ToString(CultureInfo.InvariantCulture),
      randTilt = veg.m_randTilt,
      chanceToUseGroundTilt = veg.m_chanceToUseGroundTilt,
      biome = DataManager.FromBiomes(veg.m_biome),
      biomeArea = DataManager.FromBiomeAreas(veg.m_biomeArea),
      blockCheck = veg.m_blockCheck,
      minAltitude = veg.m_minAltitude,
      maxAltitude = veg.m_maxAltitude,
      minOceanDepth = veg.m_minOceanDepth,
      maxOceanDepth = veg.m_maxOceanDepth,
      minVegetation = veg.m_minVegetation,
      maxVegetation = veg.m_maxVegetation,
      minTilt = veg.m_minTilt,
      maxTilt = veg.m_maxTilt,
      terrainDeltaRadius = veg.m_terrainDeltaRadius,
      maxTerrainDelta = veg.m_maxTerrainDelta,
      minTerrainDelta = veg.m_minTerrainDelta,
      snapToWater = veg.m_snapToWater,
      snapToStaticSolid = veg.m_snapToStaticSolid,
      groundOffset = veg.m_groundOffset,
      groupSizeMin = veg.m_groupSizeMin,
      groupSizeMax = veg.m_groupSizeMax,
      groupRadius = veg.m_groupRadius,
      inForest = veg.m_inForest,
      forestTresholdMin = veg.m_forestTresholdMin,
      forestTresholdMax = veg.m_forestTresholdMax
    };
    return data;
  }

  public static void SetupWatcher()
  {
    DataManager.SetupWatcher(Pattern, Load);
  }
}
