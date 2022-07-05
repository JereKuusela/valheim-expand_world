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
[HarmonyPatch]
public class Heights {
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBaseHeight)), HarmonyPostfix]
  static void Base(WorldGenerator __instance, ref float __result) {
    if (__instance.m_world.m_menu) return;
    var waterLevel = Configuration.WaterLevel / 200f;
    __result = waterLevel + (__result - waterLevel) * Configuration.BaseAltitudeMultiplier;
    __result += Configuration.BaseAltitudeDelta / 200f;
  }
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiomeHeight)), HarmonyPrefix]
  static void BiomeReplace(WorldGenerator __instance, ref Heightmap.Biome biome) {
    if (__instance.m_world.m_menu) return;
    if (BiomeData.BiomeToData.TryGetValue(biome, out var data)) {
      BiomeData.NameToBiome.TryGetValue(data.terrain, out biome);
    }
  }
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiomeHeight)), HarmonyPostfix]
  static void Biome(WorldGenerator __instance, ref float __result) {
    if (__instance.m_world.m_menu) return;
    var waterLevel = Configuration.WaterLevel;
    __result = waterLevel + (__result - waterLevel) * Configuration.AltitudeMultiplier;
    __result += Configuration.AltitudeDelta;
  }
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetAshlandsHeight)), HarmonyPostfix]
  static void Ashlands(WorldGenerator __instance, ref float __result) {
    var waterLevel = Configuration.WaterLevel / 200f;
    __result = waterLevel + (__result - waterLevel) * Configuration.AshlandsAltitudeMultiplier;
    __result += Configuration.AshlandsAltitudeDelta / 200f;
  }
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetDeepNorthHeight)), HarmonyPostfix]
  static void DeepNorth(WorldGenerator __instance, ref float __result) {
    var waterLevel = Configuration.WaterLevel / 200f;
    __result = waterLevel + (__result - waterLevel) * Configuration.DeepNorthAltitudeMultiplier;
    __result += Configuration.DeepNorthAltitudeDelta / 200f;
  }
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetForestHeight)), HarmonyPostfix]
  static void BlackForest(WorldGenerator __instance, float wx, float wy, ref float __result) {
    if (__instance.m_world.m_menu) return;
    var waterLevel = Configuration.WaterLevel / 200f;
    if (__instance.GetBiome(wx, wy) == Heightmap.Biome.BlackForest) {
      __result = waterLevel + (__result - waterLevel) * Configuration.BlackForestAltitudeMultiplier;
      __result += Configuration.BlackForestAltitudeDelta / 200f;
    } else {
      __result = waterLevel + (__result - waterLevel) * Configuration.MistlandsAltitudeMultiplier;
      __result += Configuration.MistlandsAltitudeDelta / 200f;
    }
  }
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetMarshHeight)), HarmonyPostfix]
  static void Swamp(WorldGenerator __instance, ref float __result) {
    var waterLevel = Configuration.WaterLevel / 200f;
    __result = waterLevel + (__result - waterLevel) * Configuration.SwampsAltitudeMultiplier;
    __result += Configuration.SwampsAltitudeDelta / 200f;
  }
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetMeadowsHeight)), HarmonyPostfix]
  static void Meadows(WorldGenerator __instance, ref float __result) {
    var waterLevel = Configuration.WaterLevel / 200f;
    __result = waterLevel + (__result - waterLevel) * Configuration.MeadowsAltitudeMultiplier;
    __result += Configuration.MeadowsAltitudeDelta / 200f;
  }
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetOceanHeight)), HarmonyPostfix]
  static void Ocean(WorldGenerator __instance, ref float __result) {
    var waterLevel = Configuration.WaterLevel / 200f;
    __result = waterLevel + (__result - waterLevel) * Configuration.OceanAltitudeMultiplier;
    __result += Configuration.OceanAltitudeDelta / 200f;
  }
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetPlainsHeight)), HarmonyPostfix]
  static void Plains(WorldGenerator __instance, ref float __result) {
    var waterLevel = Configuration.WaterLevel / 200f;
    __result = waterLevel + (__result - waterLevel) * Configuration.PlainsAltitudeMultiplier;
    __result += Configuration.PlainsAltitudeDelta / 200f;
  }
  [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetSnowMountainHeight)), HarmonyPostfix]
  static void Mountain(WorldGenerator __instance, ref float __result) {
    var waterLevel = Configuration.WaterLevel / 200f;
    __result = waterLevel + (__result - waterLevel) * Configuration.MountainAltitudeMultiplier;
    __result += Configuration.MountainAltitudeDelta / 200f;
  }
}
