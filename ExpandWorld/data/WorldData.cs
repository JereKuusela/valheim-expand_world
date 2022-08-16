using System.ComponentModel;
using YamlDotNet.Serialization;
namespace ExpandWorld;

public class WorldData {
  public string biome = "";
  [YamlIgnore]
  public Heightmap.Biome _biome = Heightmap.Biome.None;
  [DefaultValue(1000f)]
  public float maxAltitude = 1000f;
  [DefaultValue(-1000f)]
  public float minAltitude = -1000f;
  [DefaultValue(1f)]
  public float maxDistance = 1f;
  [DefaultValue(0f)]
  public float minDistance = 0f;
  [DefaultValue(1f)]
  public float maxSector = 1f;
  [DefaultValue(0f)]
  public float minSector = 0f;
  [DefaultValue(0f)]
  public float curveX = 0f;
  [DefaultValue(0f)]
  public float curveY = 0f;
  [DefaultValue(1f)]
  public float amount = 1f;
  [DefaultValue(1f)]
  public float stretch = 1f;
  [DefaultValue("")]
  public string seed = "";
  [YamlIgnore]
  public Heightmap.Biome _biomeSeed = Heightmap.Biome.None;
  [YamlIgnore]
  public int? _seed = null;
  [DefaultValue(true)]
  public bool wiggleDistance = true;
  [DefaultValue(true)]
  public bool wiggleSector = true;
}
