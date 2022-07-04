using System.Collections.Generic;
using BepInEx.Configuration;
using Service;

namespace ExpandWorld;
public class Configuration {
#nullable disable
  public static ConfigEntry<bool> configLocked;
  public static bool Locked => configLocked.Value;
  public static ConfigEntry<bool> configModifyBiomes;
  public static bool ModifyBiomes => configModifyBiomes.Value;
  public static ConfigEntry<string> configWorldRadius;
  public static float WorldRadius => ConfigWrapper.Floats[configWorldRadius];
  public static ConfigEntry<string> configWorldEdgeSize;
  public static float WorldEdgeSize => ConfigWrapper.Floats[configWorldEdgeSize];
  public static float WorldTotalRadius => WorldRadius + WorldEdgeSize;
  public static ConfigEntry<string> configMapSize;
  public static float MapSize => ConfigWrapper.Floats[configMapSize];
  public static ConfigEntry<string> configMapPixelSize;
  public static float MapPixelSize => ConfigWrapper.Floats[configMapPixelSize];


  public static ConfigEntry<bool> configRivers;
  public static bool Rivers => configRivers.Value;
  public static ConfigEntry<bool> configStreams;
  public static bool Streams => configStreams.Value;
  public static ConfigEntry<string> configWaterLevel;
  public static float WaterLevel => ConfigWrapper.Floats[configWaterLevel];
  public static ConfigEntry<string> configBaseAltitudeMultiplier;
  public static float BaseAltitudeMultiplier => ConfigWrapper.Floats[configBaseAltitudeMultiplier];
  public static ConfigEntry<string> configAltitudeMultiplier;
  public static float AltitudeMultiplier => ConfigWrapper.Floats[configAltitudeMultiplier];
  public static ConfigEntry<string> configBaseAltitudeDelta;
  public static float BaseAltitudeDelta => ConfigWrapper.Floats[configBaseAltitudeDelta];
  public static ConfigEntry<string> configAltitudeDelta;
  public static float AltitudeDelta => ConfigWrapper.Floats[configAltitudeDelta];
  public static ConfigEntry<string> configWaveMultiplier;
  public static float WaveMultiplier => ConfigWrapper.Floats[configWaveMultiplier];
  public static ConfigEntry<string> configLocationsMultiplier;
  public static float LocationsMultiplier => ConfigWrapper.Floats[configLocationsMultiplier];
  public static ConfigEntry<bool> configWaveOnlyHeight;
  public static bool WaveOnlyHeight => configWaveOnlyHeight.Value;
  public static ConfigEntry<string> configForestMultiplier;
  public static float ForestMultiplier => ConfigWrapper.Floats[configForestMultiplier];
  public static ConfigEntry<string> configWorldStretch;
  public static float WorldStretch => ConfigWrapper.Floats[configWorldStretch] == 0f ? 1f : ConfigWrapper.Floats[configWorldStretch];
  public static ConfigEntry<string> configBiomeStretch;
  public static float BiomeStretch => ConfigWrapper.Floats[configBiomeStretch] == 0f ? 1f : ConfigWrapper.Floats[configBiomeStretch];

  public static ConfigEntry<string> configDistanceWiggleLength;
  public static float DistanceWiggleLength => ConfigWrapper.Floats[configDistanceWiggleLength];
  public static ConfigEntry<string> configDistanceWiggleWidth;
  public static float DistanceWiggleWidth => ConfigWrapper.Floats[configDistanceWiggleWidth];
  public static ConfigEntry<string> configWiggleFrequency;
  public static float WiggleFrequency => ConfigWrapper.Floats[configWiggleFrequency];
  public static ConfigEntry<string> configWiggleWidth;
  public static float WiggleWidth => ConfigWrapper.Floats[configWiggleWidth];

  public static ConfigEntry<string> configMountainsAltitudeMin;
  public static float MountainsAltitudeMin => ConfigWrapper.Floats[configMountainsAltitudeMin];

  public static ConfigEntry<string> configDefaultBiome;
  public static string DefaultBiome => configDefaultBiome.Value;

  public static ConfigEntry<string> configInternalDataBiome;
  public static ConfigEntry<string> configInternalDataSpawns;
  public static ConfigEntry<string> configInternalDataEvents;
  public static ConfigEntry<bool> configDataEvents;
  public static bool DataEvents => configDataEvents.Value;
  public static ConfigEntry<bool> configDataSpawns;
  public static bool DataSpawns => configDataSpawns.Value;
  public static ConfigEntry<bool> configDataVegetation;
  public static bool DataVegetation => configDataVegetation.Value;
  public static ConfigEntry<bool> configDataLocation;
  public static bool DataLocation => configDataLocation.Value;
  public static ConfigEntry<bool> configDataBiome;
  public static bool DataBiome => configDataBiome.Value;

  public static ConfigEntry<string> configMeadowsAltitudeMultiplier;
  public static float MeadowsAltitudeMultiplier => ConfigWrapper.Floats[configMeadowsAltitudeMultiplier];
  public static ConfigEntry<string> configMeadowsAltitudeDelta;
  public static float MeadowsAltitudeDelta => ConfigWrapper.Floats[configMeadowsAltitudeDelta];
  public static ConfigEntry<string> configBlackForestAltitudeMultiplier;
  public static float BlackForestAltitudeMultiplier => ConfigWrapper.Floats[configBlackForestAltitudeMultiplier];
  public static ConfigEntry<string> configBlackForestAltitudeDelta;
  public static float BlackForestAltitudeDelta => ConfigWrapper.Floats[configBlackForestAltitudeDelta];
  public static ConfigEntry<string> configSwampsAltitudeMultiplier;
  public static float SwampsAltitudeMultiplier => ConfigWrapper.Floats[configSwampsAltitudeMultiplier];
  public static ConfigEntry<string> configSwampsAltitudeDelta;
  public static float SwampsAltitudeDelta => ConfigWrapper.Floats[configSwampsAltitudeDelta];
  public static ConfigEntry<string> configMountainAltitudeMultiplier;
  public static float MountainAltitudeMultiplier => ConfigWrapper.Floats[configMountainAltitudeMultiplier];
  public static ConfigEntry<string> configMountainAltitudeDelta;
  public static float MountainAltitudeDelta => ConfigWrapper.Floats[configMountainAltitudeDelta];
  public static ConfigEntry<string> configPlainsAltitudeMultiplier;
  public static float PlainsAltitudeMultiplier => ConfigWrapper.Floats[configPlainsAltitudeMultiplier];
  public static ConfigEntry<string> configPlainsAltitudeDelta;
  public static float PlainsAltitudeDelta => ConfigWrapper.Floats[configPlainsAltitudeDelta];
  public static ConfigEntry<string> configMistlandsAltitudeMultiplier;
  public static float MistlandsAltitudeMultiplier => ConfigWrapper.Floats[configMistlandsAltitudeMultiplier];
  public static ConfigEntry<string> configMistlandsAltitudeDelta;
  public static float MistlandsAltitudeDelta => ConfigWrapper.Floats[configMistlandsAltitudeDelta];
  public static ConfigEntry<string> configAshlandsAltitudeMultiplier;
  public static float AshlandsAltitudeMultiplier => ConfigWrapper.Floats[configAshlandsAltitudeMultiplier];
  public static ConfigEntry<string> configAshlandsAltitudeDelta;
  public static float AshlandsAltitudeDelta => ConfigWrapper.Floats[configAshlandsAltitudeDelta];
  public static ConfigEntry<string> configDeepNorthAltitudeMultiplier;
  public static float DeepNorthAltitudeMultiplier => ConfigWrapper.Floats[configDeepNorthAltitudeMultiplier];
  public static ConfigEntry<string> configDeepNorthAltitudeDelta;
  public static float DeepNorthAltitudeDelta => ConfigWrapper.Floats[configDeepNorthAltitudeDelta];
  public static ConfigEntry<string> configOceanAltitudeMultiplier;
  public static float OceanAltitudeMultiplier => ConfigWrapper.Floats[configOceanAltitudeMultiplier];
  public static ConfigEntry<string> configOceanAltitudeDelta;
  public static float OceanAltitudeDelta => ConfigWrapper.Floats[configOceanAltitudeDelta];


  public static ConfigEntry<int> configBlackForestAmount;
  public static float BlackForestAmount => ConfigWrapper.Amounts[configBlackForestAmount];
  public static ConfigEntry<int> configSwampAmount;
  public static float SwampAmount => ConfigWrapper.Amounts[configSwampAmount];
  public static ConfigEntry<int> configPlainsAmount;
  public static float PlainsAmount => ConfigWrapper.Amounts[configPlainsAmount];
  public static ConfigEntry<int> configMistlandsAmount;
  public static float MistlandsAmount => ConfigWrapper.Amounts[configMistlandsAmount];
  public static ConfigEntry<int> configMeadowsMin;
  private static int ConvertDist(ConfigEntry<int> entry) => (int)(entry.Value * WorldRadius / 100);
  public static int MeadowsMin => ConvertDist(configMeadowsMin);
  public static ConfigEntry<int> configMeadowsMax;
  public static int MeadowsMax => ConvertDist(configMeadowsMax);
  public static ConfigEntry<int> configMeadowsSectorMin;
  public static int MeadowsSectorMin => configMeadowsSectorMin.Value;
  public static ConfigEntry<int> configMeadowsSectorMax;
  public static int MeadowsSectorMax => configMeadowsSectorMax.Value;
  public static ConfigEntry<int> configBlackForestMin;
  public static int BlackForestMin => ConvertDist(configBlackForestMin);
  public static ConfigEntry<int> configBlackForestMax;
  public static int BlackForestMax => ConvertDist(configBlackForestMax);
  public static ConfigEntry<int> configBlackForestSectorMin;
  public static int BlackForestSectorMin => ConvertDist(configBlackForestSectorMin);
  public static ConfigEntry<int> configBlackForestSectorMax;
  public static int BlackForestSectorMax => ConvertDist(configBlackForestSectorMax);
  public static ConfigEntry<int> configSwampMin;
  public static int SwampMin => ConvertDist(configSwampMin);
  public static ConfigEntry<int> configSwampMax;
  public static int SwampMax => ConvertDist(configSwampMax);
  public static ConfigEntry<int> configSwampSectorMin;
  public static int SwampSectorMin => configSwampSectorMin.Value;
  public static ConfigEntry<int> configSwampSectorMax;
  public static int SwampSectorMax => configSwampSectorMax.Value;
  public static ConfigEntry<int> configMountainMin;
  public static int MountainMin => ConvertDist(configMountainMin);
  public static ConfigEntry<int> configMountainMax;
  public static int MountainMax => ConvertDist(configMountainMax);
  public static ConfigEntry<int> configMountainSectorMin;
  public static int MountainSectorMin => configMountainSectorMin.Value;
  public static ConfigEntry<int> configMountainSectorMax;
  public static int MountainSectorMax => configMountainSectorMax.Value;
  public static ConfigEntry<int> configPlainsMin;
  public static int PlainsMin => ConvertDist(configPlainsMin);
  public static ConfigEntry<int> configPlainsMax;
  public static int PlainsMax => ConvertDist(configPlainsMax);
  public static ConfigEntry<int> configPlainsSectorMin;
  public static int PlainsSectorMin => configPlainsSectorMin.Value;
  public static ConfigEntry<int> configPlainsSectorMax;
  public static int PlainsSectorMax => configPlainsSectorMax.Value;
  public static ConfigEntry<int> configMistlandsMin;
  public static int MistlandsMin => ConvertDist(configMistlandsMin);
  public static ConfigEntry<int> configMistlandsMax;
  public static int MistlandsMax => ConvertDist(configMistlandsMax);
  public static ConfigEntry<int> configMistlandsSectorMin;
  public static int MistlandsSectorMin => configMistlandsSectorMin.Value;
  public static ConfigEntry<int> configMistlandsSectorMax;
  public static int MistlandsSectorMax => configMistlandsSectorMax.Value;
  public static ConfigEntry<int> configDeepNorthMin;
  public static int DeepNorthMin => ConvertDist(configDeepNorthMin);
  public static ConfigEntry<int> configDeepNorthMax;
  public static int DeepNorthMax => ConvertDist(configDeepNorthMax);
  public static ConfigEntry<int> configDeepNorthCurvature;
  public static int DeepNorthCurvature => ConvertDist(configDeepNorthCurvature);
  public static ConfigEntry<int> configDeepNorthSectorMin;
  public static int DeepNorthSectorMin => configDeepNorthSectorMin.Value;
  public static ConfigEntry<int> configDeepNorthSectorMax;
  public static int DeepNorthSectorMax => configDeepNorthSectorMax.Value;
  public static ConfigEntry<int> configAshlandsMin;
  public static int AshlandsMin => ConvertDist(configAshlandsMin);
  public static ConfigEntry<int> configAshlandsMax;
  public static int AshlandsMax => ConvertDist(configAshlandsMax);
  public static ConfigEntry<int> configAshlandsCurvature;
  public static int AshlandsCurvature => ConvertDist(configAshlandsCurvature);
  public static ConfigEntry<int> configAshlandsSectorMin;
  public static int AshlandsSectorMin => configAshlandsSectorMin.Value;
  public static ConfigEntry<int> configAshlandsSectorMax;
  public static int AshlandsSectorMax => configAshlandsSectorMax.Value;
  public static ConfigEntry<bool> configUseOffsetX;
  public static bool UseOffsetX => configUseOffsetX.Value;
  public static ConfigEntry<string> configOffsetX;
  public static int OffsetX => ConfigWrapper.Ints[configOffsetX];
  public static ConfigEntry<bool> configUseOffsetY;
  public static bool UseOffsetY => configUseOffsetY.Value;
  public static ConfigEntry<string> configOffsetY;
  public static int OffsetY => ConfigWrapper.Ints[configOffsetY];
  public static ConfigEntry<bool> configUseHeightSeed;
  public static bool UseHeightSeed => configUseHeightSeed.Value;
  public static ConfigEntry<string> configHeightSeed;
  public static int HeightSeed => ConfigWrapper.Ints[configHeightSeed];
  public static ConfigEntry<bool> configUseBlackForestSeed;
  public static bool UseBlackForestSeed => configUseBlackForestSeed.Value;
  public static ConfigEntry<string> configBlackForestSeed;
  public static int BlackForestSeed => ConfigWrapper.Ints[configBlackForestSeed];
  public static ConfigEntry<bool> configUseSwampSeed;
  public static bool UseSwampSeed => configUseSwampSeed.Value;
  public static ConfigEntry<string> configSwampSeed;
  public static int SwampSeed => ConfigWrapper.Ints[configSwampSeed];
  public static ConfigEntry<bool> configUsePlainsSeed;
  public static bool UsePlainsSeed => configUsePlainsSeed.Value;
  public static ConfigEntry<string> configPlainsSeed;
  public static int PlainsSeed => ConfigWrapper.Ints[configPlainsSeed];
  public static ConfigEntry<bool> configUseMistlandsSeed;
  public static bool UseMistlandSeed => configUseMistlandsSeed.Value;
  public static ConfigEntry<string> configMistlandsSeed;
  public static int MistlandsSeed => ConfigWrapper.Ints[configMistlandsSeed];
  public static ConfigEntry<bool> configUseStreamSeed;
  public static bool UseStreamSeed => configUseStreamSeed.Value;
  public static ConfigEntry<string> configStreamSeed;
  public static int StreamSeed => ConfigWrapper.Ints[configStreamSeed];
  public static ConfigEntry<bool> configUseRiverSeed;
  public static bool UseRiverSeed => configUseRiverSeed.Value;
  public static ConfigEntry<string> configRiverSeed;
  public static int RiverSeed => ConfigWrapper.Ints[configRiverSeed];
#nullable enable
  public static void Init(ConfigWrapper wrapper) {
    var section = "1. General";
    configLocked = wrapper.BindLocking(section, "Locked", false, "If locked on the server, the config can't be edited by clients.");
    configModifyBiomes = wrapper.Bind(section, "Modify biomes", true, "If disabled, most biome related settings won't work. Intended for compatibility.");
    configWorldRadius = wrapper.BindFloat(section, "World radius", 10000f, "Radius of the world in meters (excluding the edge).");
    configWorldEdgeSize = wrapper.BindFloat(section, "World edge size", 500f, "Size of the edge area in meters (added to the radius for the total size).");
    configMapSize = wrapper.BindFloat(section, "Minimap size", 1f, "Increases the minimap size, but also significantly increases the generation time.");
    configMapSize.SettingChanged += (e, s) => {
      if (!Minimap.instance) return;
      var newValue = (int)(MinimapAwake.OriginalTextureSize * MapSize);
      if (newValue == Minimap.instance.m_textureSize) return;
      SetMapMode.TextureSizeChanged = true;
      Minimap.instance.m_textureSize = newValue;
      Minimap.instance.m_mapImageLarge.rectTransform.localScale = new(MapSize, MapSize, MapSize);
    };
    configMapPixelSize = wrapper.BindFloat(section, "Minimap pixel size", 1f, "Decreases the minimap detail, but doesn't affect the generation time.");
    configMapPixelSize.SettingChanged += (e, s) => {
      if (!Minimap.instance) return;
      var newValue = MinimapAwake.OriginalPixelSize * MapPixelSize;
      if (newValue == Minimap.instance.m_pixelSize) return;
      SetMapMode.ForceRegen = true;
      Minimap.instance.m_pixelSize = newValue;
    };
    configWorldStretch = wrapper.BindFloat(section, "Stretch world", 1f, "Stretches the world to a bigger area.");
    configBiomeStretch = wrapper.BindFloat(section, "Stretch biomes", 1f, "Stretches the biomes to a bigger area.");

    section = "2. Features";
    configRivers = wrapper.Bind(section, "Rivers", true, "Enables rivers.");
    configRivers.SettingChanged += (s, e) => {
      if (WorldGenerator.instance != null) {
        WorldGenerator.instance.m_riverPoints.Clear();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceStreams();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceRivers();
      }
    };
    configStreams = wrapper.Bind(section, "Streams", true, "Enables streams.");
    configStreams.SettingChanged += (s, e) => {
      if (WorldGenerator.instance != null) {
        WorldGenerator.instance.m_riverPoints.Clear();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceStreams();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceRivers();
      }
    };
    configWaterLevel = wrapper.BindFloat(section, "Water level", 30f, "Sets the altitude of the water.");
    configWaterLevel.SettingChanged += (s, e) => {
      WaterHelper.SetLevel(ZoneSystem.instance);
      WaterHelper.SetLevel(ClutterSystem.instance);
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetLevel(obj);
    };
    configForestMultiplier = wrapper.BindFloat(section, "Forest multiplier", 1f, "Multiplies the amount of forest.");
    configBaseAltitudeMultiplier = wrapper.BindFloat(section, "Base altitude multiplier", 1f, "Multiplies the base altitude.");
    configAltitudeMultiplier = wrapper.BindFloat(section, "Altitude multiplier", 1f, "Multiplies the biome altitude.");
    configBaseAltitudeDelta = wrapper.BindFloat(section, "Base altitude delta", 0f, "Adds to the base altitude.");
    configAltitudeDelta = wrapper.BindFloat(section, "Altitude delta", 0f, "Adds to the biome altitude.");
    configLocationsMultiplier = wrapper.BindFloat(section, "Locations", 1f, "Multiplies the max amount of locations.");
    configWaveMultiplier = wrapper.BindFloat(section, "Wave multiplier", 1f, "Multiplies the wave size.");
    configWaveMultiplier.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };
    configWaveOnlyHeight = wrapper.Bind(section, "Wave only height", false, "Multiplier only affects the height.");
    configWaveOnlyHeight.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };

    section = "3. Data";
    configDataBiome = wrapper.Bind(section, "Biome data", true, "Use biome data");
    configDataLocation = wrapper.Bind(section, "Location data", true, "Use location data");
    configDataVegetation = wrapper.Bind(section, "Vegetation data", true, "Use vegetation data");
    configDataEvents = wrapper.Bind(section, "Event data", true, "Use event data");
    configDataSpawns = wrapper.Bind(section, "Spawn data", true, "Use spawn data");
    configInternalDataBiome = wrapper.Bind(section, "Internal biome data", "", "Internal field for data sync.");
    configInternalDataBiome.SettingChanged += (s, e) => BiomeData.Set(configInternalDataBiome.Value);
    configInternalDataSpawns = wrapper.Bind(section, "Internal spawns data", "", "Internal field for data sync.");
    configInternalDataSpawns.SettingChanged += (s, e) => SpawnData.Set(configInternalDataSpawns.Value);
    configInternalDataEvents = wrapper.Bind(section, "Internal events data", "", "Internal field for data sync.");
    configInternalDataEvents.SettingChanged += (s, e) => EventData.Set(configInternalDataEvents.Value);
    section = "4. Biomes";
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
    configMountainsAltitudeMin = wrapper.BindFloat(section, "Mountain minimum altitude", 80f);
    configDistanceWiggleLength = wrapper.BindFloat(section, "Distance wiggle length", 500f);
    configDistanceWiggleWidth = wrapper.BindFloat(section, "Distance wiggle width", 1f);
    configWiggleFrequency = wrapper.BindFloat(section, "Wiggle frequency", 20f, "How many wiggles are per each circle.");
    configWiggleWidth = wrapper.BindFloat(section, "Wiggle width", 100f, "How many meters are the wiggles.");
    configMeadowsMin = wrapper.BindRange(section, "Meadows begin percentage", 0);
    configMeadowsMax = wrapper.BindRange(section, "Meadows end percentage", 50);
    configMeadowsSectorMin = wrapper.BindRange(section, "Meadows sector begin percentage", 0);
    configMeadowsSectorMax = wrapper.BindRange(section, "Meadows sector end percentage", 100);
    configBlackForestMin = wrapper.BindRange(section, "Black forest begin percentage", 6);
    configBlackForestMax = wrapper.BindRange(section, "Black forest end percentage", 60);
    configBlackForestSectorMin = wrapper.BindRange(section, "Black forest sector begin percentage", 0);
    configBlackForestSectorMax = wrapper.BindRange(section, "Black forest sector end percentage", 100);
    configSwampMin = wrapper.BindRange(section, "Swamp begin percentage", 20);
    configSwampMax = wrapper.BindRange(section, "Swamp end percentage", 80);
    configSwampSectorMin = wrapper.BindRange(section, "Swamp sector begin percentage", 0);
    configSwampSectorMax = wrapper.BindRange(section, "Swamp sector end percentage", 100);
    configMountainMin = wrapper.BindRange(section, "Mountain begin percentage", 0);
    configMountainMax = wrapper.BindRange(section, "Mountain end percentage", 0);
    configMountainSectorMin = wrapper.BindRange(section, "Mountain sector begin percentage", 0);
    configMountainSectorMax = wrapper.BindRange(section, "Mountain sector end percentage", 0);
    configPlainsMin = wrapper.BindRange(section, "Plains begin percentage", 30);
    configPlainsMax = wrapper.BindRange(section, "Plains end percentage", 80);
    configPlainsSectorMin = wrapper.BindRange(section, "Plains sector begin percentage", 0);
    configPlainsSectorMax = wrapper.BindRange(section, "Plains sector end percentage", 100);
    configMistlandsMin = wrapper.BindRange(section, "Mistlands begin percentage", 60);
    configMistlandsMax = wrapper.BindRange(section, "Mistlands end percentage", 100);
    configMistlandsSectorMin = wrapper.BindRange(section, "Mistlands sector begin percentage", 0);
    configMistlandsSectorMax = wrapper.BindRange(section, "Mistlands sector end percentage", 100);
    configAshlandsMin = wrapper.BindRange(section, "Ashlands begin percentage", 80);
    configAshlandsMax = wrapper.BindRange(section, "Ashlands end percentage", 100);
    configAshlandsCurvature = wrapper.BindRange(section, "Ashlands curvature percentage", 40);
    configAshlandsSectorMin = wrapper.BindRange(section, "Ashlands sector begin percentage", 75);
    configAshlandsSectorMax = wrapper.BindRange(section, "Ashlands sector end percentage", 25);
    configDeepNorthMin = wrapper.BindRange(section, "Deep north begin percentage", 80);
    configDeepNorthMax = wrapper.BindRange(section, "Deep north end percentage", 100);
    configDeepNorthCurvature = wrapper.BindRange(section, "Deep north curvature percentage", 40);
    configDeepNorthSectorMin = wrapper.BindRange(section, "Deep north sector begin percentage", 25);
    configDeepNorthSectorMax = wrapper.BindRange(section, "Deep north sector end percentage", 75);

    configBlackForestAmount = wrapper.BindAmount(section, "Black forest amount", 60);
    configSwampAmount = wrapper.BindAmount(section, "Swamp amount", 40);
    configPlainsAmount = wrapper.BindAmount(section, "Plains amount", 60);
    configMistlandsAmount = wrapper.BindAmount(section, "Mistlands amount", 50);

    configMeadowsAltitudeDelta = wrapper.BindFloat(section, "Meadows altitude delta", 0f);
    configMeadowsAltitudeMultiplier = wrapper.BindFloat(section, "Meadows altitude multiplier", 1f);
    configBlackForestAltitudeDelta = wrapper.BindFloat(section, "Black forest altitude delta", 0f);
    configBlackForestAltitudeMultiplier = wrapper.BindFloat(section, "Black forest altitude multiplier", 1f);
    configSwampsAltitudeDelta = wrapper.BindFloat(section, "Swamps altitude delta", 0f);
    configSwampsAltitudeMultiplier = wrapper.BindFloat(section, "Swamps altitude multiplier", 1f);
    configMountainAltitudeDelta = wrapper.BindFloat(section, "Mountain altitude delta", 0f);
    configMountainAltitudeMultiplier = wrapper.BindFloat(section, "Mountain altitude multiplier", 1f);
    configPlainsAltitudeDelta = wrapper.BindFloat(section, "Plains altitude delta", 0f);
    configPlainsAltitudeMultiplier = wrapper.BindFloat(section, "Plains altitude multiplier", 1f);
    configMistlandsAltitudeDelta = wrapper.BindFloat(section, "Mistlands altitude delta", 0f);
    configMistlandsAltitudeMultiplier = wrapper.BindFloat(section, "Mistlands altitude multiplier", 1f);
    configAshlandsAltitudeDelta = wrapper.BindFloat(section, "Ashlands altitude delta", 0f);
    configAshlandsAltitudeMultiplier = wrapper.BindFloat(section, "Ashlands altitude multiplier", 1f);
    configDeepNorthAltitudeDelta = wrapper.BindFloat(section, "Deep north altitude delta", 0f);
    configDeepNorthAltitudeMultiplier = wrapper.BindFloat(section, "Deep north altitude multiplier", 1f);
    configOceanAltitudeDelta = wrapper.BindFloat(section, "Ocean altitude delta", 0f);
    configOceanAltitudeMultiplier = wrapper.BindFloat(section, "Ocean altitude multiplier", 1f);

    section = "5. Seed";
    configUseOffsetX = wrapper.Bind(section, "Use custom offset X", false, "Determines x coordinate on the base height map.");
    configOffsetX = wrapper.BindInt(section, "Offset X", 0);
    configUseOffsetY = wrapper.Bind(section, "Use custom offset Y", false, "Determines y coordinate on the base height map.");
    configOffsetY = wrapper.BindInt(section, "Offset Y", 0);
    configUseHeightSeed = wrapper.Bind(section, "Use height variation seed", false, "Determines the height variation of most biomes.");
    configHeightSeed = wrapper.BindInt(section, "Height variation seed", 0);

    configUseBlackForestSeed = wrapper.Bind(section, "Use Black forest seed", false, "Determines location of Black forests.");
    configBlackForestSeed = wrapper.BindInt(section, "Black forest seed", 0);
    configUseSwampSeed = wrapper.Bind(section, "Use Swamp seed", false, "Determines location of Swamps.");
    configSwampSeed = wrapper.BindInt(section, "Swamp seed", 0);
    configUsePlainsSeed = wrapper.Bind(section, "Use Plains seed", false, "Determines location of Plains.");
    configPlainsSeed = wrapper.BindInt(section, "Plains seed", 0);
    configUseMistlandsSeed = wrapper.Bind(section, "Use Mistlands seed", false, "Determines location of Mistlands.");
    configMistlandsSeed = wrapper.BindInt(section, "Mistlands seed", 0);

    configUseStreamSeed = wrapper.Bind(section, "Use stream seed", false, "Determines stream generation");
    configStreamSeed = wrapper.BindInt(section, "Stream seed", 0);
    configUseRiverSeed = wrapper.Bind(section, "Use river seed", false, "Determines river generation");
    configRiverSeed = wrapper.BindInt(section, "River seed", 0);
  }
}
