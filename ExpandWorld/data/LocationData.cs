using System.ComponentModel;

namespace ExpandWorld;

public class LocationData
{
  public string prefab = "";
  [DefaultValue(true)]
  public bool enabled = true;
  [DefaultValue("")]
  public string dungeon = "";
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
  public float minAltitude = -1000f;
  [DefaultValue(10000f)]
  public float maxAltitude = 10000f;
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
  [DefaultValue("")]
  public string iconAlways = "";
  [DefaultValue("")]
  public string iconPlaced = "";
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
  public string[]? objectData = null;
  [DefaultValue(null)]
  public string[]? objectSwap = null;
  [DefaultValue(null)]
  public string[]? objects = null;
  [DefaultValue(0f)]
  public float exteriorRadius = 0f;
  [DefaultValue(false)]
  public bool clearArea = false;
  [DefaultValue(false)]
  public bool randomDamage = false;
  [DefaultValue("")]
  public string noBuild = "";
  [DefaultValue("")]
  public string noBuildDungeon = "";
  [DefaultValue("")]
  public string levelArea = "";
  [DefaultValue(0f)]
  public float levelRadius = 0f;
  [DefaultValue(0f)]
  public float levelBorder = 0f;
  [DefaultValue("")]
  public string paint = "";
  [DefaultValue(null)]
  public float? paintRadius = null;
  [DefaultValue(null)]
  public float? paintBorder = null;
  [DefaultValue("")]
  public string offset = "";
  [DefaultValue("piece_bpcenterpoint")]
  public string centerPiece = "piece_bpcenterpoint";
}
