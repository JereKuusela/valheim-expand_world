using System.Collections.Generic;
using BepInEx.Configuration;
using Service;

namespace ExpandWorld;
public class Configuration {
#nullable disable
  public static ConfigEntry<bool> configLocked;
  public static bool Locked => configLocked.Value;
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

  public static ConfigEntry<string> configInternalDataBiome;
  public static ConfigEntry<string> configInternalDataWorld;
  public static ConfigEntry<string> configInternalDataLocations;
  public static ConfigEntry<string> configInternalDataVegetation;
  public static ConfigEntry<string> configInternalDataClutter;
  public static ConfigEntry<string> configInternalDataSpawns;
  public static ConfigEntry<string> configInternalDataEvents;
  public static ConfigEntry<string> configInternalDataEnvironments;
  public static ConfigEntry<bool> configDataEnvironments;
  public static bool DataEnvironments => configDataEnvironments.Value;
  public static ConfigEntry<bool> configDataEvents;
  public static bool DataEvents => configDataEvents.Value;
  public static ConfigEntry<bool> configDataSpawns;
  public static bool DataSpawns => configDataSpawns.Value;
  public static ConfigEntry<bool> configDataVegetation;
  public static bool DataVegetation => configDataVegetation.Value;
  public static ConfigEntry<bool> configDataClutter;
  public static bool DataClutter => configDataClutter.Value;
  public static ConfigEntry<bool> configDataLocation;
  public static bool DataLocation => configDataLocation.Value;
  public static ConfigEntry<bool> configDataBiome;
  public static bool DataBiome => configDataBiome.Value;
  public static ConfigEntry<bool> configDataWorld;
  public static bool DataWorld => configDataWorld.Value;

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
    configWorldRadius = wrapper.BindFloat(section, "World radius", 10000f, true, "Radius of the world in meters (excluding the edge).");
    configWorldEdgeSize = wrapper.BindFloat(section, "World edge size", 500f, true, "Size of the edge area in meters (added to the radius for the total size).");
    configMapSize = wrapper.BindFloat(section, "Minimap size", 1f, false, "Increases the minimap size, but also significantly increases the generation time.");
    configMapSize.SettingChanged += (e, s) => {
      if (!Minimap.instance) return;
      var newValue = (int)(MinimapAwake.OriginalTextureSize * MapSize);
      if (newValue == Minimap.instance.m_textureSize) return;
      SetMapMode.TextureSizeChanged = true;
      Minimap.instance.m_textureSize = newValue;
      Minimap.instance.m_mapImageLarge.rectTransform.localScale = new(MapSize, MapSize, MapSize);
    };
    configMapPixelSize = wrapper.BindFloat(section, "Minimap pixel size", 1f, false, "Decreases the minimap detail, but doesn't affect the generation time.");
    configMapPixelSize.SettingChanged += (e, s) => {
      if (!Minimap.instance) return;
      var newValue = MinimapAwake.OriginalPixelSize * MapPixelSize;
      if (newValue == Minimap.instance.m_pixelSize) return;
      SetMapMode.ForceRegen = true;
      Minimap.instance.m_pixelSize = newValue;
    };
    configWorldStretch = wrapper.BindFloat(section, "Stretch world", 1f, true, "Stretches the world to a bigger area.");
    configBiomeStretch = wrapper.BindFloat(section, "Stretch biomes", 1f, true, "Stretches the biomes to a bigger area.");

    section = "2. Features";
    configRivers = wrapper.Bind(section, "Rivers", true, true, "Enables rivers.");
    configRivers.SettingChanged += (s, e) => {
      if (WorldGenerator.instance != null) {
        WorldGenerator.instance.m_riverPoints.Clear();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceStreams();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceRivers();
      }
    };
    configStreams = wrapper.Bind(section, "Streams", true, true, "Enables streams.");
    configStreams.SettingChanged += (s, e) => {
      if (WorldGenerator.instance != null) {
        WorldGenerator.instance.m_riverPoints.Clear();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceStreams();
        WorldGenerator.instance.m_streams = WorldGenerator.instance.PlaceRivers();
      }
    };
    configWaterLevel = wrapper.BindFloat(section, "Water level", 30f, true, "Sets the altitude of the water.");
    configWaterLevel.SettingChanged += (s, e) => {
      WaterHelper.SetLevel(ZoneSystem.instance);
      WaterHelper.SetLevel(ClutterSystem.instance);
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetLevel(obj);
    };
    configForestMultiplier = wrapper.BindFloat(section, "Forest multiplier", 1f, true, "Multiplies the amount of forest.");
    configBaseAltitudeMultiplier = wrapper.BindFloat(section, "Base altitude multiplier", 1f, true, "Multiplies the base altitude.");
    configAltitudeMultiplier = wrapper.BindFloat(section, "Altitude multiplier", 1f, true, "Multiplies the biome altitude.");
    configBaseAltitudeDelta = wrapper.BindFloat(section, "Base altitude delta", 0f, true, "Adds to the base altitude.");
    configAltitudeDelta = wrapper.BindFloat(section, "Altitude delta", 0f, true, "Adds to the biome altitude.");
    configLocationsMultiplier = wrapper.BindFloat(section, "Locations", 1f, true, "Multiplies the max amount of locations.");
    configWaveMultiplier = wrapper.BindFloat(section, "Wave multiplier", 1f, true, "Multiplies the wave size.");
    configWaveMultiplier.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };
    configWaveOnlyHeight = wrapper.Bind(section, "Wave only height", false, false, "Multiplier only affects the height.");
    configWaveOnlyHeight.SettingChanged += (s, e) => {
      foreach (var obj in WaterHelper.Get()) WaterHelper.SetWaveSize(obj);
    };

    section = "3. Data";
    configDataClutter = wrapper.Bind(section, "Clutter data", true, false, "Use clutter data");
    configDataClutter.SettingChanged += (s, e) => ClutterManager.FromSetting(configInternalDataClutter.Value);
    configDataWorld = wrapper.Bind(section, "World data", true, false, "Use world data");
    configDataWorld.SettingChanged += (s, e) => WorldManager.FromSetting(configInternalDataWorld.Value);
    configDataBiome = wrapper.Bind(section, "Biome data", true, false, "Use biome data");
    configDataBiome.SettingChanged += (s, e) => BiomeManager.FromSetting(configInternalDataBiome.Value);
    configDataLocation = wrapper.Bind(section, "Location data", true, false, "Use location data");
    configDataLocation.SettingChanged += (s, e) => LocationManager.FromSetting(configInternalDataLocations.Value);
    configDataVegetation = wrapper.Bind(section, "Vegetation data", true, false, "Use vegetation data");
    configDataVegetation.SettingChanged += (s, e) => VegetationManager.FromSetting(configInternalDataVegetation.Value);
    configDataEvents = wrapper.Bind(section, "Event data", true, false, "Use event data");
    configDataEvents.SettingChanged += (s, e) => EventManager.FromSetting(configInternalDataEvents.Value);
    configDataEnvironments = wrapper.Bind(section, "Environment data", true, false, "Use environment data");
    configDataEnvironments.SettingChanged += (s, e) => EnvironmentManager.FromSetting(configInternalDataEnvironments.Value);
    configDataSpawns = wrapper.Bind(section, "Spawn data", true, false, "Use spawn data");
    configDataSpawns.SettingChanged += (s, e) => SpawnManager.FromSetting(configInternalDataSpawns.Value);

    configInternalDataClutter = wrapper.Bind(section, "Internal clutter data", "", false, "Internal field for data sync.");
    configInternalDataClutter.SettingChanged += (s, e) => ClutterManager.FromSetting(configInternalDataClutter.Value);
    configInternalDataBiome = wrapper.Bind(section, "Internal biome data", "", false, "Internal field for data sync.");
    configInternalDataBiome.SettingChanged += (s, e) => BiomeManager.FromSetting(configInternalDataBiome.Value);
    configInternalDataSpawns = wrapper.Bind(section, "Internal spawns data", "", false, "Internal field for data sync.");
    configInternalDataSpawns.SettingChanged += (s, e) => SpawnManager.FromSetting(configInternalDataSpawns.Value);
    configInternalDataEvents = wrapper.Bind(section, "Internal events data", "", false, "Internal field for data sync.");
    configInternalDataEvents.SettingChanged += (s, e) => EventManager.FromSetting(configInternalDataEvents.Value);
    configInternalDataEnvironments = wrapper.Bind(section, "Internal environment data", "", false, "Internal field for data sync.");
    configInternalDataEnvironments.SettingChanged += (s, e) => EnvironmentManager.FromSetting(configInternalDataEnvironments.Value);
    configInternalDataWorld = wrapper.Bind(section, "Internal world data", "", false, "Internal field for data sync.");
    configInternalDataWorld.SettingChanged += (s, e) => WorldManager.FromSetting(configInternalDataWorld.Value);
    configInternalDataLocations = wrapper.Bind(section, "Internal locations data", "", false, "Internal field for data sync.");
    configInternalDataLocations.SettingChanged += (s, e) => LocationManager.FromSetting(configInternalDataLocations.Value);
    configInternalDataVegetation = wrapper.Bind(section, "Internal vegetation data", "", false, "Internal field for data sync.");
    configInternalDataVegetation.SettingChanged += (s, e) => VegetationManager.FromSetting(configInternalDataVegetation.Value);
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
    configDistanceWiggleLength = wrapper.BindFloat(section, "Distance wiggle length", 500f, true);
    configDistanceWiggleWidth = wrapper.BindFloat(section, "Distance wiggle width", 0.01f, true);
    configWiggleFrequency = wrapper.BindFloat(section, "Wiggle frequency", 20f, true, "How many wiggles are per each circle.");
    configWiggleWidth = wrapper.BindFloat(section, "Wiggle width", 100f, true, "How many meters are the wiggles.");

    section = "5. Seed";
    configUseOffsetX = wrapper.Bind(section, "Use custom offset X", false, true, "Determines x coordinate on the base height map.");
    configOffsetX = wrapper.BindInt(section, "Offset X", 0, true);
    configUseOffsetY = wrapper.Bind(section, "Use custom offset Y", false, true, "Determines y coordinate on the base height map.");
    configOffsetY = wrapper.BindInt(section, "Offset Y", 0, true);
    configUseHeightSeed = wrapper.Bind(section, "Use height variation seed", false, true, "Determines the height variation of most biomes.");
    configHeightSeed = wrapper.BindInt(section, "Height variation seed", 0, true);
    configUseStreamSeed = wrapper.Bind(section, "Use stream seed", false, true, "Determines stream generation");
    configStreamSeed = wrapper.BindInt(section, "Stream seed", 0, true);
    configUseRiverSeed = wrapper.Bind(section, "Use river seed", false, true, "Determines river generation");
    configRiverSeed = wrapper.BindInt(section, "River seed", 0, true);
  }
}
