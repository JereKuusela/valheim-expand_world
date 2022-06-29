using System.Collections.Generic;
using BepInEx.Configuration;
using ServerSync;
using Service;

namespace ExpandWorld;
public class Configuration {
#nullable disable
  public static ConfigEntry<bool> configLocked;
  public static bool Locked => configLocked.Value;
  public static ConfigEntry<bool> configModifyBiomes;
  public static bool ModifyBiomes => configModifyBiomes.Value;
  public static ConfigEntry<string> configWorldRadius;
  public static int WorldRadius = 0;
  public static ConfigEntry<string> configWorldEdgeSize;
  public static int WorldEdgeSize = 0;
  public static int WorldTotalRadius => WorldRadius + WorldEdgeSize;
  public static ConfigEntry<string> configMapSize;
  public static float MapSize = 0f;
  public static ConfigEntry<string> configMapPixelSize;
  public static float MapPixelSize = 0f;


  public static ConfigEntry<bool> configRivers;
  public static bool Rivers => configRivers.Value;
  public static ConfigEntry<bool> configStreams;
  public static bool Streams => configStreams.Value;
  public static ConfigEntry<string> configWaterLevel;
  public static float WaterLevel = 0f;
  public static ConfigEntry<string> configBaseAltitudeMultiplier;
  public static float BaseAltitudeMultiplier = 0f;
  public static ConfigEntry<string> configAltitudeMultiplier;
  public static float AltitudeMultiplier = 0f;
  public static ConfigEntry<string> configBaseAltitudeDelta;
  public static float BaseAltitudeDelta = 0f;
  public static ConfigEntry<string> configAltitudeDelta;
  public static float AltitudeDelta = 0f;
  public static ConfigEntry<string> configWaveMultiplier;
  public static float WaveMultiplier = 0f;
  public static ConfigEntry<bool> configWaveOnlyHeight;
  public static bool WaveOnlyHeight => configWaveOnlyHeight.Value;
  public static ConfigEntry<string> configForestMultiplier;
  public static float ForestMultiplier = 0f;


  public static ConfigEntry<string> configMountainsAltitudeMin;
  public static int MountainsAltitudeMin = 0;

  public static ConfigEntry<string> configDefaultBiome;
  public static string DefaultBiome => configDefaultBiome.Value;
  public static ConfigEntry<int> configBlackForestAmount;
  public static float BlackForestAmount = 0f;
  public static ConfigEntry<int> configSwampAmount;
  public static float SwampAmount = 0f;
  public static ConfigEntry<int> configPlainsAmount;
  public static float PlainsAmount = 0f;
  public static ConfigEntry<int> configMistlandsAmount;
  public static float MistlandsAmount = 0f;
  public static ConfigEntry<int> configMeadowsMin;
  public static int MeadowsMin => configMeadowsMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configMeadowsMax;
  public static int MeadowsMax => configMeadowsMax.Value * WorldRadius / 100;
  public static ConfigEntry<int> configMeadowsSectorMin;
  public static int MeadowsSectorMin => configMeadowsSectorMin.Value;
  public static ConfigEntry<int> configMeadowsSectorMax;
  public static int MeadowsSectorMax => configMeadowsSectorMax.Value;
  public static ConfigEntry<int> configBlackForestMin;
  public static int BlackForestMin => configBlackForestMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configBlackForestMax;
  public static int BlackForestMax => configBlackForestMax.Value * WorldRadius / 100;
  public static ConfigEntry<int> configBlackForestSectorMin;
  public static int BlackForestSectorMin => configBlackForestSectorMin.Value;
  public static ConfigEntry<int> configBlackForestSectorMax;
  public static int BlackForestSectorMax => configBlackForestSectorMax.Value;
  public static ConfigEntry<int> configSwampMin;
  public static int SwampMin => configSwampMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configSwampMax;
  public static int SwampMax => configSwampMax.Value * WorldRadius / 100;
  public static ConfigEntry<int> configSwampSectorMin;
  public static int SwampSectorMin => configSwampSectorMin.Value;
  public static ConfigEntry<int> configSwampSectorMax;
  public static int SwampSectorMax => configSwampSectorMax.Value;
  public static ConfigEntry<int> configMountainMin;
  public static int MountainMin => configMountainMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configMountainMax;
  public static int MountainMax => configMountainMax.Value * WorldRadius / 100;
  public static ConfigEntry<int> configMountainSectorMin;
  public static int MountainSectorMin => configMountainSectorMin.Value;
  public static ConfigEntry<int> configMountainSectorMax;
  public static int MountainSectorMax => configMountainSectorMax.Value;
  public static ConfigEntry<int> configPlainsMin;
  public static int PlainsMin => configPlainsMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configPlainsMax;
  public static int PlainsMax => configPlainsMax.Value * WorldRadius / 100;
  public static ConfigEntry<int> configPlainsSectorMin;
  public static int PlainsSectorMin => configPlainsSectorMin.Value;
  public static ConfigEntry<int> configPlainsSectorMax;
  public static int PlainsSectorMax => configPlainsSectorMax.Value;
  public static ConfigEntry<int> configMistlandsMin;
  public static int MistlandsMin => configMistlandsMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configMistlandsMax;
  public static int MistlandsMax => configMistlandsMax.Value * WorldRadius / 100;
  public static ConfigEntry<int> configMistlandsSectorMin;
  public static int MistlandsSectorMin => configMistlandsSectorMin.Value;
  public static ConfigEntry<int> configMistlandsSectorMax;
  public static int MistlandsSectorMax => configMistlandsSectorMax.Value;
  public static ConfigEntry<int> configDeepNorthMin;
  public static int DeepNorthMin => configDeepNorthMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configDeepNorthMax;
  public static int DeepNorthMax => configDeepNorthMax.Value * WorldTotalRadius / 100;
  public static ConfigEntry<int> configDeepNorthCurvature;
  public static int DeepNorthCurvature => configDeepNorthCurvature.Value * WorldRadius / 100;
  public static ConfigEntry<int> configDeepNorthSectorMin;
  public static int DeepNorthSectorMin => configDeepNorthSectorMin.Value;
  public static ConfigEntry<int> configDeepNorthSectorMax;
  public static int DeepNorthSectorMax => configDeepNorthSectorMax.Value;
  public static ConfigEntry<int> configAshlandsMin;
  public static int AshlandsMin => configAshlandsMin.Value * WorldRadius / 100;
  public static ConfigEntry<int> configAshlandsMax;
  public static int AshlandsMax => configAshlandsMax.Value * WorldTotalRadius / 100;
  public static ConfigEntry<int> configAshlandsCurvature;
  public static int AshlandsCurvature => configAshlandsCurvature.Value * WorldRadius / 100;
  public static ConfigEntry<int> configAshlandsSectorMin;
  public static int AshlandsSectorMin => configAshlandsSectorMin.Value;
  public static ConfigEntry<int> configAshlandsSectorMax;
  public static int AshlandsSectorMax => configAshlandsSectorMax.Value;
  public static ConfigEntry<bool> configUseOffsetX;
  public static bool UseOffsetX => configUseOffsetX.Value;
  public static ConfigEntry<string> configOffsetX;
  public static int OffsetX = 0;
  public static ConfigEntry<bool> configUseOffsetY;
  public static bool UseOffsetY => configUseOffsetY.Value;
  public static ConfigEntry<string> configOffsetY;
  public static int OffsetY = 0;
  public static ConfigEntry<bool> configUseHeightSeed;
  public static bool UseHeightSeed => configUseHeightSeed.Value;
  public static ConfigEntry<string> configHeightSeed;
  public static int HeightSeed = 0;
  public static ConfigEntry<bool> configUseBlackForestSeed;
  public static bool UseBlackForestSeed => configUseBlackForestSeed.Value;
  public static ConfigEntry<string> configBlackForestSeed;
  public static int BlackForestSeed = 0;
  public static ConfigEntry<bool> configUseSwampSeed;
  public static bool UseSwampSeed => configUseSwampSeed.Value;
  public static ConfigEntry<string> configSwampSeed;
  public static int SwampSeed = 0;
  public static ConfigEntry<bool> configUsePlainsSeed;
  public static bool UsePlainsSeed => configUsePlainsSeed.Value;
  public static ConfigEntry<string> configPlainsSeed;
  public static int PlainsSeed = 0;
  public static ConfigEntry<bool> configUseMistlandsSeed;
  public static bool UseMistlandSeed => configUseMistlandsSeed.Value;
  public static ConfigEntry<string> configMistlandsSeed;
  public static int MistlandsSeed = 0;
  public static ConfigEntry<bool> configUseStreamSeed;
  public static bool UseStreamSeed => configUseStreamSeed.Value;
  public static ConfigEntry<string> configStreamSeed;
  public static int StreamSeed = 0;
  public static ConfigEntry<bool> configUseRiverSeed;
  public static bool UseRiverSeed => configUseRiverSeed.Value;
  public static ConfigEntry<string> configRiverSeed;
  public static int RiverSeed = 0;
#nullable enable
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
  private static float Convert(ConfigEntry<int> entry) => 1f - (float)entry.Value / 100f;
  public static void Init(ConfigSync configSync, ConfigFile configFile) {
    ConfigWrapper wrapper = new("expand_config", configFile, configSync);
    var section = "1. General";
    configLocked = wrapper.BindLocking(section, "Locked", false, "If locked on the server, the config can't be edited by clients.");
    configModifyBiomes = wrapper.Bind(section, "Modify biomes", true, "Can be disabled if another mod is affecting biomes.");
    configModifyBiomes.SettingChanged += ForceRegen;
    configWorldRadius = wrapper.Bind(section, "World radius", "10000", "Radius of the world in meters (excluding the edge).");
    configWorldRadius.SettingChanged += ForceRegen;
    configWorldRadius.SettingChanged += (s, e) => WorldRadius = ConfigWrapper.TryParseInt(configWorldRadius);
    WorldRadius = ConfigWrapper.TryParseInt(configWorldRadius);
    configWorldEdgeSize = wrapper.Bind(section, "World edge size", "500", "Size of the edge area in meters.");
    configWorldEdgeSize.SettingChanged += ForceRegen;
    configWorldEdgeSize.SettingChanged += (s, e) => WorldEdgeSize = ConfigWrapper.TryParseInt(configWorldEdgeSize);
    WorldEdgeSize = ConfigWrapper.TryParseInt(configWorldEdgeSize);
    configMapSize = wrapper.Bind(section, "Minimap size multiplier", "1", "Multiplier to the minimap size.");
    configMapSize.SettingChanged += (e, s) => {
      MapSize = ConfigWrapper.TryParseFloat(configMapSize);
      if (!Minimap.instance) return;
      var newValue = (int)(MinimapAwake.OriginalTextureSize * MapSize);
      if (newValue == Minimap.instance.m_textureSize) return;
      SetMapMode.TextureSizeChanged = true;
      Minimap.instance.m_textureSize = newValue;
      Minimap.instance.m_mapImageLarge.rectTransform.localScale = new(MapSize, MapSize, MapSize);
    };
    MapSize = ConfigWrapper.TryParseFloat(configMapSize);
    configMapPixelSize = wrapper.Bind(section, "Minimap pixel size multiplier", "1", "Granularity of the minimap.");
    configMapPixelSize.SettingChanged += (e, s) => {
      MapPixelSize = ConfigWrapper.TryParseFloat(configMapPixelSize);
      if (!Minimap.instance) return;
      var newValue = MinimapAwake.OriginalPixelSize * MapPixelSize;
      if (newValue == Minimap.instance.m_pixelSize) return;
      SetMapMode.ForceRegen = true;
      Minimap.instance.m_pixelSize = newValue;
    };
    MapPixelSize = ConfigWrapper.TryParseFloat(configMapPixelSize);

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
    configWaterLevel.SettingChanged += (s, e) => WaterLevel = ConfigWrapper.TryParseFloat(configWaterLevel);
    WaterLevel = ConfigWrapper.TryParseFloat(configWaterLevel);
    configForestMultiplier = wrapper.Bind(section, "Forest multiplier", "1", "Multiplies the amount of forest.");
    configForestMultiplier.SettingChanged += ForceRegen;
    configForestMultiplier.SettingChanged += (s, e) => ForestMultiplier = ConfigWrapper.TryParseFloat(configForestMultiplier);
    ForestMultiplier = ConfigWrapper.TryParseFloat(configForestMultiplier);
    configBaseAltitudeMultiplier = wrapper.Bind(section, "Base altitude multiplier", "1", "Multiplies the base altitude.");
    configBaseAltitudeMultiplier.SettingChanged += ForceRegen;
    configBaseAltitudeMultiplier.SettingChanged += (s, e) => BaseAltitudeMultiplier = ConfigWrapper.TryParseFloat(configBaseAltitudeMultiplier);
    BaseAltitudeMultiplier = ConfigWrapper.TryParseFloat(configBaseAltitudeMultiplier);
    configAltitudeMultiplier = wrapper.Bind(section, "Altitude multiplier", "1", "Multiplies the biome altitude.");
    configAltitudeMultiplier.SettingChanged += ForceRegen;
    configAltitudeMultiplier.SettingChanged += (s, e) => AltitudeMultiplier = ConfigWrapper.TryParseFloat(configAltitudeMultiplier);
    AltitudeMultiplier = ConfigWrapper.TryParseFloat(configAltitudeMultiplier);
    configBaseAltitudeDelta = wrapper.Bind(section, "Base altitude delta", "0", "Adds to the base altitude.");
    configBaseAltitudeDelta.SettingChanged += ForceRegen;
    configBaseAltitudeDelta.SettingChanged += (s, e) => BaseAltitudeDelta = ConfigWrapper.TryParseFloat(configBaseAltitudeDelta);
    BaseAltitudeDelta = ConfigWrapper.TryParseFloat(configBaseAltitudeDelta);
    configAltitudeDelta = wrapper.Bind(section, "Altitude delta", "0", "Adds to the biome altitude.");
    configAltitudeDelta.SettingChanged += ForceRegen;
    configAltitudeDelta.SettingChanged += (s, e) => AltitudeDelta = ConfigWrapper.TryParseFloat(configAltitudeDelta);
    AltitudeDelta = ConfigWrapper.TryParseFloat(configAltitudeDelta);
    configWaveMultiplier = wrapper.Bind(section, "Wave multiplier", "1", "Multiplies the wave size.");
    configWaveMultiplier.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };
    configWaveMultiplier.SettingChanged += (s, e) => WaveMultiplier = ConfigWrapper.TryParseFloat(configWaveMultiplier);
    WaveMultiplier = ConfigWrapper.TryParseFloat(configWaveMultiplier);
    configWaveOnlyHeight = wrapper.Bind(section, "Wave only height", false, "Multiplier only affects the height.");
    configWaveOnlyHeight.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };

    section = "3. Biomes";
    List<string> biomes = new() {
      Heightmap.Biome.AshLands.ToString(),
      Heightmap.Biome.BlackForest.ToString(),
      Heightmap.Biome.DeepNorth.ToString(),
      Heightmap.Biome.Meadows.ToString(),
      Heightmap.Biome.Mistlands.ToString(),
      Heightmap.Biome.Mountain.ToString(),
      Heightmap.Biome.Ocean.ToString(),
      Heightmap.Biome.Plains.ToString(),
      Heightmap.Biome.Swamp.ToString(),
      Heightmap.Biome.None.ToString()
    };
    biomes.Sort();
    configDefaultBiome = wrapper.Bind(section, "Default biome", Heightmap.Biome.BlackForest.ToString(), new ConfigDescription("", new AcceptableValueList<string>(biomes.ToArray())));
    configDefaultBiome.SettingChanged += ForceRegen;
    configMountainsAltitudeMin = wrapper.Bind(section, "Mountains minimum altitude", "80", "");
    configMountainsAltitudeMin.SettingChanged += ForceRegen;
    configMountainsAltitudeMin.SettingChanged += (s, e) => MountainsAltitudeMin = ConfigWrapper.TryParseInt(configMountainsAltitudeMin);
    MountainsAltitudeMin = ConfigWrapper.TryParseInt(configMountainsAltitudeMin);
    configMeadowsMin = wrapper.Bind(section, "Meadows begin percentage", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMeadowsMin.SettingChanged += ForceRegen;
    configMeadowsMax = wrapper.Bind(section, "Meadows end percentage", 50, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMeadowsMax.SettingChanged += ForceRegen;
    configMeadowsSectorMin = wrapper.Bind(section, "Meadows sector begin percentage", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMeadowsSectorMin.SettingChanged += ForceRegen;
    configMeadowsSectorMax = wrapper.Bind(section, "Meadows sector end percentage", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMeadowsSectorMax.SettingChanged += ForceRegen;
    configBlackForestMin = wrapper.Bind(section, "Black forest begin percentage", 6, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configBlackForestMin.SettingChanged += ForceRegen;
    configBlackForestMax = wrapper.Bind(section, "Black forest end percentage", 60, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configBlackForestMax.SettingChanged += ForceRegen;
    configBlackForestSectorMin = wrapper.Bind(section, "Black forest sector begin percentage", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configBlackForestSectorMin.SettingChanged += ForceRegen;
    configBlackForestSectorMax = wrapper.Bind(section, "Black forest sector end percentage", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configBlackForestSectorMax.SettingChanged += ForceRegen;
    configSwampMin = wrapper.Bind(section, "Swamp begin percentage", 20, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configSwampMin.SettingChanged += ForceRegen;
    configSwampMax = wrapper.Bind(section, "Swamp end percentage", 80, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configSwampMax.SettingChanged += ForceRegen;
    configSwampSectorMin = wrapper.Bind(section, "Swamp sector begin percentage", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configSwampSectorMin.SettingChanged += ForceRegen;
    configSwampSectorMax = wrapper.Bind(section, "Swamp sector end percentage", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configSwampSectorMax.SettingChanged += ForceRegen;
    configMountainMin = wrapper.Bind(section, "Mountain begin percentage", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMountainMin.SettingChanged += ForceRegen;
    configMountainMax = wrapper.Bind(section, "Mountain end percentage", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMountainMax.SettingChanged += ForceRegen;
    configMountainSectorMin = wrapper.Bind(section, "Mountain sector begin percentage", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMountainSectorMin.SettingChanged += ForceRegen;
    configMountainSectorMax = wrapper.Bind(section, "Mountain sector end percentage", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMountainSectorMax.SettingChanged += ForceRegen;
    configPlainsMin = wrapper.Bind(section, "Plains begin percentage", 30, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configPlainsMin.SettingChanged += ForceRegen;
    configPlainsMax = wrapper.Bind(section, "Plains end percentage", 80, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configPlainsMax.SettingChanged += ForceRegen;
    configPlainsSectorMin = wrapper.Bind(section, "Plains sector begin percentage", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configPlainsSectorMin.SettingChanged += ForceRegen;
    configPlainsSectorMax = wrapper.Bind(section, "Plains sector end percentage", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configPlainsSectorMax.SettingChanged += ForceRegen;
    configMistlandsMin = wrapper.Bind(section, "Mistlands begin percentage", 60, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMistlandsMin.SettingChanged += ForceRegen;
    configMistlandsMax = wrapper.Bind(section, "Mistlands end percentage", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMistlandsMax.SettingChanged += ForceRegen;
    configMistlandsSectorMin = wrapper.Bind(section, "Mistlands sector begin percentage", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMistlandsSectorMin.SettingChanged += ForceRegen;
    configMistlandsSectorMax = wrapper.Bind(section, "Mistlands sector end percentage", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMistlandsSectorMax.SettingChanged += ForceRegen;
    configAshlandsMin = wrapper.Bind(section, "Ashlands begin percentage", 80, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configAshlandsMin.SettingChanged += ForceRegen;
    configAshlandsMax = wrapper.Bind(section, "Ashlands end percentage", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configAshlandsMax.SettingChanged += ForceRegen;
    configAshlandsCurvature = wrapper.Bind(section, "Ashlands curvature percentage", 40, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configAshlandsCurvature.SettingChanged += ForceRegen;
    configAshlandsSectorMin = wrapper.Bind(section, "Ashlands sector begin percentage", 75, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configAshlandsSectorMin.SettingChanged += ForceRegen;
    configAshlandsSectorMax = wrapper.Bind(section, "Ashlands sector end percentage", 25, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configAshlandsSectorMax.SettingChanged += ForceRegen;
    configDeepNorthMin = wrapper.Bind(section, "Deep north begin percentage", 80, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configDeepNorthMin.SettingChanged += ForceRegen;
    configDeepNorthMax = wrapper.Bind(section, "Deep north end percentage", 100, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configDeepNorthMax.SettingChanged += ForceRegen;
    configDeepNorthCurvature = wrapper.Bind(section, "Deep north curvature percentage", 40, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configDeepNorthCurvature.SettingChanged += ForceRegen;
    configDeepNorthSectorMin = wrapper.Bind(section, "Deep north sector begin percentage", 25, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configDeepNorthSectorMin.SettingChanged += ForceRegen;
    configDeepNorthSectorMax = wrapper.Bind(section, "Deep north sector end percentage", 75, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configDeepNorthSectorMax.SettingChanged += ForceRegen;

    configBlackForestAmount = wrapper.Bind(section, "Black forest amount", 40, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configBlackForestAmount.SettingChanged += ForceRegen;
    configBlackForestAmount.SettingChanged += (s, e) => BlackForestAmount = Convert(configBlackForestAmount);
    BlackForestAmount = Convert(configBlackForestAmount);
    configSwampAmount = wrapper.Bind(section, "Swamp amount", 60, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configSwampAmount.SettingChanged += ForceRegen;
    configSwampAmount.SettingChanged += (s, e) => SwampAmount = Convert(configSwampAmount);
    SwampAmount = Convert(configSwampAmount);
    configPlainsAmount = wrapper.Bind(section, "Plains amount", 40, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configPlainsAmount.SettingChanged += ForceRegen;
    configPlainsAmount.SettingChanged += (s, e) => PlainsAmount = Convert(configPlainsAmount);
    PlainsAmount = Convert(configPlainsAmount);
    configMistlandsAmount = wrapper.Bind(section, "Mistlands amount", 50, new ConfigDescription("", new AcceptableValueRange<int>(0, 100)));
    configMistlandsAmount.SettingChanged += ForceRegen;
    configMistlandsAmount.SettingChanged += (s, e) => MistlandsAmount = Convert(configMistlandsAmount);
    MistlandsAmount = Convert(configMistlandsAmount);

    section = "4. Seed";
    configUseOffsetX = wrapper.Bind(section, "Use custom offset X", false, "Determines x coordinate on the base height map.");
    configUseOffsetX.SettingChanged += ForceRegen;
    configOffsetX = wrapper.Bind(section, "Offset X", "0", "");
    configOffsetX.SettingChanged += ForceRegen;
    configOffsetX.SettingChanged += (s, e) => OffsetX = ConfigWrapper.TryParseInt(configOffsetX);
    OffsetX = ConfigWrapper.TryParseInt(configOffsetX);
    configUseOffsetY = wrapper.Bind(section, "Use custom offset Y", false, "Determines y coordinate on the base height map.");
    configUseOffsetY.SettingChanged += ForceRegen;
    configOffsetY = wrapper.Bind(section, "Offset Y", "0", "");
    configOffsetY.SettingChanged += ForceRegen;
    configOffsetY.SettingChanged += (s, e) => OffsetY = ConfigWrapper.TryParseInt(configOffsetY);
    OffsetY = ConfigWrapper.TryParseInt(configOffsetY);
    configUseHeightSeed = wrapper.Bind(section, "Use height variation seed", false, "Determines the height variation of most biomes.");
    configUseHeightSeed.SettingChanged += ForceRegen;
    configHeightSeed = wrapper.Bind(section, "Height variation seed", "0", "");
    configHeightSeed.SettingChanged += ForceRegen;
    configHeightSeed.SettingChanged += (s, e) => HeightSeed = ConfigWrapper.TryParseInt(configHeightSeed);
    HeightSeed = ConfigWrapper.TryParseInt(configHeightSeed);

    configUseBlackForestSeed = wrapper.Bind(section, "Use Black forest seed", false, "Determines location of Black forests.");
    configUseBlackForestSeed.SettingChanged += ForceRegen;
    configBlackForestSeed = wrapper.Bind(section, "Black forest seed", "0", "");
    configBlackForestSeed.SettingChanged += ForceRegen;
    configBlackForestSeed.SettingChanged += (s, e) => BlackForestSeed = ConfigWrapper.TryParseInt(configBlackForestSeed);
    BlackForestSeed = ConfigWrapper.TryParseInt(configBlackForestSeed);
    configUseSwampSeed = wrapper.Bind(section, "Use Swamp seed", false, "Determines location of Swamps.");
    configUseSwampSeed.SettingChanged += ForceRegen;
    configSwampSeed = wrapper.Bind(section, "Swamp seed", "0", "");
    configSwampSeed.SettingChanged += ForceRegen;
    configSwampSeed.SettingChanged += (s, e) => SwampSeed = ConfigWrapper.TryParseInt(configSwampSeed);
    SwampSeed = ConfigWrapper.TryParseInt(configSwampSeed);
    configUsePlainsSeed = wrapper.Bind(section, "Use Plains seed", false, "Determines location of Plains.");
    configUsePlainsSeed.SettingChanged += ForceRegen;
    configPlainsSeed = wrapper.Bind(section, "Plains seed", "0", "");
    configPlainsSeed.SettingChanged += ForceRegen;
    configPlainsSeed.SettingChanged += (s, e) => PlainsSeed = ConfigWrapper.TryParseInt(configPlainsSeed);
    PlainsSeed = ConfigWrapper.TryParseInt(configPlainsSeed);
    configUseMistlandsSeed = wrapper.Bind(section, "Use Mistlands seed", false, "Determines location of Mistlands.");
    configUseMistlandsSeed.SettingChanged += ForceRegen;
    configMistlandsSeed = wrapper.Bind(section, "Mistlands seed", "0", "");
    configMistlandsSeed.SettingChanged += ForceRegen;
    configMistlandsSeed.SettingChanged += (s, e) => MistlandsSeed = ConfigWrapper.TryParseInt(configMistlandsSeed);
    MistlandsSeed = ConfigWrapper.TryParseInt(configMistlandsSeed);

    configUseStreamSeed = wrapper.Bind(section, "Use stream seed", false, "Determines stream generation");
    configUseStreamSeed.SettingChanged += ForceRegen;
    configStreamSeed = wrapper.Bind(section, "Stream seed", "0", "");
    configStreamSeed.SettingChanged += ForceRegen;
    configStreamSeed.SettingChanged += (s, e) => StreamSeed = ConfigWrapper.TryParseInt(configStreamSeed);
    StreamSeed = ConfigWrapper.TryParseInt(configStreamSeed);
    configUseRiverSeed = wrapper.Bind(section, "Use river seed", false, "Determines river generation");
    configUseRiverSeed.SettingChanged += ForceRegen;
    configRiverSeed = wrapper.Bind(section, "River seed", "0", "");
    configRiverSeed.SettingChanged += ForceRegen;
    configRiverSeed.SettingChanged += (s, e) => RiverSeed = ConfigWrapper.TryParseInt(configRiverSeed);
    RiverSeed = ConfigWrapper.TryParseInt(configRiverSeed);
  }
}
