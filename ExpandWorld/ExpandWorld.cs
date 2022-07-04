using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Service;

namespace ExpandWorld;
[BepInPlugin(GUID, NAME, VERSION)]
public class ExpandWorld : BaseUnityPlugin {
  const string GUID = "expand_world";
  const string NAME = "Expand World";
  const string VERSION = "1.1";
#nullable disable
  public static ManualLogSource Log;
#nullable enable
  public static ServerSync.ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    MinimumRequiredVersion = VERSION
  };
  public static string ConfigPath = "";
  public void Awake() {
    Log = Logger;
    ConfigWrapper wrapper = new("expand_config", Config, ConfigSync);
    ConfigPath = Path.GetDirectoryName(Config.ConfigFilePath);
    Configuration.Init(wrapper);
    Harmony harmony = new(GUID);
    harmony.PatchAll();
    SetupWatcher();
    BiomeData.SetupWatcher();
    LocationData.SetupWatcher();
    VegetationData.SetupWatcher();
  }

  private void OnDestroy() {
    // Should prevent sending the cleared values.
    ServerSync.ConfigSync.ProcessingServerUpdate = true;
    Configuration.configInternalDataBiome.Value = "";
    Config.Save();
  }

  private void SetupWatcher() {
    FileSystemWatcher watcher = new(ConfigPath, Path.GetFileName(Config.ConfigFilePath));
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
