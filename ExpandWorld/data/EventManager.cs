using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace ExpandWorld;

public class EventManager
{
  public static string FileName = "expand_events.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  public static string Pattern = "expand_events*.yaml";
  public static Dictionary<string, List<string>> EventToRequirentEnvironment = new();

  public static RandomEvent FromData(EventData data)
  {
    var random = new RandomEvent
    {
      m_name = data.name,
      m_spawn = data.spawns.Select(SpawnManager.FromData).ToList(),
      m_enabled = data.enabled,
      m_random = data.random,
      m_duration = data.duration,
      m_nearBaseOnly = data.nearBaseOnly,
      m_pauseIfNoPlayerInArea = data.pauseIfNoPlayerInArea,
      m_biome = Data.ToBiomes(data.biome),
      m_requiredGlobalKeys = Data.ToList(data.requiredGlobalKeys),
      m_notRequiredGlobalKeys = Data.ToList(data.notRequiredGlobalKeys),
      m_startMessage = data.startMessage,
      m_endMessage = data.endMessage,
      m_forceMusic = data.forceMusic,
      m_forceEnvironment = data.forceEnvironment
    };
    EventToRequirentEnvironment[data.name] = Data.ToList(data.requiredEnvironments);
    return random;
  }
  public static EventData ToData(RandomEvent random)
  {
    EventData data = new()
    {
      name = random.m_name,
      spawns = random.m_spawn.Select(SpawnManager.ToData).ToArray(),
      enabled = random.m_enabled,
      random = random.m_random,
      duration = random.m_duration,
      nearBaseOnly = random.m_nearBaseOnly,
      pauseIfNoPlayerInArea = random.m_pauseIfNoPlayerInArea,
      biome = Data.FromBiomes(random.m_biome),
      requiredGlobalKeys = Data.FromList(random.m_requiredGlobalKeys),
      notRequiredGlobalKeys = Data.FromList(random.m_notRequiredGlobalKeys),
      startMessage = random.m_startMessage,
      endMessage = random.m_endMessage,
      forceMusic = random.m_forceMusic,
      forceEnvironment = random.m_forceEnvironment
    };
    return data;
  }

  public static void ToFile()
  {
    if (!Helper.IsServer() || !Configuration.DataEvents) return;
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(RandEventSystem.instance.m_events.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    if (!Helper.IsServer()) return;
    var yaml = Configuration.DataEvents ? Data.Read(Pattern) : "";
    Configuration.valueEventData.Value = yaml;
    Set(yaml);
  }
  public static void FromSetting(string yaml)
  {
    if (Helper.IsClient()) Set(yaml);
  }
  private static void Set(string yaml)
  {
    if (yaml == "" || !Configuration.DataEvents) return;
    try
    {
      EventToRequirentEnvironment.Clear();
      var data = Data.Deserialize<EventData>(yaml, FileName).Select(FromData).ToList();
      ExpandWorld.Log.LogInfo($"Reloading event data ({data.Count} entries).");
      RandEventSystem.instance.m_events = data;
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
}
