using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExpandWorld;

public class LocationSyncData
{
  public string prefab = "";
  public float exteriorRadius = 0f;
  public bool noBuild = false;
}

public class LocationSyncManager {

  public static void Sync(List<LocationData> data) {
    if (Helper.IsClient()) return;
    var sync = data.Select(loc => new LocationSyncData() {
      prefab = loc.prefab,
      exteriorRadius = loc.exteriorRadius,
      noBuild = loc.noBuild
    }).ToList();
    Configuration.valueLocationData.Value = Data.Serializer().Serialize(sync);
  }
  public static void Load(string yaml) {
    if (Helper.IsServer() || yaml == "") return;
    try {
      var locs = ZoneSystem.instance.m_locations.ToDictionary(loc => loc.m_prefabName);
      var data = Data.Deserialize<LocationSyncData>(yaml, "");
      ExpandWorld.Log.LogInfo($"Reloading {locs.Count} location data.");
      foreach (var item in data) {
        if (!locs.TryGetValue(item.prefab, out var loc)) {
          loc = new();
          loc.m_prefabName = item.prefab;
          loc.m_hash = item.prefab.GetStableHashCode();
          loc.m_prefab = new();
          loc.m_netViews = new();
          loc.m_randomSpawns = new();
          loc.m_location = LocationManager.GetBluePrintLocation(loc.m_prefabName);
          locs[item.prefab] = loc;
        }
        loc.m_exteriorRadius = item.exteriorRadius;
        if (loc.m_location) {
          loc.m_location.m_exteriorRadius = item.exteriorRadius;
          loc.m_location.m_noBuild = item.noBuild;
        }
      }
      ZoneSystem.instance.m_locations = locs.Values.ToList();
      LocationManager.UpdateHashes(); 
    }
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.StackTrace);
    }
  }
}
