using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), new[] { typeof(ZoneSystem.ZoneLocation) })]
public class GuaranteeLocations
{
  static void GuaranteeStartLocation(ZoneSystem zs, ZoneSystem.ZoneLocation location)
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
  static void Finalizer(ZoneSystem __instance, ZoneSystem.ZoneLocation location)
  {
    GuaranteeStartLocation(__instance, location);
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

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PlaceVegetation))]
public class ClearAreasFromAdjacentZones
{
  static void Prefix(ZoneSystem __instance, Vector2i zoneID, List<ZoneSystem.ClearArea> clearAreas)
  {
    for (var i = zoneID.x - 1; i <= zoneID.x + 1; i++)
    {
      for (var j = zoneID.y - 1; j <= zoneID.y + 1; j++)
      {
        // Current zone alredy handled.
        if (i == zoneID.x && j == zoneID.y) continue;
        // No location in the zone.
        if (!__instance.m_locationInstances.TryGetValue(new(i, j), out var item)) continue;
        // Check for corrupted locations.
        if (item.m_location == null || item.m_location.m_location == null) continue;
        // No clear area in the location.
        if (!item.m_location.m_location.m_clearArea) continue;
        // If fits inside the zone, no need to add it.
        if (item.m_location.m_exteriorRadius < 32f) continue;
        clearAreas.Add(new(item.m_position, item.m_location.m_exteriorRadius));
      }
    }
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GetRandomPointInZone), typeof(Vector2i), typeof(float))]
public class FixGetRandomPointInZone
{
  static Vector3 Postfix(Vector3 result, ZoneSystem __instance, Vector2i zone, float locationRadius)
  {
    // If fits inside the zone, vanilla code works.
    if (locationRadius < 32f) return result;
    // Otherwise hardcode at the zone center to ensure the best fit.
    return __instance.GetZonePos(zone);
  }
}