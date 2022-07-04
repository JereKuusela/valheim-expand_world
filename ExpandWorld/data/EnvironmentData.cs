using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ExpandWorld;

[Serializable]
public class EnvironmentData {
  public string name = "";
  public bool isDefault = false;
  public bool isWet = false;
  public bool isFreezing = false;
  public bool isFreezingAtNight = false;
  public bool isCold = false;
  public bool isColdAtNight = false;
  public bool alwaysDark = false;
  public Color ambColorNight = Color.white;
  public Color ambColorDay = Color.white;
  public Color fogColorNight = Color.white;
  public Color fogColorMorning = Color.white;
  public Color fogColorDay = Color.white;
  public Color fogColorEvening = Color.white;
  public Color fogColorSunNight = Color.white;
  public Color fogColorSunMorning = Color.white;
  public Color fogColorSunDay = Color.white;
  public Color fogColorSunEvening = Color.white;
  public float fogDensityNight = 0.01f;
  public float fogDensityMorning = 0.01f;
  public float fogDensityDay = 0.01f;
  public float fogDensityEvening = 0.01f;
  public Color sunColorNight = Color.white;
  public Color sunColorMorning = Color.white;
  public Color sunColorDay = Color.white;
  public Color sunColorEvening = Color.white;
  public float lightIntensityDay = 1.2f;
  public float lightIntensityNight = 0f;
  public float sunAngle = 60f;
  public float windMin = 0f;
  public float windMax = 1f;
  public float rainCloudAlpha;
  public float ambientVol = 0.3f;
  public string ambientList = "";
  public string musicMorning = "";
  public string musicEvening = "";
  public string musicDay = "";
  public string musicNight = "";
  public static EnvSetup FromData(EnvironmentData data) {
    var env = new EnvSetup();
    return env;
  }
  public static EnvironmentData ToData(EnvSetup env) {
    EnvironmentData data = new();
    return data;
  }

  public static void Save(string fileName) {
    var yaml = Data.Serializer().Serialize(EnvMan.instance.m_environments.Select(ToData).ToList());
    File.WriteAllText(fileName, yaml);
  }
  public static void Load(string fileName) {

  }
  public static void Set(string raw) {
    if (raw == "") return;
    var data = Data.Deserializer().Deserialize<List<EnvironmentData>>(raw)
      .Select(FromData).ToList();
    if (data.Count == 0) return;
    ExpandWorld.Log.LogInfo($"Reloading {data.Count} environment data.");
    foreach (var list in LocationList.m_allLocationLists)
      list.m_environments.Clear();
    EnvMan.instance.m_environments.Clear();
    foreach (var env in data)
      EnvMan.instance.AppendEnvironment(env);
  }
}