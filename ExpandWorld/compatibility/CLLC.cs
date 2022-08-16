using System;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace ExpandWorld;

public class CLLCPatcher {
  public const string GUID = "org.bepinex.plugins.creaturelevelcontrol";
  private static void Call(Type type, string name) => type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic, null, new Type[0], new ParameterModifier[0])?.Invoke(null, new object[0]);
  public static void InitConfiguration() {
    if (CLLC == null) return;
    var type = CLLC.GetType("CreatureLevelControl.ConfigLoader");
    if (type == null) return;
    if (IsDelayed)
      Call(type, "loadConfigFile");
  }
  private static Assembly? CLLC;
  public static void Run() {
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var info)) return;
    CLLC = info.Instance.GetType().Assembly;
    if (CLLC == null) return;
    Harmony harmony = new("expand_world.cllc");
    Patch(harmony, CLLC);
  }
  private static void Patch(Harmony harmony, Assembly assembly) {
    var mOriginal = AccessTools.Method(assembly.GetType("CreatureLevelControl.ConfigLoader"), "loadConfigFile", new Type[0]);
    if (mOriginal == null) {
      ExpandWorld.Log.LogWarning("\"CLLC That\" detected. Unable to patch \"loadConfigFile\" for biome compatibility.");
      return;
    }
    ExpandWorld.Log.LogInfo("\"CLLC That\" detected. Patching \"loadConfigFile\" for biome compatibility.");
    var mPrefix = SymbolExtensions.GetMethodInfo(() => Prefix());
    harmony.Patch(mOriginal, new(mPrefix));
  }
  private static bool IsDelayed = false;
  static bool Prefix() {
    IsDelayed = !Data.BiomesLoaded;
    return Data.BiomesLoaded;
  }
  [HarmonyPatch(typeof(Game), nameof(Game.Logout)), HarmonyPrefix]
  static void CleanUp() {
    IsDelayed = false;
  }
}
