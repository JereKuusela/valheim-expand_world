using System.IO;
using BepInEx;
using BepInEx.Configuration;
using ServerSync;
using Service;

namespace ExpandWorld;
public partial class Configuration
{
#nullable disable
  public static ConfigEntry<bool> configServerOnly;
  public static bool ServerOnly => configServerOnly.Value;
  public static ConfigEntry<bool> configRegenerateMap;
  public static bool RegenerateMap => configRegenerateMap.Value;
  public static ConfigEntry<string> configEventInterval;
  public static float EventInterval => ConfigWrapper.Floats[configEventInterval];
  public static ConfigEntry<string> configEventChance;
  public static float EventChance => ConfigWrapper.Floats[configEventChance];


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
  public static CustomSyncedValue<string> valueClutterData;
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
  public static ConfigEntry<bool> configDataRooms;
  public static bool DataRooms => configDataRooms.Value;

  public static ConfigEntry<bool> configDataLocation;
  public static bool DataLocation => configDataLocation.Value;
  public static ConfigEntry<bool> configDataBiome;
  public static bool DataBiome => configDataBiome.Value;
  public static ConfigEntry<bool> configDataWorld;
  public static bool DataWorld => configDataWorld.Value;
  public static ConfigEntry<bool> configDataMigration;
  public static bool DataMigration => configDataMigration.Value;
  public static ConfigEntry<bool> configDataReload;
  public static bool DataReload => configDataReload.Value;

  public static ConfigEntry<string> configBlueprintFolder;
  public static string BlueprintGlobalFolder => Path.Combine("BepInEx", "config", configBlueprintFolder.Value);
  public static string BlueprintLocalFolder => Path.Combine(Paths.ConfigPath, configBlueprintFolder.Value);
#nullable enable
  public static void Init(ConfigWrapper wrapper)
  {
    var section = "1. General";
    configRegenerateMap = wrapper.Bind(section, "Regenerate map", true, false, "If true, the world map is regenerated automatically on data changes.");
    configServerOnly = wrapper.Bind(section, "Server only", false, false, "If true, enables server side only mode and clients can't have the mod installed.");
    configZoneSpawners = wrapper.Bind(section, "Zone spawners", true, false, "If disabled, zone spawners are not generated.");

    section = "2. Features";

    configDistanceWiggleLength = wrapper.BindFloat(section, "Distance wiggle length", 500f, true);
    configDistanceWiggleWidth = wrapper.BindFloat(section, "Distance wiggle width", 0.01f, true);
    configWiggleFrequency = wrapper.BindFloat(section, "Wiggle frequency", 20f, true, "How many wiggles are per each circle.");
    configWiggleWidth = wrapper.BindFloat(section, "Wiggle width", 100f, true, "How many meters are the wiggles.");

    configEventChance = wrapper.BindFloat(section, "Random event chance", 20, false, "The chance to try starting a random event.");
    configEventChance.SettingChanged += (s, e) => RandomEventSystem.Setup(RandEventSystem.instance);
    configEventInterval = wrapper.BindFloat(section, "Random event interval", 46, false, "How often the random events are checked (minutes).");
    configEventInterval.SettingChanged += (s, e) => RandomEventSystem.Setup(RandEventSystem.instance);

    section = "3. Data";
    configDataReload = wrapper.Bind(section, "Automatic data reload", true, false, "Data is loaded automatically on file changes. Requires restart to take effect.");
    configDataMigration = wrapper.Bind(section, "Automatic data migration", true, false, "Automatically add missing location, rooms and vegetation entries.");
    configDataEnvironments = wrapper.Bind(section, "Environment data", true, false, "Use environment data");
    configDataEnvironments.SettingChanged += (s, e) => EnvironmentManager.FromSetting(valueEnvironmentData.Value);
    configDataBiome = wrapper.Bind(section, "Biome data", true, true, "Use biome data");
    configDataBiome.SettingChanged += (s, e) => BiomeManager.FromSetting(valueBiomeData.Value);
    configDataClutter = wrapper.Bind(section, "Clutter data", true, false, "Use clutter data");
    configDataClutter.SettingChanged += (s, e) => ClutterManager.FromSetting(valueClutterData.Value);
    configDataDungeons = wrapper.Bind(section, "Dungeon data", true, false, "Use dungeon data");
    configDataDungeons.SettingChanged += (s, e) => Dungeon.Loader.Load();
    configDataRooms = wrapper.Bind(section, "Room data", true, false, "Use dungeon room data");
    configDataRooms.SettingChanged += (s, e) => RoomLoading.Load();
    configDataWorld = wrapper.Bind(section, "World data", true, true, "Use world data");
    configDataWorld.SettingChanged += (s, e) => WorldManager.FromSetting(valueWorldData.Value);
    configDataLocation = wrapper.Bind(section, "Location data", true, false, "Use location data");
    configDataLocation.SettingChanged += (s, e) => LocationLoading.Load();
    configDataVegetation = wrapper.Bind(section, "Vegetation data", true, false, "Use vegetation data");
    configDataVegetation.SettingChanged += (s, e) => VegetationLoading.Load();
    configDataEvents = wrapper.Bind(section, "Event data", true, false, "Use event data");
    configDataEvents.SettingChanged += (s, e) => EventManager.FromSetting(valueEventData.Value);
    configDataSpawns = wrapper.Bind(section, "Spawn data", true, false, "Use spawn data");
    configDataSpawns.SettingChanged += (s, e) => SpawnManager.FromSetting(valueSpawnData.Value);
    configBlueprintFolder = wrapper.Bind(section, "Blueprint folder", "PlanBuild", false, "Folder relative to the config folder.");

    valueNoBuildData = wrapper.AddValue("no_build_data");
    valueNoBuildData.ValueChanged += () => NoBuildManager.Load(valueNoBuildData.Value);
    valueEnvironmentData = wrapper.AddValue("environment_data");
    valueEnvironmentData.ValueChanged += () => EnvironmentManager.FromSetting(valueEnvironmentData.Value);
    valueBiomeData = wrapper.AddValue("biome_data");
    valueBiomeData.ValueChanged += () => BiomeManager.FromSetting(valueBiomeData.Value);
    valueClutterData = wrapper.AddValue("clutter_data");
    valueClutterData.ValueChanged += () => ClutterManager.FromSetting(valueClutterData.Value);
    valueSpawnData = wrapper.AddValue("spawn_data");
    valueSpawnData.ValueChanged += () => SpawnManager.FromSetting(valueSpawnData.Value);
    valueEventData = wrapper.AddValue("event_data");
    valueEventData.ValueChanged += () => EventManager.FromSetting(valueEventData.Value);
    valueWorldData = wrapper.AddValue("world_data");
    valueWorldData.ValueChanged += () => WorldManager.FromSetting(valueWorldData.Value);
  }
}
