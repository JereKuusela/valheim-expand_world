using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Service;
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
  [DefaultValue(null)]
  public int? seed = null;
  [DefaultValue(true)]
  public bool wiggle = true;

  public static List<WorldData> GetDefault() {
    return new() {
      new() {
        biome = "ocean",
        _biome = Heightmap.Biome.Ocean,
        maxAltitude = 4f
      },
      new() {
        biome = "ashlands",
        _biome = Heightmap.Biome.AshLands,
        curveY = -0.4f,
        minDistance = 0.8f,
      },
      new() {
        biome = "mountain",
        _biome = Heightmap.Biome.Mountain,
        minAltitude = 80f,
      },
      new() {
        biome = "deepnorth",
        _biome = Heightmap.Biome.DeepNorth,
        curveY = 0.4f,
        minDistance = 0.8f,
      },
      new() {
        biome = "swamp",
        _biome = Heightmap.Biome.Swamp,
        wiggle = false,
        minDistance = 0.2f,
        maxDistance = 0.8f,
        minAltitude = 10f,
        maxAltitude = 50f,
        amount = 0.4f,
      },
      new() {
        biome = "mistlands",
        _biome = Heightmap.Biome.Mistlands,
        minDistance = 0.6f,
        amount = 0.5f,
      },
      new() {
        biome = "plains",
        _biome = Heightmap.Biome.Plains,
        minDistance = 0.3f,
        maxDistance = 0.8f,
        amount = 0.6f,
      },
      new() {
        biome = "blackforest",
        _biome = Heightmap.Biome.BlackForest,
        minDistance = 0.06f,
        maxDistance = 0.6f,
        amount = 0.6f,
      },
      new() {
        biome = "meadows",
        _biome = Heightmap.Biome.Meadows,
        maxDistance = 0.5f,
      },
      new() {
        biome = "blackforest",
        _biome = Heightmap.Biome.BlackForest,
      },
    };
  }

  public static WorldData FromData(WorldData data) {
    data._biome = Data.ToBiomes(new string[] { data.biome });
    return data;
  }
  public static WorldData ToData(WorldData biome) => biome;

  public static void ToFile(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataWorld) return;
    var yaml = Data.Serializer().Serialize(GetBiome.Data.Select(ToData).ToList());
    File.WriteAllText(fileName, yaml);
  }
  public static void FromFile(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataWorld) return;
    var raw = File.ReadAllText(fileName);
    Configuration.configInternalDataWorld.Value = raw;
    if (LoadData.IsLoading) Set(raw);
  }
  public static void FromSetting(string raw) {
    if (!LoadData.IsLoading) Set(raw);
  }
  private static void Set(string raw) {
    if (raw == "" || !Configuration.DataWorld) return;
    var data = Data.Deserializer().Deserialize<List<WorldData>>(raw)
      .Select(FromData).ToList();
    if (data.Count == 0) {
      ExpandWorld.Log.LogWarning($"Failed to load any world data.");
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading {data.Count} world data.");
    GetBiome.Data = data;
    ConfigWrapper.ForceRegen();
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Data.WorldFile, FromFile);
  }
}