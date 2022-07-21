using System.IO;
using System.Linq;
namespace ExpandWorld;

public class EventManager {
  public static string FileName = "expand_events.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.ConfigPath, FileName);
  public static string Pattern = "expand_events*.yaml";
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
    data.spawns = random.m_spawn.Select(SpawnManager.ToData).ToArray();
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

  public static void ToFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataEvents) return;
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(RandEventSystem.instance.m_events.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
    Configuration.valueEventData.Value = yaml;
  }
  public static void FromFile() {
    if (!ZNet.instance.IsServer()) return;
    var yaml = Configuration.DataEvents ? Data.Read(Pattern) : "";
    Configuration.valueEventData.Value = yaml;
    Set(yaml);
  }
  public static void FromSetting(string yaml) {
    if (!ZNet.instance.IsServer()) Set(yaml);
  }
  private static void Set(string yaml) {
    if (yaml == "" || !Configuration.DataEvents) return;
    var data = Data.Deserialize<EventData>(yaml, FileName)
    .Select(FromData).ToList();
    if (data.Count == 0) {
      ExpandWorld.Log.LogWarning($"Failed to load any event data.");
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading {data.Count} event data.");
    foreach (var list in LocationList.m_allLocationLists)
      list.m_events.Clear();
    RandEventSystem.instance.m_events = data;
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Pattern, FromFile);
  }
}
