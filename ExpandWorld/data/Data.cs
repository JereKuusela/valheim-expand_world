
using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using HarmonyLib;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ExpandWorld;

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations))]
public class LoadData {
  public static bool IsLoading = false;
  static void Prefix() {
    if (!ZNet.instance.IsServer()) return;
    IsLoading = true;
    if (File.Exists(Data.BiomeFile))
      BiomeData.Load(Data.BiomeFile);
    if (File.Exists(Data.WorldFile))
      WorldData.Load(Data.WorldFile);
    if (File.Exists(Data.VegFile))
      VegetationData.Load(Data.VegFile);
    if (File.Exists(Data.LocFile))
      LocationData.Load(Data.LocFile);
    if (File.Exists(Data.EventFile))
      EventData.Load(Data.EventFile);
    if (File.Exists(Data.SpawnFile))
      SpawnData.Load(Data.SpawnFile);
    IsLoading = false;
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
public class SaveData {
  static void Postfix() {
    if (!ZNet.instance.IsServer()) return;
    if (!File.Exists(Data.BiomeFile))
      BiomeData.Save(Data.BiomeFile);
    if (!File.Exists(Data.WorldFile))
      WorldData.Save(Data.WorldFile);
    if (!File.Exists(Data.VegFile))
      VegetationData.Save(Data.VegFile);
    if (!File.Exists(Data.LocFile))
      LocationData.Save(Data.LocFile);
    if (!File.Exists(Data.EventFile))
      EventData.Save(Data.EventFile);
    // Spawn data handle elsewhere.
  }
}
[HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Awake))]
public class HandleSpawnData {
  // File exist check might be bit too slow for constant checking.
  static bool Done = false;
  public static List<SpawnSystem.SpawnData>? Override = null;
  static void Postfix(SpawnSystem __instance) {
    if (ZNet.instance.IsServer() && !Done) {
      if (!File.Exists(Data.SpawnFile))
        SpawnData.Save(Data.SpawnFile);
      Done = true;
    }
    if (Override != null) {
      __instance.m_spawnLists.Clear();
      __instance.m_spawnLists.Add(new() { m_spawners = Override });
    }
  }
}

[HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.UpdateSpawning))]
public class Spawn_WaitForConfigSync {
  static bool Prefix() => ExpandWorld.ConfigSync.IsSourceOfTruth || ExpandWorld.ConfigSync.InitialSyncDone;
}
public static class Data {
  public static string SpawnFile = Path.Combine(ExpandWorld.ConfigPath, "expand_spawns.yaml");
  public static string VegFile = Path.Combine(ExpandWorld.ConfigPath, "expand_vegetation.yaml");
  public static string LocFile = Path.Combine(ExpandWorld.ConfigPath, "expand_locations.yaml");
  public static string BiomeFile = Path.Combine(ExpandWorld.ConfigPath, "expand_biomes.yaml");
  public static string WorldFile = Path.Combine(ExpandWorld.ConfigPath, "expand_world.yaml");
  public static string EventFile = Path.Combine(ExpandWorld.ConfigPath, "expand_events.yaml");

  public static void SetupWatcher(string file, Action<string> action) {
    FileSystemWatcher watcher = new(Path.GetDirectoryName(file), Path.GetFileName(file));
    watcher.Changed += (s, e) => action(file);
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }
  public static IDeserializer Deserializer() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
  public static ISerializer Serializer() => new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults).Build();

  public static string[] FromBiomes(Heightmap.Biome biome) {
    List<string> biomes = new();
    var number = 1;
    var biomeNumber = (int)biome;
    while (number <= biomeNumber) {
      if ((number & biomeNumber) > 0) {
        if (BiomeData.BiomeToName.TryGetValue((Heightmap.Biome)number, out var name))
          biomes.Add(name);
        else
          biomes.Add(number.ToString());
      }
      number *= 2;
    }
    return biomes.ToArray();
  }
  public static string[] FromBiomeAreas(Heightmap.BiomeArea biomeArea) {
    List<string> biomesAreas = new();
    if ((biomeArea & Heightmap.BiomeArea.Edge) > 0) {
      biomesAreas.Add("edge");
      biomeArea -= Heightmap.BiomeArea.Edge;
    }
    if ((biomeArea & Heightmap.BiomeArea.Median) > 0) {
      biomesAreas.Add("median");
      biomeArea -= Heightmap.BiomeArea.Median;
    }
    if (biomeArea > 0) biomesAreas.Add(biomeArea.ToString());
    return biomesAreas.ToArray();
  }
  public static Heightmap.Biome ToBiomes(string[] m_biome) {
    Heightmap.Biome result = 0;
    foreach (var biome in m_biome) {
      if (BiomeData.NameToBiome.TryGetValue(biome.ToLower(), out var number))
        result += (int)number;
      else {
        if (int.TryParse(biome, out var value)) result += value;
        else throw new InvalidOperationException($"Invalid biome {biome}.");
      }
    }
    return result;
  }
  public static Heightmap.BiomeArea ToBiomeAreas(string[] m_biome) {
    Heightmap.BiomeArea result = 0;
    foreach (var biome in m_biome) {
      var name = biome.ToLower();
      if (name == "edge") result += (int)Heightmap.BiomeArea.Edge;
      else if (name == "median") result += (int)Heightmap.BiomeArea.Median;
      else {
        if (int.TryParse(biome, out var value)) result += value;
        else throw new InvalidOperationException($"Invalid biome area {biome}.");
      }
    }
    return result;
  }
}