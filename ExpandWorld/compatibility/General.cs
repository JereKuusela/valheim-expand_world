using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
namespace ExpandWorld;

#pragma warning disable IDE0051
[HarmonyPatch]
public class TryParseBiome
{
  static IEnumerable<MethodBase> TargetMethods()
  {
    return typeof(Enum).GetMethods()
        .Where(method => method.Name.StartsWith("TryParse"))
        .Select(method => method.MakeGenericMethod(typeof(Heightmap.Biome)))
        .Cast<MethodBase>();
  }

  static bool Prefix(string value, ref Heightmap.Biome result, ref bool __result)
  {
    __result = BiomeManager.TryGetBiome(value, out result);
    return false;
  }
}
[HarmonyPatch]
public class TryParseTheme
{
  static IEnumerable<MethodBase> TargetMethods()
  {
    return typeof(Enum).GetMethods()
        .Where(method => method.Name.StartsWith("TryParse"))
        .Select(method => method.MakeGenericMethod(typeof(Room.Theme)))
        .Cast<MethodBase>();
  }
  static bool Prefix(string value, ref Room.Theme result, ref bool __result)
  {
    __result = RoomLoading.TryGetTheme(value, out result);
    return false;
  }
}
#pragma warning restore  IDE0051
[HarmonyPatch(typeof(Enum), nameof(Enum.GetValues))]
public class GetValues
{
  static bool Prefix(Type enumType, ref Array __result)
  {
    if (enumType == typeof(Heightmap.Biome))
    {
      __result = BiomeManager.BiomeToDisplayName.Keys.ToArray();
      return false;
    }
    if (enumType == typeof(Room.Theme))
    {
      __result = RoomLoading.ThemeToName.Keys.ToArray();
      return false;
    }
    return true;
  }
}
[HarmonyPatch(typeof(Enum), nameof(Enum.GetNames))]
public class GetNames
{
  static bool Prefix(Type enumType, ref string[] __result)
  {
    if (enumType == typeof(Heightmap.Biome))
    {
      __result = BiomeManager.BiomeToDisplayName.Values.ToArray();
      return false;
    }
    if (enumType == typeof(Room.Theme))
    {
      __result = RoomLoading.ThemeToName.Values.ToArray();
      return false;
    }
    return true;
  }
}
[HarmonyPatch(typeof(Enum), nameof(Enum.GetName))]
public class GetName
{
  static bool Prefix(Type enumType, object value, ref string __result)
  {
    if (enumType == typeof(Heightmap.Biome))
    {
      if (BiomeManager.TryGetDisplayName((Heightmap.Biome)value, out var result))
        __result = result;
      else
        __result = "None";
      return false;
    }
    if (enumType == typeof(Room.Theme))
    {
      if (RoomLoading.ThemeToName.TryGetValue((Room.Theme)value, out var result))
        __result = result;
      else
        __result = ((int)value).ToString();
      return false;
    }
    return true;
  }
}
[HarmonyPatch(typeof(Enum), nameof(Enum.Parse), typeof(Type), typeof(string))]
public class EnumParse
{
  static bool Prefix(Type enumType, string value, ref object __result)
  {
    if (enumType == typeof(Heightmap.Biome))
    {
      if (BiomeManager.TryGetBiome(value, out var biome))
      {
        __result = biome;
        return false;
      }
      // Let the original function handle the throwing.
    }
    if (enumType == typeof(Room.Theme))
    {
      if (RoomLoading.TryGetTheme(value, out var theme))
      {
        __result = theme;
        return false;
      }
      // Let the original function handle the throwing.
    }
    return true;
  }
}
[HarmonyPatch(typeof(Enum), nameof(Enum.Parse), typeof(Type), typeof(string), typeof(bool))]
public class ParseIgnoreCase
{
  static bool Prefix(Type enumType, string value, ref object __result)
  {
    if (enumType == typeof(Heightmap.Biome))
    {
      if (BiomeManager.TryGetBiome(value, out var biome))
      {
        __result = biome;
        return false;
      }
      // Let the original function handle the throwing.
    }
    if (enumType == typeof(Room.Theme))
    {
      if (RoomLoading.TryGetTheme(value, out var theme))
      {
        __result = theme;
        return false;
      }
      // Let the original function handle the throwing.
    }
    return true;
  }
}
