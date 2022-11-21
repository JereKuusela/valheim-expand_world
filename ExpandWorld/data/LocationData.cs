using System.Collections.Generic;
using System.ComponentModel;
namespace ExpandWorld;

public class LocationData
{
  public string prefab = "";
  [DefaultValue(true)]
  public bool enabled = true;
  [DefaultValue("")]
  public string biome = "";
  [DefaultValue("")]
  public string biomeArea = "";
  public int quantity = 0;
  [DefaultValue(0f)]
  public float minDistance = 0f;
  [DefaultValue(0f)]
  public float maxDistance = 0f;
  [DefaultValue(-1000f)]
  public float minAltitude = 0f;
  [DefaultValue(1000f)]
  public float maxAltitude = 1000f;
  [DefaultValue(false)]
  public bool prioritized = false;
  [DefaultValue(false)]
  public bool centerFirst = false;
  [DefaultValue(false)]
  public bool unique = false;
  [DefaultValue("")]
  public string group = "";
  [DefaultValue(0f)]
  public float minDistanceFromSimilar = 0f;
  [DefaultValue(false)]
  public bool iconAlways = false;
  [DefaultValue(false)]
  public bool iconPlaced = false;
  [DefaultValue(false)]
  public bool randomRotation = false;
  [DefaultValue(false)]
  public bool slopeRotation = false;
  [DefaultValue(false)]
  public bool snapToWater = false;
  [DefaultValue(0f)]
  public float minTerrainDelta = 0f;
  [DefaultValue(10f)]
  public float maxTerrainDelta = 10f;
  [DefaultValue(false)]
  public bool inForest = false;
  [DefaultValue(0f)]
  public float forestTresholdMin = 0f;
  [DefaultValue(1f)]
  public float forestTresholdMax = 1f;
  [DefaultValue("")]
  public string data = "";
  [DefaultValue(null)]
  public Dictionary<string, string>? objectData = null;
  [DefaultValue(null)]
  public Dictionary<string, string>? objectSwap = null;
  [DefaultValue(null)]
  public Dictionary<string, string>? objects = null;
  [DefaultValue(0f)]
  public float clearRadius = 0f;
}
