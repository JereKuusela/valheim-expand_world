using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GuaranteeLocations {
  static void GuaranteeSpawn(ZoneSystem zs, ZoneSystem.ZoneLocation location) {
    if (location.m_prefabName == Game.instance.m_StartLocation && Count(zs, location) == 0) {
      ExpandWorld.Log.LogInfo($"Forcefully placing {location.m_prefabName} location at the center.");
      var locationRadius = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
      Vector3 randomPointInZone = zs.GetRandomPointInZone(new Vector2i(0, 0), locationRadius);
      zs.RegisterLocation(location, randomPointInZone, false);
    }
  }
  static int Count(ZoneSystem zs, ZoneSystem.ZoneLocation location) => zs.m_locationInstances.Values.Count(loc => loc.m_location.m_prefabName == location.m_prefabName);
  static void AltitudeTweak(ZoneSystem zs, ZoneSystem.ZoneLocation location) {
    if (Count(zs, location) == 0 && (location.m_minAltitude > 0f || location.m_maxAltitude < 500f)) {
      ExpandWorld.Log.LogInfo($"Lowering altitude requirement for {location.m_prefabName} location.");
      var minAltitude = location.m_minAltitude;
      var maxAltitude = location.m_maxAltitude;
      location.m_minAltitude -= 50f;
      location.m_maxAltitude += 100f;
      zs.GenerateLocations(location);
      location.m_minAltitude = minAltitude;
      location.m_maxAltitude = maxAltitude;
    }
  }
  static void Finalizer(ZoneSystem __instance, ZoneSystem.ZoneLocation location) {
    GuaranteeSpawn(__instance, location);
    AltitudeTweak(__instance, location);
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GenerateLocationsQuantity {
  static void Prefix(ZoneSystem.ZoneLocation location, ref int __state) {
    if (location.m_prefabName == Game.instance.m_StartLocation) return;
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