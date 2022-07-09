using System;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
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

  private  static CancellationTokenSource? cancelTokenSource = null;
  public static Action Debounce(Action func, int milliseconds = 300)
  {
    return () =>
    {
      cancelTokenSource?.Cancel();
      cancelTokenSource = new CancellationTokenSource();

      Task.Delay(milliseconds, cancelTokenSource.Token)
        .ContinueWith(t =>
        {
            if (t.IsCompleted)
              func();
        });
    };
}

  public static float HeightToBaseHeight(float altitude) => altitude / 200f;
  public static float AltitudeToHeight(float altitude) => Configuration.WaterLevel + altitude;
  public static float AltitudeToBaseHeight(float altitude) => HeightToBaseHeight(AltitudeToHeight(altitude));
}

