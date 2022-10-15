using System;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
namespace ExpandWorld;

public class CustomRaidsPatcher {
  public const string GUID = "asharppen.valheim.custom_raids";
  private static void Call(Type type, string name) => type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, new object[0]);
  public static void InitConfiguration() {
    if (CustomRaids == null) return;
    var type = CustomRaids.GetType("Valheim.CustomRaids.Raids.Managers.RaidConfigManager");
    if (type == null) return;
    if (IsDelayed)
      Call(type, "ApplyConfigs");
  }
  private static Assembly? CustomRaids;
  public static void Run() {
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var info)) return;
    CustomRaids = info.Instance.GetType().Assembly;
    if (CustomRaids == null) return;
    ExpandWorld.Log.LogInfo("\"Custom Raids\" detected. Disabling event data.");
    Configuration.configDataEvents.Value = false;
    Harmony harmony = new("expand_world.custom_raids");
    PatchApplyConfigs(harmony, CustomRaids);
    PatchCreateEvent(harmony, CustomRaids);
  }
  private static void PatchApplyConfigs(Harmony harmony, Assembly assembly) {
    var mOriginal = AccessTools.Method(assembly.GetType("Valheim.CustomRaids.Raids.Managers.RaidConfigManager"), "ApplyConfigs");
    if (mOriginal == null) {
      ExpandWorld.Log.LogWarning("\"Custom Raids\" detected. Unable to patch \"ApplyConfigs\" for biome compatibility.");
      return;
    }
    ExpandWorld.Log.LogInfo("\"Custom Raids\" detected. Patching \"ApplyConfigs\" for biome compatibility.");
    var mPrefix = SymbolExtensions.GetMethodInfo(() => Prefix());
    harmony.Patch(mOriginal, new(mPrefix));
  }
  private static bool IsDelayed = false;
  static bool Prefix() {
    IsDelayed = !Data.BiomesLoaded;
    return Data.BiomesLoaded;
  }
  private static void PatchCreateEvent(Harmony harmony, Assembly assembly) {
    var mOriginal = AccessTools.Method(assembly.GetType("Valheim.CustomRaids.Raids.Managers.RaidConfigManager"), "CreateEvent");
    if (mOriginal == null) {
      ExpandWorld.Log.LogWarning("\"Custom Raids\" detected. Unable to patch \"CreateEvent\" for biome compatibility.");
      return;
    }
    ExpandWorld.Log.LogInfo("\"Custom Raids\" detected. Patching \"CreateEvent\" for biome compatibility.");
    var mPostfix = SymbolExtensions.GetMethodInfo((RandomEvent ev) => Postfix(ev));
    harmony.Patch(mOriginal, null, new(mPostfix));
  }
  static RandomEvent Postfix(RandomEvent ev) {
    if (ev.m_biome == (Heightmap.Biome)1023)
      ev.m_biome = BiomeManager.MaxBiome;
    foreach (var spawn in ev.m_spawn) {
      if (spawn.m_biome == (Heightmap.Biome)1023)
        spawn.m_biome = BiomeManager.MaxBiome;
    }
    return ev;
  }
  [HarmonyPatch(typeof(Game), nameof(Game.Logout)), HarmonyPrefix]
  static void CleanUp() {
    IsDelayed = false;
  }
}
