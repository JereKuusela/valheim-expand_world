using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace ExpandWorld;

[Serializable]
public class EventData {
  public string name = "";
  public bool enabled = true;
  public float duration = 60f;
  public bool nearBaseOnly = true;
  public string[] biome = new string[0];
  public string[] requiredGlobalKeys = new string[0];
  public string[] notRequiredGlobalKeys = new string[0];
  public string startMessage = "";
  public string endMessage = "";
  public string forceMusic = "";
  public string forceEnvironment = "";
  public SpawnData[] spawns = new SpawnData[0];
  public bool pauseIfNoPlayerInArea = true;
  public bool random = true;

  public static RandomEvent FromData(EventData data) {
    var random = new RandomEvent();
    random.m_name = data.name;
    random.m_spawn = data.spawns.Select(SpawnData.FromData).ToList();
    random.m_enabled = data.enabled;
    random.m_random = data.random;
    random.m_duration = data.duration;
    random.m_nearBaseOnly = data.nearBaseOnly;
    random.m_pauseIfNoPlayerInArea = data.pauseIfNoPlayerInArea;
    random.m_biome = Data.ToBiomes(data.biome);
    random.m_requiredGlobalKeys = data.requiredGlobalKeys.ToList();
    random.m_notRequiredGlobalKeys = data.notRequiredGlobalKeys.ToList();
    random.m_startMessage = data.startMessage;
    random.m_endMessage = data.endMessage;
    random.m_forceMusic = data.forceMusic;
    random.m_forceEnvironment = data.forceEnvironment;
    return random;
  }
  public static EventData ToData(RandomEvent random) {
    EventData data = new();
    data.name = random.m_name;
    data.spawns = random.m_spawn.Select(SpawnData.ToData).ToArray();
    data.enabled = random.m_enabled;
    data.random = random.m_random;
    data.duration = random.m_duration;
    data.nearBaseOnly = random.m_nearBaseOnly;
    data.pauseIfNoPlayerInArea = random.m_pauseIfNoPlayerInArea;
    data.biome = Data.FromBiomes(random.m_biome);
    data.requiredGlobalKeys = random.m_requiredGlobalKeys.ToArray();
    data.notRequiredGlobalKeys = random.m_notRequiredGlobalKeys.ToArray();
    data.startMessage = random.m_startMessage;
    data.endMessage = random.m_endMessage;
    data.forceMusic = random.m_forceMusic;
    data.forceEnvironment = random.m_forceEnvironment;
    return data;
  }

  public static void Save(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataEvents) return;
    var yaml = Data.Serializer().Serialize(RandEventSystem.instance.m_events.Select(ToData).ToList());
    File.WriteAllText(fileName, yaml);
  }
  public static void Load(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataEvents) return;
    Configuration.configInternalDataEvents.Value = File.ReadAllText(fileName);
  }
  public static void Set(string raw) {
    if (raw == "" || !Configuration.DataEvents) return;
    var data = Data.Deserializer().Deserialize<List<EventData>>(raw)
    .Select(FromData).ToList();
    if (data.Count == 0) return;
    ExpandWorld.Log.LogInfo($"Reloading {data.Count} event data.");
    foreach (var list in LocationList.m_allLocationLists)
      list.m_events.Clear();
    RandEventSystem.instance.m_events = data;
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Data.EventFile, Load);
  }
}
