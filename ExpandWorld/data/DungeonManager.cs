using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

[HarmonyPatch]
public class DungeonManager
{
  public static string FileName = "expand_dungeons.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  public static string Pattern = "expand_dungeons*.yaml";

  public static Dictionary<string, DungeonGenerator> Generators = new();
  private static Dictionary<string, DungeonData> DungeonData = new();
  // To make them case insensitive.
  private static Dictionary<string, string> RoomNames = new();
  public static DungeonGenerator FromData(DungeonData data)
  {
    var obj = new GameObject(data.name);
    var dg = obj.AddComponent<DungeonGenerator>();
    if (Enum.TryParse<DungeonGenerator.Algorithm>(data.algorithm, true, out var algorithm))
      dg.m_algorithm = algorithm;
    else
      ExpandWorld.Log.LogWarning($"Failed to find dungeon algorithm {data.algorithm}.");
    DungeonData[data.name] = data;
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
    dg.m_requiredRooms = Data.ToList(data.requiredRooms).Select(s => RoomNames.TryGetValue(s, out var name) ? name : s).ToList();
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
    data.themes = Data.FromEnum(dg.m_themes);
    data.interiorTransform = dg.m_useCustomInteriorTransform;
    if (dg.m_algorithm == DungeonGenerator.Algorithm.Dungeon)
    {
      data.doorChance = dg.m_doorChance;
      data.doorTypes = dg.m_doorTypes.Select(type => new DungeonDoorData()
      {
        chance = type.m_chance,
        connectionType = type.m_connectionType,
        prefab = Utils.GetPrefabName(type.m_prefab)
      }).ToArray();
      data.maxRooms = dg.m_maxRooms;
      data.minRequiredRooms = dg.m_minRequiredRooms;
      data.minRooms = dg.m_minRooms;
      data.requiredRooms = Data.FromList(dg.m_requiredRooms);
      data.alternative = dg.m_alternativeFunctionality;
    }
    if (dg.m_algorithm == DungeonGenerator.Algorithm.CampRadial)
    {
      data.campRadiusMax = dg.m_campRadiusMax;
      data.campRadiusMin = dg.m_campRadiusMin;
      data.minRooms = dg.m_minRooms;
      data.maxRooms = dg.m_maxRooms;
      data.maxTilt = dg.m_maxTilt;
      data.perimeterBuffer = dg.m_perimeterBuffer;
      data.perimeterSections = dg.m_perimeterSections;
      data.minAltitude = dg.m_minAltitude;
    }
    if (dg.m_algorithm == DungeonGenerator.Algorithm.CampGrid)
    {
      data.maxTilt = dg.m_maxTilt;
      data.tileWidth = dg.m_tileWidth;
      data.gridSize = dg.m_gridSize;
      data.spawnChance = dg.m_spawnChance;
    }
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
  }
  public static void FromFile()
  {
    if (!Helper.IsServer()) return;
    var yaml = Configuration.DataDungeons ? Data.Read(Pattern) : "";
    Set(yaml);
  }
  private static void Set(string yaml)
  {
    DungeonData.Clear();
    if (yaml == "" || !Configuration.DataDungeons) return;
    try
    {
      RoomNames = DungeonDB.instance.m_rooms.ToDictionary(room => room.m_room.name.ToLowerInvariant(), room => room.m_room.name);
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
    dg.gameObject.name = name;
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
  [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.SetupAvailableRooms)), HarmonyPostfix]
  public static void SetupAvailableRooms(DungeonGenerator __instance)
  {
    var name = Utils.GetPrefabName(__instance.gameObject);
    if (!DungeonData.TryGetValue(name, out var data)) return;
    var excludedRooms = Parse.Split(data.excludedRooms).Select(s => s.ToLowerInvariant()).ToHashSet();
    if (excludedRooms.Count == 0) return;
    DungeonGenerator.m_availableRooms = DungeonGenerator.m_availableRooms.Where(room => !excludedRooms.Contains(room.m_room.gameObject.name.ToLowerInvariant())).ToList();

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