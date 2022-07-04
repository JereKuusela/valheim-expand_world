using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
namespace ExpandWorld;

[Serializable]
public class BiomeEnvironment {
  public string environment = "";
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

[Serializable]
public class BiomeData {
  public string biome = "";
  public List<BiomeEnvironment> environments = new();
  public string musicMorning = "morning";
  public string musicEvening = "evening";
  public string musicDay = "";
  public string musicNight = "";
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
    data.environments = biome.m_environments.Select(BiomeEnvironment.Serialize).ToList();
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
    var data = Data.Deserializer().Deserialize<List<BiomeData>>(raw)
      .Select(FromData).ToList();
    if (data.Count == 0) return;
    ExpandWorld.Log.LogInfo($"Reloading {data.Count} biome data.");
    foreach (var list in LocationList.m_allLocationLists)
      list.m_biomeEnvironments.Clear();
    EnvMan.instance.m_biomes.Clear();
    foreach (var biome in data)
      EnvMan.instance.AppendBiomeSetup(biome);
  }
  public static void SetupWatcher() {
    FileSystemWatcher watcher = new(Path.GetDirectoryName(Data.BiomeFile), Path.GetFileName(Data.BiomeFile));
    watcher.Changed += (s, e) => Load(Data.BiomeFile);
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }
}