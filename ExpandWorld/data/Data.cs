
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ExpandWorld;

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations))]
public class LoadData {
  public static bool IsLoading = false;
  static List<ZoneSystem.ZoneLocation> Locations = new();
  [HarmonyPriority(Priority.VeryLow)]
  static void Prefix() {
    EnvironmentManager.SetOriginals();
    LocationManager.Locations = null;
    if (!ZNet.instance.IsServer()) return;
    IsLoading = true;
    EnvironmentManager.FromFile();
    BiomeManager.FromFile();
    WorldManager.FromFile();
    VegetationManager.FromFile();
    LocationManager.FromFile();
    EventManager.FromFile();
    SpawnManager.FromFile();
    ClutterManager.FromFile();
    // Little hack to stop the default location setup since it won't work with custom locations.
    Locations = ZoneSystem.instance.m_locations;
    if (Configuration.valueLocationData.Value != "" || !Configuration.DataLocation)
      ZoneSystem.instance.m_locations = new();
  }
  [HarmonyPriority(Priority.VeryLow)]
  static void Postfix() {
    if (!ZNet.instance.IsServer()) return;
    IsLoading = false;
    ZoneSystem.instance.m_locations = Locations;
    // Delayed here since some mods add new locations after SetupLocations.
    LocationManager.Setup();
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
    // Spawn data handled elsewhere.
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
  static bool Prefix() => Data.IsReady;
}
public class Data : MonoBehaviour {
  public static bool IsReady => ExpandWorld.ConfigSync.IsSourceOfTruth || ExpandWorld.ConfigSync.InitialSyncDone;
  public static bool BiomesLoaded = false;

  public static void SetupWatcher(string pattern, Action action) {
    FileSystemWatcher watcher = new(ExpandWorld.ConfigPath, pattern);
    watcher.Created += (s, e) => action();
    watcher.Changed += (s, e) => action();
    watcher.Renamed += (s, e) => action();
    watcher.Deleted += (s, e) => action();
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }
  public static IDeserializer Deserializer() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
    .WithTypeConverter(new FloatConverter()).Build();
  public static IDeserializer DeserializerUnSafe() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
  .WithTypeConverter(new FloatConverter()).IgnoreUnmatchedProperties().Build();
  public static ISerializer Serializer() => new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases()
    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults).WithTypeConverter(new FloatConverter())
      .WithAttributeOverride<Color>(c => c.gamma, new YamlIgnoreAttribute())
      .WithAttributeOverride<Color>(c => c.grayscale, new YamlIgnoreAttribute())
      .WithAttributeOverride<Color>(c => c.linear, new YamlIgnoreAttribute())
      .WithAttributeOverride<Color>(c => c.maxColorComponent, new YamlIgnoreAttribute())
      .Build();

  public static List<T> Deserialize<T>(string raw, string fileName) {
    try {
      return Deserializer().Deserialize<List<T>>(raw);
    } catch (Exception ex1) {
      ExpandWorld.Log.LogError($"{fileName}: {ex1.Message}");
      try {
        return DeserializerUnSafe().Deserialize<List<T>>(raw);
      } catch (Exception) {
        return new();
      }
    }
  }

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
  public static string Read(string pattern) {
    var data = Directory.GetFiles(ExpandWorld.ConfigPath, pattern).Select(name => File.ReadAllText(name));
    return string.Join("\n", data);
  }
  public static void Sanity(ref Color color) {
    if (color.r > 1.0f) color.r /= 255f;
    if (color.g > 1.0f) color.g /= 255f;
    if (color.b > 1.0f) color.b /= 255f;
    if (color.a > 1.0f) color.a /= 255f;
  }
  public static Color Sanity(Color color) {
    if (color.r > 1.0f) color.r /= 255f;
    if (color.g > 1.0f) color.g /= 255f;
    if (color.b > 1.0f) color.b /= 255f;
    if (color.a > 1.0f) color.a /= 255f;
    return color;
  }

}
#nullable disable
public class FloatConverter : IYamlTypeConverter {
  public bool Accepts(Type type) => type == typeof(float);

  public object ReadYaml(IParser parser, Type type) {
    var scalar = (YamlDotNet.Core.Events.Scalar)parser.Current;
    var number = float.Parse(scalar.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
    parser.MoveNext();
    return number;
  }

  public void WriteYaml(IEmitter emitter, object value, Type type) {
    var number = (float)value;
    emitter.Emit(new YamlDotNet.Core.Events.Scalar(number.ToString("0.###", CultureInfo.InvariantCulture)));
  }
}
#nullable enable
