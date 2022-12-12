using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
namespace ExpandWorld;

[HarmonyPatch]
public class DungeonManager
{
  public static string FileName = "expand_dungeons.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  public static string Pattern = "expand_dungeons*.yaml";

  private static Dictionary<string, DungeonGenerator> Generators = new();
  public static DungeonGenerator FromData(DungeonData data)
  {
    DungeonGenerator dg = new();
    if (Enum.TryParse<DungeonGenerator.Algorithm>(data.algorithm, true, out var algorithm))
      dg.m_algorithm = algorithm;
    else
      ExpandWorld.Log.LogWarning($"Failed to find dungeon algorithm {data.algorithm}.");
    dg.m_alternativeFunctionality = data.alternative;
    dg.m_campRadiusMax = data.campRadiusMax;
    dg.m_campRadiusMin = data.campRadiusMin;
    dg.m_doorChance = data.doorChance;
    dg.m_doorTypes = data.doorTypes.Select(type => new DungeonGenerator.DoorDef()
    {
      m_chance = type.chance,
      m_connectionType = type.connectionType,
      m_prefab = Data.ToPrefab(type.prefab)
    }).ToList();
    dg.m_maxRooms = data.maxRooms;
    dg.m_minRooms = data.minRooms;
    dg.m_maxTilt = data.maxTilt;
    dg.m_minAltitude = data.minAltitude;
    dg.m_minRequiredRooms = data.minRequiredRooms;
    dg.m_requiredRooms = Data.ToList(data.requiredRooms);
    dg.m_themes = Data.ToEnum<Room.Theme>(data.themes);
    dg.m_tileWidth = data.tileWidth;
    dg.m_spawnChance = data.spawnChance;
    dg.m_perimeterSections = data.perimeterSections;
    dg.m_perimeterBuffer = data.perimeterBuffer;
    dg.m_useCustomInteriorTransform = data.interiorTransform;
    return dg;
  }
  public static DungeonData ToData(DungeonGenerator dg)
  {
    DungeonData data = new();
    data.name = Utils.GetPrefabName(dg.gameObject);
    data.algorithm = dg.m_algorithm.ToString();
    data.alternative = dg.m_alternativeFunctionality;
    data.campRadiusMax = dg.m_campRadiusMax;
    data.campRadiusMin = dg.m_campRadiusMin;
    data.doorChance = dg.m_doorChance;
    data.doorTypes = dg.m_doorTypes.Select(type => new DungeonDoorData()
    {
      chance = type.m_chance,
      connectionType = type.m_connectionType,
      prefab = Utils.GetPrefabName(type.m_prefab)
    }).ToArray();
    data.interiorTransform = dg.m_useCustomInteriorTransform;
    data.maxRooms = dg.m_maxRooms;
    data.maxTilt = dg.m_maxTilt;
    data.minAltitude = dg.m_minAltitude;
    data.minRequiredRooms = dg.m_minRequiredRooms;
    data.minRooms = dg.m_minRooms;
    data.perimeterBuffer = dg.m_perimeterBuffer;
    data.perimeterSections = dg.m_perimeterSections;
    data.requiredRooms = Data.FromList(dg.m_requiredRooms);
    data.spawnChance = dg.m_spawnChance;
    data.themes = Data.FromEnum(dg.m_themes);
    data.tileWidth = dg.m_tileWidth;
    return data;
  }

  public static void ToFile()
  {
    if (!Helper.IsServer() || !Configuration.DataDungeons) return;
    if (File.Exists(FilePath)) return;
    var dgs = ZoneSystem.instance.m_locations
      .Select(loc => loc.m_prefab ? loc.m_prefab.GetComponentInChildren<DungeonGenerator>() : null!)
      .Where(dg => dg != null)
      .Distinct(new DgComparer()).ToList();
    var yaml = Data.Serializer().Serialize(dgs.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
    Configuration.valueDungeonData.Value = yaml;
  }
  public static void FromFile()
  {
    if (!Helper.IsServer()) return;
    var yaml = Configuration.DataDungeons ? Data.Read(Pattern) : "";
    Configuration.valueDungeonData.Value = yaml;
    Set(yaml);
  }
  public static void FromSetting(string yaml)
  {
    //NOTREADY if (Helper.IsClient()) Set(yaml);
  }
  private static void Set(string yaml)
  {
    if (yaml == "" || !Configuration.DataDungeons) return;
    try
    {
      Generators = Data.Deserialize<DungeonData>(yaml, FileName).ToDictionary(data => data.name, FromData);
      ExpandWorld.Log.LogInfo($"Reloading {Generators.Count} dungeon data.");
    }
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.StackTrace);
    }
  }
  public static void SetupWatcher()
  {
    Data.SetupWatcher(Pattern, FromFile);
  }

  public static void Override(DungeonGenerator dg, string name)
  {
    if (!Generators.TryGetValue(name, out var value)) return;
    dg.m_algorithm = value.m_algorithm;
    dg.m_alternativeFunctionality = value.m_alternativeFunctionality;
    dg.m_campRadiusMax = value.m_campRadiusMax;
    dg.m_campRadiusMin = value.m_campRadiusMin;
    dg.m_doorChance = value.m_doorChance;
    dg.m_doorTypes = value.m_doorTypes;
    dg.m_maxRooms = value.m_maxRooms;
    dg.m_minRooms = value.m_minRooms;
    dg.m_maxTilt = value.m_maxTilt;
    dg.m_minAltitude = value.m_minAltitude;
    dg.m_minRequiredRooms = value.m_minRequiredRooms;
    dg.m_requiredRooms = value.m_requiredRooms;
    dg.m_themes = value.m_themes;
    dg.m_tileWidth = value.m_tileWidth;
    dg.m_spawnChance = value.m_spawnChance;
    dg.m_perimeterSections = value.m_perimeterSections;
    dg.m_perimeterBuffer = value.m_perimeterBuffer;
    dg.m_useCustomInteriorTransform = value.m_useCustomInteriorTransform;
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