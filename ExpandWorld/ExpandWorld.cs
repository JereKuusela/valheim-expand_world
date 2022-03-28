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

  [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Awake))]
  public class ZoneSystemAwake {
    public static void Postfix(ZoneSystem __instance) {
      __instance.m_activeArea = Settings.ActiveArea;
    }
  }
}
