using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace ExpandWorld;
public class BiomeManager {
  public static string FileName = "expand_biomes.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.ConfigPath, FileName);
  public static string Pattern = "expand_biomes*.yaml";

  public static EnvEntry FromData(BiomeEnvironment data) {
    EnvEntry env = new();
    env.m_environment = data.environment;
    env.m_weight = data.weight;
    return env;
  }
  public static BiomeEnvironment ToData(EnvEntry env) {
    BiomeEnvironment data = new();
    data.environment = env.m_environment;
    data.weight = env.m_weight;
    return data;
  }
  private static Dictionary<string, Heightmap.Biome> DefaultNameToBiome = new() {
    { "meadows", Heightmap.Biome.Meadows},
    { "swamp", Heightmap.Biome.Swamp},
    { "mountain", Heightmap.Biome.Mountain},
    { "blackforest", Heightmap.Biome.BlackForest},
    { "plains", Heightmap.Biome.Plains},
    { "ashlands", Heightmap.Biome.AshLands},
    { "deepnorth", Heightmap.Biome.DeepNorth},
    { "ocean", Heightmap.Biome.Ocean},
    { "mistlands", Heightmap.Biome.Mistlands},
  };
  private static Dictionary<string, Heightmap.Biome> NameToBiome = DefaultNameToBiome;
  public static Dictionary<Heightmap.Biome, string> BiomeToName = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
  private static Dictionary<Heightmap.Biome, Heightmap.Biome> BiomeToTerrain = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Value);
  private static Dictionary<Heightmap.Biome, BiomeData> BiomeToData = new();
  public static bool TryGetData(Heightmap.Biome biome, out BiomeData data) => BiomeToData.TryGetValue(biome, out data);
  public static bool TryGetData(int biome, out BiomeData data) => BiomeToData.TryGetValue((Heightmap.Biome)biome, out data);
  public static bool TryGetBiome(string name, out Heightmap.Biome biome) => NameToBiome.TryGetValue(name.ToLower(), out biome);
  public static bool TryGetName(Heightmap.Biome biome, out string name) => BiomeToName.TryGetValue(biome, out name);
  public static bool TryGetName(int biome, out string name) => BiomeToName.TryGetValue((Heightmap.Biome)biome, out name);
  public static Heightmap.Biome[] Biomes = BiomeToName.Keys.OrderBy(s => s).ToArray();
  public static Heightmap.Biome GetTerrain(Heightmap.Biome biome) => BiomeToTerrain[biome];
  public static BiomeEnvSetup FromData(BiomeData data) {
    var biome = new BiomeEnvSetup();
    biome.m_biome = Data.ToBiomes(new string[] { data.biome });
    biome.m_environments = data.environments.Select(FromData).ToList();
    biome.m_musicMorning = data.musicMorning;
    biome.m_musicEvening = data.musicEvening;
    biome.m_musicDay = data.musicDay;
    biome.m_musicNight = data.musicNight;
    return biome;
  }
  public static BiomeData ToData(BiomeEnvSetup biome) {
    BiomeData data = new();
    data.biome = Data.FromBiomes(biome.m_biome).FirstOrDefault();
    data.environments = biome.m_environments.Select(ToData).ToArray();
    data.musicMorning = biome.m_musicMorning;
    data.musicEvening = biome.m_musicEvening;
    data.musicDay = biome.m_musicDay;
    data.musicNight = biome.m_musicNight;
    data.color = Heightmap.GetBiomeColor(biome.m_biome);
    data.mapColor = MapGeneration.GetPixelColor32(biome.m_biome);
    // Reduces the mountains on the map.
    data.mapColorMultiplier = biome.m_biome == Heightmap.Biome.AshLands ? 0.5f : 1f;
    return data;
  }

  public static void ToFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataBiome) return;
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(EnvMan.instance.m_biomes.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
    Configuration.valueBiomeData.Value = yaml;
  }
  public static void FromFile() {
    if (!ZNet.instance.IsServer()) return;
    var yaml = Configuration.DataBiome ? Data.Read(Pattern) : "";
    Configuration.valueBiomeData.Value = yaml;
    Set(yaml);
  }
  public static void FromSetting(string yaml) {
    if (!ZNet.instance.IsServer()) Set(yaml);
  }
  public static bool BiomeForestMultiplier = false;
  private static void Load(string yaml) {
    if (yaml == "" || !Configuration.DataBiome) return;
    var rawData = Data.Deserialize<BiomeData>(yaml, FileName);
    if (rawData.Count == 0) {
      ExpandWorld.Log.LogWarning($"Failed to load any biome data.");
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading {rawData.Count} biome data.");
    BiomeToData.Clear();
    NameToBiome = DefaultNameToBiome.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    var biomeNumber = ((int)Heightmap.Biome.Mistlands * 2);
    foreach (var biome in rawData) {
      biome.biome = biome.biome.ToLower();
      biome.terrain = biome.terrain.ToLower();
      Data.Sanity(ref biome.mapColor);
      Data.Sanity(ref biome.color);
      Data.Sanity(ref biome.paint);
      if (biome.name != "") {
        var key = "biome_" + ((Heightmap.Biome)biomeNumber).ToString().ToLower();
        Localization.instance.m_translations[key] = biome.name;
      }
      if (NameToBiome.ContainsKey(biome.biome)) {
        BiomeToData[NameToBiome[biome.biome]] = biome;
        continue;
      }
      NameToBiome.Add(biome.biome, (Heightmap.Biome)biomeNumber);
      BiomeToData[(Heightmap.Biome)biomeNumber] = biome;
      biomeNumber *= 2;
    }
    BiomeToName = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    BiomeToTerrain = rawData.ToDictionary(data => NameToBiome[data.biome], data => {
      if (NameToBiome.TryGetValue(data.terrain, out var terrain))
        return terrain;
      return NameToBiome[data.biome];
    });
    Biomes = BiomeToName.Keys.OrderBy(s => s).ToArray();
    Heightmap.tempBiomeWeights = new float[biomeNumber / 2 + 1];
    BiomeForestMultiplier = rawData.Any(data => data.forestMultiplier != 1f);
    var data = rawData.Select(FromData).ToList();
    foreach (var list in LocationList.m_allLocationLists)
      list.m_biomeEnvironments.Clear();
    EnvMan.instance.m_biomes.Clear();
    foreach (var biome in data)
      EnvMan.instance.AppendBiomeSetup(biome);
    EnvMan.instance.m_environmentPeriod = -1;
    EnvMan.instance.m_firstEnv = true;
    Generate.World();
    CLLCWrapper.UpdateBiomes();
  }
  private static void Set(string yaml) {
    Load(yaml);
    Data.BiomesLoaded = true;
    SpawnThatPatcher.InitConfiguration();

  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Pattern, FromFile);
  }
}