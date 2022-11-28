using System;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
namespace ExpandWorld;

public class SpawnThatPatcher
{
  public const string GUID = "asharppen.valheim.spawn_that";
  private static void Call(Type type, string name) => type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, new object[0]);
  public static void InitConfiguration()
  {
    if (SpawnThat == null) return;
    var type = SpawnThat.GetType("SpawnThat.Lifecycle.LifecycleManager");
    if (type == null) return;
    if (IsSingleplayerDelayed)
      Call(type, "InitSingleplayer");
    if (IsDedicatedDelayed)
      Call(type, "InitDedicated");
  }
  private static Assembly? SpawnThat;
  public static void Run()
  {
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var info)) return;
    SpawnThat = info.Instance.GetType().Assembly;
    if (SpawnThat == null) return;
    ExpandWorld.Log.LogInfo("\"Spawn That\" detected. Disabling spawns data.");
    Configuration.configDataSpawns.Value = false;
    Harmony harmony = new("expand_world.spawn_that");
    PatchSingleplayer(harmony, SpawnThat);
    PatchDedicated(harmony, SpawnThat);
  }
  private static void PatchSingleplayer(Harmony harmony, Assembly assembly)
  {
    var mOriginal = AccessTools.Method(assembly.GetType("SpawnThat.Lifecycle.LifecycleManager"), "InitSingleplayer");
    if (mOriginal == null)
    {
      ExpandWorld.Log.LogWarning("\"Spawn That\" detected. Unable to patch \"InitSingleplayer\" for biome compatibility.");
      return;
    }
    ExpandWorld.Log.LogInfo("\"Spawn That\" detected. Patching \"InitSingleplayer\" for biome compatibility.");
    var mPrefix = SymbolExtensions.GetMethodInfo(() => SingleplayerPrefix());
    harmony.Patch(mOriginal, new(mPrefix));
  }
  private static void PatchDedicated(Harmony harmony, Assembly assembly)
  {
    var mOriginal = AccessTools.Method(assembly.GetType("SpawnThat.Lifecycle.LifecycleManager"), "InitDedicated");
    if (mOriginal == null)
    {
      ExpandWorld.Log.LogWarning("\"Spawn That\" detected. Unable to patch \"InitDedicated\" for biome compatibility.");
      return;
    }
    ExpandWorld.Log.LogInfo("\"Spawn That\" detected. Patching \"InitDedicated\" for biome compatibility.");
    var mPrefix = SymbolExtensions.GetMethodInfo(() => DedicatedPrefix());
    harmony.Patch(mOriginal, new(mPrefix));
  }
  private static bool IsSingleplayerDelayed = false;
  static bool SingleplayerPrefix()
  {
    IsSingleplayerDelayed = !Data.BiomesLoaded;
    return Data.BiomesLoaded;
  }
  private static bool IsDedicatedDelayed = false;
  static bool DedicatedPrefix()
  {
    IsDedicatedDelayed = !Data.BiomesLoaded;
    return Data.BiomesLoaded;
  }
  [HarmonyPatch(typeof(ZNet), nameof(ZNet.Awake)), HarmonyPrefix]
  static void CleanUpOnJoin()
  {
    IsSingleplayerDelayed = false;
    IsDedicatedDelayed = false;
  }
  [HarmonyPatch(typeof(Game), nameof(Game.Logout)), HarmonyPrefix]
  static void CleanUpOnExit()
  {
    IsSingleplayerDelayed = false;
    IsDedicatedDelayed = false;
  }
}
