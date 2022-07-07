
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
  static void Prefix() {
    EnvironmentManager.Originals.Clear();
    if (!ZNet.instance.IsServer()) return;
    Data.IsLoading = true;
    EnvironmentManager.FromFile();
    BiomeManager.FromFile();
    WorldManager.FromFile();
    VegetationManager.FromFile();
    LocationManager.FromFile();
    EventManager.FromFile();
    SpawnManager.FromFile();
    ClutterManager.FromFile();
    Data.IsLoading = false;
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
public class SaveData {
  static void Postfix() {
    if (!ZNet.instance.IsServer()) return;
    BiomeManager.ToFile();
    WorldManager.ToFile();
    VegetationManager.ToFile();
    LocationManager.ToFile();
    EventManager.ToFile();
    EnvironmentManager.ToFile();
    ClutterManager.ToFile();
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
      SpawnManager.ToFile();
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
  public static bool IsLoading = false;
  public static void SetupWatcher(string file, Action action) {
    FileSystemWatcher watcher = new(Path.GetDirectoryName(file), Path.GetFileName(file));
    watcher.Changed += (s, e) => action();
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
        if (BiomeManager.TryGetName(number, out var name))
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
      if (BiomeManager.TryGetBiome(biome, out var number))
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