
using System;
using System.Linq;
using UnityEngine;

namespace ExpandWorld.Dungeon;

public partial class Loader
{
  public static FakeDungeonGenerator From(DungeonData data)
  {
    FakeDungeonGenerator dg = new();
    if (Enum.TryParse<DungeonGenerator.Algorithm>(data.algorithm, true, out var algorithm))
      dg.m_algorithm = algorithm;
    else
      ExpandWorld.Log.LogWarning($"Failed to find dungeon algorithm {data.algorithm}.");
    dg.name = data.name;
    if (data.bounds == "")
      dg.m_zoneSize = new Vector3(64f, 64f, 64f);
    else if (data.bounds.Contains(","))
      dg.m_zoneSize = Parse.VectorXZY(data.bounds, new Vector3(64f, 64f, 64f));
    else
      dg.m_zoneSize = Parse.Float(data.bounds) * Vector3.one;
    dg.m_alternativeFunctionality = data.alternative || data.roomWeights;
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
    dg.m_excludedRooms = RoomLoading.ParseRooms(data.excludedRooms).ToHashSet();
    dg.m_requiredRooms = RoomLoading.ParseRooms(data.requiredRooms);
    dg.m_themes = Data.ToList(data.themes);
    dg.m_tileWidth = data.tileWidth;
    dg.m_spawnChance = data.spawnChance;
    dg.m_gridSize = data.gridSize;
    dg.m_perimeterSections = data.perimeterSections;
    dg.m_perimeterBuffer = data.perimeterBuffer;
    dg.m_useCustomInteriorTransform = data.interiorTransform;
    if (data.objectSwap != null)
    {
      dg.m_objectSwaps = Spawn.LoadSwaps(data.objectSwap);
      //ExpandWorld.Log.LogDebug($"Loaded {dg.m_objectSwaps.Count} object swaps for {dg.name}.");
    }
    if (data.objectData != null)
      dg.m_objectData = Spawn.LoadData(data.objectData);
    return dg;
  }
  public static DungeonData To(DungeonGenerator dg)
  {
    DungeonData data = new()
    {
      name = Utils.GetPrefabName(dg.gameObject),
      algorithm = dg.m_algorithm.ToString(),
      themes = Data.FromEnum(dg.m_themes),
      interiorTransform = dg.m_useCustomInteriorTransform
    };
    if (dg.m_zoneSize.x == dg.m_zoneSize.y && dg.m_zoneSize.y == dg.m_zoneSize.z)
      data.bounds = Helper.Print(dg.m_zoneSize.x);
    else
      data.bounds = Helper.Print(dg.m_zoneSize);
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
      data.requiredRooms = global::ExpandWorld.Data.FromList(dg.m_requiredRooms);
      data.roomWeights = dg.m_alternativeFunctionality;
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
}
