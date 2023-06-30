using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
namespace ExpandWorld;

public class BiomeEnvironment
{
  public string environment = "";
  [DefaultValue(1f)]
  public float weight = 1f;
}

public class BiomeYaml
{
  public string biome = "";
  [DefaultValue("")]
  public string name = "";
  [DefaultValue("")]
  public string terrain = "";
  [DefaultValue("")]
  public string nature = "";
  [DefaultValue(1f)]
  public float altitudeMultiplier = 1f;
  [DefaultValue(1f)]
  public float waterDepthMultiplier = 1f;
  [DefaultValue(-1000f)]
  public float minimumAltitude = -1000f;
  [DefaultValue(10000f)]
  public float maximumAltitude = 10000f;
  [DefaultValue(0.5f)]
  public float excessFactor = 0.5f;
  [DefaultValue(1f)]
  public float forestMultiplier = 1f;
  [DefaultValue(0f)]
  public float altitudeDelta = 0f;
  public BiomeEnvironment[] environments = new BiomeEnvironment[0];
  [DefaultValue("")]
  public string paint = "";
  public Color color = new(0, 0, 0, 0);
  [DefaultValue(1f)]
  public float mapColorMultiplier = 1f;
  public Color mapColor = new(0, 0, 0, 0);
  [DefaultValue("")]
  public string musicMorning = "morning";
  [DefaultValue("")]
  public string musicEvening = "evening";
  [DefaultValue("")]
  public string musicDay = "";
  [DefaultValue("")]
  public string musicNight = "";
  [DefaultValue(false)]
  public bool noBuild = false;
  [DefaultValue("")]
  public string statusEffects = "";
  [DefaultValue("")]
  public string dayStatusEffects = "";
  [DefaultValue("")]
  public string nightStatusEffects = "";
}

public class BiomeData
{
  public bool noBuild = false;
  public float altitudeMultiplier = 1f;
  public float waterDepthMultiplier = 1f;
  public float altitudeDelta = 0f;
  public float excessFactor = 0.5f;
  public float minimumAltitude = -1000f;
  public float maximumAltitude = 10000f;
  public float mapColorMultiplier = 1f;
  public Color color = new(0, 0, 0, 0);
  public Color mapColor = new(0, 0, 0, 0);
  public float forestMultiplier = 1f;

  public List<Status> statusEffects = new();
  public List<Status> dayStatusEffects = new();
  public List<Status> nightStatusEffects = new();

  public BiomeData(BiomeYaml data) {
    noBuild = data.noBuild;
    altitudeMultiplier = data.altitudeMultiplier;
    waterDepthMultiplier = data.waterDepthMultiplier;
    altitudeDelta = data.altitudeDelta;
    excessFactor = data.excessFactor;
    minimumAltitude = data.minimumAltitude;
    maximumAltitude = data.maximumAltitude;
    mapColorMultiplier = data.mapColorMultiplier;
    color = data.color;
    mapColor = data.mapColor;
    forestMultiplier = data.forestMultiplier;
    if (data.statusEffects != "")
      statusEffects = DataManager.ToList(data.statusEffects).Select(s => new Status(s)).ToList();
    if (data.dayStatusEffects != "")
      dayStatusEffects = DataManager.ToList(data.dayStatusEffects).Select(s => new Status(s)).ToList();
    if (data.nightStatusEffects != "")
      nightStatusEffects = DataManager.ToList(data.nightStatusEffects).Select(s => new Status(s)).ToList();
  }
  public bool IsValid() =>
    statusEffects.Count > 0 ||
    dayStatusEffects.Count > 0 ||
    nightStatusEffects.Count > 0 ||
    altitudeMultiplier != 1f ||
    waterDepthMultiplier != 1f ||
    altitudeDelta != 0f ||
    excessFactor != 0.5f ||
    minimumAltitude != -1000f ||
    maximumAltitude != 10000f ||
    mapColorMultiplier != 1f ||
    noBuild ||
    mapColor.a != 0 ||
    forestMultiplier != 1f;
}