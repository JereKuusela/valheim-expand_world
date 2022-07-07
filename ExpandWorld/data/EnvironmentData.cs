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
}