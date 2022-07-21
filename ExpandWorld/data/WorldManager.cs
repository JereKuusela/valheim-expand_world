using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace ExpandWorld;

public class WorldManager {
  public static string FileName = "expand_world.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.ConfigPath, FileName);
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
    if (data.minSector < 0f) data.minSector += 1f;
    if (data.maxSector > 1f) data.maxSector -= 1f;
    return data;
  }
  public static WorldData ToData(WorldData biome) => biome;

  public static void ToFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataWorld) return;
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(GetBiome.Data.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
    Configuration.valueWorldData.Value = yaml;
  }
  public static void FromFile() {
    if (!ZNet.instance.IsServer()) return;
    var yaml = Configuration.DataWorld ? Data.Read(Pattern) : "";
    Configuration.valueWorldData.Value = yaml;
    Set(yaml);
  }
  public static void FromSetting(string yaml) {
    if (!ZNet.instance.IsServer()) Set(yaml);
  }
  private static void Set(string yaml) {
    if (yaml == "" || !Configuration.DataWorld) return;
    var data = Data.Deserialize<WorldData>(yaml, FileName)
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