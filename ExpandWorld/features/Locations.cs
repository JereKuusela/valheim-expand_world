using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GuaranteeLocations
{
  static void GuaranteeSpawn(ZoneSystem zs, ZoneSystem.ZoneLocation location)
  {
    if (location.m_prefabName == Game.instance.m_StartLocation && Count(zs, location) == 0)
    {
      ExpandWorld.Log.LogInfo($"Forcefully placing {location.m_prefabName} location at the center.");
      var locationRadius = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
      Vector3 randomPointInZone = zs.GetRandomPointInZone(new Vector2i(0, 0), locationRadius);
      zs.RegisterLocation(location, randomPointInZone, false);
    }
  }
  static int Count(ZoneSystem zs, ZoneSystem.ZoneLocation location) => zs.m_locationInstances.Values.Count(loc => loc.m_location.m_prefabName == location.m_prefabName);
  static void AltitudeTweak(ZoneSystem zs, ZoneSystem.ZoneLocation location)
  {
    var shouldExist = location.m_enable && location.m_quantity > 0 && Configuration.LocationsMultiplier > 0f;
    var exists = Count(zs, location) > 0;
    var canTweak = location.m_minAltitude > 0f || location.m_maxAltitude < 500f;
    if (shouldExist && !exists && canTweak)
    {
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
  static void Finalizer(ZoneSystem __instance, ZoneSystem.ZoneLocation location)
  {
    GuaranteeSpawn(__instance, location);
    AltitudeTweak(__instance, location);
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GenerateLocationsQuantity
{
  static void Prefix(ZoneSystem.ZoneLocation location, ref int __state)
  {
    __state = location.m_quantity;
    if (location.m_prefabName == Game.instance.m_StartLocation) return;
    location.m_quantity = Mathf.RoundToInt(location.m_quantity * Configuration.LocationsMultiplier);
  }
  static void Postfix(ZoneSystem.ZoneLocation location, int __state)
  {
    location.m_quantity = __state;
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GenerateLocationsMin
{
  static void Prefix(ZoneSystem.ZoneLocation location, ref float __state)
  {
    __state = location.m_minDistance;
    if (location.m_minDistance > 1f)
      location.m_minDistance *= Configuration.WorldRadius / 10000f;
    else
      location.m_minDistance *= Configuration.WorldRadius;
  }
  static void Postfix(ZoneSystem.ZoneLocation location, float __state)
  {
    location.m_minDistance = __state;
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GenerateLocationsMax
{
  static void Prefix(ZoneSystem.ZoneLocation location, ref float __state)
  {
    __state = location.m_maxDistance;
    if (location.m_maxDistance > 1f)
      location.m_maxDistance *= Configuration.WorldRadius / 10000f;
    else
      location.m_maxDistance *= Configuration.WorldRadius;
  }
  static void Postfix(ZoneSystem.ZoneLocation location, float __state)
  {
    location.m_maxDistance = __state;
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetRandomZone))]
public class GetRandomZone
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.Replace(matcher, 10000f, () => Configuration.WorldRadius);
    return matcher.InstructionEnumeration();
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GenerateLocations
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.Replace(matcher, 10000f, () => Configuration.WorldRadius);
    return matcher.InstructionEnumeration();
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PlaceLocations))]
public class PlaceLocationsFailsafe
{
  static bool Prefix(ZoneSystem __instance, Vector2i zoneID)
  {
    if (__instance.m_locationInstances.TryGetValue(zoneID, out var locationInstance))
      return locationInstance.m_location != null && locationInstance.m_location.m_prefab;
    return true;
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PlaceZoneCtrl))]
public class PlaceZoneCtrl
{
  static bool Prefix() => Configuration.ZoneSpawners;
}