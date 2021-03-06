using System.ComponentModel;
using UnityEngine;
namespace ExpandWorld;

public class BiomeEnvironment {
  public string environment = "";
  [DefaultValue(1f)]
  public float weight = 1f;
}

public class BiomeData {
  public string biome = "";
  [DefaultValue("")]
  public string name = "";
  [DefaultValue("")]
  public string terrain = "";
  [DefaultValue(1f)]
  public float altitudeMultiplier = 1f;
  [DefaultValue(1f)]
  public float forestMultiplier = 1f;
  [DefaultValue(0f)]
  public float altitudeDelta = 0f;
  public BiomeEnvironment[] environments = new BiomeEnvironment[0];
  public Color paint = new Color(0, 0, 0, 0);
  public Color color = new Color(0, 0, 0, 0);
  [DefaultValue(1f)]
  public float mapColorMultiplier = 1f;
  public Color mapColor = new Color(0, 0, 0, 0);
  [DefaultValue("")]
  public string musicMorning = "morning";
  [DefaultValue("")]
  public string musicEvening = "evening";
  [DefaultValue("")]
  public string musicDay = "";
  [DefaultValue("")]
  public string musicNight = "";
}
