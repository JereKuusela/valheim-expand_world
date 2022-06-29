using HarmonyLib;

namespace ExpandWorld;
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiomeHeight))]
public class StretchBiomeHeight {
  static void Prefix(WorldGenerator __instance, ref float wx, ref float wy) {
    if (__instance.m_world.m_menu) return;
    wx /= Configuration.WorldStretch;
    wy /= Configuration.WorldStretch;
  }
}
