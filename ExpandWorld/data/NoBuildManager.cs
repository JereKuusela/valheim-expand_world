using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

public class NoBuildData
{
  public float X;
  public float Z;
  public float radius;
}

public class NoBuildManager
{

  public static void UpdateData()
  {
    if (ZoneSystem.instance.m_locationInstances.Count == 0) return;
    var locations = ZoneSystem.instance.m_locationInstances.Values.Where(loc => loc.m_location?.m_location?.m_noBuild == true);
    var data = locations.Select(loc => new NoBuildData()
    {
      X = loc.m_position.x,
      Z = loc.m_position.z,
      radius = loc.m_location.m_location.GetMaxRadius()
    }).ToList();
    Configuration.valueNoBuildData.Value = Data.Serializer().Serialize(data);
  }
  private static Dictionary<Vector2i, NoBuildData> NoBuild = new();
  public static bool IsInsideNoBuild(Vector3 point)
  {
    var zs = ZoneSystem.instance;
    var zone = zs.GetZone(point);
    for (var i = zone.x - 1; i <= zone.x + 1; i++)
    {
      for (var j = zone.y - 1; j <= zone.y + 1; j++)
      {
        if (!NoBuild.TryGetValue(new Vector2i(i, j), out var noBuild)) continue;
        if (Utils.DistanceXZ(new(noBuild.X, 0, noBuild.Z), point) < noBuild.radius)
          return true;
      }
    }
    return false;
  }
  public static void Load(string yaml)
  {
    ExpandWorld.Log.LogInfo($"Reloading {yaml} no build data.");
    if (yaml == "") return;
    try
    {
      var data = Data.Deserialize<NoBuildData>(yaml, "");
      ExpandWorld.Log.LogInfo($"Reloading {data.Count} no build data.");
      NoBuild = data.ToDictionary(data => ZoneSystem.instance.GetZone(new(data.X, 0, data.Z)));
    }
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.StackTrace);
    }
  }
}

[HarmonyPatch(typeof(Location), nameof(Location.IsInsideNoBuildLocation))]
public class IsInsideNoBuildLocation
{

  static bool Prefix(Vector3 point, ref bool __result)
  {
    __result = NoBuildManager.IsInsideNoBuild(point);
    return false;
  }
}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Load))]
public class ZoneSystemLoad
{
  static void Postfix()
  {
    if (Helper.IsClient()) return;
    NoBuildManager.UpdateData();
  }
}