using System.ComponentModel;
namespace ExpandWorld;

public class SpawnData {
  public string prefab = "";
  [DefaultValue(true)]
  public bool enabled = true;
  public string[] biome = new string[0];
  [DefaultValue("")]
  public string biomeArea = "";
  [DefaultValue(100f)]
  public float spawnChance = 100f;
  public int maxSpawned = 1;
  public float spawnInterval = 0f;
  [DefaultValue(1)]
  public int maxLevel = 1;
  [DefaultValue(1)]
  public int minLevel = 1;
  [DefaultValue(-10000f)]
  public float minAltitude = -1000f;
  [DefaultValue(1000f)]
  public float maxAltitude = 1000f;
  [DefaultValue(true)]
  public bool spawnAtDay = true;
  [DefaultValue(true)]
  public bool spawnAtNight = true;
  [DefaultValue("")]
  public string requiredGlobalKey = "";
  [DefaultValue(new string[0])]
  public string[] requiredEnvironments = new string[0];
  public float spawnDistance = 10f;
  [DefaultValue(0f)]
  public float spawnRadiusMin = 0f;
  [DefaultValue(0f)]
  public float spawnRadiusMax = 0f;
  [DefaultValue(1)]
  public int groupSizeMin = 1;
  [DefaultValue(1)]
  public int groupSizeMax = 1;
  [DefaultValue(0f)]
  public float groupRadius = 3f;
  [DefaultValue(0f)]
  public float minTilt = 0f;
  [DefaultValue(35f)]
  public float maxTilt = 35f;
  [DefaultValue(true)]
  public bool inForest = true;
  [DefaultValue(true)]
  public bool outsideForest = true;
  [DefaultValue(0f)]
  public float minOceanDepth = 0f;
  [DefaultValue(0f)]
  public float maxOceanDepth = 0f;
  [DefaultValue(false)]
  public bool huntPlayer = false;
  [DefaultValue(0.5f)]
  public float groundOffset = 0.5f;
  [DefaultValue(0f)]
  public float levelUpMinCenterDistance = 0f;
}
