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
    if (BiomeManager.TryGetData(biome, out var data)) {
      __result = data.color;
      return false;
    }
    return true;
  }
}

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiome), new[] { typeof(float), typeof(float) })]
public class GetBiome {
  public static List<WorldData> Data = WorldManager.GetDefault();

  private static float GetOffset(WorldGenerator obj, Heightmap.Biome biome) {
    if (biome == Heightmap.Biome.Mistlands) return obj.m_offset4;
    if (biome == Heightmap.Biome.BlackForest) return obj.m_offset2;
    if (biome == Heightmap.Biome.Plains) return obj.m_offset1;
    return obj.m_offset0;
  }
  private static float ConvertDist(float percent) => percent * Configuration.WorldRadius;

  private static Heightmap.Biome Get(WorldGenerator obj, float wx, float wy) {
    var magnitude = new Vector2(Configuration.WorldStretch * wx, Configuration.WorldStretch * wy).magnitude;
    if (magnitude > Configuration.WorldTotalRadius)
      return Heightmap.Biome.Ocean;
    var height = obj.GetBaseHeight(wx, wy, false) * 200f;
    var num = obj.WorldAngle(wx, wy) * Configuration.WiggleWidth;
    var angle = (Mathf.Atan2(wx, wy) + Mathf.PI) / 2f / Mathf.PI;
    angle += Configuration.DistanceWiggleWidth * Mathf.Sin(magnitude / Configuration.DistanceWiggleLength);
    if (angle < 0f) angle += 1f;
    if (angle >= 1f) angle -= 1f;
    var radius = Configuration.WorldRadius;
    var bx = wx / Configuration.BiomeStretch;
    var by = wy / Configuration.BiomeStretch;

    foreach (var item in Data) {
      if (item.minAltitude > height || item.maxAltitude < height) continue;
      var mag = magnitude;
      var min = ConvertDist(item.minDistance) + (item.wiggle ? num : 0f);
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
      var angleOk = min > max ? (angle >= min || angle < max) : angle >= min && angle < max;
      if (!angleOk) continue;
      var seed = item.seed ?? GetOffset(obj, item._biome);
      if (Mathf.PerlinNoise((seed + bx) * 0.001f, (seed + by) * 0.001f) < 1 - item.amount) continue;
      return item._biome;
    }
    return Heightmap.Biome.Ocean;
  }
  static bool Prefix(WorldGenerator __instance, ref float wx, ref float wy, ref Heightmap.Biome __result) {
    var obj = __instance;
    if (obj.m_world.m_menu) return true;
    wx /= Configuration.WorldStretch;
    wy /= Configuration.WorldStretch;
    if (!Configuration.DataWorld) return true;
    __result = Get(obj, wx, wy);
    return false;
  }
}
