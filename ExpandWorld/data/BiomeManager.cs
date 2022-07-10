using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Service;
using UnityEngine;
namespace ExpandWorld;

public class BiomeManager {
  public static string FileName = Path.Combine(ExpandWorld.ConfigPath, "expand_biomes.yaml");
  
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
  private static Dictionary<string, Heightmap.Biome> NameToBiome = DefaultNameToBiome;
  private static Dictionary<Heightmap.Biome, string> BiomeToName = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
  private static Dictionary<Heightmap.Biome, BiomeData> BiomeToData = new();
  public static bool TryGetData(Heightmap.Biome biome, out BiomeData data) => BiomeToData.TryGetValue(biome, out data);
  public static bool TryGetData(int biome, out BiomeData data) => BiomeToData.TryGetValue((Heightmap.Biome)biome, out data);
  public static bool TryGetBiome(string name, out Heightmap.Biome biome) => NameToBiome.TryGetValue(name.ToLower(), out biome);
  public static bool TryGetName(Heightmap.Biome biome, out string name) => BiomeToName.TryGetValue(biome, out name);
  public static bool TryGetName(int biome, out string name) => BiomeToName.TryGetValue((Heightmap.Biome)biome, out name);
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
    return data;
  }

  public static void ToFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataBiome) return;
    if (File.Exists(FileName)) return;
    var yaml = Data.Serializer().Serialize(EnvMan.instance.m_biomes.Select(ToData).ToList());
    Configuration.valueBiomeData.Value = yaml;
    File.WriteAllText(FileName, yaml);
  }
  public static void FromFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataBiome) return;
    if (!File.Exists(FileName)) return;
    var yaml = File.ReadAllText(FileName);
    Configuration.valueBiomeData.Value = yaml;
    if (Data.IsLoading) Set(yaml);
  }
  public static void FromSetting(string raw) {
    if (!Data.IsLoading) Set(raw);
  }
  private static void Set(string raw) {
    if (raw == "" || !Configuration.DataBiome) return;
    var rawData = Data.Deserializer().Deserialize<List<BiomeData>>(raw);
    if (rawData.Count == 0) {
      ExpandWorld.Log.LogWarning($"Failed to load any biome data.");
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading {rawData.Count} biome data.");
    BiomeToData.Clear();
    NameToBiome = DefaultNameToBiome.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    var biomeNumber = ((int)Heightmap.Biome.Mistlands * 2);
    foreach (var biome in rawData) {
      if (biome.name != "") {
        var key = "biome_" + ((Heightmap.Biome)biomeNumber).ToString().ToLower();
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
    EnvMan.instance.m_environmentPeriod = -1;
    EnvMan.instance.m_firstEnv = true;
    Generate.World();
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(FileName, FromFile);
  }
}