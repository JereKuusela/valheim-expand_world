using BepInEx.Configuration;
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
    configWaveMultiplier = wrapper.BindFloat(section, "Wave multiplier", 1f, false, "Multiplies the wave size.");
    configWaveMultiplier.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };
    configWaveOnlyHeight = wrapper.Bind(section, "Wave only height", true, false, "Multiplier only affects the height.");
    configWaveOnlyHeight.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };
    configLakeSearchInterval = wrapper.BindFloat(section, "Lake search interval", 128f, true, "How often a point is checked for lakes (meters). Increase to find more smaller lakes.");
    configLakeDepth = wrapper.BindFloat(section, "Lake depth", -20f, true, "How deep the point must be to be considered a lake. Increase to find more shallow lakes.");
    configLakeMergeRadius = wrapper.BindFloat(section, "Lake merge radius", 800f, true, "How big area is merged to a single lake. Decrease to get more lakes.");
    configLakeMaxDistance1 = wrapper.BindFloat(section, "Lake max distance 1", 2000f, true, "Lakes within this distance get a river between them. Increase to place more and longer rivers.");
    configLakeMaxDistance2 = wrapper.BindFloat(section, "Lake max distance 2", 5000f, true, "Fallback. Lakes without a river do a longer search and place one river to a random lake. Increase to enable very long rivers without increasing the total amount that much. ");
    configRiverCheckInterval = wrapper.BindFloat(section, "River check interval", 128f, true, "How often the river altitude is checked. Both `River max altitude` and `Lake point depth`.");
    configRiverCurveWaveLength = wrapper.BindFloat(section, "River curve wave length", 20f, true, "How often the river changes direction.");
    configRiverCurveWidth = wrapper.BindFloat(section, "River curve width", 15f, true, "How wide the curves are.");
    configRiverMaxAltitude = wrapper.BindFloat(section, "River max altitude", 50f, true, "The river is not valid if this terrain altitude is found between the lakes.");
    configRiverMinWidth = wrapper.BindFloat(section, "River min width", 60f, true, "or each river, the minimum width is randomly selected between this and selected maximum width. So the average width is closer to the `River minimum width` than the `River maximum width`.");
    configRiverMaxWidth = wrapper.BindFloat(section, "River max width", 100f, true, "For each river, the maximum width is randomly selected between this and `River minimum width`.");
    configRiverSeed = wrapper.BindInt(section, "River seed", null, true, "Seed which determines the random river widths. By default derived from the world seed.");

    configStreams = wrapper.Bind(section, "Streams", true, true, "Enables streams.");
    configStreamSeed = wrapper.BindInt(section, "Stream seed", null, true, "Seed which determines the stream positions. By default derived from the world seed.");
    configStreamMaxAmount = wrapper.BindInt(section, "Stream max amount", 3000, true, "How many times the code tries to place a stream. This is NOT scaled with the world radius.");
    configStreamSearchIterations = wrapper.BindInt(section, "Stream search iterations", 100, true, "How many times the code tries to find a suitable start and end point.");
    configStreamStartMinAltitude = wrapper.BindFloat(section, "Stream start min altitude", -4f, true, "Minimum terrain height for stream starts.");
    configStreamStartMaxAltitude = wrapper.BindFloat(section, "Stream start max altitude", 1f, true, "Maximum terrain height for stream starts.");
    configStreamEndMinAltitude = wrapper.BindFloat(section, "Stream end min altitude", 6f, true, "Minimum terrain height for stream ends.");
    configStreamEndMaxAltitude = wrapper.BindFloat(section, "Stream end max altitude", 14f, true, "Maximum terrain height for stream ends.");
    configStreamMaxWidth = wrapper.BindFloat(section, "Stream max width", 20f, true, "For each stream, the maximum width is randomly selected between this and `Stream minimum width`.");
    configStreamMinWidth = wrapper.BindFloat(section, "Stream min width", 20f, true, "For each stream, the minimum width is randomly selected between this and selected maximum width. So the average width is closer to the `Stream minimum width` than the `Stream maximum width`");
    configStreamMinLength = wrapper.BindFloat(section, "Stream min length", 80f, true, "Minimum length for streams.");
    configStreamMaxLength = wrapper.BindFloat(section, "Stream max length", 200f, true, "Maximum length for streams.");
    configStreamCurveWaveLength = wrapper.BindFloat(section, "Stream curve wave length", 20f, true, "How wide the curves are.");
    configStreamCurveWidth = wrapper.BindFloat(section, "Stream curve width", 15f, true, "How often the stream changes direction.");
  }
}
