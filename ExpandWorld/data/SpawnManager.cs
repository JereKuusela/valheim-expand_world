using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

public class SpawnManager
{
  public static string FileName = "expand_spawns.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  public static string Pattern = "expand_spawns*.yaml";
  public static SpawnSystem.SpawnData FromData(SpawnData data)
  {
    var spawn = new SpawnSystem.SpawnData();
    spawn.m_prefab = Data.ToPrefab(data.prefab);
    if (data.data != "")
    {
      ZPackage pkg = new(data.data);
      ZDO zdo = new();
      Data.Deserialize(zdo, pkg);
      ZDO[spawn] = zdo;
    }
    spawn.m_enabled = data.enabled;
    spawn.m_biome = Data.ToBiomes(data.biome);
    spawn.m_biomeArea = Data.ToBiomeAreas(data.biomeArea);
    spawn.m_maxSpawned = data.maxSpawned;
    spawn.m_spawnInterval = data.spawnInterval;
    spawn.m_spawnChance = data.spawnChance;
    spawn.m_spawnDistance = data.spawnDistance;
    spawn.m_spawnRadiusMin = data.spawnRadiusMin;
    spawn.m_spawnRadiusMax = data.spawnRadiusMax;
    spawn.m_requiredGlobalKey = data.requiredGlobalKey;
    spawn.m_requiredEnvironments = Data.ToList(data.requiredEnvironments);
    spawn.m_groupSizeMin = data.groupSizeMin;
    spawn.m_groupSizeMax = data.groupSizeMax;
    spawn.m_spawnAtDay = data.spawnAtDay;
    spawn.m_spawnAtNight = data.spawnAtNight;
    spawn.m_groupRadius = data.groupRadius;
    spawn.m_minAltitude = data.minAltitude;
    spawn.m_maxAltitude = data.maxAltitude;
    if (spawn.m_minAltitude == -10000f)
      spawn.m_minAltitude = spawn.m_maxAltitude > 0f ? 0f : -1000f;
    spawn.m_minTilt = data.minTilt;
    spawn.m_maxTilt = data.maxTilt;
    spawn.m_inForest = data.inForest;
    spawn.m_outsideForest = data.outsideForest;
    spawn.m_minOceanDepth = data.minOceanDepth;
    spawn.m_maxOceanDepth = data.maxOceanDepth;
    spawn.m_huntPlayer = data.huntPlayer;
    spawn.m_groundOffset = data.groundOffset;
    spawn.m_maxLevel = data.maxLevel;
    spawn.m_minLevel = data.minLevel;
    spawn.m_levelUpMinCenterDistance = data.levelUpMinCenterDistance;
    spawn.m_overrideLevelupChance = data.overrideLevelupChance;
    return spawn;
  }
  public static SpawnData ToData(SpawnSystem.SpawnData spawn)
  {
    SpawnData data = new();
    data.prefab = spawn.m_prefab.name;
    data.enabled = spawn.m_enabled;
    data.biome = Data.FromBiomes(spawn.m_biome);
    data.biomeArea = Data.FromBiomeAreas(spawn.m_biomeArea);
    data.maxSpawned = spawn.m_maxSpawned;
    data.spawnInterval = spawn.m_spawnInterval;
    data.spawnChance = spawn.m_spawnChance;
    data.spawnDistance = spawn.m_spawnDistance;
    data.spawnRadiusMin = spawn.m_spawnRadiusMin;
    data.spawnRadiusMax = spawn.m_spawnRadiusMax;
    data.requiredGlobalKey = spawn.m_requiredGlobalKey;
    data.requiredEnvironments = Data.FromList(spawn.m_requiredEnvironments);
    data.spawnAtDay = spawn.m_spawnAtDay;
    data.spawnAtNight = spawn.m_spawnAtNight;
    data.groupSizeMin = spawn.m_groupSizeMin;
    data.groupSizeMax = spawn.m_groupSizeMax;
    data.groupRadius = spawn.m_groupRadius;
    data.minAltitude = spawn.m_minAltitude;
    data.maxAltitude = spawn.m_maxAltitude;
    data.minTilt = spawn.m_minTilt;
    data.maxTilt = spawn.m_maxTilt;
    data.inForest = spawn.m_inForest;
    data.outsideForest = spawn.m_outsideForest;
    data.minOceanDepth = spawn.m_minOceanDepth;
    data.maxOceanDepth = spawn.m_maxOceanDepth;
    data.huntPlayer = spawn.m_huntPlayer;
    data.groundOffset = spawn.m_groundOffset;
    data.maxLevel = spawn.m_maxLevel;
    data.minLevel = spawn.m_minLevel;
    data.levelUpMinCenterDistance = spawn.m_levelUpMinCenterDistance;
    data.overrideLevelupChance = spawn.m_overrideLevelupChance;
    return data;
  }
  public static bool IsValid(SpawnSystem.SpawnData spawn) => spawn.m_prefab;
  public static string Save()
  {
    var spawnSystem = SpawnSystem.m_instances.FirstOrDefault();
    if (spawnSystem == null) return "";
    var spawns = spawnSystem.m_spawnLists.SelectMany(s => s.m_spawners);
    var yaml = Data.Serializer().Serialize(spawns.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
    return yaml;
  }
  public static void ToFile()
  {
    if (!Helper.IsServer() || !Configuration.DataSpawns) return;
    if (File.Exists(FilePath)) return;
    var yaml = Save();
    Configuration.valueSpawnData.Value = yaml;
  }
  public static void FromFile()
  {
    if (!Helper.IsServer()) return;
    var yaml = Configuration.DataSpawns ? Data.Read(Pattern) : "";
    Configuration.valueSpawnData.Value = yaml;
    Set(yaml);
  }
  public static void FromSetting(string yaml)
  {
    if (Helper.IsClient()) Set(yaml);
  }
  private static void Set(string yaml)
  {
    if (yaml == "" || !Configuration.DataSpawns) return;
    try
    {
      ZDO.Clear();
      var data = Data.Deserialize<SpawnData>(yaml, FileName)
        .Select(FromData).Where(IsValid).ToList();
      if (data.Count == 0)
      {
        ExpandWorld.Log.LogWarning($"Failed to load any spawn data.");
        return;
      }
      ExpandWorld.Log.LogInfo($"Reloading {data.Count} spawn data.");
      HandleSpawnData.Override = data;
      foreach (var system in SpawnSystem.m_instances)
      {
        system.m_spawnLists.Clear();
        system.m_spawnLists.Add(new() { m_spawners = data });
      }
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

  public static Dictionary<SpawnSystem.SpawnData, ZDO> ZDO = new();

}


[HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Spawn))]
public class SpawnZDO
{
  static void Prefix(SpawnSystem.SpawnData critter, Vector3 spawnPoint)
  {
    if (!SpawnManager.ZDO.TryGetValue(critter, out var data)) return;
    if (!critter.m_prefab.TryGetComponent<ZNetView>(out var view)) return;
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(spawnPoint);
    Data.CopyData(data.Clone(), ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = Quaternion.identity;
    ZNetView.m_initZDO.m_type = view.m_type;
    ZNetView.m_initZDO.m_distant = view.m_distant;
    ZNetView.m_initZDO.m_persistent = view.m_persistent;
    ZNetView.m_initZDO.m_prefab = view.GetPrefabName().GetStableHashCode();
    ZNetView.m_initZDO.m_dataRevision = 1;
  }
}