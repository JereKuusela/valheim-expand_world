using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace ExpandWorld;

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.WorldAngle))]
public class WorldAngle {
  static bool Prefix(float wx, float wy, ref float __result) {
    __result = Mathf.Sin(Mathf.Atan2(wx, wy) * Configuration.WiggleFrequency);
    return false;
  }
}

[HarmonyPatch(typeof(Heightmap), nameof(Heightmap.GetBiomeColor), new[] { typeof(Heightmap.Biome) })]
public class GetBiomeColor {
  static bool Prefix(Heightmap.Biome biome, ref Color32 __result) {
    if (!BiomeManager.TryGetData(biome, out var data)) return true;
    __result = data.color;
    return false;
  }
}

[HarmonyPatch(typeof(Heightmap), nameof(Heightmap.GetBiome))]
public class HeightmapGetBiome {
  private static Dictionary<Heightmap.Biome, float> Weights = new();

  public static Heightmap.Biome GetBiome(Heightmap __instance, float x, float z) {
    var values = Enum.GetValues(typeof(Heightmap.Biome)) as Heightmap.Biome[];
    if (values == null) return Heightmap.Biome.None;
    for (var i = 0; i < values.Length; i++)
      Weights[values[i]] = 0f;
    Weights[__instance.m_cornerBiomes[0]] += __instance.Distance(x, z, 0f, 0f);
    Weights[__instance.m_cornerBiomes[1]] += __instance.Distance(x, z, 1f, 0f);
    Weights[__instance.m_cornerBiomes[2]] += __instance.Distance(x, z, 0f, 1f);
    Weights[__instance.m_cornerBiomes[3]] += __instance.Distance(x, z, 1f, 1f);
    var result = Heightmap.Biome.None;
    var num = -99999f;
    foreach (var kvp in Weights) {
      if (kvp.Value > num) {
        result = kvp.Key;
        num = kvp.Value;
      }
    }
    return result;
  }
  static bool Prefix(Heightmap __instance, Vector3 point, ref Heightmap.Biome __result) {
    if (__instance.m_isDistantLod) return true;
    if (__instance.m_cornerBiomes[0] == __instance.m_cornerBiomes[1] && __instance.m_cornerBiomes[0] == __instance.m_cornerBiomes[2] && __instance.m_cornerBiomes[0] == __instance.m_cornerBiomes[3]) {
      __result = __instance.m_cornerBiomes[0];
      return false;
    }
    var x = point.x;
    var z = point.z;
    __instance.WorldToNormalizedHM(point, out x, out z);
    __result = GetBiome(__instance, x, z);
    return false;
  }
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.Initialize))]
public class ResetBiomeOffsets {
  static void Prefix() {
    GetBiome.Offsets.Clear();
  }
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.Pregenerate))]
public class SetBiomeOffsets {
  [HarmonyPriority(Priority.VeryHigh)]
  static void Prefix(WorldGenerator __instance) {
    if (GetBiome.Offsets.Count > 0) return;
    GetBiome.Offsets[Heightmap.Biome.Swamp] = __instance.m_offset0;
    GetBiome.Offsets[Heightmap.Biome.Plains] = __instance.m_offset1;
    GetBiome.Offsets[Heightmap.Biome.BlackForest] = __instance.m_offset2;
    // Not used in the base game code but might as well reuse the value.
    GetBiome.Offsets[Heightmap.Biome.Meadows] = __instance.m_offset3;
    GetBiome.Offsets[Heightmap.Biome.Mistlands] = __instance.m_offset4;
    GetBiome.Offsets[Heightmap.Biome.AshLands] = (float)UnityEngine.Random.Range(-10000, 10000);
    GetBiome.Offsets[Heightmap.Biome.DeepNorth] = (float)UnityEngine.Random.Range(-10000, 10000);
    GetBiome.Offsets[Heightmap.Biome.Mountain] = (float)UnityEngine.Random.Range(-10000, 10000);
    GetBiome.Offsets[Heightmap.Biome.Ocean] = (float)UnityEngine.Random.Range(-10000, 10000);
  }
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiome), new[] { typeof(float), typeof(float) })]
public class GetBiome {
  public static List<WorldData> Data = WorldManager.GetDefault();
  public static Dictionary<Heightmap.Biome, float> Offsets = new();

  private static float GetOffset(WorldGenerator obj, Heightmap.Biome biome) {
    if (Offsets.TryGetValue(biome, out var value)) return value;
    return obj.m_offset0;
  }
  private static float ConvertDist(float percent) => percent * Configuration.WorldRadius;

  private static Heightmap.Biome Get(WorldGenerator obj, float wx, float wy) {
    var magnitude = new Vector2(Configuration.WorldStretch * wx, Configuration.WorldStretch * wy).magnitude;
    if (magnitude > Configuration.WorldTotalRadius)
      return Heightmap.Biome.Ocean;
    var altitude = Helper.BaseHeightToAltitude(obj.GetBaseHeight(wx, wy, false));
    var num = obj.WorldAngle(wx, wy) * Configuration.WiggleWidth;
    var baseAngle = (Mathf.Atan2(wx, wy) + Mathf.PI) / 2f / Mathf.PI;
    var wiggledAngle = baseAngle + Configuration.DistanceWiggleWidth * Mathf.Sin(magnitude / Configuration.DistanceWiggleLength);
    if (wiggledAngle < 0f) wiggledAngle += 1f;
    if (wiggledAngle >= 1f) wiggledAngle -= 1f;
    var radius = Configuration.WorldRadius;
    var bx = wx / Configuration.BiomeStretch;
    var by = wy / Configuration.BiomeStretch;

    foreach (var item in Data) {
      if (item.minAltitude > altitude || item.maxAltitude < altitude) continue;
      var mag = magnitude;
      var min = ConvertDist(item.minDistance);
      if (min > 0)
        min += (item.wiggleDistance ? num : 0f);
      var max = ConvertDist(item.maxDistance);
      if (item.curveX != 0f || item.curveY != 0f) {
        var curveX = ConvertDist(item.curveX);
        var curveY = ConvertDist(item.curveY);
        mag = new Vector2(Configuration.WorldStretch * wx + curveX, Configuration.WorldStretch * wy + curveY).magnitude;
        min += new Vector2(curveX, curveY).magnitude;
      }
      var distOk = mag > min && (max >= radius || mag < max);
      if (!distOk) continue;
      min = item.minSector;
      max = item.maxSector;
      var angle = item.wiggleSector ? wiggledAngle : baseAngle;
      var angleOk = min > max ? (angle >= min || angle < max) : angle >= min && angle < max;
      if (!angleOk) continue;
      var seed = item._seed ?? GetOffset(obj, item._biomeSeed);
      if (item.amount < 1f && Mathf.PerlinNoise((seed + bx / item.stretch) * 0.001f, (seed + by / item.stretch) * 0.001f) < 1 - item.amount) continue;
      return item._biome;
    }
    return Heightmap.Biome.Ocean;
  }
  static bool Prefix(WorldGenerator __instance, ref float wx, ref float wy, ref Heightmap.Biome __result) {
    if (__instance.m_world.m_menu) return true;
    wx /= Configuration.WorldStretch;
    wy /= Configuration.WorldStretch;
    if (!Configuration.DataWorld) return true;
    __result = Get(__instance, wx, wy);
    return false;
  }
}

[HarmonyPatch(typeof(Heightmap), nameof(Heightmap.ApplyModifiers))]
public class ApplyModifiers {
  private static Dictionary<Heightmap.Biome, float> Weights = new();

  static void Prefix(Heightmap __instance) {
    if (__instance.m_isDistantLod) return;
    if (!BiomeManager.BiomePaint) return;
    var paint = __instance.m_paintMask;
    var biomes = __instance.m_cornerBiomes;
    if (biomes[0] == biomes[1] && biomes[0] == biomes[2] && biomes[0] == biomes[3]) {
      if (!BiomeManager.TryGetData(biomes[0], out var data))
        return;
      if (data.paint.Equals(new Color()))
        return;
      var pixels = new Color[paint.width * paint.height];
      for (var i = 0; i < pixels.Length; i++)
        pixels[i] = data.paint;
      paint.SetPixels(pixels);
    } else {
      var pixels = new Color[paint.width * paint.height];
      for (var z = 0; z < paint.height; z++) {
        for (var x = 0; x < paint.width; x++) {
          var biome = HeightmapGetBiome.GetBiome(__instance, x, z);
          if (!BiomeManager.TryGetData(biome, out var data))
            continue;
          if (data.paint.Equals(new Color()))
            continue;
          pixels[x + z * paint.height] = data.paint;
        }
      }
      paint.SetPixels(pixels);
    }
  }

}
