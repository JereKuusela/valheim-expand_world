using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
namespace ExpandWorld;

public class WorldSizeHelper {
  public static IEnumerable<CodeInstruction> EdgeCheck(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
                .MatchForward(
                     useEnd: false,
                     new CodeMatch(OpCodes.Ldc_R4, 10420f))
                .SetAndAdvance( // Replace the fixed meters with a custom function.
                    OpCodes.Call,
                    Transpilers.EmitDelegate<Func<float>>(
                        () => Configuration.WorldTotalRadius - 80).operand)
                .MatchForward(
                     useEnd: false,
                     new CodeMatch(OpCodes.Ldc_R4, 10500f))
                .SetAndAdvance( // Replace the fixed meters with a custom function.
                    OpCodes.Call,
                    Transpilers.EmitDelegate<Func<float>>(
                        () => Configuration.WorldTotalRadius).operand)
                .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(Ship), nameof(Ship.ApplyEdgeForce))]
public class ApplyEdgeForce {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => WorldSizeHelper.EdgeCheck(instructions);
}

[HarmonyPatch(typeof(Player), nameof(Player.EdgeOfWorldKill))]
public class EdgeOfWorldKill {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => WorldSizeHelper.EdgeCheck(instructions);
}

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetForestFactor))]
public class GetForestFactor {
  static void Postfix(ref float __result) {
    if (Configuration.ForestMultiplier == 0f)
      __result = 100f;
    else
      __result /= Configuration.ForestMultiplier;
  }
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBaseHeight))]
public class GetBaseHeight {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 100000f))
        .Advance(2)
        .SetAndAdvance( // Replace the m_offset0 value with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<WorldGenerator, float>>(
                (WorldGenerator instance) => Configuration.UseOffsetX ? Configuration.OffsetX : instance.m_offset0).operand)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 100000f))
        .Advance(2)
        .SetAndAdvance( // Replace the m_offset1 value with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<WorldGenerator, float>>(
                (WorldGenerator instance) => Configuration.UseOffsetX ? Configuration.OffsetY : instance.m_offset1).operand)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10000f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Configuration.WorldRadius / Configuration.WorldStretch).operand)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10000f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Configuration.WorldRadius / Configuration.WorldStretch).operand)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10500f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Configuration.WorldTotalRadius / Configuration.WorldStretch).operand)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10500f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Configuration.WorldTotalRadius / Configuration.WorldStretch).operand)
        .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetEdgeHeight))]
public class GetEdgeHeight {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10490f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Configuration.WorldTotalRadius - 10f).operand)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10500f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Configuration.WorldTotalRadius).operand)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10000f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Configuration.WorldRadius).operand)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10100f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Configuration.WorldRadius + 100f).operand)
        .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(WaterVolume), nameof(WaterVolume.SetupMaterial))]
public class SetupMaterial {
  public static void Refresh() {
    var objects = UnityEngine.Object.FindObjectsOfType<WaterVolume>();
    foreach (var water in objects) {
      water.m_waterSurface.material.SetFloat("_WaterEdge", Configuration.WorldTotalRadius);
    }
  }
  public static void Prefix(WaterVolume __instance) {
    var obj = __instance;
    obj.m_waterSurface.material.SetFloat("_WaterEdge", Configuration.WorldTotalRadius);
  }
}
[HarmonyPatch(typeof(EnvMan), nameof(EnvMan.UpdateWind))]
public class UpdateWind {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10500f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Configuration.WorldRadius).operand)
        // Removes the subtraction of m_edgeOfWorldWidth (already applied above).
        .SetOpcodeAndAdvance(OpCodes.Nop)
        .SetOpcodeAndAdvance(OpCodes.Nop)
        .SetOpcodeAndAdvance(OpCodes.Nop)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10500f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Configuration.WorldRadius).operand)
        // Removes the subtraction of m_edgeOfWorldWidth (already applied above).
        .SetOpcodeAndAdvance(OpCodes.Nop)
        .SetOpcodeAndAdvance(OpCodes.Nop)
        .SetOpcodeAndAdvance(OpCodes.Nop)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10500f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Configuration.WorldTotalRadius).operand)
        .InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.PlaceRivers))]
public class PlaceRivers {
  static bool Prefix(ref List<WorldGenerator.River> __result) {
    if (Configuration.Rivers) return true;
    __result = new();
    return false;
  }
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.PlaceStreams))]
public class PlaceStreams {
  static bool Prefix(ref List<WorldGenerator.River> __result) {
    if (Configuration.Streams) return true;
    __result = new();
    return false;
  }
}
