using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace ExpandWorld;

public class EnvironmentYaml
{
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
  [DefaultValue(0f)]
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
  public Color ambColorDay = Color.white;
  public Color ambColorNight = Color.white;
  public Color sunColorMorning = Color.white;
  public Color sunColorDay = Color.white;
  public Color sunColorEvening = Color.white;
  public Color sunColorNight = Color.white;
  public Color fogColorMorning = Color.white;
  public Color fogColorDay = Color.white;
  public Color fogColorEvening = Color.white;
  public Color fogColorNight = Color.white;
  public Color fogColorSunMorning = Color.white;
  public Color fogColorSunDay = Color.white;
  public Color fogColorSunEvening = Color.white;
  public Color fogColorSunNight = Color.white;
  [DefaultValue(0.01f)]
  public float fogDensityMorning = 0.01f;
  [DefaultValue(0.01f)]
  public float fogDensityDay = 0.01f;
  [DefaultValue(0.01f)]
  public float fogDensityEvening = 0.01f;
  [DefaultValue(0.01f)]
  public float fogDensityNight = 0.01f;
  [DefaultValue(1.2f)]
  public float lightIntensityDay = 1.2f;
  [DefaultValue(0f)]
  public float lightIntensityNight = 0f;
  [DefaultValue(60f)]
  public float sunAngle = 60f;
  [DefaultValue("")]
  public string statusEffects = "";
  [DefaultValue("")]
  public string dayStatusEffects = "";
  [DefaultValue("")]
  public string nightStatusEffects = "";
}

public class EnvironmentData
{
  public List<Status> statusEffects = new();
  public List<Status> dayStatusEffects = new();
  public List<Status> nightStatusEffects = new();
  public EnvironmentData(EnvironmentYaml data) {
    if (data.statusEffects != "")
      statusEffects = DataManager.ToList(data.statusEffects).Select(s => new Status(s)).ToList();
    if (data.dayStatusEffects != "")
      dayStatusEffects = DataManager.ToList(data.dayStatusEffects).Select(s => new Status(s)).ToList();
    if (data.nightStatusEffects != "")
      nightStatusEffects = DataManager.ToList(data.nightStatusEffects).Select(s => new Status(s)).ToList();
  }
  public bool IsValid() => statusEffects.Count > 0 || dayStatusEffects.Count > 0 || nightStatusEffects.Count > 0;
}