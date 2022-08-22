using System.Collections.Generic;
using HarmonyLib;

namespace ExpandWorld;

[HarmonyPatch]
public class HeightSeed {
  static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions) {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.ReplaceSeed(matcher, nameof(WorldGenerator.m_offset3), (WorldGenerator instance) => Configuration.HeightSeed ?? instance.m_offset3);
    return matcher.InstructionEnumeration();
  }
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetAshlandsHeight)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> Ashlands(IEnumerable<CodeInstruction> instructions) => Transpile(instructions);
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetForestHeight)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> Forest(IEnumerable<CodeInstruction> instructions) => Transpile(instructions);
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetMeadowsHeight)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> Meadows(IEnumerable<CodeInstruction> instructions) => Transpile(instructions);
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetDeepNorthHeight)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DeepNorth(IEnumerable<CodeInstruction> instructions) => Transpile(instructions);
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetPlainsHeight)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> Plains(IEnumerable<CodeInstruction> instructions) => Transpile(instructions);
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetSnowMountainHeight)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> Mountain(IEnumerable<CodeInstruction> instructions) => Transpile(instructions);
}

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBaseHeight))]
public class BaseHeight {
  static void Postfix(WorldGenerator __instance, ref float __result) {
    if (__instance.m_world.m_menu) return;
    var waterLevel = Helper.HeightToBaseHeight(Configuration.WaterLevel);
    __result = waterLevel + (__result - waterLevel) * Configuration.AltitudeMultiplier;
    __result += Helper.HeightToBaseHeight(Configuration.AltitudeDelta);
  }
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiomeHeight))]
public class BiomeHeight {
  static void Prefix(WorldGenerator __instance, ref float wx, ref float wy, ref Heightmap.Biome biome, ref Heightmap.Biome __state) {
    if (__instance.m_world.m_menu) return;
    wx /= Configuration.WorldStretch;
    wy /= Configuration.WorldStretch;
    __state = biome;
    biome = BiomeManager.GetTerrain(biome);
  }
  static void Postfix(WorldGenerator __instance, Heightmap.Biome __state, ref float __result) {
    if (__instance.m_world.m_menu) return;
    var waterLevel = Configuration.WaterLevel;
    if (BiomeManager.TryGetData(__state, out var data))
      __result = waterLevel + (__result - waterLevel) * data.altitudeMultiplier + data.altitudeDelta;
    if (Configuration.WaterDepthMultiplier != 1f && __result < waterLevel)
      __result = waterLevel + (__result - waterLevel) * Configuration.WaterDepthMultiplier;
  }
}
