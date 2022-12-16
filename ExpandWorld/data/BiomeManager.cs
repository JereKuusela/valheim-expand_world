using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ExpandWorld;
public class BiomeManager
{
  public static string FileName = "expand_biomes.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  public static string Pattern = "expand_biomes*.yaml";

  public static EnvEntry FromData(BiomeEnvironment data)
  {
    EnvEntry env = new();
    env.m_environment = data.environment;
    env.m_weight = data.weight;
    return env;
  }
  public static BiomeEnvironment ToData(EnvEntry env)
  {
    BiomeEnvironment data = new();
    data.environment = env.m_environment;
    data.weight = env.m_weight;
    return data;
  }
  private static Dictionary<string, Heightmap.Biome> OriginalBiomes = new() {
    { "None", Heightmap.Biome.None},
    { "Meadows", Heightmap.Biome.Meadows},
    { "Swamp", Heightmap.Biome.Swamp},
    { "Mountain", Heightmap.Biome.Mountain},
    { "BlackForest", Heightmap.Biome.BlackForest},
    { "Plains", Heightmap.Biome.Plains},
    { "AshLands", Heightmap.Biome.AshLands},
    { "DeepNorth", Heightmap.Biome.DeepNorth},
    { "Ocean", Heightmap.Biome.Ocean},
    { "Mistlands", Heightmap.Biome.Mistlands},
  };
  ///<summary>Lower case biome names for easier data loading.</summary>
  private static Dictionary<string, Heightmap.Biome> NameToBiome = OriginalBiomes.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);
  ///<summary>Original biome names because some mods rely on Enum.GetName(s) returning uppercase values.</summary>
  public static Dictionary<Heightmap.Biome, string> BiomeToDisplayName = OriginalBiomes.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
  private static Dictionary<Heightmap.Biome, Heightmap.Biome> BiomeToTerrain = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Value);
  private static Dictionary<Heightmap.Biome, Heightmap.Biome> BiomeToNature = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Value);
  private static Dictionary<Heightmap.Biome, BiomeData> BiomeToData = new();
  public static bool TryGetData(Heightmap.Biome biome, out BiomeData data) => BiomeToData.TryGetValue(biome, out data);
  public static bool TryGetBiome(string name, out Heightmap.Biome biome) => NameToBiome.TryGetValue(name.ToLower(), out biome);
  public static Heightmap.Biome GetBiome(string name) => NameToBiome.TryGetValue(name.ToLower(), out var biome) ? biome : Heightmap.Biome.None;
  public static bool TryGetDisplayName(Heightmap.Biome biome, out string name) => BiomeToDisplayName.TryGetValue(biome, out name);
  public static Heightmap.Biome GetTerrain(Heightmap.Biome biome) => BiomeToTerrain.TryGetValue(biome, out var terrain) ? terrain : biome;
  public static Heightmap.Biome GetNature(Heightmap.Biome biome) => BiomeToNature.TryGetValue(biome, out var nature) ? nature : biome;
  public static Heightmap.Biome MaxBiome = (Heightmap.Biome)((2 * (int)Heightmap.Biome.Mistlands) - 1);
  public static BiomeEnvSetup FromData(BiomeData data)
  {
    var biome = new BiomeEnvSetup();
    biome.m_biome = Data.ToBiomes(data.biome);
    biome.m_environments = data.environments.Select(FromData).ToList();
    biome.m_musicMorning = data.musicMorning;
    biome.m_musicEvening = data.musicEvening;
    biome.m_musicDay = data.musicDay;
    biome.m_musicNight = data.musicNight;
    return biome;
  }
  public static BiomeData ToData(BiomeEnvSetup biome)
  {
    BiomeData data = new();
    data.biome = Data.FromBiomes(biome.m_biome);
    data.environments = biome.m_environments.Select(ToData).ToArray();
    data.musicMorning = biome.m_musicMorning;
    data.musicEvening = biome.m_musicEvening;
    data.musicDay = biome.m_musicDay;
    data.musicNight = biome.m_musicNight;
    data.color = Heightmap.GetBiomeColor(biome.m_biome);
    data.mapColor = Minimap.instance.GetPixelColor(biome.m_biome);
    // Reduces the mountains on the map.
    data.mapColorMultiplier = biome.m_biome == Heightmap.Biome.AshLands ? 0.5f : 1f;
    return data;
  }

  public static void ToFile()
  {
    if (!Helper.IsServer() || !Configuration.DataBiome) return;
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(EnvMan.instance.m_biomes.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
    Configuration.valueBiomeData.Value = yaml;
  }
  public static void FromFile()
  {
    if (!Helper.IsServer()) return;
    var yaml = Configuration.DataBiome ? Data.Read(Pattern) : "";
    Configuration.valueBiomeData.Value = yaml;
    Set(yaml);
  }
  public static void NamesFromFile()
  {
    if (!Configuration.DataBiome) return;
    LoadNames(Data.Read(Pattern));
  }
  public static void FromSetting(string yaml)
  {
    if (Helper.IsClient()) Set(yaml);
  }
  public static bool BiomeForestMultiplier = false;
  public static bool BiomePaint = false;

  private static List<BiomeData> Parse(string yaml)
  {
    List<BiomeData> rawData = new();
    if (Configuration.DataBiome)
    {
      try
      {
        rawData = Data.Deserialize<BiomeData>(yaml, FileName);
      }
      catch (Exception e)
      {
        ExpandWorld.Log.LogWarning($"Failed to load any biome data.");
        ExpandWorld.Log.LogError(e.StackTrace);
      }
    }
    return rawData;
  }
  public static void SetNames(Dictionary<Heightmap.Biome, string> names)
  {
    BiomeToDisplayName = names;
    NameToBiome = BiomeToDisplayName.ToDictionary(kvp => kvp.Value.ToLower(), kvp => kvp.Key);
    ExpandWorld.Log.LogInfo($"Received {BiomeToDisplayName.Count} biome names.");
  }
  private static void LoadNames(string yaml)
  {
    var rawData = Parse(yaml);
    if (rawData.Count > 0)
      ExpandWorld.Log.LogInfo($"Preloading {rawData.Count} biome names.");
    var originalNames = OriginalBiomes.Select(kvp => kvp.Key.ToLower()).ToHashSet();
    BiomeToDisplayName = OriginalBiomes.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    var biomeNumber = ((int)Heightmap.Biome.Mistlands * 2);
    foreach (var item in rawData)
    {
      if (originalNames.Contains(item.biome.ToLower())) continue;
      var biome = (Heightmap.Biome)biomeNumber;
      BiomeToDisplayName[biome] = item.biome;
      biomeNumber *= 2;
    }
    NameToBiome = BiomeToDisplayName.ToDictionary(kvp => kvp.Value.ToLower(), kvp => kvp.Key);
  }
  private static void Load(string yaml)
  {
    if (yaml == "" || !Configuration.DataBiome) return;
    var rawData = Parse(yaml);
    if (rawData.Count > 0)
      ExpandWorld.Log.LogInfo($"Reloading {rawData.Count} biome data.");
    BiomeToData.Clear();
    NameToBiome = OriginalBiomes.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);
    BiomeToDisplayName = OriginalBiomes.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
    var biomeNumber = ((int)Heightmap.Biome.Mistlands * 2);
    foreach (var item in rawData)
    {
      var biome = (Heightmap.Biome)biomeNumber;
      var isDefaultBiome = false;
      if (NameToBiome.TryGetValue(item.biome.ToLower(), out var defaultBiome))
      {
        isDefaultBiome = true;
        biome = defaultBiome;
      }
      Data.Sanity(ref item.mapColor);
      Data.Sanity(ref item.color);
      Data.Sanity(ref item.paint);
      if (!BiomeToDisplayName.ContainsKey(biome))
        BiomeToDisplayName[biome] = item.biome;
      if (item.name != "" || !isDefaultBiome)
      {
        var name = item.name == "" ? biome.ToString() : item.name;
        var key = "biome_" + biome.ToString().ToLower();
        Localization.instance.m_translations[key] = name;
      }
      if (isDefaultBiome)
      {
        BiomeToData[biome] = item;
        continue;
      }
      NameToBiome.Add(item.biome.ToLower(), biome);
      BiomeToData[biome] = item;
      biomeNumber *= 2;
    }
    MaxBiome = (Heightmap.Biome)(biomeNumber - 1);
    BiomeToTerrain = rawData.ToDictionary(data => GetBiome(data.biome), data =>
    {
      if (TryGetBiome(data.terrain, out var terrain))
        return terrain;
      return GetBiome(data.biome);
    });
    BiomeToNature = rawData.ToDictionary(data => GetBiome(data.biome), data =>
    {
      if (TryGetBiome(data.nature, out var nature))
        return nature;
      if (TryGetBiome(data.terrain, out var terrain))
        return terrain;
      return GetBiome(data.biome);
    });
    // This is not used because base game code is overriden (low performance).
    // But better still set as a failsafe.
    Heightmap.tempBiomeWeights = new float[biomeNumber / 2 + 1];
    BiomeForestMultiplier = rawData.Any(data => data.forestMultiplier != 1f);
    BiomePaint = rawData.Any(data => data.paint != new Color());
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
  private static void Set(string yaml)
  {
    Load(yaml);
    Data.BiomesLoaded = true;
  }
  public static void SetupWatcher()
  {
    var callback = () =>
    {
      if (ZNet.m_instance == null) NamesFromFile();
      else FromFile();
    };
    Data.SetupWatcher(Pattern, callback);
  }
}