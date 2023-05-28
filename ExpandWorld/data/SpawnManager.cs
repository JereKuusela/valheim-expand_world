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
  public static Dictionary<SpawnSystem.SpawnData, List<BlueprintObject>> Objects = new();
  public static SpawnSystem.SpawnData FromData(SpawnData data)
  {
    var spawn = new SpawnSystem.SpawnData
    {
      m_prefab = Data.ToPrefab(data.prefab),
      m_enabled = data.enabled,
      m_biome = Data.ToBiomes(data.biome),
      m_biomeArea = Data.ToBiomeAreas(data.biomeArea),
      m_maxSpawned = data.maxSpawned,
      m_spawnInterval = data.spawnInterval,
      m_spawnChance = data.spawnChance,
      m_spawnDistance = data.spawnDistance,
      m_spawnRadiusMin = data.spawnRadiusMin,
      m_spawnRadiusMax = data.spawnRadiusMax,
      m_requiredGlobalKey = data.requiredGlobalKey,
      m_requiredEnvironments = Data.ToList(data.requiredEnvironments),
      m_groupSizeMin = data.groupSizeMin,
      m_groupSizeMax = data.groupSizeMax,
      m_spawnAtDay = data.spawnAtDay,
      m_spawnAtNight = data.spawnAtNight,
      m_groupRadius = data.groupRadius,
      m_minAltitude = data.minAltitude,
      m_maxAltitude = data.maxAltitude,
      m_minTilt = data.minTilt,
      m_maxTilt = data.maxTilt,
      m_inForest = data.inForest,
      m_outsideForest = data.outsideForest,
      m_minOceanDepth = data.minOceanDepth,
      m_maxOceanDepth = data.maxOceanDepth,
      m_huntPlayer = data.huntPlayer,
      m_groundOffset = data.groundOffset,
      m_maxLevel = data.maxLevel,
      m_minLevel = data.minLevel,
      m_levelUpMinCenterDistance = data.levelUpMinCenterDistance,
      m_overrideLevelupChance = data.overrideLevelupChance
    };
    if (spawn.m_minAltitude == -10000f)
      spawn.m_minAltitude = spawn.m_maxAltitude > 0f ? 0f : -1000f;
    if (data.data != "")
    {
      ZPackage pkg = new(data.data);
      ZDO zdo = new();
      Data.Deserialize(zdo, pkg);
      ZDO[spawn] = zdo;
    }
    if (data.objects != null)
    {
      Objects[spawn] = data.objects.Select(s => Parse.Split(s)).Select(split => new BlueprintObject(
        split[0],
        Parse.VectorXZY(split, 1),
        Quaternion.identity,
        Vector3.one,
        Data.ToZDO(split.Length > 5 ? split[5] : ""),
        Parse.Float(split, 4, 1f)
      )).ToList();
    }
    return spawn;
  }
  public static SpawnData ToData(SpawnSystem.SpawnData spawn)
  {
    SpawnData data = new()
    {
      prefab = spawn.m_prefab.name,
      enabled = spawn.m_enabled,
      biome = Data.FromBiomes(spawn.m_biome),
      biomeArea = Data.FromBiomeAreas(spawn.m_biomeArea),
      maxSpawned = spawn.m_maxSpawned,
      spawnInterval = spawn.m_spawnInterval,
      spawnChance = spawn.m_spawnChance,
      spawnDistance = spawn.m_spawnDistance,
      spawnRadiusMin = spawn.m_spawnRadiusMin,
      spawnRadiusMax = spawn.m_spawnRadiusMax,
      requiredGlobalKey = spawn.m_requiredGlobalKey,
      requiredEnvironments = Data.FromList(spawn.m_requiredEnvironments),
      spawnAtDay = spawn.m_spawnAtDay,
      spawnAtNight = spawn.m_spawnAtNight,
      groupSizeMin = spawn.m_groupSizeMin,
      groupSizeMax = spawn.m_groupSizeMax,
      groupRadius = spawn.m_groupRadius,
      minAltitude = spawn.m_minAltitude,
      maxAltitude = spawn.m_maxAltitude,
      minTilt = spawn.m_minTilt,
      maxTilt = spawn.m_maxTilt,
      inForest = spawn.m_inForest,
      outsideForest = spawn.m_outsideForest,
      minOceanDepth = spawn.m_minOceanDepth,
      maxOceanDepth = spawn.m_maxOceanDepth,
      huntPlayer = spawn.m_huntPlayer,
      groundOffset = spawn.m_groundOffset,
      maxLevel = spawn.m_maxLevel,
      minLevel = spawn.m_minLevel,
      levelUpMinCenterDistance = spawn.m_levelUpMinCenterDistance,
      overrideLevelupChance = spawn.m_overrideLevelupChance
    };
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
    HandleSpawnData.Override = null;
    ZDO.Clear();
    Objects.Clear();
    if (yaml == "" || !Configuration.DataSpawns) return;
    try
    {
      var data = Data.Deserialize<SpawnData>(yaml, FileName)
        .Select(FromData).Where(IsValid).ToList();
      if (data.Count == 0)
      {
        ExpandWorld.Log.LogWarning($"Failed to load any spawn data.");
        return;
      }
      ExpandWorld.Log.LogInfo($"Reloading spawn data ({data.Count} entries).");
      HandleSpawnData.Override = data;
      foreach (var system in SpawnSystem.m_instances)
      {
        system.m_spawnLists.Clear();
        system.m_spawnLists.Add(new() { m_spawners = data });
      }
    }
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.Message);
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

  private static string PrefabOverride(string dummy, string prefab)
  {
    return prefab;
  }
  static ZDO? DataOverride(ZDO? zdo, string source, string prefab) => zdo;
  static void Postfix(SpawnSystem.SpawnData critter, Vector3 spawnPoint)
  {
    if (!SpawnManager.Objects.TryGetValue(critter, out var objects)) return;
    foreach (var obj in objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      Spawn.BPO("", obj, spawnPoint, Quaternion.identity, DataOverride, PrefabOverride, null);
    }
  }
}
