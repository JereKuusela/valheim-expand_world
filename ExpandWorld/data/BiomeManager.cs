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
    EnvEntry env = new()
    {
      m_environment = data.environment,
      m_weight = data.weight
    };
    return env;
  }
  public static BiomeEnvironment ToData(EnvEntry env)
  {
    BiomeEnvironment data = new()
    {
      environment = env.m_environment,
      weight = env.m_weight
    };
    return data;
  }
  private static readonly Dictionary<string, Heightmap.Biome> OriginalBiomes = new() {
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
  private static readonly Dictionary<Heightmap.Biome, Color> BiomeToColor = new();
  private static readonly Dictionary<Heightmap.Biome, BiomeData> BiomeToData = new();
  public static bool TryGetColor(Heightmap.Biome biome, out Color color) => BiomeToColor.TryGetValue(biome, out color);
  public static bool TryGetData(Heightmap.Biome biome, out BiomeData data) => BiomeToData.TryGetValue(biome, out data);
  public static bool TryGetBiome(string name, out Heightmap.Biome biome) => NameToBiome.TryGetValue(name.ToLower(), out biome);
  public static Heightmap.Biome GetBiome(string name) => NameToBiome.TryGetValue(name.ToLower(), out var biome) ? biome : Heightmap.Biome.None;
  public static bool TryGetDisplayName(Heightmap.Biome biome, out string name) => BiomeToDisplayName.TryGetValue(biome, out name);
  public static Heightmap.Biome GetTerrain(Heightmap.Biome biome) => BiomeToTerrain.TryGetValue(biome, out var terrain) ? terrain : biome;
  public static Heightmap.Biome GetNature(Heightmap.Biome biome) => BiomeToNature.TryGetValue(biome, out var nature) ? nature : biome;
  public static BiomeEnvSetup FromData(BiomeData data)
  {
    var biome = new BiomeEnvSetup
    {
      m_biome = Data.ToBiomes(data.biome),
      m_environments = data.environments.Select(FromData).ToList(),
      m_musicMorning = data.musicMorning,
      m_musicEvening = data.musicEvening,
      m_musicDay = data.musicDay,
      m_musicNight = data.musicNight
    };
    return biome;
  }
  public static BiomeData ToData(BiomeEnvSetup biome)
  {
    BiomeData data = new()
    {
      biome = Data.FromBiomes(biome.m_biome),
      environments = biome.m_environments.Select(ToData).ToArray(),
      musicMorning = biome.m_musicMorning,
      musicEvening = biome.m_musicEvening,
      musicDay = biome.m_musicDay,
      musicNight = biome.m_musicNight,
      color = Heightmap.GetBiomeColor(biome.m_biome),
      mapColor = Minimap.instance.GetPixelColor(biome.m_biome),
      // Reduces the mountains on the map.
      mapColorMultiplier = biome.m_biome == Heightmap.Biome.AshLands ? 0.5f : 1f
    };
    return data;
  }

  public static void ToFile()
  {
    if (!Helper.IsServer() || !Configuration.DataBiome) return;
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(EnvMan.instance.m_biomes.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
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
        ExpandWorld.Log.LogError(e.Message);
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
      ExpandWorld.Log.LogInfo($"Preloading biome names ({rawData.Count} entries).");
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
  private static List<BiomeEnvSetup> Environments = new();
  private static void Load(string yaml)
  {
    if (yaml == "" || !Configuration.DataBiome) return;
    var rawData = Parse(yaml);
    if (rawData.Count > 0)
      ExpandWorld.Log.LogInfo($"Reloading biome data ({rawData.Count} entries).");
    BiomeToData.Clear();
    BiomeToColor.Clear();
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
      if (item.paint != "") BiomeToColor[biome] = Terrain.ParsePaint(item.paint);
      biomeNumber *= 2;
    }
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
    Environments = rawData.Select(FromData).ToList();
    // This tracks if content (environments) have been loaded.
    if (ZoneSystem.instance.m_locationsByHash.Count > 0)
      LoadEnvironments();
    Generate.World();
  }
  public static void LoadEnvironments()
  {
    if (!Configuration.DataBiome || Environments.Count == 0) return;
    SetupBiomeEnvs(Environments);
  }
  private static void SetupBiomeEnvs(List<BiomeEnvSetup> data)
  {
    var em = EnvMan.instance;
    foreach (var list in LocationList.m_allLocationLists)
      list.m_biomeEnvironments.Clear();
    em.m_biomes.Clear();
    foreach (var biome in data)
      em.AppendBiomeSetup(biome);
    em.m_environmentPeriod = -1;
    em.m_firstEnv = true;

  }
  private static void Set(string yaml)
  {
    Load(yaml);
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