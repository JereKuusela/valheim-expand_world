using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Service;
using UnityEngine;

namespace ExpandWorld;
[BepInDependency(SpawnThat.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(GUID, NAME, VERSION)]
public class ExpandWorld : BaseUnityPlugin
{
  public const string GUID = "expand_world";
  public const string NAME = "Expand World";
  public const string VERSION = "1.44";
#nullable disable
  public static ManualLogSource Log;
#nullable enable
  public static ServerSync.ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    ModRequired = true,
    IsLocked = true
  };
  public static string ConfigName = "";
  public static string YamlDirectory = "";
  public void Awake()
  {
    Log = Logger;
    ConfigName = $"{GUID}.cfg";
    YamlDirectory = Path.Combine(Paths.ConfigPath, GUID);
    YamlCleanUp();
    if (!Directory.Exists(YamlDirectory))
      Directory.CreateDirectory(YamlDirectory);
    ConfigWrapper wrapper = new("expand_config", Config, ConfigSync);
    Configuration.Init(wrapper);
    Harmony harmony = new(GUID);
    harmony.PatchAll();
    try
    {

      SetupWatcher();
      BiomeManager.SetupWatcher();
      LocationLoading.SetupWatcher();
      VegetationLoading.SetupWatcher();
      EventManager.SetupWatcher();
      SpawnManager.SetupWatcher();
      WorldManager.SetupWatcher();
      ClutterManager.SetupWatcher();
      EnvironmentManager.SetupWatcher();
      Dungeon.Loader.SetupWatcher();
      RoomLoading.SetupWatcher();
      Data.SetupBlueprintWatcher();
    }
    catch (Exception e)
    {
      Log.LogError(e);
    }
  }
  public void Start()
  {
    BiomeManager.NamesFromFile();
    SpawnThat.Run();
    CustomRaids.Run();
    Marketplace.Run();
  }
  public void LateUpdate()
  {
    Generate.CheckRegen(Time.deltaTime);
  }
#pragma warning disable IDE0051
  private void OnDestroy()
  {
    Config.Save();
  }
#pragma warning restore IDE0051

  private void SetupWatcher()
  {
    FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigName);
    watcher.Changed += ReadConfigValues;
    watcher.Created += ReadConfigValues;
    watcher.Renamed += ReadConfigValues;
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }
  private void YamlCleanUp()
  {
    try
    {
      if (!Directory.Exists(YamlDirectory)) return;
      if (File.Exists(Path.Combine(Paths.ConfigPath, ConfigName))) return;
      Directory.Delete(YamlDirectory, true);
    }
    catch
    {
      Log.LogWarning("Failed to remove old yaml files.");
    }
  }
  private void ReadConfigValues(object sender, FileSystemEventArgs e)
  {
    if (!File.Exists(Config.ConfigFilePath)) return;
    try
    {
      Log.LogDebug("ReadConfigValues called");
      Config.Reload();
    }
    catch
    {
      Log.LogError($"There was an issue loading your {Config.ConfigFilePath}");
      Log.LogError("Please check your config entries for spelling and format!");
    }
  }

}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public class SetCommands
{
  static void Postfix()
  {
    new DebugCommands();
  }
}



[HarmonyPatch(typeof(ZRpc), nameof(ZRpc.SetLongTimeout))]
public class IncreaseTimeout
{
  static bool Prefix()
  {
    if (Configuration.ServerOnly) return true;
    ZRpc.m_timeout = 300f;
    ZLog.Log(string.Format("ZRpc timeout set to {0}s ", ZRpc.m_timeout));
    return false;
  }
}
