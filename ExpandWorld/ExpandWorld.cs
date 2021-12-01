using BepInEx;
using HarmonyLib;

namespace ExpandWorld {
  [BepInPlugin("valheim.jere.expand_world", "ExpandWorld", "1.0.0.0")]
  public class ExpandWorld : BaseUnityPlugin {

    public void Awake() {
      Settings.Init(Config);
      var harmony = new Harmony("valheim.jere.expand_world");
      harmony.PatchAll();
    }
  }

  [HarmonyPatch(typeof(ZoneSystem), "Awake")]
  public class ZoneSystemAwake {
    // Copy paste from the base game code.
    public static void Postfix(ZoneSystem __instance) {
      __instance.m_activeArea = Settings.ActiveArea;
    }
  }
}
