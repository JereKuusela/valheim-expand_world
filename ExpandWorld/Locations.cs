using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GuaranteeStart {
  static void Postfix(ZoneSystem __instance, ZoneSystem.ZoneLocation location) {
    if (location.m_prefabName == Game.instance.m_StartLocation && __instance.CountNrOfLocation(location) == 0) {
      ExpandWorld.Log.LogInfo("Forcefully placing start location at the center.");
      var locationRadius = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
      Vector3 randomPointInZone = __instance.GetRandomPointInZone(new Vector2i(0, 0), locationRadius);
      __instance.RegisterLocation(location, randomPointInZone, false);
    }
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GenerateLocationsQuantity {
  static void Prefix(ZoneSystem.ZoneLocation location, ref int __state) {
    __state = location.m_quantity;
    location.m_quantity = Mathf.RoundToInt(location.m_quantity * Configuration.LocationsMultiplier);
  }
  static void Postfix(ZoneSystem.ZoneLocation location, int __state) {
    location.m_quantity = __state;
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GenerateLocationsMin {
  static void Prefix(ZoneSystem.ZoneLocation location, ref float __state) {
    __state = location.m_minDistance;
    location.m_minDistance *= Configuration.WorldRadius / 10000f;
  }
  static void Postfix(ZoneSystem.ZoneLocation location, float __state) {
    location.m_minDistance = __state;
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GenerateLocationsMax {
  static void Prefix(ZoneSystem.ZoneLocation location, ref float __state) {
    __state = location.m_maxDistance;
    location.m_maxDistance *= Configuration.WorldRadius / 10000f;
  }
  static void Postfix(ZoneSystem.ZoneLocation location, float __state) {
    location.m_maxDistance = __state;
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetRandomZone))]
public class GetRandomZone {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
                .MatchForward(
                     useEnd: false,
                     new CodeMatch(OpCodes.Ldc_R4, 10000f))
                .SetAndAdvance( // Replace the fixed meters with a custom function.
                    OpCodes.Call,
                    Transpilers.EmitDelegate<Func<float>>(
                        () => Configuration.WorldRadius).operand)
                .InstructionEnumeration();
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GenerateLocations {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    return new CodeMatcher(instructions)
                .MatchForward(
                     useEnd: false,
                     new CodeMatch(OpCodes.Ldc_R4, 10000f))
                .SetAndAdvance( // Replace the fixed meters with a custom function.
                    OpCodes.Call,
                    Transpilers.EmitDelegate<Func<float>>(
                        () => Configuration.WorldRadius).operand)
                .InstructionEnumeration();
  }
}