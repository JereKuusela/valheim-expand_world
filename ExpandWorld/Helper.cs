using System;
using System.Reflection.Emit;
using HarmonyLib;
namespace ExpandWorld;

public static class Helper {
  public static CodeMatcher Replace(CodeMatcher instructions, float value, Func<float> call) {
    return instructions
      .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, value))
      .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate(call).operand);
  }
  public static CodeMatcher Replace(CodeMatcher instructions, sbyte value, Func<int> call) {
    return instructions
      .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, value))
      .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate(call).operand);
  }
  public static CodeMatcher Replace(CodeMatcher instructions, int value, Func<int> call) {
    return instructions
      .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, value))
      .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate(call).operand);
  }

  public static CodeMatcher ReplaceSeed(CodeMatcher instructions, string name, Func<WorldGenerator, int> call) {
    return instructions
      .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(WorldGenerator), name)))
      .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate(call).operand);
  }
  public static CodeMatcher ReplaceSeed(CodeMatcher instructions, string name, Func<WorldGenerator, float> call) {
    return instructions
      .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(WorldGenerator), name)))
      .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate(call).operand);
  }
  public static CodeMatcher ReplaceStretch(CodeMatcher instructions, OpCode code) {
    return instructions
      .MatchForward(false, new CodeMatch(code))
      .Advance(1)
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(Configuration), nameof(Configuration.WorldStretch))))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Div));
  }


  public static float HeightToBaseHeight(float altitude) => altitude / 200f;
  public static float AltitudeToHeight(float altitude) => Configuration.WaterLevel + altitude;
  public static float AltitudeToBaseHeight(float altitude) => HeightToBaseHeight(AltitudeToHeight(altitude));
  public static float BaseHeightToAltitude(float baseHeight) => baseHeight * 200f - Configuration.WaterLevel;
  public static bool IsServer() => ZNet.instance && !ZNet.instance.IsServer();
  public static bool IsClient() => ZNet.instance && ZNet.instance.IsServer();
}

