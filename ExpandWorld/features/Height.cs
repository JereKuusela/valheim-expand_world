using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiomeHeight))]
public class BiomeHeight
{
  static void Prefix(WorldGenerator __instance, ref Heightmap.Biome biome, ref Heightmap.Biome __state)
  {
    if (__instance.m_world.m_menu) return;
    __state = biome;
    biome = BiomeManager.GetTerrain(biome);
  }
  static void Postfix(WorldGenerator __instance, Heightmap.Biome __state, ref Color mask, ref float __result)
  {
    // TODO: Add patch for minimap generation and modify height while there.
    //if (BiomeManager.TryGetData(biome, out var data))
    //              biomeHeight = Configuration.WaterLevel + (biomeHeight - Configuration.WaterLevel) * data.mapColorMultiplier;
    if (__instance.m_world.m_menu) return;
    if (BiomeManager.TryGetColor(__state, out var color))
      mask = color;

    if (BiomeManager.TryGetData(__state, out var data))
    {
      __result -= World.WaterLevel;
      __result *= data.altitudeMultiplier;
      __result += data.altitudeDelta;
      if (__result < 0f)
      {
        __result *= data.waterDepthMultiplier;
      }
      if (__result > data.maximumAltitude)
        __result = data.maximumAltitude + Mathf.Pow(__result - data.maximumAltitude, data.excessFactor);
      if (__result < data.minimumAltitude)
        __result = data.minimumAltitude - Mathf.Pow(data.minimumAltitude - __result, data.excessFactor);
      __result += World.WaterLevel;
    }
  }
}
