using System.IO;
using BepInEx;
using BepInEx.Configuration;
using ServerSync;
using Service;
using UnityEngine;

namespace ExpandWorld;
public partial class Configuration
{
#nullable disable
  public static ConfigEntry<string> configWorldRadius;
  public static float WorldRadius => ConfigWrapper.Floats[configWorldRadius];
  public static ConfigEntry<string> configWorldEdgeSize;
  public static float WorldEdgeSize => ConfigWrapper.Floats[configWorldEdgeSize];
  public static float WorldTotalRadius => WorldRadius + WorldEdgeSize;
  public static ConfigEntry<string> configMapSize;
  public static float MapSize => ConfigWrapper.Floats[configMapSize];
  public static ConfigEntry<string> configMapPixelSize;
  public static float MapPixelSize => ConfigWrapper.Floats[configMapPixelSize];
  public static ConfigEntry<bool> configServerOnly;
  public static bool ServerOnly => configServerOnly.Value;
  public static ConfigEntry<string> configEventInterval;
  public static float EventInterval => ConfigWrapper.Floats[configEventInterval];
  public static ConfigEntry<string> configEventChance;
  public static float EventChance => ConfigWrapper.Floats[configEventChance];

  public static ConfigEntry<string> configAltitudeMultiplier;
  public static float AltitudeMultiplier => ConfigWrapper.Floats[configAltitudeMultiplier];
  public static ConfigEntry<string> configAltitudeDelta;
  public static float AltitudeDelta => ConfigWrapper.Floats[configAltitudeDelta];

  public static ConfigEntry<string> configLocationsMultiplier;
  public static float LocationsMultiplier => ConfigWrapper.Floats[configLocationsMultiplier];

  public static ConfigEntry<string> configForestMultiplier;
  public static float ForestMultiplier => ConfigWrapper.Floats[configForestMultiplier];
  public static ConfigEntry<string> configWorldStretch;
  // Special treatment for easier transpiling and performance.
  public static float WorldStretch = 1f;
  public static ConfigEntry<string> configBiomeStretch;
  public static float BiomeStretch => ConfigWrapper.Floats[configBiomeStretch] == 0f ? 1f : ConfigWrapper.Floats[configBiomeStretch];

  public static ConfigEntry<bool> configZoneSpawners;
  public static bool ZoneSpawners => configZoneSpawners.Value;

  public static ConfigEntry<string> configDistanceWiggleLength;
  public static float DistanceWiggleLength => ConfigWrapper.Floats[configDistanceWiggleLength];
  public static ConfigEntry<string> configDistanceWiggleWidth;
  public static float DistanceWiggleWidth => ConfigWrapper.Floats[configDistanceWiggleWidth];
  public static ConfigEntry<string> configWiggleFrequency;
  public static float WiggleFrequency => ConfigWrapper.Floats[configWiggleFrequency];
  public static ConfigEntry<string> configWiggleWidth;
  public static float WiggleWidth => ConfigWrapper.Floats[configWiggleWidth];

  public static CustomSyncedValue<string> valueBiomeData;
  public static CustomSyncedValue<string> valueWorldData;
  public static CustomSyncedValue<string> valueVegetationData;
  public static CustomSyncedValue<string> valueClutterData;
  public static CustomSyncedValue<string> valueDungeonData;
  public static CustomSyncedValue<string> valueSpawnData;
  public static CustomSyncedValue<string> valueEventData;
  public static CustomSyncedValue<string> valueEnvironmentData;
  public static CustomSyncedValue<string> valueNoBuildData;
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
  public static ConfigEntry<bool> configDataDungeons;
  public static bool DataDungeons => configDataDungeons.Value;

  public static ConfigEntry<bool> configDataLocation;
  public static bool DataLocation => configDataLocation.Value;
  public static ConfigEntry<bool> configDataBiome;
  public static bool DataBiome => configDataBiome.Value;
  public static ConfigEntry<bool> configDataWorld;
  public static bool DataWorld => configDataWorld.Value;

  public static ConfigEntry<string> configSeed;
  public static string Seed => configSeed.Value;

  public static ConfigEntry<string> configOffsetX;
  public static int? OffsetX => ConfigWrapper.Ints[configOffsetX];
  public static ConfigEntry<string> configOffsetY;
  public static int? OffsetY => ConfigWrapper.Ints[configOffsetY];
  public static ConfigEntry<string> configHeightSeed;
  public static int? HeightSeed => ConfigWrapper.Ints[configHeightSeed];
  public static ConfigEntry<string> configPlanBuildFolder;
  public static string PlanBuildGlobalFolder => configPlanBuildFolder.Value;
  public static string PlanBuildLocalFolder => Path.Combine(Paths.GameRootPath, configPlanBuildFolder.Value);
  public static ConfigEntry<string> configBuildShareFolder;
  public static string BuildShareGlobalFolder => configBuildShareFolder.Value;
  public static string BuildShareLocalFolder => Path.Combine(Paths.GameRootPath, configBuildShareFolder.Value);


#nullable enable
  public static void Init(ConfigWrapper wrapper)
  {
    var section = "1. General";
    configWorldRadius = wrapper.BindFloat(section, "World radius", 10000f, true, "Radius of the world in meters (excluding the edge).");
    configWorldEdgeSize = wrapper.BindFloat(section, "World edge size", 500f, true, "Size of the edge area in meters (added to the radius for the total size).");
    configMapSize = wrapper.BindFloat(section, "Minimap size", 1f, false, "Increases the minimap size, but also significantly increases the generation time.");
    configMapSize.SettingChanged += (e, s) =>
    {
      if (!Minimap.instance) return;
      var newValue = (int)(MinimapAwake.OriginalTextureSize * MapSize);
      if (newValue == Minimap.instance.m_textureSize) return;
      Minimap.instance.m_maxZoom = MinimapAwake.OriginalMinZoom * Mathf.Max(1f, MapSize);
      MapGeneration.UpdateTextureSize(Minimap.instance, newValue);
      Generate.Map();
    };
    configServerOnly = wrapper.Bind(section, "Server only", false, false, "If true, enables server side only mode and clients can't have the mod installed.");
    configMapPixelSize = wrapper.BindFloat(section, "Minimap pixel size", 1f, false, "Decreases the minimap detail, but doesn't affect the generation time.");
    configMapPixelSize.SettingChanged += (e, s) =>
    {
      if (!Minimap.instance) return;
      var newValue = MinimapAwake.OriginalPixelSize * MapPixelSize;
      if (newValue == Minimap.instance.m_pixelSize) return;
      Minimap.instance.m_pixelSize = newValue;
      Generate.Map();
    };
    configWorldStretch = wrapper.BindFloat(section, "Stretch world", 1f, true, "Stretches the world to a bigger area.");
    configWorldStretch.SettingChanged += (s, e) =>
    {
      WorldStretch = ConfigWrapper.Floats[configWorldStretch] == 0f ? 1f : ConfigWrapper.Floats[configWorldStretch];
    };
    WorldStretch = ConfigWrapper.Floats[configWorldStretch] == 0f ? 1f : ConfigWrapper.Floats[configWorldStretch];
    configBiomeStretch = wrapper.BindFloat(section, "Stretch biomes", 1f, true, "Stretches the biomes to a bigger area.");
    configZoneSpawners = wrapper.Bind(section, "Zone spawners", true, false, "If disabled, zone spawners are not generated.");

    section = "2. Features";

    configForestMultiplier = wrapper.BindFloat(section, "Forest multiplier", 1f, true, "Multiplies the amount of forest.");
    configAltitudeMultiplier = wrapper.BindFloat(section, "Altitude multiplier", 1f, true, "Multiplies the altitude.");
    configAltitudeDelta = wrapper.BindFloat(section, "Altitude delta", 0f, true, "Adds to the altitude.");
    configLocationsMultiplier = wrapper.BindFloat(section, "Locations", 1f, true, "Multiplies the max amount of locations.");
    configDistanceWiggleLength = wrapper.BindFloat(section, "Distance wiggle length", 500f, true);
    configDistanceWiggleWidth = wrapper.BindFloat(section, "Distance wiggle width", 0.01f, true);
    configWiggleFrequency = wrapper.BindFloat(section, "Wiggle frequency", 20f, true, "How many wiggles are per each circle.");
    configWiggleWidth = wrapper.BindFloat(section, "Wiggle width", 100f, true, "How many meters are the wiggles.");
    configOffsetX = wrapper.BindInt(section, "Offset X", null, true);
    configOffsetY = wrapper.BindInt(section, "Offset Y", null, true);
    configSeed = wrapper.Bind(section, "Seed", "", false);
    configSeed.SettingChanged += (s, e) =>
    {
      if (Seed == "") return;
      if (WorldGenerator.instance == null) return;
      var world = WorldGenerator.instance.m_world;
      if (world.m_menu) return;
      world.m_seedName = Seed;
      world.m_seed = Seed.GetStableHashCode();
      // Prevents default generate (better use the debounced).
      world.m_menu = true;
      WorldGenerator.Initialize(world);
      world.m_menu = false;
      Generate.World();
    };
    configHeightSeed = wrapper.BindInt(section, "Height variation seed", null, true);
    configEventChance = wrapper.BindFloat(section, "Random event chance", 20, false, "The chance to try starting a random event.");
    configEventChance.SettingChanged += (s, e) => RandomEventSystem.Setup(RandEventSystem.instance);
    configEventInterval = wrapper.BindFloat(section, "Random event interval", 46, false, "How often the random events are checked (minutes).");
    configEventInterval.SettingChanged += (s, e) => RandomEventSystem.Setup(RandEventSystem.instance);

    InitWater(wrapper);

    section = "4. Data";
    configDataEnvironments = wrapper.Bind(section, "Environment data", true, false, "Use environment data");
    configDataEnvironments.SettingChanged += (s, e) => EnvironmentManager.FromSetting(valueEnvironmentData.Value);
    configDataBiome = wrapper.Bind(section, "Biome data", true, false, "Use biome data");
    configDataBiome.SettingChanged += (s, e) => BiomeManager.FromSetting(valueBiomeData.Value);
    configDataClutter = wrapper.Bind(section, "Dungeon data", true, false, "Use dungeon data");
    configDataClutter.SettingChanged += (s, e) => DungeonManager.FromSetting(valueDungeonData.Value);
    configDataDungeons = wrapper.Bind(section, "Clutter data", true, false, "Use clutter data");
    configDataDungeons.SettingChanged += (s, e) => ClutterManager.FromSetting(valueClutterData.Value);
    configDataWorld = wrapper.Bind(section, "World data", true, false, "Use world data");
    configDataWorld.SettingChanged += (s, e) => WorldManager.FromSetting(valueWorldData.Value);
    configDataLocation = wrapper.Bind(section, "Location data", true, false, "Use location data");
    configDataLocation.SettingChanged += (s, e) => LocationManager.Load();
    configDataVegetation = wrapper.Bind(section, "Vegetation data", true, false, "Use vegetation data");
    configDataVegetation.SettingChanged += (s, e) => VegetationManager.FromSetting(valueVegetationData.Value);
    configDataEvents = wrapper.Bind(section, "Event data", true, false, "Use event data");
    configDataEvents.SettingChanged += (s, e) => EventManager.FromSetting(valueEventData.Value);
    configDataSpawns = wrapper.Bind(section, "Spawn data", true, false, "Use spawn data");
    configDataSpawns.SettingChanged += (s, e) => SpawnManager.FromSetting(valueSpawnData.Value);
    configPlanBuildFolder = wrapper.Bind(section, "Plan Build folder", "BepInEx/config/PlanBuild", false, "Folder relative to the Valheim.exe.");
    configBuildShareFolder = wrapper.Bind(section, "Build Share folder", "BuildShare/Builds", false, "Folder relative to the Valheim.exe.");

    valueNoBuildData = wrapper.AddValue("no_build_data");
    valueNoBuildData.ValueChanged += () => NoBuildManager.Load(valueNoBuildData.Value);
    valueEnvironmentData = wrapper.AddValue("environment_data");
    valueEnvironmentData.ValueChanged += () => EnvironmentManager.FromSetting(valueEnvironmentData.Value);
    valueBiomeData = wrapper.AddValue("biome_data");
    valueBiomeData.ValueChanged += () => BiomeManager.FromSetting(valueBiomeData.Value);
    valueClutterData = wrapper.AddValue("clutter_data");
    valueClutterData.ValueChanged += () => ClutterManager.FromSetting(valueClutterData.Value);
    valueDungeonData = wrapper.AddValue("dungeon_data");
    valueDungeonData.ValueChanged += () => DungeonManager.FromSetting(valueDungeonData.Value);
    valueSpawnData = wrapper.AddValue("spawn_data");
    valueSpawnData.ValueChanged += () => SpawnManager.FromSetting(valueSpawnData.Value);
    valueEventData = wrapper.AddValue("event_data");
    valueEventData.ValueChanged += () => EventManager.FromSetting(valueEventData.Value);
    valueWorldData = wrapper.AddValue("world_data");
    valueWorldData.ValueChanged += () => WorldManager.FromSetting(valueWorldData.Value);
    valueVegetationData = wrapper.AddValue("vegetation_data");
    valueVegetationData.ValueChanged += () => VegetationManager.FromSetting(valueVegetationData.Value);
  }
}
