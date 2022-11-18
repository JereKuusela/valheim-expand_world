using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
namespace ExpandWorld;

public class WorldSizeHelper
{
  public static IEnumerable<CodeInstruction> EdgeCheck(IEnumerable<CodeInstruction> instructions)
  {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.Replace(matcher, 10420f, () => Configuration.WorldTotalRadius - 80);
    matcher = Helper.Replace(matcher, 10500f, () => Configuration.WorldTotalRadius);
    return matcher.InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(Ship), nameof(Ship.ApplyEdgeForce))]
public class ApplyEdgeForce
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => WorldSizeHelper.EdgeCheck(instructions);
}

[HarmonyPatch(typeof(Player), nameof(Player.EdgeOfWorldKill))]
public class EdgeOfWorldKill
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => WorldSizeHelper.EdgeCheck(instructions);

  // Safer to simply skip when in dungeons.
  static bool Prefix(Player __instance) => __instance.transform.position.y < 4000f;
}

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetForestFactor))]
public class GetForestFactor
{
  static void Postfix(WorldGenerator __instance, Vector3 pos, ref float __result)
  {
    var multiplier = Configuration.ForestMultiplier;
    if (__instance != null && BiomeManager.BiomeForestMultiplier)
    {
      var biome = __instance.GetBiome(pos);
      if (BiomeManager.TryGetData(biome, out var data))
      {
        multiplier *= data.forestMultiplier;
      }
    }
    if (multiplier == 0f)
      __result = 100f;
    else
      __result /= multiplier;
  }
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBaseHeight))]
public class GetBaseHeight
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.ReplaceSeed(matcher, nameof(WorldGenerator.m_offset0), (WorldGenerator instance) => instance.m_offset0);
    matcher = Helper.ReplaceSeed(matcher, nameof(WorldGenerator.m_offset1), (WorldGenerator instance) => instance.m_offset1);
    matcher = Helper.ReplaceSeed(matcher, nameof(WorldGenerator.m_offset0), (WorldGenerator instance) => Configuration.OffsetX ?? instance.m_offset0);
    matcher = Helper.ReplaceSeed(matcher, nameof(WorldGenerator.m_offset1), (WorldGenerator instance) => Configuration.OffsetY ?? instance.m_offset1);
    matcher = Helper.Replace(matcher, 10000f, () => Configuration.WorldRadius / Configuration.WorldStretch);
    matcher = Helper.Replace(matcher, 10000f, () => Configuration.WorldRadius / Configuration.WorldStretch);
    matcher = Helper.Replace(matcher, 10500f, () => Configuration.WorldTotalRadius / Configuration.WorldStretch);
    matcher = Helper.Replace(matcher, 10490f, () => (Configuration.WorldTotalRadius - 10f) / Configuration.WorldStretch);
    matcher = Helper.Replace(matcher, 10500f, () => Configuration.WorldTotalRadius / Configuration.WorldStretch);
    return matcher.InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(WaterVolume), nameof(WaterVolume.SetupMaterial))]
public class SetupMaterial
{
  public static void Refresh()
  {
    var objects = UnityEngine.Object.FindObjectsOfType<WaterVolume>();
    foreach (var water in objects)
    {
      water.m_waterSurface.material.SetFloat("_WaterEdge", Configuration.WorldTotalRadius);
    }
  }
  public static void Prefix(WaterVolume __instance)
  {
    var obj = __instance;
    obj.m_waterSurface.material.SetFloat("_WaterEdge", Configuration.WorldTotalRadius);
  }
}
[HarmonyPatch(typeof(EnvMan), nameof(EnvMan.UpdateWind))]
public class UpdateWind
{
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.Replace(matcher, 10500f, () => Configuration.WorldRadius);
    // Removes the subtraction of m_edgeOfWorldWidth (already applied above).
    matcher = matcher
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop);
    matcher = Helper.Replace(matcher, 10500f, () => Configuration.WorldRadius);
    // Removes the subtraction of m_edgeOfWorldWidth (already applied above).
    matcher = matcher
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop)
      .SetOpcodeAndAdvance(OpCodes.Nop);
    matcher = Helper.Replace(matcher, 10500f, () => Configuration.WorldTotalRadius);
    return matcher.InstructionEnumeration();
  }
}
