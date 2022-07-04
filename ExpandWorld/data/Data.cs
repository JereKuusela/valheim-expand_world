
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
    if (File.Exists(Data.VegFile))
      VegetationData.Load(Data.VegFile);
    if (File.Exists(Data.LocFile))
      LocationData.Load(Data.LocFile);
    if (File.Exists(Data.BiomeFile))
      BiomeData.Load(Data.BiomeFile);
    IsLoading = false;
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
public class SaveData {
  static void Postfix() {
    if (!ZNet.instance.IsServer()) return;
    if (!File.Exists(Data.VegFile))
      VegetationData.Save(Data.VegFile);
    if (!File.Exists(Data.LocFile))
      LocationData.Save(Data.LocFile);
    if (!File.Exists(Data.BiomeFile))
      BiomeData.Save(Data.BiomeFile);
  }


}
public static class Data {
  public static string VegFile = Path.Combine(ExpandWorld.ConfigPath, "expand_world_vegetation.yaml");
  public static string LocFile = Path.Combine(ExpandWorld.ConfigPath, "expand_world_locations.yaml");
  public static string BiomeFile = Path.Combine(ExpandWorld.ConfigPath, "expand_world_biomes.yaml");

  public static void SetupWatcher(string file, Action<string> action) {
    FileSystemWatcher watcher = new(Path.GetDirectoryName(file), Path.GetFileName(file));
    watcher.Changed += (s, e) => action(file);
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }
  public static IDeserializer Deserializer() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
  public static ISerializer Serializer() => new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

  public static string[] FromBiomes(Heightmap.Biome biome) {
    List<string> biomes = new();
    if ((biome & Heightmap.Biome.AshLands) > 0) {
      biomes.Add("ashlands");
      biome -= Heightmap.Biome.AshLands;
    }
    if ((biome & Heightmap.Biome.BlackForest) > 0) {
      biomes.Add("blackforest");
      biome -= Heightmap.Biome.BlackForest;
    }
    if ((biome & Heightmap.Biome.DeepNorth) > 0) {
      biomes.Add("deepnorth");
      biome -= Heightmap.Biome.DeepNorth;
    }
    if ((biome & Heightmap.Biome.Meadows) > 0) {
      biomes.Add("meadows");
      biome -= Heightmap.Biome.Meadows;
    }
    if ((biome & Heightmap.Biome.Mistlands) > 0) {
      biomes.Add("mistlands");
      biome -= Heightmap.Biome.Mistlands;
    }
    if ((biome & Heightmap.Biome.Mountain) > 0) {
      biomes.Add("mountain");
      biome -= Heightmap.Biome.Mountain;
    }
    if ((biome & Heightmap.Biome.Ocean) > 0) {
      biomes.Add("ocean");
      biome -= Heightmap.Biome.Ocean;
    }
    if ((biome & Heightmap.Biome.Plains) > 0) {
      biomes.Add("plains");
      biome -= Heightmap.Biome.Plains;
    }
    if ((biome & Heightmap.Biome.Swamp) > 0) {
      biomes.Add("swamp");
      biome -= Heightmap.Biome.Swamp;
    }
    if (biome > 0) biomes.Add(biome.ToString());
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
      var name = biome.ToLower();
      if (name == "ashlands") result += (int)Heightmap.Biome.AshLands;
      else if (name == "blackforest") result += (int)Heightmap.Biome.BlackForest;
      else if (name == "deepnorth") result += (int)Heightmap.Biome.DeepNorth;
      else if (name == "meadows") result += (int)Heightmap.Biome.Meadows;
      else if (name == "mistlands") result += (int)Heightmap.Biome.Mistlands;
      else if (name == "mountain") result += (int)Heightmap.Biome.Mountain;
      else if (name == "ocean") result += (int)Heightmap.Biome.Ocean;
      else if (name == "plains") result += (int)Heightmap.Biome.Plains;
      else if (name == "swamp") result += (int)Heightmap.Biome.Swamp;
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