using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace ExpandWorld;

public class WaterHelper {
  public static WaterVolume[] Get() => UnityEngine.Object.FindObjectsOfType<WaterVolume>();
  public static void SetLevel(WaterVolume obj) {
    if (!obj.m_useGlobalWind) return;
    var position = obj.transform.position;
    position.y = Settings.WaterLevel;
    obj.transform.position = position;
    position = obj.m_waterSurface.transform.position;
    position.y = Settings.WaterLevel;
    obj.m_waterSurface.transform.position = position;
    if (OriginalScale == Vector3.zero) OriginalScale = obj.m_waterSurface.transform.localScale;
    var scale = OriginalScale;
    if (Settings.WaveOnlyHeight) scale.y *= Settings.WaveMultiplier;
    else scale *= Settings.WaveMultiplier;
    obj.m_waterSurface.transform.localScale = scale;
  }
  public static void SetLevel(ZoneSystem obj) {
    if (obj != null) obj.m_waterLevel = Settings.WaterLevel;
  }
  public static void SetLevel(ClutterSystem obj) {
    if (obj != null) obj.m_waterLevel = Settings.WaterLevel - 3f;
  }
  private static Vector3 OriginalScale = Vector3.zero;
  public static void SetWaveSize(WaterVolume obj) {
    if (!obj.m_useGlobalWind) return;
    if (OriginalScale == Vector3.zero) OriginalScale = obj.m_waterSurface.transform.localScale;
    var scale = OriginalScale;
    if (Settings.WaveOnlyHeight) scale.y *= Settings.WaveMultiplier;
    else scale *= Settings.WaveMultiplier;
    obj.m_waterSurface.transform.localScale = scale;
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Awake))]
public class SetZoneSystemWaterLevel {
  static void Postfix(ZoneSystem __instance) => WaterHelper.SetLevel(__instance);
}
[HarmonyPatch(typeof(ClutterSystem), nameof(ClutterSystem.Awake))]
public class SetClutterSystemWaterLevel {
  static void Postfix(ClutterSystem __instance) => WaterHelper.SetLevel(__instance);
}
[HarmonyPatch(typeof(WaterVolume), nameof(WaterVolume.Awake))]
public class SetWaterVolumeWaterLevel {
  static void Postfix(WaterVolume __instance) {
    WaterHelper.SetLevel(__instance);
    WaterHelper.SetWaveSize(__instance);
  }
}
[HarmonyPatch(typeof(WaterVolume), nameof(WaterVolume.CalcWave), new Type[] { typeof(Vector3), typeof(float), typeof(float), typeof(float) })]
public class CalcWave {
  static void Postfix(ref float __result) {
    __result *= Settings.WaveMultiplier;
  }
}
[HarmonyPatch(typeof(WaterVolume), nameof(WaterVolume.GetWaterSurface))]
public class GetWaterSurface {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
        .MatchForward(
             useEnd: false,
             new CodeMatch(OpCodes.Ldc_R4, 10500f))
        .SetAndAdvance( // Replace the fixed meters with a custom function.
            OpCodes.Call,
            Transpilers.EmitDelegate<Func<float>>(
                () => Settings.WorldTotalRadius).operand)
        .InstructionEnumeration();
  }
}
