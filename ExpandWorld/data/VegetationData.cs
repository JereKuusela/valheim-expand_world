using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
namespace ExpandWorld;

public class VegetationData {
  public string prefab = "";
  [DefaultValue(true)]
  public bool enabled = true;
  [DefaultValue(1f)]
  public float min = 1f;
  [DefaultValue(1f)]
  public float max = 1f;
  [DefaultValue(false)]
  public bool forcePlacement = false;
  [DefaultValue(1f)]
  public float scaleMin = 1f;
  [DefaultValue(1f)]
  public float scaleMax = 1f;
  [DefaultValue(0f)]
  public float randTilt = 0f;
  [DefaultValue(0f)]
  public float chanceToUseGroundTilt = 0f;
  public string[] biome = new string[0];
  public string[] biomeArea = new string[0];
  [DefaultValue(true)]
  public bool blockCheck = true;
  [DefaultValue(0f)]
  public float minAltitude = 0f;
  [DefaultValue(1000f)]
  public float maxAltitude = 1000f;
  [DefaultValue(0f)]
  public float minOceanDepth = 0f;
  [DefaultValue(0f)]
  public float maxOceanDepth = 0f;
  [DefaultValue(0f)]
  public float minTilt = 0f;
  [DefaultValue(90f)]
  public float maxTilt = 90f;
  [DefaultValue(0f)]
  public float terrainDeltaRadius = 0f;
  [DefaultValue(10f)]
  public float maxTerrainDelta = 10f;
  [DefaultValue(0f)]
  public float minTerrainDelta = 0f;
  [DefaultValue(false)]
  public bool snapToWater = false;
  [DefaultValue(0f)]
  public float groundOffset = 0f;
  [DefaultValue(1)]
  public int groupSizeMin = 1;
  [DefaultValue(1)]
  public int groupSizeMax = 1;
  [DefaultValue(0f)]
  public float groupRadius = 0f;
  [DefaultValue(false)]
  public bool inForest = false;
  [DefaultValue(0f)]
  public float forestTresholdMin = 0f;
  [DefaultValue(0f)]
  public float forestTresholdMax = 0f;
  public static ZoneSystem.ZoneVegetation FromData(VegetationData data) {
    var veg = new ZoneSystem.ZoneVegetation();
    if (ZNetScene.instance.m_namedPrefabs.TryGetValue(data.prefab.GetStableHashCode(), out var obj))
      veg.m_prefab = obj;
    else
      ExpandWorld.Log.LogWarning($"Vegetation prefab {data.prefab} not found!");
    veg.m_enable = data.enabled;
    veg.m_min = data.min;
    veg.m_max = data.max;
    veg.m_forcePlacement = data.forcePlacement;
    veg.m_scaleMin = data.scaleMin;
    veg.m_scaleMax = data.scaleMax;
    veg.m_randTilt = data.randTilt;
    veg.m_chanceToUseGroundTilt = data.chanceToUseGroundTilt;
    veg.m_biome = Data.ToBiomes(data.biome);
    veg.m_biomeArea = Data.ToBiomeAreas(data.biomeArea);
    veg.m_blockCheck = data.blockCheck;
    veg.m_minAltitude = data.minAltitude;
    veg.m_maxAltitude = data.maxAltitude;
    veg.m_minOceanDepth = data.minOceanDepth;
    veg.m_maxOceanDepth = data.maxOceanDepth;
    veg.m_minTilt = data.minTilt;
    veg.m_maxTilt = data.maxTilt;
    veg.m_terrainDeltaRadius = data.terrainDeltaRadius;
    veg.m_maxTerrainDelta = data.maxTerrainDelta;
    veg.m_minTerrainDelta = data.minTerrainDelta;
    veg.m_snapToWater = data.snapToWater;
    veg.m_groundOffset = data.groundOffset;
    veg.m_groupSizeMin = data.groupSizeMin;
    veg.m_groupSizeMax = data.groupSizeMax;
    veg.m_groupRadius = data.groupRadius;
    veg.m_inForest = data.inForest;
    veg.m_forestTresholdMin = data.forestTresholdMin;
    veg.m_forestTresholdMax = data.forestTresholdMax;
    return veg;
  }
  public static bool IsValid(ZoneSystem.ZoneVegetation veg) => veg.m_prefab && veg.m_prefab.GetComponent<ZNetView>() != null;
  public static VegetationData ToData(ZoneSystem.ZoneVegetation veg) {
    VegetationData data = new();
    data.enabled = veg.m_enable;
    data.prefab = veg.m_prefab.name;
    data.min = veg.m_min;
    data.max = veg.m_max;
    data.forcePlacement = veg.m_forcePlacement;
    data.scaleMin = veg.m_scaleMin;
    data.scaleMax = veg.m_scaleMax;
    data.randTilt = veg.m_randTilt;
    data.chanceToUseGroundTilt = veg.m_chanceToUseGroundTilt;
    data.biome = Data.FromBiomes(veg.m_biome);
    data.biomeArea = Data.FromBiomeAreas(veg.m_biomeArea);
    data.blockCheck = veg.m_blockCheck;
    data.minAltitude = veg.m_minAltitude;
    data.maxAltitude = veg.m_maxAltitude;
    data.minOceanDepth = veg.m_minOceanDepth;
    data.maxOceanDepth = veg.m_maxOceanDepth;
    data.minTilt = veg.m_minTilt;
    data.maxTilt = veg.m_maxTilt;
    data.terrainDeltaRadius = veg.m_terrainDeltaRadius;
    data.maxTerrainDelta = veg.m_maxTerrainDelta;
    data.minTerrainDelta = veg.m_minTerrainDelta;
    data.snapToWater = veg.m_snapToWater;
    data.groundOffset = veg.m_groundOffset;
    data.groupSizeMin = veg.m_groupSizeMin;
    data.groupSizeMax = veg.m_groupSizeMax;
    data.groupRadius = veg.m_groupRadius;
    data.inForest = veg.m_inForest;
    data.forestTresholdMin = veg.m_forestTresholdMin;
    data.forestTresholdMax = veg.m_forestTresholdMax;
    return data;
  }

  public static void ToFile(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataVegetation) return;
    var yaml = Data.Serializer().Serialize(ZoneSystem.instance.m_vegetation.Where(IsValid).Select(ToData).ToList());
    File.WriteAllText(fileName, yaml);
  }
  public static void FromFile(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataVegetation) return;
    var raw = File.ReadAllText(fileName);
    Configuration.configInternalDataVegetation.Value = raw;
    if (LoadData.IsLoading) Set(raw);
  }
  public static void FromSetting(string raw) {
    if (!LoadData.IsLoading) Set(raw);
  }
  private static void Set(string raw) {
    if (raw == "" || !Configuration.DataVegetation) return;
    var data = Data.Deserializer().Deserialize<List<VegetationData>>(raw)
    .Select(FromData).ToList();
    if (data.Count == 0) {
      ExpandWorld.Log.LogWarning($"Failed to load any vegetation data.");
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading {data.Count} vegetation data.");
    foreach (var list in LocationList.m_allLocationLists)
      list.m_vegetation.Clear();
    ZoneSystem.instance.m_vegetation = data;
    if (!LoadData.IsLoading)
      ZoneSystem.instance.ValidateVegetation();
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Data.VegFile, FromFile);
  }
}
