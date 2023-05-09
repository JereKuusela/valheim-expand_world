using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;

namespace ExpandWorld.Dungeon;

// Dungeons don't have configuration, instead each generator is a prefab.
// So the data is loaded to custom classes and then applied to the prefabs. 
[HarmonyPatch]
public partial class Loader
{
  public static string FileName = "expand_dungeons.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  public static string Pattern = "expand_dungeons*.yaml";

  public static void Initialize()
  {
    Load();
  }

  private static void ToFile()
  {
    // Dungeons don't have configuration so the data must be pulled from locations.
    var dgs = ZoneSystem.instance.m_locations
      .Select(loc => loc.m_prefab ? loc.m_prefab.GetComponentInChildren<DungeonGenerator>() : null!)
      .Where(dg => dg != null)
      .Distinct(new DgComparer()).ToList();
    var yaml = global::ExpandWorld.Data.Serializer().Serialize(dgs.Select(To).ToList());
    File.WriteAllText(FilePath, yaml);
  }

  private static Dictionary<string, FakeDungeonGenerator> FromFile()
  {
    try
    {
      var data = Data.Deserialize<DungeonData>(Data.Read(Pattern), FileName);
      return data.ToDictionary(data => data.name, From);
    }
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.Message);
      ExpandWorld.Log.LogError(e.StackTrace);
    }
    return new();
  }

  public static void Load()
  {
    Spawner.Generators.Clear();
    if (Helper.IsClient()) return;
    if (!Configuration.DataRooms)
    {
      ExpandWorld.Log.LogInfo($"Reloading default dungeon entries).");
      return;
    }
    if (!File.Exists(FilePath))
    {
      ToFile();
      return; // Watcher triggers reload.
    }

    var data = FromFile();
    if (data.Count == 0)
    {
      ExpandWorld.Log.LogWarning($"Failed to load any dungeon data.");
      ExpandWorld.Log.LogInfo($"Reloading default dungeon data.");
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading dungeon data ({data.Count} entries).");
    Spawner.Generators = data;
  }

  public static void SetupWatcher()
  {
    Data.SetupWatcher(Pattern, Load);
  }
}

class DgComparer : IEqualityComparer<DungeonGenerator>
{
  public bool Equals(DungeonGenerator dg1, DungeonGenerator dg2)
  {
    return dg1.name == dg2.name;
  }

  public int GetHashCode(DungeonGenerator dg)
  {
    return dg.name.GetHashCode();
  }
}