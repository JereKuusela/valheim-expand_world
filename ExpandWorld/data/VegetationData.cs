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
}
