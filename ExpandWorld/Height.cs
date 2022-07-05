using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ExpandWorld;

[HarmonyPatch]
public class HeightSeed {
  static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
         .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(WorldGenerator), nameof(WorldGenerator.m_offset3))))
         .Repeat(matcher => matcher
           .SetAndAdvance(
             OpCodes.Call,
             Transpilers.EmitDelegate<Func<WorldGenerator, float>>(
                 (WorldGenerator instance) => Configuration.UseHeightSeed ? Configuration.HeightSeed : instance.m_offset3).operand)
         )
         .InstructionEnumeration();
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
    var waterLevel = Configuration.WaterLevel / 200f;
    __result = waterLevel + (__result - waterLevel) * Configuration.BaseAltitudeMultiplier;
    __result += Configuration.BaseAltitudeDelta / 200f;
  }
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBaseHeight))]
public class BiomeHeight {
  static void Prefix(WorldGenerator __instance, ref Heightmap.Biome biome) {
    if (__instance.m_world.m_menu) return;
    if (BiomeData.BiomeToData.TryGetValue(biome, out var data)) {
      BiomeData.NameToBiome.TryGetValue(data.terrain, out biome);
    }
  }
  static void Postfix(WorldGenerator __instance, Heightmap.Biome biome, ref float __result) {
    if (__instance.m_world.m_menu) return;
    var waterLevel = Configuration.WaterLevel;
    if (BiomeData.BiomeToData.TryGetValue(biome, out var data)) {
      __result = waterLevel + (__result - waterLevel) * data.altitudeMultiplier;
      __result += data.altitudeDelta;
    }
    __result = waterLevel + (__result - waterLevel) * Configuration.AltitudeMultiplier;
    __result += Configuration.AltitudeDelta;
  }
}
