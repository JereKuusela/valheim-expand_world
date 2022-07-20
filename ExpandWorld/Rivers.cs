using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
namespace ExpandWorld;

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.FindLakes))]
public class FindLakes {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.Replace(matcher, -10000f, () => -Configuration.WorldRadius);
    matcher = Helper.Replace(matcher, -10000f, () => -Configuration.WorldRadius);
    matcher = Helper.Replace(matcher, 10000f, () => Configuration.WorldRadius);
    matcher = Helper.ReplaceStretch(matcher, OpCodes.Ldloc_3);
    matcher = Helper.ReplaceStretch(matcher, OpCodes.Ldloc_2);
    matcher = Helper.Replace(matcher, 0.05f, () => Helper.AltitudeToBaseHeight(Configuration.LakeDepth));
    matcher = Helper.Replace(matcher, 128f, () => Configuration.LakeSearchInterval);
    matcher = Helper.Replace(matcher, 10000f, () => Configuration.WorldRadius);
    matcher = Helper.Replace(matcher, 128f, () => Configuration.LakeSearchInterval);
    matcher = Helper.Replace(matcher, 10000f, () => Configuration.WorldRadius);
    matcher = Helper.Replace(matcher, 800f, () => Configuration.LakeMergeRadius);
    return matcher.InstructionEnumeration();
  }
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.PlaceRivers))]
public class PlaceRivers {
  static bool Prefix(ref List<WorldGenerator.River> __result) {
    if (Configuration.Rivers) return true;
    __result = new();
    return false;
  }

  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.ReplaceSeed(matcher, nameof(WorldGenerator.m_riverSeed), (WorldGenerator obj) => Configuration.RiverSeed ?? obj.m_riverSeed);
    matcher = Helper.Replace(matcher, 2000f, () => Configuration.LakeMaxDistance1);
    matcher = Helper.Replace(matcher, 0.4f, () => Helper.AltitudeToBaseHeight(Configuration.RiverMaxAltitude));
    matcher = Helper.Replace(matcher, 128f, () => Configuration.RiverCheckInterval);
    matcher = Helper.Replace(matcher, 5000f, () => Configuration.LakeMaxDistance2);
    matcher = Helper.Replace(matcher, 0.4f, () => Helper.AltitudeToBaseHeight(Configuration.RiverMaxAltitude));
    matcher = Helper.Replace(matcher, 128f, () => Configuration.RiverCheckInterval);
    matcher = Helper.Replace(matcher, 60f, () => Configuration.RiverMinWidth);
    matcher = Helper.Replace(matcher, 100f, () => Configuration.RiverMaxWidth);
    matcher = Helper.Replace(matcher, 60f, () => Configuration.RiverMinWidth);
    matcher = Helper.Replace(matcher, 15f, () => Configuration.RiverCurveWidth);
    matcher = Helper.Replace(matcher, 20f, () => Configuration.RiverCurveWaveLength);
    return matcher.InstructionEnumeration();
  }
}


[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.IsRiverAllowed))]
public class IsRiverAllowed {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.ReplaceStretch(matcher, OpCodes.Ldfld);
    matcher = Helper.ReplaceStretch(matcher, OpCodes.Ldfld);
    matcher = Helper.Replace(matcher, 0.05f, () => Helper.AltitudeToBaseHeight(Configuration.LakeDepth));
    return matcher.InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.PlaceStreams))]
public class PlaceStreams {
  static bool Prefix(ref List<WorldGenerator.River> __result) {
    if (Configuration.Streams) return true;
    __result = new();
    return false;
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.ReplaceSeed(matcher, nameof(WorldGenerator.m_streamSeed), (WorldGenerator obj) => Configuration.StreamSeed ?? obj.m_streamSeed);
    matcher = Helper.Replace(matcher, (sbyte)100, () => Configuration.StreamSearchIterations ?? 100);
    matcher = Helper.Replace(matcher, 26f, () => Helper.AltitudeToHeight(Configuration.StreamStartMinAltitude));
    matcher = Helper.Replace(matcher, 31f, () => Helper.AltitudeToHeight(Configuration.StreamStartMaxAltitude));
    matcher = Helper.Replace(matcher, (sbyte)100, () => Configuration.StreamSearchIterations ?? 100);
    matcher = Helper.Replace(matcher, 36f, () => Helper.AltitudeToHeight(Configuration.StreamEndMinAltitude));
    matcher = Helper.Replace(matcher, 44f, () => Helper.AltitudeToHeight(Configuration.StreamEndMaxAltitude));
    matcher = Helper.Replace(matcher, 80f, () => Configuration.StreamMinLength);
    matcher = Helper.Replace(matcher, 200f, () => Configuration.StreamMaxLength);
    matcher = Helper.Replace(matcher, 26f, () => Helper.AltitudeToHeight(Configuration.StreamStartMinAltitude));
    matcher = Helper.Replace(matcher, 44f, () => Helper.AltitudeToHeight(Configuration.StreamEndMaxAltitude));
    matcher = Helper.Replace(matcher, 20f, () => Configuration.StreamMinWidth);
    matcher = Helper.Replace(matcher, 20f, () => Configuration.StreamMaxWidth);
    matcher = Helper.Replace(matcher, 15f, () => Configuration.StreamCurveWidth);
    matcher = Helper.Replace(matcher, 20f, () => Configuration.StreamCurveWaveLength);
    matcher = Helper.Replace(matcher, 3000, () => Configuration.StreamMaxAmount ?? 3000);
    return matcher.InstructionEnumeration();
  }
}

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.FindStreamStartPoint))]
public class FindStreamStartPoint {
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
    var matcher = new CodeMatcher(instructions);
    matcher = Helper.Replace(matcher, -10000f, () => -Configuration.WorldRadius);
    matcher = Helper.Replace(matcher, 10000f, () => Configuration.WorldRadius);
    matcher = Helper.Replace(matcher, -10000f, () => -Configuration.WorldRadius);
    matcher = Helper.Replace(matcher, 10000f, () => Configuration.WorldRadius);
    return matcher.InstructionEnumeration();
  }
}
[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.AddRivers))]
public class AddRivers {
  // Rivers are placed at unstretched positions.
  static void Prefix(ref float wx, ref float wy) {
    wx *= Configuration.WorldStretch;
    wy *= Configuration.WorldStretch;
  }
}