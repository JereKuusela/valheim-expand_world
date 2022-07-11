using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace ExpandWorld;

public class WorldManager {
  public static string FileName = Path.Combine(ExpandWorld.ConfigPath, "expand_world.yaml");
  public static string Pattern = "expand_world*.yaml";
  public static List<WorldData> GetDefault() {
    return new() {
      new() {
        biome = "ocean",
        _biome = Heightmap.Biome.Ocean,
        maxAltitude = -26f
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
        minAltitude = 50f,
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
        wiggleDistance = false,
        minDistance = 0.2f,
        maxDistance = 0.8f,
        minAltitude = -20f,
        maxAltitude = 20f,
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

  public static void ToFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataWorld) return;
    if (File.Exists(FileName)) return;
    var yaml = Data.Serializer().Serialize(GetBiome.Data.Select(ToData).ToList());
    Configuration.valueWorldData.Value = yaml;
    File.WriteAllText(FileName, yaml);
  }
  public static void FromFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataWorld) return;
    if (!File.Exists(FileName)) return;
    var yaml = Data.Read(Pattern);
    Configuration.valueWorldData.Value = yaml;
    if (Data.IsLoading) Set(yaml);
  }
  public static void FromSetting(string raw) {
    if (!Data.IsLoading) Set(raw);
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
    Generate.World();
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Pattern, FromFile);
  }
}