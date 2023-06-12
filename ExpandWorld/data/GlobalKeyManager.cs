
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace ExpandWorld;

// Helper to keep track of hashed lower case keys.
[HarmonyPatch(typeof(ZoneSystem))]
public class GlobalKeyManager
{
  static HashSet<int> GlobalKeys = new();
  static void LoadKeys(ZoneSystem zs)
  {
    GlobalKeys = zs.m_globalKeys.Select(s => s.ToLowerInvariant().GetHashCode()).ToHashSet();
  }
  public static bool HasAny(int[] keys) => keys.Any(GlobalKeys.Contains);
  public static bool HasEvery(int[] keys) => keys.All(GlobalKeys.Contains);

  [HarmonyPatch(nameof(ZoneSystem.Load)), HarmonyPostfix]
  static void AfterLoad(ZoneSystem __instance) => LoadKeys(__instance);
  [HarmonyPatch(nameof(ZoneSystem.ResetGlobalKeys)), HarmonyPostfix]
  static void AfterReset(ZoneSystem __instance) => LoadKeys(__instance);
  [HarmonyPatch(nameof(ZoneSystem.RPC_SetGlobalKey)), HarmonyPostfix]
  static void AfterSet(ZoneSystem __instance) => LoadKeys(__instance);
  [HarmonyPatch(nameof(ZoneSystem.RPC_RemoveGlobalKey)), HarmonyPostfix]
  static void AfterRemove(ZoneSystem __instance) => LoadKeys(__instance);
}