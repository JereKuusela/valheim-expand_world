using System;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

public static class Helper
{
  public static CodeMatcher Replace(CodeMatcher instructions, float value, Func<float> call)
  {
    return instructions
      .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, value))
      .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate(call).operand);
  }
  public static CodeMatcher Replace(CodeMatcher instructions, sbyte value, Func<int> call)
  {
    return instructions
      .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_S, value))
      .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate(call).operand);
  }
  public static CodeMatcher Replace(CodeMatcher instructions, int value, Func<int> call)
  {
    return instructions
      .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4, value))
      .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate(call).operand);
  }

  public static CodeMatcher ReplaceSeed(CodeMatcher instructions, string name, Func<WorldGenerator, int> call)
  {
    return instructions
      .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(WorldGenerator), name)))
      .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate(call).operand);
  }
  public static CodeMatcher ReplaceSeed(CodeMatcher instructions, string name, Func<WorldGenerator, float> call)
  {
    return instructions
      .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(WorldGenerator), name)))
      .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate(call).operand);
  }
  public static CodeMatcher ReplaceStretch(CodeMatcher instructions, OpCode code)
  {
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
  public static bool IsServer() => ZNet.instance && ZNet.instance.IsServer();
  public static bool IsClient() => ZNet.instance && !ZNet.instance.IsServer();
  public static Vector3 RandomValue(Range<Vector3> range)
  {
    if (range.Uniform)
    {
      var multiplier = UnityEngine.Random.Range(0f, 1f);
      return new(
        range.Min.x + (range.Max.x - range.Min.x) * multiplier,
        range.Min.y + (range.Max.y - range.Min.y) * multiplier,
        range.Min.z + (range.Max.z - range.Min.z) * multiplier);
    }
    else
    {
      return new(
        UnityEngine.Random.Range(range.Min.x, range.Max.x),
        UnityEngine.Random.Range(range.Min.y, range.Max.y),
        UnityEngine.Random.Range(range.Min.z, range.Max.z));
    }
  }

  public static bool IsMultiAxis(Range<Vector3> range)
  {
    // Same value would always return the same value.
    if (range.Min == range.Max) return false;
    // Without uniform, each axis would be separate resulting in multiple possible values.
    if (!range.Uniform) return true;
    return range.Min.normalized != Vector3.one || range.Max.normalized != Vector3.one;
  }
}

