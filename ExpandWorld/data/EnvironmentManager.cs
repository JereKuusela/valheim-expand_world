using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ExpandWorld;

public class EnvironmentManager
{
  public static string FileName = "expand_environments.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  public static string Pattern = "expand_environments*.yaml";
  private static Dictionary<string, EnvSetup> Originals = new();
  public static EnvSetup FromData(EnvironmentData data)
  {
    EnvSetup env = new()
    {
      m_psystems = new GameObject[0]
    };
    if (Originals.TryGetValue(data.particles, out var setup))
      env = setup.Clone();
    else if (Originals.TryGetValue(data.name, out setup))
      env = setup.Clone();
    else
      ExpandWorld.Log.LogWarning($"Failed to find a particle system \"{data.particles}\". Make sure field \"particles\" it set correctly.");

    env.m_name = data.name;
    env.m_default = data.isDefault;
    env.m_isWet = data.isWet;
    env.m_isFreezing = data.isFreezing;
    env.m_isFreezingAtNight = data.isFreezingAtNight;
    env.m_isCold = data.isCold;
    env.m_isColdAtNight = data.isColdAtNight;
    env.m_alwaysDark = data.alwaysDark;
    env.m_ambColorNight = DataManager.Sanity(data.ambColorNight);
    env.m_ambColorDay = DataManager.Sanity(data.ambColorDay);
    env.m_fogColorNight = DataManager.Sanity(data.fogColorNight);
    env.m_fogColorMorning = DataManager.Sanity(data.fogColorMorning);
    env.m_fogColorDay = DataManager.Sanity(data.fogColorDay);
    env.m_fogColorEvening = DataManager.Sanity(data.fogColorEvening);
    env.m_fogColorSunNight = DataManager.Sanity(data.fogColorSunNight);
    env.m_fogColorSunMorning = DataManager.Sanity(data.fogColorSunMorning);
    env.m_fogColorSunDay = DataManager.Sanity(data.fogColorSunDay);
    env.m_fogColorSunEvening = DataManager.Sanity(data.fogColorSunEvening);
    env.m_fogDensityNight = data.fogDensityNight;
    env.m_fogDensityMorning = data.fogDensityMorning;
    env.m_fogDensityDay = data.fogDensityDay;
    env.m_fogDensityEvening = data.fogDensityEvening;
    env.m_sunColorNight = DataManager.Sanity(data.sunColorNight);
    env.m_sunColorMorning = DataManager.Sanity(data.sunColorMorning);
    env.m_sunColorDay = DataManager.Sanity(data.sunColorDay);
    env.m_sunColorEvening = DataManager.Sanity(data.sunColorEvening);
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
  public static EnvironmentData ToData(EnvSetup env)
  {
    EnvironmentData data = new()
    {
      name = env.m_name,
      isDefault = env.m_default,
      isWet = env.m_isWet,
      isFreezing = env.m_isFreezing,
      isFreezingAtNight = env.m_isFreezingAtNight,
      isCold = env.m_isCold,
      isColdAtNight = env.m_isColdAtNight,
      alwaysDark = env.m_alwaysDark,
      ambColorNight = env.m_ambColorNight,
      ambColorDay = env.m_ambColorDay,
      fogColorNight = env.m_fogColorNight,
      fogColorMorning = env.m_fogColorMorning,
      fogColorDay = env.m_fogColorDay,
      fogColorEvening = env.m_fogColorEvening,
      fogColorSunNight = env.m_fogColorSunNight,
      fogColorSunMorning = env.m_fogColorSunMorning,
      fogColorSunDay = env.m_fogColorSunDay,
      fogColorSunEvening = env.m_fogColorSunEvening,
      fogDensityNight = env.m_fogDensityNight,
      fogDensityMorning = env.m_fogDensityMorning,
      fogDensityDay = env.m_fogDensityDay,
      fogDensityEvening = env.m_fogDensityEvening,
      sunColorNight = env.m_sunColorNight,
      sunColorMorning = env.m_sunColorMorning,
      sunColorDay = env.m_sunColorDay,
      sunColorEvening = env.m_sunColorEvening,
      lightIntensityDay = env.m_lightIntensityDay,
      lightIntensityNight = env.m_lightIntensityNight,
      sunAngle = env.m_sunAngle,
      windMin = env.m_windMin,
      windMax = env.m_windMax,
      rainCloudAlpha = env.m_rainCloudAlpha,
      ambientVol = env.m_ambientVol,
      ambientList = env.m_ambientList,
      musicMorning = env.m_musicMorning,
      musicEvening = env.m_musicEvening,
      musicDay = env.m_musicDay,
      musicNight = env.m_musicNight
    };
    return data;
  }

  public static void ToFile()
  {
    if (Helper.IsClient() || !Configuration.DataEnvironments) return;
    if (File.Exists(FilePath)) return;
    var yaml = DataManager.Serializer().Serialize(EnvMan.instance.m_environments.Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    if (Helper.IsClient()) return;
    var yaml = Configuration.DataEnvironments ? DataManager.Read(Pattern) : "";
    Configuration.valueEnvironmentData.Value = yaml;
    Set(yaml);
  }

  private static void SetOriginals()
  {
    var newOriginals = LocationList.m_allLocationLists
      .Select(list => list.m_environments)
      .Append(EnvMan.instance.m_environments)
      .SelectMany(list => list).ToLookup(env => env.m_name, env => env).ToDictionary(kvp => kvp.Key, kvp => kvp.First());
    // Needs to be set once per world. This can be checked detected by checking location lists.
    if (newOriginals.Count > 0) Originals = newOriginals;
  }
  public static void FromSetting(string yaml)
  {
    if (Helper.IsClient()) Set(yaml);
  }
  private static void Set(string yaml)
  {
    SetOriginals();
    if (yaml == "" || !Configuration.DataEnvironments) return;
    try
    {
      var data = DataManager.Deserialize<EnvironmentData>(yaml, FileName)
        .Select(FromData).ToList();
      if (data.Count == 0)
      {
        ExpandWorld.Log.LogWarning($"Failed to load any environment data.");
        return;
      }
      ExpandWorld.Log.LogInfo($"Reloading environment data ({data.Count} entries).");
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
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.Message);
      ExpandWorld.Log.LogError(e.StackTrace);
    }
  }
  public static void SetupWatcher()
  {
    DataManager.SetupWatcher(Pattern, FromFile);
  }
}