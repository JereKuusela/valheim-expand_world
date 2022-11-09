using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace ExpandWorld;

public class EventManager {
  public static string FileName = "expand_events.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.ConfigPath, FileName);
  public static string Pattern = "expand_events*.yaml";
  public static Dictionary<string, List<string>> EventToRequirentEnvironment = new();

  public static RandomEvent FromData(EventData data) {
    var random = new RandomEvent();
    random.m_name = data.name;
    random.m_spawn = data.spawns.Select(SpawnManager.FromData).ToList();
    random.m_enabled = data.enabled;
    random.m_random = data.random;
    random.m_duration = data.duration;
    random.m_nearBaseOnly = data.nearBaseOnly;
    random.m_pauseIfNoPlayerInArea = data.pauseIfNoPlayerInArea;
    random.m_biome = Data.ToBiomes(data.biome);
    random.m_requiredGlobalKeys = Data.ToList(data.requiredGlobalKeys);
    random.m_notRequiredGlobalKeys = Data.ToList(data.notRequiredGlobalKeys);
    random.m_startMessage = data.startMessage;
    random.m_endMessage = data.endMessage;
    random.m_forceMusic = data.forceMusic;
    random.m_forceEnvironment = data.forceEnvironment;
    EventToRequirentEnvironment[data.name] = Data.ToList(data.requiredEnvironments);
    return random;
  }
  public static EventData ToData(RandomEvent random) {
    EventData data = new();
    data.name = random.m_name;
    data.spawns = random.m_spawn.Select(SpawnManager.ToData).ToArray();
    data.enabled = random.m_enabled;
    data.random = random.m_random;
    data.duration = random.m_duration;
    data.nearBaseOnly = random.m_nearBaseOnly;
    data.pauseIfNoPlayerInArea = random.m_pauseIfNoPlayerInArea;
    data.biome = Data.FromBiomes(random.m_biome);
    data.requiredGlobalKeys = Data.FromList(random.m_requiredGlobalKeys);
    data.notRequiredGlobalKeys = Data.FromList(random.m_notRequiredGlobalKeys);
    data.startMessage = random.m_startMessage;
    data.endMessage = random.m_endMessage;
    data.forceMusic = random.m_forceMusic;
    data.forceEnvironment = random.m_forceEnvironment;
    return data;
  }

  public static void ToFile() {
    if (!Helper.IsServer() || !Configuration.DataEvents) return;
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(RandEventSystem.instance.m_events.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
    Configuration.valueEventData.Value = yaml;
  }
  public static void FromFile() {
    if (!Helper.IsServer()) return;
    var yaml = Configuration.DataEvents ? Data.Read(Pattern) : "";
    Configuration.valueEventData.Value = yaml;
    Set(yaml);
  }
  public static void FromSetting(string yaml) {
    if (Helper.IsClient()) Set(yaml);
  }
  private static void Set(string yaml) {
    if (yaml == "" || !Configuration.DataEvents) return;
    try {
      EventToRequirentEnvironment.Clear();
      var data = Data.Deserialize<EventData>(yaml, FileName).Select(FromData).ToList();
      ExpandWorld.Log.LogInfo($"Reloading {data.Count} event data.");
      foreach (var list in LocationList.m_allLocationLists)
        list.m_events.Clear();
      RandEventSystem.instance.m_events = data;
    } catch (Exception e) {
      ExpandWorld.Log.LogError(e.StackTrace);
    }
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Pattern, FromFile);
  }
}
