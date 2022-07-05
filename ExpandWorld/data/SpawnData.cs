using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace ExpandWorld;

public class SpawnData {
  public string prefab = "";
  public bool enabled = true;
  public string[] biome = new string[0];
  public string[] biomeArea = new string[0];
  public float spawnChance = 100f;
  public int maxSpawned = 1;
  public float spawnInterval = 0f;
  public int maxLevel = 1;
  public int minLevel = 1;
  public string requiredGlobalKey = "";
  public string[] requiredEnvironments = new string[0];
  public float spawnDistance = 10f;
  public float spawnRadiusMin = 0f;
  public float spawnRadiusMax = 0f;
  public int groupSizeMin = 1;
  public int groupSizeMax = 1;
  public float groupRadius = 3f;
  public float minAltitude = -1000f;
  public float maxAltitude = 1000f;
  public float minTilt = 0f;
  public float maxTilt = 35f;
  public bool inForest = true;
  public bool outsideForest = true;
  public float minOceanDepth = 0f;
  public float maxOceanDepth = 0f;
  public bool huntPlayer = false;
  public float groundOffset = 0.5f;
  public float levelUpMinCenterDistance = 0f;


  public static SpawnSystem.SpawnData FromData(SpawnData data) {
    var spawn = new SpawnSystem.SpawnData();
    if (ZNetScene.instance.m_namedPrefabs.TryGetValue(data.prefab.GetStableHashCode(), out var obj))
      spawn.m_prefab = obj;
    else
      ExpandWorld.Log.LogWarning($"Spawn prefab {data.prefab} not found!");
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
    spawn.m_requiredEnvironments = data.requiredEnvironments.ToList();
    spawn.m_groupSizeMin = data.groupSizeMin;
    spawn.m_groupSizeMax = data.groupSizeMax;
    spawn.m_groupRadius = data.groupRadius;
    spawn.m_minAltitude = data.minAltitude;
    spawn.m_maxAltitude = data.maxAltitude;
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
    return spawn;
  }
  public static SpawnData ToData(SpawnSystem.SpawnData spawn) {
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
    data.requiredEnvironments = spawn.m_requiredEnvironments.ToArray();
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
    return data;
  }

  public static void Save(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataSpawns) return;
    var spawnSystem = SpawnSystem.m_instances.FirstOrDefault();
    if (spawnSystem == null) return;
    var spawns = spawnSystem.m_spawnLists.SelectMany(s => s.m_spawners);
    var yaml = Data.Serializer().Serialize(spawns.Select(ToData).ToList());
    File.WriteAllText(fileName, yaml);
  }
  public static void Load(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataBiome) return;
    Configuration.configInternalDataSpawns.Value = File.ReadAllText(fileName);
  }

  public static void Set(string raw) {
    if (raw == "" || !Configuration.DataSpawns) return;
    var data = Data.Deserializer().Deserialize<List<SpawnData>>(raw)
      .Select(FromData).ToList();
    if (data.Count == 0) return;
    ExpandWorld.Log.LogInfo($"Reloading {data.Count} spawn data.");
    HandleSpawnData.Override = data;
    foreach (var system in SpawnSystem.m_instances) {
      system.m_spawnLists.Clear();
      system.m_spawnLists.Add(new() { m_spawners = data });
    }
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Data.SpawnFile, Load);
  }
}
