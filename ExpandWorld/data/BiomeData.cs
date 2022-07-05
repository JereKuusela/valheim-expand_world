using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
namespace ExpandWorld;

public class BiomeEnvironment {
  public string environment = "";
  [DefaultValue(1f)]
  public float weight = 1f;
  public static EnvEntry Deserialize(BiomeEnvironment data) {
    EnvEntry env = new();
    env.m_environment = data.environment;
    env.m_weight = data.weight;
    return env;
  }
  public static BiomeEnvironment Serialize(EnvEntry env) {
    BiomeEnvironment data = new();
    data.environment = env.m_environment;
    data.weight = env.m_weight;
    return data;
  }
}

public class BiomeData {
  public string biome = "";
  [DefaultValue("")]
  public string name = "";
  [DefaultValue("")]
  public string terrain = "";
  [DefaultValue(1f)]
  public float altitudeMultiplier = 1f;
  [DefaultValue(0f)]
  public float altitudeDelta = 0f;
  public BiomeEnvironment[] environments = new BiomeEnvironment[0];
  [DefaultValue("")]
  public string musicMorning = "morning";
  [DefaultValue("")]
  public string musicEvening = "evening";
  [DefaultValue("")]
  public string musicDay = "";
  [DefaultValue("")]
  public string musicNight = "";
  private static Dictionary<string, Heightmap.Biome> DefaultNameToBiome = new() {
    { "none", Heightmap.Biome.None},
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
  public static Dictionary<string, Heightmap.Biome> NameToBiome = DefaultNameToBiome;
  public static Dictionary<Heightmap.Biome, string> BiomeToName = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
  public static Dictionary<Heightmap.Biome, BiomeData> BiomeToData = new();
  public static BiomeEnvSetup FromData(BiomeData data) {
    var biome = new BiomeEnvSetup();
    biome.m_biome = Data.ToBiomes(new string[] { data.biome });
    biome.m_environments = data.environments.Select(BiomeEnvironment.Deserialize).ToList();
    biome.m_musicMorning = data.musicMorning;
    biome.m_musicEvening = data.musicEvening;
    biome.m_musicDay = data.musicDay;
    biome.m_musicNight = data.musicNight;
    return biome;
  }
  public static BiomeData ToData(BiomeEnvSetup biome) {
    BiomeData data = new();
    data.biome = Data.FromBiomes(biome.m_biome).FirstOrDefault();
    data.environments = biome.m_environments.Select(BiomeEnvironment.Serialize).ToArray();
    data.musicMorning = biome.m_musicMorning;
    data.musicEvening = biome.m_musicEvening;
    data.musicDay = biome.m_musicDay;
    data.musicNight = biome.m_musicNight;
    return data;
  }

  public static void Save(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataBiome) return;
    var yaml = Data.Serializer().Serialize(EnvMan.instance.m_biomes.Select(ToData).ToList());
    File.WriteAllText(fileName, yaml);
  }
  public static void Load(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataBiome) return;
    Configuration.configInternalDataBiome.Value = File.ReadAllText(fileName);
  }
  public static void Set(string raw) {
    if (raw == "" || !Configuration.DataBiome) return;
    var rawData = Data.Deserializer().Deserialize<List<BiomeData>>(raw);
    if (rawData.Count == 0) return;
    ExpandWorld.Log.LogInfo($"Reloading {rawData.Count} biome data.");
    BiomeToData.Clear();
    NameToBiome = DefaultNameToBiome.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    var biomeNumber = ((int)Heightmap.Biome.Mistlands * 2);
    foreach (var biome in rawData) {
      if (biome.name != "") {
        var key = "biome_" + ((Heightmap.Biome)biomeNumber).ToString().ToLower();
        ExpandWorld.Log.LogInfo("Translate: " + biome.name + " to " + key);
        Localization.instance.m_translations[key] = biome.name;
      }
      if (NameToBiome.ContainsKey(biome.biome.ToLower())) {
        BiomeToData[NameToBiome[biome.biome.ToLower()]] = biome;
        continue;
      }
      NameToBiome.Add(biome.biome.ToLower(), (Heightmap.Biome)biomeNumber);
      BiomeToData[(Heightmap.Biome)biomeNumber] = biome;
      biomeNumber *= 2;
    }
    BiomeToName = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    Heightmap.tempBiomeWeights = new float[biomeNumber / 2 + 1];
    var data = rawData.Select(FromData).ToList();
    foreach (var list in LocationList.m_allLocationLists)
      list.m_biomeEnvironments.Clear();
    EnvMan.instance.m_biomes.Clear();
    foreach (var biome in data)
      EnvMan.instance.AppendBiomeSetup(biome);
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Data.BiomeFile, Load);
  }
}