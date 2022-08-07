using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Service;
using UnityEngine;

namespace ExpandWorld;
[BepInDependency(SpawnThatPatcher.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(GUID, NAME, VERSION)]
public class ExpandWorld : BaseUnityPlugin {
  public const string GUID = "expand_world";
  public const string NAME = "Expand World";
  public const string VERSION = "1.3";
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
  public static string ConfigPath = "";
  public void Awake() {
    Log = Logger;
    ConfigName = $"{GUID}.cfg";
    ConfigPath = Path.Combine(Paths.ConfigPath, GUID);
    if (!Directory.Exists(ConfigPath))
      Directory.CreateDirectory(ConfigPath);
    ConfigWrapper wrapper = new("expand_config", Config, ConfigSync);
    Configuration.Init(wrapper);
    Harmony harmony = new(GUID);
    harmony.PatchAll();
    SetupWatcher();
    BiomeManager.SetupWatcher();
    LocationManager.SetupWatcher();
    VegetationManager.SetupWatcher();
    EventManager.SetupWatcher();
    SpawnManager.SetupWatcher();
    WorldManager.SetupWatcher();
    ClutterManager.SetupWatcher();
    EnvironmentManager.SetupWatcher();
  }
  public void Start() {
    SpawnThatPatcher.Run();
    JotunnWrapper.Run();
  }
  public void LateUpdate() {
    Generate.CheckRegen(Time.deltaTime);
  }
  private void OnDestroy() {
    Config.Save();
  }

  private void SetupWatcher() {
    FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigName);
    watcher.Changed += ReadConfigValues;
    watcher.Created += ReadConfigValues;
    watcher.Renamed += ReadConfigValues;
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }

  private void ReadConfigValues(object sender, FileSystemEventArgs e) {
    if (!File.Exists(Config.ConfigFilePath)) return;
    try {
      Log.LogDebug("ReadConfigValues called");
      Config.Reload();
    } catch {
      Log.LogError($"There was an issue loading your {Config.ConfigFilePath}");
      Log.LogError("Please check your config entries for spelling and format!");
    }
  }

}

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public class SetCommands {
  static void Postfix() {
    new DebugCommands();
  }
}

