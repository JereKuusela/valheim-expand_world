using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace ExpandWorld;
public class HeightHelper {
  public static IEnumerable<CodeInstruction> HeightSeed(IEnumerable<CodeInstruction> instructions) {
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
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetAshlandsHeight))]
public class GetAshlandsHeight {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HeightHelper.HeightSeed(instructions);
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetDeepNorthHeight))]
public class GetDeepNorthHeight {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HeightHelper.HeightSeed(instructions);
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetForestHeight))]
public class GetForestHeight {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HeightHelper.HeightSeed(instructions);
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetMeadowsHeight))]
public class GetMeadowsHeight {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HeightHelper.HeightSeed(instructions);
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetPlainsHeight))]
public class GetPlainsHeight {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HeightHelper.HeightSeed(instructions);
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetSnowMountainHeight))]
public class GetSnowMountainHeight {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => HeightHelper.HeightSeed(instructions);
}
