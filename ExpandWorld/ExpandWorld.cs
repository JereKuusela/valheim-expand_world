using BepInEx;
using HarmonyLib;

namespace ExpandWorld;
[BepInPlugin(GUID, NAME, VERSION)]
public class ExpandWorld : BaseUnityPlugin {
  const string GUID = "expand_world";
  const string NAME = "Expand World";
  const string VERSION = "1.0";
  public static ServerSync.ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    MinimumRequiredVersion = VERSION
  };

  public void Awake() {
    Configuration.Init(ConfigSync, Config);
    Harmony harmony = new(GUID);
    harmony.PatchAll();
  }
}
