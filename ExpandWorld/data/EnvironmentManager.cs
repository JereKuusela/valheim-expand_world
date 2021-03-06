using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace ExpandWorld;

public class EnvironmentManager {
  public static string FileName = "expand_environments.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.ConfigPath, FileName);
  public static string Pattern = "expand_environments*.yaml";
  public static Dictionary<string, EnvSetup> Originals = new();
  public static EnvSetup FromData(EnvironmentData data) {
    var env = new EnvSetup();
    if (Originals.TryGetValue(data.particles, out var setup))
      env = setup.Clone();
    else if (Originals.TryGetValue(data.name, out setup))
      env = setup.Clone();
    env.m_name = data.name;
    env.m_default = data.isDefault;
    env.m_isWet = data.isWet;
    env.m_isFreezing = data.isFreezing;
    env.m_isFreezingAtNight = data.isFreezingAtNight;
    env.m_isCold = data.isCold;
    env.m_isColdAtNight = data.isColdAtNight;
    env.m_alwaysDark = data.alwaysDark;
    env.m_ambColorNight = Data.Sanity(data.ambColorNight);
    env.m_ambColorDay = Data.Sanity(data.ambColorDay);
    env.m_fogColorNight = Data.Sanity(data.fogColorNight);
    env.m_fogColorMorning = Data.Sanity(data.fogColorMorning);
    env.m_fogColorDay = Data.Sanity(data.fogColorDay);
    env.m_fogColorEvening = Data.Sanity(data.fogColorEvening);
    env.m_fogColorSunNight = Data.Sanity(data.fogColorSunNight);
    env.m_fogColorSunMorning = Data.Sanity(data.fogColorSunMorning);
    env.m_fogColorSunDay = Data.Sanity(data.fogColorSunDay);
    env.m_fogColorSunEvening = Data.Sanity(data.fogColorSunEvening);
    env.m_fogDensityNight = data.fogDensityNight;
    env.m_fogDensityMorning = data.fogDensityMorning;
    env.m_fogDensityDay = data.fogDensityDay;
    env.m_fogDensityEvening = data.fogDensityEvening;
    env.m_sunColorNight = Data.Sanity(data.sunColorNight);
    env.m_sunColorMorning = Data.Sanity(data.sunColorMorning);
    env.m_sunColorDay = Data.Sanity(data.sunColorDay);
    env.m_sunColorEvening = Data.Sanity(data.sunColorEvening);
    env.m_lightIntensityDay = data.lightIntensityDay;
    env.m_lightIntensityNight = data.lightIntensityNight;
    env.m_sunAngle = data.sunAngle;
    env.m_windMin = data.windMin;
    env.m_windMax = data.windMax;
    env.m_rainCloudAlpha = data.rainCloudAlpha;
    env.m_ambientVol = data.ambientVol;
    env.m_ambientList = data.ambientList;
    env.m_musicMorning = data.musicMorning;
    env.m_musicEvening = data.musicEvening;
    env.m_musicDay = data.musicDay;
    env.m_musicNight = data.musicNight;
    return env;
  }
  public static EnvironmentData ToData(EnvSetup env) {
    EnvironmentData data = new();
    data.name = env.m_name;
    data.isDefault = env.m_default;
    data.isWet = env.m_isWet;
    data.isFreezing = env.m_isFreezing;
    data.isFreezingAtNight = env.m_isFreezingAtNight;
    data.isCold = env.m_isCold;
    data.isColdAtNight = env.m_isColdAtNight;
    data.alwaysDark = env.m_alwaysDark;
    data.ambColorNight = env.m_ambColorNight;
    data.ambColorDay = env.m_ambColorDay;
    data.fogColorNight = env.m_fogColorNight;
    data.fogColorMorning = env.m_fogColorMorning;
    data.fogColorDay = env.m_fogColorDay;
    data.fogColorEvening = env.m_fogColorEvening;
    data.fogColorSunNight = env.m_fogColorSunNight;
    data.fogColorSunMorning = env.m_fogColorSunMorning;
    data.fogColorSunDay = env.m_fogColorSunDay;
    data.fogColorSunEvening = env.m_fogColorSunEvening;
    data.fogDensityNight = env.m_fogDensityNight;
    data.fogDensityMorning = env.m_fogDensityMorning;
    data.fogDensityDay = env.m_fogDensityDay;
    data.fogDensityEvening = env.m_fogDensityEvening;
    data.sunColorNight = env.m_sunColorNight;
    data.sunColorMorning = env.m_sunColorMorning;
    data.sunColorDay = env.m_sunColorDay;
    data.sunColorEvening = env.m_sunColorEvening;
    data.lightIntensityDay = env.m_lightIntensityDay;
    data.lightIntensityNight = env.m_lightIntensityNight;
    data.sunAngle = env.m_sunAngle;
    data.windMin = env.m_windMin;
    data.windMax = env.m_windMax;
    data.rainCloudAlpha = env.m_rainCloudAlpha;
    data.ambientVol = env.m_ambientVol;
    data.ambientList = env.m_ambientList;
    data.musicMorning = env.m_musicMorning;
    data.musicEvening = env.m_musicEvening;
    data.musicDay = env.m_musicDay;
    data.musicNight = env.m_musicNight;
    return data;
  }

  public static void ToFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataEnvironments) return;
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(EnvMan.instance.m_environments.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
    Configuration.valueEnvironmentData.Value = yaml;
  }
  public static void FromFile() {
    if (!ZNet.instance.IsServer()) return;
    var yaml = Configuration.DataEnvironments ? Data.Read(Pattern) : "";
    Configuration.valueEnvironmentData.Value = yaml;
    Set(yaml);
  }

  public static void SetOriginals() {
    Originals = LocationList.m_allLocationLists
      .Select(list => list.m_environments)
      .Append(EnvMan.instance.m_environments)
      .SelectMany(list => list).ToDictionary(env => env.m_name, env => env);
  }
  public static void FromSetting(string yaml) {
    if (!ZNet.instance.IsServer()) Set(yaml);
  }
  private static void Set(string yaml) {
    if (yaml == "" || !Configuration.DataEnvironments) return;
    if (Originals.Count == 0) SetOriginals();
    var data = Data.Deserialize<EnvironmentData>(yaml, FileName)
      .Select(FromData).ToList();
    if (data.Count == 0) {
      ExpandWorld.Log.LogWarning($"Failed to load any environment data.");
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading {data.Count} environment data.");
    foreach (var list in LocationList.m_allLocationLists)
      list.m_environments.Clear();
    var em = EnvMan.instance;
    em.m_environments.Clear();
    foreach (var env in data)
      em.AppendEnvironment(env);
    em.m_environmentPeriod = -1;
    em.m_firstEnv = true;
    foreach (var biome in em.m_biomes)
      em.InitializeBiomeEnvSetup(biome);
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(Pattern, FromFile);
  }
}