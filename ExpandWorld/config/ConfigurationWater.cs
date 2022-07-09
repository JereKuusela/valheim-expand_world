using System.Collections.Generic;
using BepInEx.Configuration;
using ServerSync;
using Service;

namespace ExpandWorld;
public partial class Configuration {
#nullable disable
public static ConfigEntry<bool> configRivers;
  public static bool Rivers => configRivers.Value;
  public static ConfigEntry<bool> configStreams;
  public static bool Streams => configStreams.Value;
  public static ConfigEntry<string> configWaterLevel;
  public static float WaterLevel => ConfigWrapper.Floats[configWaterLevel];
  public static ConfigEntry<string> configWaveMultiplier;
  public static float WaveMultiplier => ConfigWrapper.Floats[configWaveMultiplier];
  public static ConfigEntry<bool> configWaveOnlyHeight;
  public static bool WaveOnlyHeight => configWaveOnlyHeight.Value;
  public static ConfigEntry<string> configLakeSearchInterval;
  public static float LakeSearchInterval => ConfigWrapper.Floats[configLakeSearchInterval];
  public static ConfigEntry<string> configLakeDepth;
  public static float LakeDepth => ConfigWrapper.Floats[configLakeDepth];
  public static ConfigEntry<string> configLakeMergeRadius;
  public static float LakeMergeRadius => ConfigWrapper.Floats[configLakeMergeRadius];
  public static ConfigEntry<string> configLakeMaxDistance1;
  public static float LakeMaxDistance1 => ConfigWrapper.Floats[configLakeMaxDistance1];
  public static ConfigEntry<string> configLakeMaxDistance2;
  public static float LakeMaxDistance2 => ConfigWrapper.Floats[configLakeMaxDistance2];
  public static ConfigEntry<string> configRiverMaxAltitude;
  public static float RiverMaxAltitude => ConfigWrapper.Floats[configRiverMaxAltitude];
  public static ConfigEntry<string> configRiverCheckInterval;
  public static float RiverCheckInterval => ConfigWrapper.Floats[configRiverCheckInterval];
  public static ConfigEntry<string> configRiverSeed;
  public static int? RiverSeed => ConfigWrapper.Ints[configRiverSeed];
  public static ConfigEntry<string> configRiverMinWidth;
  public static float RiverMinWidth => ConfigWrapper.Floats[configRiverMinWidth];
  public static ConfigEntry<string> configRiverMaxWidth;
  public static float RiverMaxWidth => ConfigWrapper.Floats[configRiverMaxWidth];
  public static ConfigEntry<string> configRiverCurveWidth;
  public static float RiverCurveWidth => ConfigWrapper.Floats[configRiverCurveWidth];
  public static ConfigEntry<string> configRiverCurveWaveLength;
  public static float RiverCurveWaveLength => ConfigWrapper.Floats[configRiverCurveWaveLength];
  
  public static ConfigEntry<string> configStreamSeed;
  public static int? StreamSeed => ConfigWrapper.Ints[configStreamSeed];
  public static ConfigEntry<string> configStreamMaxAmount;
  public static int? StreamMaxAmount => ConfigWrapper.Ints[configStreamMaxAmount];
  public static ConfigEntry<string> configStreamSearchIterations;
  public static int? StreamSearchIterations => ConfigWrapper.Ints[configStreamSearchIterations];
  public static ConfigEntry<string> configStreamStartMinAltitude;
  public static float StreamStartMinAltitude => ConfigWrapper.Floats[configStreamStartMinAltitude];
  public static ConfigEntry<string> configStreamStartMaxAltitude;
  public static float StreamStartMaxAltitude => ConfigWrapper.Floats[configStreamStartMaxAltitude];
  public static ConfigEntry<string> configStreamEndMinAltitude;
  public static float StreamEndMinAltitude => ConfigWrapper.Floats[configStreamEndMinAltitude];
  public static ConfigEntry<string> configStreamEndMaxAltitude;
  public static float StreamEndMaxAltitude => ConfigWrapper.Floats[configStreamEndMaxAltitude];
  public static ConfigEntry<string> configStreamMinWidth;
  public static float StreamMinWidth => ConfigWrapper.Floats[configStreamMinWidth];
  public static ConfigEntry<string> configStreamMaxWidth;
  public static float StreamMaxWidth => ConfigWrapper.Floats[configStreamMaxWidth];
  public static ConfigEntry<string> configStreamMinLength;
  public static float StreamMinLength => ConfigWrapper.Floats[configStreamMinLength];
  public static ConfigEntry<string> configStreamMaxLength;
  public static float StreamMaxLength => ConfigWrapper.Floats[configStreamMaxLength];
  public static ConfigEntry<string> configStreamCurveWidth;
  public static float StreamCurveWidth => ConfigWrapper.Floats[configStreamCurveWidth];
  public static ConfigEntry<string> configStreamCurveWaveLength;
  public static float StreamCurveWaveLength => ConfigWrapper.Floats[configStreamCurveWaveLength];
#nullable enable
  public static void InitWater(ConfigWrapper wrapper) {
    var section = "3. Water";
    configRivers = wrapper.Bind(section, "Rivers", true, true, "Enables rivers.");
    configWaterLevel = wrapper.BindFloat(section, "Water level", 30f, true, "Sets the altitude of the water.");
    configWaterLevel.SettingChanged += (s, e) => {
      WaterHelper.SetLevel(ZoneSystem.instance);
      WaterHelper.SetLevel(ClutterSystem.instance);
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetLevel(obj);
    };
        configWaveMultiplier = wrapper.BindFloat(section, "Wave multiplier", 1f, true, "Multiplies the wave size.");
    configWaveMultiplier.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };
    configWaveOnlyHeight = wrapper.Bind(section, "Wave only height", false, false, "Multiplier only affects the height.");
    configWaveOnlyHeight.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };
    configLakeSearchInterval = wrapper.BindFloat(section, "Lake search interval", 128f, true, "How often a point is checked for lakes (meters).");
    configLakeDepth = wrapper.BindFloat(section, "Lake depth", -20f, true, "How deep the lake.");
    configLakeMergeRadius = wrapper.BindFloat(section, "Lake merge radius", 800f, true, "How deep the lake.");
    configLakeMaxDistance1 = wrapper.BindFloat(section, "Lake max distance 1", 2000f, true, "How deep the lake.");
    configLakeMaxDistance2 = wrapper.BindFloat(section, "Lake max distance 2", 5000f, true, "How deep the lake.");
    configRiverCheckInterval = wrapper.BindFloat(section, "River check interval", 128f, true, "How deep the lake.");
    configRiverCurveWaveLength = wrapper.BindFloat(section, "River curve wave length", 1f/20f, true, "How deep the lake.");
    configRiverCurveWidth = wrapper.BindFloat(section, "River curve width", 1f/15f, true, "How deep the lake.");
    configRiverMaxAltitude = wrapper.BindFloat(section, "River max altitude", 50f, true, "How deep the lake.");
    configRiverMinWidth = wrapper.BindFloat(section, "River min width", 60f, true, "How deep the lake.");
    configRiverMaxWidth = wrapper.BindFloat(section, "River max width", 100f, true, "How deep the lake.");
    configRiverSeed = wrapper.BindInt(section, "River seed", null, true);
    
    configStreams = wrapper.Bind(section, "Streams", true, true, "Enables streams.");
    configStreamSeed = wrapper.BindInt(section, "Stream seed", null, true);
    configStreamCurveWaveLength = wrapper.BindFloat(section, "Stream curve wave length", 1f/20f, true);
    configStreamCurveWidth = wrapper.BindFloat(section, "Stream curve width", 1f/15f, true);
    configStreamEndMaxAltitude = wrapper.BindFloat(section, "Stream end max height", 14f, true);
    configStreamEndMinAltitude = wrapper.BindFloat(section, "Stream end min height", 6f, true);
    configStreamMaxAmount = wrapper.BindInt(section, "Stream max amount", 3000, true);
    configStreamMaxWidth = wrapper.BindFloat(section, "Stream max width", 20f, true);
    configStreamMinWidth = wrapper.BindFloat(section, "Stream min width", 20f, true);
    configStreamMaxLength = wrapper.BindFloat(section, "Stream max length", 200f, true);
    configStreamMinLength = wrapper.BindFloat(section, "Stream min length", 80f, true);
    configStreamSearchIterations = wrapper.BindInt(section, "Stream search iterations", 100, true);
    configStreamStartMaxAltitude = wrapper.BindFloat(section, "Stream start max height", 1f, true);
    configStreamStartMinAltitude = wrapper.BindFloat(section, "Stream start min height", -4f, true);
    
    
  }
}
