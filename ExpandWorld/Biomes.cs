using System;
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

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiome), new[] { typeof(float), typeof(float) })]
public class GetBiome {
  // Copy paste from the base game code.

  private static Heightmap.Biome Get(WorldGenerator obj, float wx, float wy) {
    var magnitude = new Vector2(Configuration.WorldStretch * wx, Configuration.WorldStretch * wy).magnitude;
    var baseHeight = obj.GetBaseHeight(wx, wy, false);
    var num = obj.WorldAngle(wx, wy) * Configuration.WiggleWidth;
    var angle = 50f * (Mathf.Atan2(wx, wy) + Mathf.PI) / Mathf.PI;
    angle += Configuration.DistanceWiggleWidth * Mathf.Sin(magnitude / Configuration.DistanceWiggleLength);
    var radius = Configuration.WorldRadius;
    if (magnitude > Configuration.WorldTotalRadius || baseHeight <= 0.02f) {
      return Heightmap.Biome.Ocean;
    }
    var min = Configuration.AshlandsMin;
    var max = Configuration.AshlandsMax;
    var curve = Configuration.AshlandsCurvature;
    var curvedMagnitude = new Vector2(Configuration.WorldStretch * wx, Configuration.WorldStretch * wy - curve).magnitude;
    float amin = Configuration.AshlandsSectorMin;
    float amax = Configuration.AshlandsSectorMax;
    var distOk = curvedMagnitude > min + curve + num && (max >= radius || magnitude < max);
    var angleOk = amin > amax ? (angle >= amin || angle < amax) : angle >= amin && angle < amax;
    if (angleOk && distOk) {
      return Heightmap.Biome.AshLands;
    }
    var mountainsHeight = Configuration.MountainsAltitudeMin / 200f;
    if (baseHeight > mountainsHeight) {
      return Heightmap.Biome.Mountain;
    }
    min = Configuration.DeepNorthMin;
    max = Configuration.DeepNorthMax;
    curve = Configuration.DeepNorthCurvature;
    curvedMagnitude = new Vector2(Configuration.WorldStretch * wx, Configuration.WorldStretch * wy + curve).magnitude;
    amin = Configuration.DeepNorthSectorMin;
    amax = Configuration.DeepNorthSectorMax;
    distOk = curvedMagnitude > min + curve + num && (max >= radius || magnitude < max);
    angleOk = amin > amax ? (angle >= amin || angle < amax) : angle >= amin && angle < amax;
    if (angleOk && distOk) {
      return Heightmap.Biome.DeepNorth;
    }
    min = Configuration.MountainMin;
    max = Configuration.MountainMax;
    amin = Configuration.MountainSectorMin;
    amax = Configuration.MountainSectorMax;
    distOk = magnitude > min && (max >= radius || magnitude < max);
    angleOk = amin > amax ? (angle >= amin || angle < amax) : angle >= amin && angle < amax;
    if (angleOk && distOk) {
      return Heightmap.Biome.Mountain;
    }
    min = Configuration.SwampMin;
    max = Configuration.SwampMax;
    amin = Configuration.SwampSectorMin;
    amax = Configuration.SwampSectorMax;
    distOk = magnitude > min && (max >= radius || magnitude < max);
    angleOk = amin > amax ? (angle >= amin || angle < amax) : angle >= amin && angle < amax;
    var seed = Configuration.UseSwampSeed ? Configuration.SwampSeed : obj.m_offset0;
    if (angleOk && Mathf.PerlinNoise((seed + wx) * 0.001f, (seed + wy) * 0.001f) > Configuration.SwampAmount && distOk && baseHeight > 0.05f && baseHeight < 0.25f) {
      return Heightmap.Biome.Swamp;
    }
    min = Configuration.MistlandsMin;
    max = Configuration.MistlandsMax;
    amin = Configuration.MistlandsSectorMin;
    amax = Configuration.MistlandsSectorMax;
    distOk = magnitude > min + num && (max >= radius || magnitude < max);
    angleOk = amin > amax ? (angle >= amin || angle < amax) : angle >= amin && angle < amax;
    seed = Configuration.UseMistlandSeed ? Configuration.MistlandsSeed : obj.m_offset4;
    if (angleOk && Mathf.PerlinNoise((seed + wx) * 0.001f, (seed + wy) * 0.001f) > Configuration.MistlandsAmount && distOk) {
      return Heightmap.Biome.Mistlands;
    }
    min = Configuration.PlainsMin;
    max = Configuration.PlainsMax;
    amin = Configuration.PlainsSectorMin;
    amax = Configuration.PlainsSectorMax;
    distOk = magnitude > min + num && (max >= radius || magnitude < max);
    angleOk = amin > amax ? (angle >= amin || angle < amax) : angle >= amin && angle < amax;
    seed = Configuration.UsePlainsSeed ? Configuration.PlainsSeed : obj.m_offset1;
    if (angleOk && Mathf.PerlinNoise((seed + wx) * 0.001f, (seed + wy) * 0.001f) > Configuration.PlainsAmount && distOk) {
      return Heightmap.Biome.Plains;
    }
    min = Configuration.BlackForestMin;
    max = Configuration.BlackForestMax;
    amin = Configuration.BlackForestSectorMin;
    amax = Configuration.BlackForestSectorMax;
    distOk = magnitude > min + num && (max >= radius || magnitude < max);
    angleOk = amin > amax ? (angle >= amin || angle < amax) : angle >= amin && angle < amax;
    seed = Configuration.UseBlackForestSeed ? Configuration.BlackForestSeed : obj.m_offset2;
    if (angleOk && Mathf.PerlinNoise((seed + wx) * 0.001f, (seed + wy) * 0.001f) > Configuration.BlackForestAmount && distOk) {
      return Heightmap.Biome.BlackForest;
    }
    min = Configuration.MeadowsMin;
    max = Configuration.MeadowsMax;
    amin = Configuration.MeadowsSectorMin;
    amax = Configuration.MeadowsSectorMax;
    distOk = magnitude > min + num && (max >= radius || magnitude < max);
    angleOk = amin > amax ? (angle >= amin || angle < amax) : angle >= amin && angle < amax;
    if (angleOk && distOk) return Heightmap.Biome.Meadows;
    if (Enum.TryParse<Heightmap.Biome>(Configuration.DefaultBiome, true, out var biome))
      return biome;
    return Heightmap.Biome.BlackForest;
  }
  static bool Prefix(WorldGenerator __instance, ref float wx, ref float wy, ref Heightmap.Biome __result) {
    var obj = __instance;
    if (obj.m_world.m_menu) return true;
    wx /= Configuration.WorldStretch;
    wy /= Configuration.WorldStretch;
    if (!Configuration.ModifyBiomes) return true;
    __result = Get(obj, wx, wy);
    return false;
  }
}
