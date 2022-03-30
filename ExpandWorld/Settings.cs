using BepInEx.Configuration;
using ServerSync;
using Service;
using UnityEngine;

namespace ExpandWorld;
public class Settings {
  public static ConfigEntry<bool> configLocked;
  public static bool Locked => configLocked.Value;
  public static ConfigEntry<bool> configModifyBiomes;
  public static bool ModifyBiomes => configModifyBiomes.Value;
  public static ConfigEntry<string> configWorldRadius;
  public static int WorldRadius => ConfigWrapper.TryParseInt(configWorldRadius);
  public static ConfigEntry<string> configWorldEdgeSize;
  public static int WorldEdgeSize => ConfigWrapper.TryParseInt(configWorldEdgeSize);
  public static int WorldTotalRadius => WorldRadius + WorldEdgeSize;
  public static ConfigEntry<string> configMapSize;
  public static float MapSize => ConfigWrapper.TryParseFloat(configMapSize);
  public static ConfigEntry<string> configMapPixelSize;
  public static float MapPixelSize => ConfigWrapper.TryParseFloat(configMapPixelSize);
  public static ConfigEntry<string> configServerTimeout;
  public static float ServerTimeout => ConfigWrapper.TryParseFloat(configServerTimeout);
  public static ConfigEntry<string> configActivateArea;
  public static int ActiveArea => ConfigWrapper.TryParseInt(configActivateArea);


  public static ConfigEntry<bool> configRivers;
  public static bool Rivers => configRivers.Value;
  public static ConfigEntry<bool> configStreams;
  public static bool Streams => configStreams.Value;
  public static ConfigEntry<string> configWaterLevel;
  public static float WaterLevel => ConfigWrapper.TryParseFloat(configWaterLevel);
  public static ConfigEntry<string> configAltitudeMultiplier;
  public static float AltitudeMultiplier => ConfigWrapper.TryParseFloat(configAltitudeMultiplier);
  public static ConfigEntry<string> configWaveMultiplier;
  public static float WaveMultiplier => ConfigWrapper.TryParseFloat(configWaveMultiplier);
  public static ConfigEntry<bool> configWaveOnlyHeight;
  public static bool WaveOnlyHeight => configWaveOnlyHeight.Value;




  public static ConfigEntry<string> configMountainsAltitudeMin;
  public static int MountainsAltitudeMin => ConfigWrapper.TryParseInt(configMountainsAltitudeMin);

  public static ConfigEntry<int> configMeadowsMin;
  public static int MeadowsMin => configMeadowsMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configMeadowsMax;
  public static int MeadowsMax => configMeadowsMax.Value * WorldRadius / 100;
  public static ConfigEntry<int> configBlackForestMin;
  public static int BlackForestMin => configBlackForestMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configBlackForestMax;
  public static int BlackForestMax => configBlackForestMax.Value * WorldRadius / 100;
  public static ConfigEntry<int> configSwampMin;
  public static int SwampMin => configSwampMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configSwampMax;
  public static int SwampMax => configSwampMax.Value * WorldRadius / 100;
  public static ConfigEntry<int> configPlainsMin;
  public static int PlainsMin => configPlainsMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configPlainsMax;
  public static int PlainsMax => configPlainsMax.Value * WorldRadius / 100;
  public static ConfigEntry<int> configMistlandsMin;
  public static int MistlandsMin => configMistlandsMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configMistlandsMax;
  public static int MistlandsMax => configMistlandsMax.Value * WorldRadius / 100;
  public static ConfigEntry<int> configDeepNorthMin;
  public static int DeepNorthMin => configDeepNorthMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configDeepNorthMax;
  public static int DeepNorthMax => configDeepNorthMax.Value * WorldTotalRadius / 100;
  public static ConfigEntry<int> configDeepNorthCurvature;
  public static int DeepNorthCurvature => configDeepNorthCurvature.Value * WorldRadius / 100;
  public static ConfigEntry<int> configAshlandsMin;
  public static int AshlandsMin => configAshlandsMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configAshlandsMax;
  public static int AshlandsMax => configAshlandsMax.Value * WorldTotalRadius / 100;
  public static ConfigEntry<int> configAshlandsCurvature;
  public static int AshlandsCurvature => configAshlandsCurvature.Value * WorldRadius / 100;
  public static ConfigEntry<bool> configUseOffsetX;
  public static bool UseOffsetX => configUseOffsetX.Value;
  public static ConfigEntry<string> configOffsetX;
  public static int OffsetX => ConfigWrapper.TryParseInt(configOffsetX);
  public static ConfigEntry<bool> configUseOffsetY;
  public static bool UseOffsetY => configUseOffsetY.Value;
  public static ConfigEntry<string> configOffsetY;
  public static int OffsetY => ConfigWrapper.TryParseInt(configOffsetY);
  public static ConfigEntry<bool> configUseHeightSeed;
  public static bool UseHeightSeed => configUseHeightSeed.Value;
  public static ConfigEntry<string> configHeightSeed;
  public static int HeightSeed => ConfigWrapper.TryParseInt(configHeightSeed);
  public static ConfigEntry<bool> configUseBlackForestSeed;
  public static bool UseBlackForestSeed => configUseBlackForestSeed.Value;
  public static ConfigEntry<string> configBlackForestSeed;
  public static int BlackForestSeed => ConfigWrapper.TryParseInt(configBlackForestSeed);
  public static ConfigEntry<bool> configUseSwampSeed;
  public static bool UseSwampSeed => configUseSwampSeed.Value;
  public static ConfigEntry<string> configSwampSeed;
  public static int SwampSeed => ConfigWrapper.TryParseInt(configSwampSeed);
  public static ConfigEntry<bool> configUsePlainsSeed;
  public static bool UsePlainsSeed => configUsePlainsSeed.Value;
  public static ConfigEntry<string> configPlainsSeed;
  public static int PlainsSeed => ConfigWrapper.TryParseInt(configPlainsSeed);
  public static ConfigEntry<bool> configUseMistlandsSeed;
  public static bool UseMistlandSeed => configUseMistlandsSeed.Value;
  public static ConfigEntry<string> configMistlandsSeed;
  public static int MistlandsSeed => ConfigWrapper.TryParseInt(configMistlandsSeed);
  public static ConfigEntry<bool> configUseStreamSeed;
  public static bool UseStreamSeed => configUseStreamSeed.Value;
  public static ConfigEntry<string> configStreamSeed;
  public static int StreamSeed => ConfigWrapper.TryParseInt(configStreamSeed);
  public static ConfigEntry<bool> configUseRiverSeed;
  public static bool UseRiverSeed => configUseRiverSeed.Value;
  public static ConfigEntry<string> configRiverSeed;
  public static int RiverSeed => ConfigWrapper.TryParseInt(configRiverSeed);

  private static void ForceRegen(object e, System.EventArgs s) => ForceRegen();
  private static void ForceRegen() {
    if (ZoneSystem.instance != null) {
      foreach (var heightmap in Heightmap.m_heightmaps) {
        heightmap.m_buildData = null;
        heightmap.Regenerate();
      }
    }
    if (ClutterSystem.instance != null) ClutterSystem.instance.m_forceRebuild = true;
    SetMapMode.ForceRegen = true;
  }
  public static void Init(ConfigSync configSync, ConfigFile configFile) {
    var wrapper = new ConfigWrapper("expand_config", configFile, configSync);
    var section = "1. General";
    configLocked = wrapper.BindLocking(section, "Locked", false, "If locked on the server, the config can't be edited by clients.");
    configModifyBiomes = wrapper.Bind(section, "Modify biomes", true, "Can be disabled if another mod is affecting biomes.");
    configModifyBiomes.SettingChanged += ForceRegen;
    configWorldRadius = wrapper.Bind(section, "World radius", "10000", "Radius of the world in meters (excluding the edge).");
    configWorldRadius.SettingChanged += ForceRegen;
    configWorldEdgeSize = wrapper.Bind(section, "World edge size", "500", "Size of the edge area in meters.");
    configWorldEdgeSize.SettingChanged += ForceRegen;
    configMapSize = wrapper.Bind(section, "Minimap size multiplier", "1", "Multiplier to the minimap size.");
    configMapSize.SettingChanged += (e, s) => {
      if (!Minimap.instance) return;
      var newValue = (int)(MinimapAwake.OriginalTextureSize * MapSize);
      if (newValue == Minimap.instance.m_textureSize) return;
      SetMapMode.TextureSizeChanged = true;
      Minimap.instance.m_textureSize = newValue;
      Minimap.instance.m_mapImageLarge.rectTransform.localScale = new Vector3(MapSize, MapSize, MapSize);
    };
    configMapPixelSize = wrapper.Bind(section, "Minimap pixel size multiplier", "1", "Granularity of the minimap.");
    configMapPixelSize.SettingChanged += (e, s) => {
      if (!Minimap.instance) return;
      var newValue = MinimapAwake.OriginalPixelSize * MapPixelSize;
      if (newValue == Minimap.instance.m_pixelSize) return;
      SetMapMode.ForceRegen = true;
      Minimap.instance.m_pixelSize = newValue;
    };
    configServerTimeout = wrapper.Bind(section, "Timeout in servers", "30", "Timeout before clients get disconnected. Must be increased for bigger map sizes (for the initial loading).");
    configServerTimeout.SettingChanged += (e, s) => {
      ZRpc.m_timeout = ServerTimeout;
    };
    ZRpc.m_timeout = ServerTimeout;
    configActivateArea = wrapper.Bind(section, "Active area", "2", "Amounts of zones loaded around the player.");
    configActivateArea.SettingChanged += (e, s) => {
      if (ZoneSystem.instance) ZoneSystem.instance.m_activeArea = ActiveArea;
    };

    section = "2. Features";
    configRivers = wrapper.Bind(section, "Rivers", true, "Enables rivers.");
    configRivers.SettingChanged += (s, e) => {
      if (WorldGenerator.instance != null) {
        WorldGenerator.instance.m_riverPoints.Clear();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceStreams();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceRivers();
      }
      ForceRegen();
    };
    configStreams = wrapper.Bind(section, "Streams", true, "Enables streams.");
    configStreams.SettingChanged += (s, e) => {
      if (WorldGenerator.instance != null) {
        WorldGenerator.instance.m_riverPoints.Clear();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceStreams();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceRivers();
      }
      ForceRegen();
    };
    configWaterLevel = wrapper.Bind(section, "Water level", "30", "Sets the altitude of the water.");
    configWaterLevel.SettingChanged += (s, e) => {
      WaterHelper.SetLevel(ZoneSystem.instance);
      WaterHelper.SetLevel(ClutterSystem.instance);
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetLevel(obj);
      ForceRegen();
    };
    configAltitudeMultiplier = wrapper.Bind(section, "Altitude multiplier", "1", "Multiplies the world altitude.");
    configAltitudeMultiplier.SettingChanged += ForceRegen;
    configWaveMultiplier = wrapper.Bind(section, "Wave multiplier", "1", "Multiplies the wave size.");
    configWaveMultiplier.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };
    configWaveOnlyHeight = wrapper.Bind(section, "Wave only height", false, "Multiplier only affects the height.");
    configWaveOnlyHeight.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };

    section = "3. Biomes";
    configMountainsAltitudeMin = wrapper.Bind(section, "Mountains minimum altitude", "80", "");
    configMountainsAltitudeMin.SettingChanged += ForceRegen;
    configMeadowsMin = wrapper.Bind(section, "Meadows start percentage", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMeadowsMin.SettingChanged += ForceRegen;
    configMeadowsMax = wrapper.Bind(section, "Meadows end percentage", 50, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMeadowsMax.SettingChanged += ForceRegen;
    configBlackForestMin = wrapper.Bind(section, "Black forest start percentage", 6, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configBlackForestMin.SettingChanged += ForceRegen;
    configBlackForestMax = wrapper.Bind(section, "Black forest end percentage", 60, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configBlackForestMax.SettingChanged += ForceRegen;
    configSwampMin = wrapper.Bind(section, "Swamp start percentage", 20, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configSwampMin.SettingChanged += ForceRegen;
    configSwampMax = wrapper.Bind(section, "Swamp end percentage", 80, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configSwampMax.SettingChanged += ForceRegen;
    configPlainsMin = wrapper.Bind(section, "Plains start percentage", 30, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configPlainsMin.SettingChanged += ForceRegen;
    configPlainsMax = wrapper.Bind(section, "Plains end percentage", 80, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configPlainsMax.SettingChanged += ForceRegen;
    configMistlandsMin = wrapper.Bind(section, "Mistlands start percentage", 60, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMistlandsMin.SettingChanged += ForceRegen;
    configMistlandsMax = wrapper.Bind(section, "Mistlands end percentage", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMistlandsMax.SettingChanged += ForceRegen;
    configAshlandsMin = wrapper.Bind(section, "Ashlands start percentage", 80, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configAshlandsMin.SettingChanged += ForceRegen;
    configAshlandsMax = wrapper.Bind(section, "Ashlands end percentage", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configAshlandsMax.SettingChanged += ForceRegen;
    configAshlandsCurvature = wrapper.Bind(section, "Ashlands curvature percentage", 40, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configAshlandsCurvature.SettingChanged += ForceRegen;
    configDeepNorthMin = wrapper.Bind(section, "Deep north start percentage", 80, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configDeepNorthMin.SettingChanged += ForceRegen;
    configDeepNorthMax = wrapper.Bind(section, "Deep north end percentage", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configDeepNorthMax.SettingChanged += ForceRegen;
    configDeepNorthCurvature = wrapper.Bind(section, "Deep north curvature percentage", 40, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configDeepNorthCurvature.SettingChanged += ForceRegen;

    section = "4. Seed";
    configUseOffsetX = wrapper.Bind(section, "Use custom offset X", false, "Determines x coordinate on the base height map.");
    configUseOffsetX.SettingChanged += ForceRegen;
    configOffsetX = wrapper.Bind(section, "Offset X", "0", "");
    configOffsetX.SettingChanged += ForceRegen;
    configUseOffsetY = wrapper.Bind(section, "Use custom offset Y", false, "Determines y coordinate on the base height map.");
    configUseOffsetY.SettingChanged += ForceRegen;
    configOffsetY = wrapper.Bind(section, "Offset Y", "0", "");
    configOffsetY.SettingChanged += ForceRegen;
    configUseHeightSeed = wrapper.Bind(section, "Use height variation seed", false, "Determines the height variation of most biomes.");
    configUseHeightSeed.SettingChanged += ForceRegen;
    configHeightSeed = wrapper.Bind(section, "Height variation seed", "0", "");
    configHeightSeed.SettingChanged += ForceRegen;
    configUseBlackForestSeed = wrapper.Bind(section, "Use Black forest seed", false, "Determines location of Black forests.");
    configUseBlackForestSeed.SettingChanged += ForceRegen;
    configBlackForestSeed = wrapper.Bind(section, "Black forest seed", "0", "");
    configBlackForestSeed.SettingChanged += ForceRegen;
    configUseSwampSeed = wrapper.Bind(section, "Use Swamp seed", false, "Determines location of Swamps.");
    configUseSwampSeed.SettingChanged += ForceRegen;
    configSwampSeed = wrapper.Bind(section, "Swamp seed", "0", "");
    configSwampSeed.SettingChanged += ForceRegen;
    configUsePlainsSeed = wrapper.Bind(section, "Use Plains seed", false, "Determines location of Plains.");
    configUsePlainsSeed.SettingChanged += ForceRegen;
    configPlainsSeed = wrapper.Bind(section, "Plains seed", "0", "");
    configPlainsSeed.SettingChanged += ForceRegen;
    configUseMistlandsSeed = wrapper.Bind(section, "Use Mistlands seed", false, "Determines location of Mistlands.");
    configUseMistlandsSeed.SettingChanged += ForceRegen;
    configMistlandsSeed = wrapper.Bind(section, "Mistlands seed", "0", "");
    configMistlandsSeed.SettingChanged += ForceRegen;
    configUseStreamSeed = wrapper.Bind(section, "Use stream seed", false, "Determines stream generation");
    configUseStreamSeed.SettingChanged += ForceRegen;
    configStreamSeed = wrapper.Bind(section, "Stream seed", "0", "");
    configStreamSeed.SettingChanged += ForceRegen;
    configUseRiverSeed = wrapper.Bind(section, "Use river seed", false, "Determines river generation");
    configUseRiverSeed.SettingChanged += ForceRegen;
    configRiverSeed = wrapper.Bind(section, "River seed", "0", "");
    configRiverSeed.SettingChanged += ForceRegen;
  }
}
