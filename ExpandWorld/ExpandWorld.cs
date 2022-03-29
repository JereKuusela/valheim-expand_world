using BepInEx;
using HarmonyLib;

namespace ExpandWorld;
[BepInPlugin("valheim.jere.expand_world", "ExpandWorld", "1.0.0.0")]
public class ExpandWorld : BaseUnityPlugin {
  ServerSync.ConfigSync ConfigSync = new ServerSync.ConfigSync("valheim.jere.expand_world")
  {
    DisplayName = "ExpandWorld",
    CurrentVersion = "1.0.0",
    MinimumRequiredVersion = "1.0.0"
  };

  public void Awake() {
    Settings.Init(ConfigSync, Config);
    var harmony = new Harmony("valheim.jere.expand_world");
    harmony.PatchAll();
  }
}


[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Awake))]
public class ZoneSystemAwake {
  static void Postfix(ZoneSystem __instance) {
    __instance.m_activeArea = Settings.ActiveArea;
  }
}
