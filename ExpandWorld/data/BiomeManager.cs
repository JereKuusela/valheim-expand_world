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
  public static Dictionary<Heightmap.Biome, string> BiomeToName = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
  ///<summary>Original biome names because some mods rely on Enum.GetName(s) returning uppercase values.</summary>
  public static Dictionary<Heightmap.Biome, string> BiomeToDisplayName = OriginalBiomes.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
  private static Dictionary<Heightmap.Biome, Heightmap.Biome> BiomeToTerrain = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Value);
  private static Dictionary<Heightmap.Biome, Heightmap.Biome> BiomeToNature = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Value);
  private static Dictionary<Heightmap.Biome, BiomeData> BiomeToData = new();
  public static bool TryGetData(Heightmap.Biome biome, out BiomeData data) => BiomeToData.TryGetValue(biome, out data);
  public static bool TryGetData(int biome, out BiomeData data) => BiomeToData.TryGetValue((Heightmap.Biome)biome, out data);
  public static bool TryGetBiome(string name, out Heightmap.Biome biome) => NameToBiome.TryGetValue(name.ToLower(), out biome);
  public static bool TryGetDisplayName(Heightmap.Biome biome, out string name) => BiomeToDisplayName.TryGetValue(biome, out name);
  public static bool TryGetDisplayName(int biome, out string name) => BiomeToDisplayName.TryGetValue((Heightmap.Biome)biome, out name);
  public static Heightmap.Biome[] Biomes = BiomeToName.Keys.OrderBy(s => s).ToArray();
  public static Heightmap.Biome GetTerrain(Heightmap.Biome biome) => BiomeToTerrain[biome];
  public static Heightmap.Biome GetNature(Heightmap.Biome biome) => BiomeToNature.TryGetValue((Heightmap.Biome)biome, out var nature) ? nature : biome;
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
  public static void FromSetting(string yaml)
  {
    if (Helper.IsClient()) Set(yaml);
  }
  public static bool BiomeForestMultiplier = false;
  public static bool BiomePaint = false;
  private static void Load(string yaml)
  {
    if (yaml == "" || !Configuration.DataBiome) return;
    try
    {
      var rawData = Data.Deserialize<BiomeData>(yaml, FileName);
      if (rawData.Count == 0)
      {
        ExpandWorld.Log.LogWarning($"Failed to load any biome data.");
        return;
      }
      ExpandWorld.Log.LogInfo($"Reloading {rawData.Count} biome data.");
      BiomeToData.Clear();
      NameToBiome = OriginalBiomes.ToDictionary(kvp => kvp.Key.ToLower(), kvp => kvp.Value);
      BiomeToDisplayName = OriginalBiomes.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
      var biomeNumber = ((int)Heightmap.Biome.Mistlands * 2);
      foreach (var item in rawData)
      {
        item.biome = item.biome.ToLower();
        item.terrain = item.terrain.ToLower();
        item.nature = item.nature.ToLower();
        var biome = (Heightmap.Biome)biomeNumber;
        var isDefaultBiome = false;
        if (NameToBiome.TryGetValue(item.biome, out var defaultBiome))
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
        NameToBiome.Add(item.biome, biome);
        BiomeToData[biome] = item;
        biomeNumber *= 2;
      }
      MaxBiome = (Heightmap.Biome)(biomeNumber - 1);
      BiomeToName = NameToBiome.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
      BiomeToTerrain = rawData.ToDictionary(data => NameToBiome[data.biome], data =>
      {
        if (NameToBiome.TryGetValue(data.terrain, out var terrain))
          return terrain;
        return NameToBiome[data.biome];
      });
      BiomeToNature = rawData.ToDictionary(data => NameToBiome[data.biome], data =>
      {
        if (NameToBiome.TryGetValue(data.nature, out var nature))
          return nature;
        if (NameToBiome.TryGetValue(data.terrain, out var terrain))
          return terrain;
        return NameToBiome[data.biome];
      });
      Biomes = BiomeToName.Keys.OrderBy(s => s).ToArray();
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
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.StackTrace);
    }
  }
  private static void Set(string yaml)
  {
    Load(yaml);
    Data.BiomesLoaded = true;
    SpawnThatPatcher.InitConfiguration();
    CustomRaidsPatcher.InitConfiguration();
    CLLCPatcher.InitConfiguration();

  }
  public static void SetupWatcher()
  {
    Data.SetupWatcher(Pattern, FromFile);
  }
}