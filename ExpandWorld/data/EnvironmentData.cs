using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ExpandWorld;
public class ColorF {
  [DefaultValue(0f)]
  public float r = 0f;
  [DefaultValue(0f)]
  public float g = 0f;
  [DefaultValue(0f)]
  public float b = 0f;
  [DefaultValue(0f)]
  public float a = 0f;
  public static ColorF ToData(Color color) => new ColorF()
  {
    r = color.r,
    g = color.g,
    b = color.b,
    a = color.a
  };
  public static Color FromData(ColorF color) => new Color()
  {
    r = color.r,
    g = color.g,
    b = color.b,
    a = color.a
  };
  public static ColorF white = new ColorF() { r = 1f, g = 1f, b = 1f, a = 1f };
}
public class EnvironmentData {
  public string name = "";
  [DefaultValue("")]
  public string particles = "";
  [DefaultValue(false)]
  public bool isDefault = false;
  [DefaultValue(false)]
  public bool isWet = false;
  [DefaultValue(false)]
  public bool isFreezing = false;
  [DefaultValue(false)]
  public bool isFreezingAtNight = false;
  [DefaultValue(false)]
  public bool isCold = false;
  [DefaultValue(false)]
  public bool isColdAtNight = false;
  [DefaultValue(false)]
  public bool alwaysDark = false;
  public float windMin = 0f;
  [DefaultValue(1f)]
  public float windMax = 1f;
  [DefaultValue(0f)]
  public float rainCloudAlpha = 0f;
  [DefaultValue(0.3f)]
  public float ambientVol = 0.3f;
  [DefaultValue("")]
  public string ambientList = "";
  [DefaultValue("")]
  public string musicMorning = "";
  [DefaultValue("")]
  public string musicEvening = "";
  [DefaultValue("")]
  public string musicDay = "";
  [DefaultValue("")]
  public string musicNight = "";
  public ColorF ambColorNight = ColorF.white;
  public ColorF ambColorDay = ColorF.white;
  public ColorF fogColorNight = ColorF.white;
  public ColorF fogColorMorning = ColorF.white;
  public ColorF fogColorDay = ColorF.white;
  public ColorF fogColorEvening = ColorF.white;
  public ColorF fogColorSunNight = ColorF.white;
  public ColorF fogColorSunMorning = ColorF.white;
  public ColorF fogColorSunDay = ColorF.white;
  public ColorF fogColorSunEvening = ColorF.white;
  [DefaultValue(0.01f)]
  public float fogDensityNight = 0.01f;
  [DefaultValue(0.01f)]
  public float fogDensityMorning = 0.01f;
  [DefaultValue(0.01f)]
  public float fogDensityDay = 0.01f;
  [DefaultValue(0.01f)]
  public float fogDensityEvening = 0.01f;
  public ColorF sunColorNight = ColorF.white;
  public ColorF sunColorMorning = ColorF.white;
  public ColorF sunColorDay = ColorF.white;
  public ColorF sunColorEvening = ColorF.white;
  [DefaultValue(1.2f)]
  public float lightIntensityDay = 1.2f;
  [DefaultValue(0f)]
  public float lightIntensityNight = 0f;
  [DefaultValue(60f)]
  public float sunAngle = 60f;
  [DefaultValue(0f)]
  private static Dictionary<string, EnvSetup> Originals = new();
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
    env.m_ambColorNight = ColorF.FromData(data.ambColorNight);
    env.m_ambColorDay = ColorF.FromData(data.ambColorDay);
    env.m_fogColorNight = ColorF.FromData(data.fogColorNight);
    env.m_fogColorMorning = ColorF.FromData(data.fogColorMorning);
    env.m_fogColorDay = ColorF.FromData(data.fogColorDay);
    env.m_fogColorEvening = ColorF.FromData(data.fogColorEvening);
    env.m_fogColorSunNight = ColorF.FromData(data.fogColorSunNight);
    env.m_fogColorSunMorning = ColorF.FromData(data.fogColorSunMorning);
    env.m_fogColorSunDay = ColorF.FromData(data.fogColorSunDay);
    env.m_fogColorSunEvening = ColorF.FromData(data.fogColorSunEvening);
    env.m_fogDensityNight = data.fogDensityNight;
    env.m_fogDensityMorning = data.fogDensityMorning;
    env.m_fogDensityDay = data.fogDensityDay;
    env.m_fogDensityEvening = data.fogDensityEvening;
    env.m_sunColorNight = ColorF.FromData(data.sunColorNight);
    env.m_sunColorMorning = ColorF.FromData(data.sunColorMorning);
    env.m_sunColorDay = ColorF.FromData(data.sunColorDay);
    env.m_sunColorEvening = ColorF.FromData(data.sunColorEvening);
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
    data.ambColorNight = ColorF.ToData(env.m_ambColorNight);
    data.ambColorDay = ColorF.ToData(env.m_ambColorDay);
    data.fogColorNight = ColorF.ToData(env.m_fogColorNight);
    data.fogColorMorning = ColorF.ToData(env.m_fogColorMorning);
    data.fogColorDay = ColorF.ToData(env.m_fogColorDay);
    data.fogColorEvening = ColorF.ToData(env.m_fogColorEvening);
    data.fogColorSunNight = ColorF.ToData(env.m_fogColorSunNight);
    data.fogColorSunMorning = ColorF.ToData(env.m_fogColorSunMorning);
    data.fogColorSunDay = ColorF.ToData(env.m_fogColorSunDay);
    data.fogColorSunEvening = ColorF.ToData(env.m_fogColorSunEvening);
    data.fogDensityNight = env.m_fogDensityNight;
    data.fogDensityMorning = env.m_fogDensityMorning;
    data.fogDensityDay = env.m_fogDensityDay;
    data.fogDensityEvening = env.m_fogDensityEvening;
    data.sunColorNight = ColorF.ToData(env.m_sunColorNight);
    data.sunColorMorning = ColorF.ToData(env.m_sunColorMorning);
    data.sunColorDay = ColorF.ToData(env.m_sunColorDay);
    data.sunColorEvening = ColorF.ToData(env.m_sunColorEvening);
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

  public static void ToFile(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataEnvironments) return;
    var yaml = Data.Serializer().Serialize(EnvMan.instance.m_environments.Select(ToData).ToList());
    File.WriteAllText(fileName, yaml);
  }
  public static void FromFile(string fileName) {
    if (!ZNet.instance.IsServer() || !Configuration.DataEnvironments) return;
    var raw = File.ReadAllText(fileName);
    Configuration.configInternalDataEnvironments.Value = raw;
    if (LoadData.IsLoading) Set(raw);
  }

  private static void SetOriginals() {
    Originals = LocationList.m_allLocationLists
      .Select(list => list.m_environments)
      .Append(EnvMan.instance.m_environments)
      .SelectMany(list => list).ToDictionary(env => env.m_name, env => env);
  }
  public static void FromSetting(string raw) {
    if (!LoadData.IsLoading) Set(raw);
  }
  private static void Set(string raw) {
    if (raw == "" || !Configuration.DataEnvironments) return;
    if (Originals.Count == 0) SetOriginals();
    var data = Data.Deserializer().Deserialize<List<EnvironmentData>>(raw)
      .Select(FromData).ToList();
    if (data.Count == 0) return;
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
    Data.SetupWatcher(Data.EnvironmentFile, FromFile);
  }
}