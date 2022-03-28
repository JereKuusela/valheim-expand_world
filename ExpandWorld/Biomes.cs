using System;
using HarmonyLib;
using UnityEngine;
namespace ExpandWorld {

  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiome), new Type[] { typeof(float), typeof(float) })]
  public class GetBiome {
    // Copy paste from the base game code.

    private static Heightmap.Biome Get(WorldGenerator obj, float wx, float wy) {
      float magnitude = new Vector2(wx, wy).magnitude;
      float baseHeight = obj.GetBaseHeight(wx, wy, false);
      float num = obj.WorldAngle(wx, wy) * 100f;
      if (magnitude > Settings.WorldTotalRadius) {
        return Heightmap.Biome.Ocean;
      }
      var min = Settings.AshlandsMin;
      var max = Settings.AshlandsMax;
      var curve = Settings.AshlandsCurvature;
      var curvedMagnitude = new Vector2(wx, wy - curve).magnitude;
      if (wy < 0 && curvedMagnitude > min + curve + num && magnitude < max) {
        return Heightmap.Biome.AshLands;
      }
      if ((double)baseHeight <= 0.02) {
        return Heightmap.Biome.Ocean;
      }
      min = Settings.DeepNorthMin;
      max = Settings.DeepNorthMax;
      curve = Settings.DeepNorthCurvature;
      curvedMagnitude = new Vector2(wx, wy + curve).magnitude;
      if (curvedMagnitude > min + curve + num && magnitude < max) {
        if (baseHeight > 0.4f) {
          return Heightmap.Biome.Mountain;
        }
        return Heightmap.Biome.DeepNorth;
      } else {
        if (baseHeight > 0.4f) {
          return Heightmap.Biome.Mountain;
        }
        min = Settings.SwampMin;
        max = Settings.SwampMax;
        var seed = Settings.UseSwampSeed ? Settings.SwampSeed : obj.m_offset0;
        if (Mathf.PerlinNoise((seed + wx) * 0.001f, (seed + wy) * 0.001f) > 0.6f && magnitude > min && magnitude < max && baseHeight > 0.05f && baseHeight < 0.25f) {
          return Heightmap.Biome.Swamp;
        }
        min = Settings.MistlandsMin;
        max = Settings.MistlandsMax;
        seed = Settings.UseMistlandSeed ? Settings.MistlandsSeed : obj.m_offset4;
        if (Mathf.PerlinNoise((seed + wx) * 0.001f, (seed + wy) * 0.001f) > 0.5f && magnitude > min + num && magnitude < max) {
          return Heightmap.Biome.Mistlands;
        }
        min = Settings.PlainsMin;
        max = Settings.PlainsMax;
        seed = Settings.UsePlainsSeed ? Settings.PlainsSeed : obj.m_offset1;
        if (Mathf.PerlinNoise((seed + wx) * 0.001f, (seed + wy) * 0.001f) > 0.4f && magnitude > min + num && magnitude < max) {
          return Heightmap.Biome.Plains;
        }
        min = Settings.BlackForestMin;
        max = Settings.BlackForestMax;
        seed = Settings.UseBlackForestSeed ? Settings.BlackForestSeed : obj.m_offset2;
        if (Mathf.PerlinNoise((seed + wx) * 0.001f, (seed + wy) * 0.001f) > 0.4f && magnitude > min + num && magnitude < max) {
          return Heightmap.Biome.BlackForest;
        }
        min = Settings.MeadowsMin;
        max = Settings.MeadowsMax;
        if (magnitude > max + num || magnitude < min) {
          return Heightmap.Biome.BlackForest;
        }
        return Heightmap.Biome.Meadows;
      }
    }
    public static bool Prefix(WorldGenerator __instance, float wx, float wy, ref Heightmap.Biome __result) {
      if (!Settings.ModifyBiomes) return true;
      var obj = __instance;
      if (obj.m_world.m_menu) return true;
      __result = Get(obj, wx, wy);
      return false;
    }
  }
}