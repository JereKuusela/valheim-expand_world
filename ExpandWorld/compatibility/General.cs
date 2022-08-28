using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
namespace ExpandWorld;

[HarmonyPatch]
public class TryParse {
  static IEnumerable<MethodBase> TargetMethods() {
    return typeof(Enum).GetMethods()
        .Where(method => method.Name.StartsWith("TryParse"))
        .Select(method => method.MakeGenericMethod(typeof(Heightmap.Biome)))
        .Cast<MethodBase>();
  }
  static bool Prefix(string value, ref Heightmap.Biome result, ref bool __result) {
    __result = BiomeManager.TryGetBiome(value, out result);
    return false;
  }
}

[HarmonyPatch(typeof(Enum), nameof(Enum.GetValues))]
public class GetValues {
  static bool Prefix(Type enumType, ref Array __result) {
    if (enumType != typeof(Heightmap.Biome)) return true;
    __result = BiomeManager.Biomes;
    return false;
  }
}
[HarmonyPatch(typeof(Enum), nameof(Enum.GetNames))]
public class GetNames {
  static bool Prefix(Type enumType, ref string[] __result) {
    if (enumType != typeof(Heightmap.Biome)) return true;
    __result = BiomeManager.BiomeToName.Values.ToArray();
    return false;
  }
}
[HarmonyPatch(typeof(Enum), nameof(Enum.GetName))]
public class GetName {
  static bool Prefix(Type enumType, object value, ref string __result) {
    if (enumType != typeof(Heightmap.Biome)) return true;
    __result = BiomeManager.BiomeToName[(Heightmap.Biome)value];
    return false;
  }
}
[HarmonyPatch(typeof(Enum), nameof(Enum.Parse), typeof(Type), typeof(string))]
public class Parse {
  static bool Prefix(Type enumType, string value, ref object __result) {
    if (enumType != typeof(Heightmap.Biome)) return true;
    if (BiomeManager.TryGetBiome(value, out var biome))
      __result = biome;
    else
      return true; // Let the original function handle the throwing.
    return false;
  }
}
[HarmonyPatch(typeof(Enum), nameof(Enum.Parse), typeof(Type), typeof(string), typeof(bool))]
public class ParseIgnoreCase {
  static bool Prefix(Type enumType, string value, ref object __result) {
    if (enumType != typeof(Heightmap.Biome)) return true;
    if (BiomeManager.TryGetBiome(value, out var biome))
      __result = biome;
    else
      return true; // Let the original function handle the throwing.
    return false;
  }
}
