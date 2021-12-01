using HarmonyLib;
using UnityEngine;
namespace ExpandWorld {

  [HarmonyPatch(typeof(WorldGenerator), "GetAshlandsHeight")]
  public class GetAshlandsHeight {
    public static bool Prefix(WorldGenerator __instance, float wx, float wy, ref float __result) {
      var obj = __instance;
      float wx2 = wx;
      float wy2 = wy;
      float num = obj.GetBaseHeight(wx, wy, false);
      wx += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      wy += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      float num2 = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
      num2 += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num2 * 0.5f;
      num += num2 * 0.1f;
      num += 0.1f;
      num += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
      num += Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
      __result = obj.AddRivers(wx2, wy2, num);
      return false;
    }
  }
  [HarmonyPatch(typeof(WorldGenerator), "GetDeepNorthHeight")]
  public class GetDeepNorthHeight {
    public static bool Prefix(WorldGenerator __instance, float wx, float wy, ref float __result) {
      var obj = __instance;
      float wx2 = wx;
      float wy2 = wy;
      float num = obj.GetBaseHeight(wx, wy, false);
      wx += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      wy += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      float num2 = Mathf.Max(0f, num - 0.4f);
      num += num2;
      float num3 = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
      num3 += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num3 * 0.5f;
      num += num3 * 0.2f;
      num *= 1.2f;
      num = obj.AddRivers(wx2, wy2, num);
      num += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
      __result = num + Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
      return false;
    }
  }
  [HarmonyPatch(typeof(WorldGenerator), "GetForestHeight")]
  public class GetForestHeight {
    public static bool Prefix(WorldGenerator __instance, float wx, float wy, ref float __result) {
      var obj = __instance;
      float wx2 = wx;
      float wy2 = wy;
      float num = obj.GetBaseHeight(wx, wy, false);
      wx += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      wy += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      float num2 = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
      num2 += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num2 * 0.5f;
      num += num2 * 0.1f;
      num = obj.AddRivers(wx2, wy2, num);
      num += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
      __result = num + Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
      return false;
    }
  }
  [HarmonyPatch(typeof(WorldGenerator), "GetMeadowsHeight")]
  public class GetMeadowsHeight {
    public static bool Prefix(WorldGenerator __instance, float wx, float wy, ref float __result) {
      var obj = __instance;
      float wx2 = wx;
      float wy2 = wy;
      float baseHeight = obj.GetBaseHeight(wx, wy, false);
      wx += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      wy += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      float num = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
      num += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num * 0.5f;
      float num2 = baseHeight;
      num2 += num * 0.1f;
      float num3 = 0.15f;
      float num4 = num2 - num3;
      float num5 = Mathf.Clamp01(baseHeight / 0.4f);
      if (num4 > 0f) {
        num2 -= num4 * (1f - num5) * 0.75f;
      }
      num2 = obj.AddRivers(wx2, wy2, num2);
      num2 += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
      __result = num2 + Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
      return false;
    }
  }
  [HarmonyPatch(typeof(WorldGenerator), "GetPlainsHeight")]
  public class GetPlainsHeight {
    public static bool Prefix(WorldGenerator __instance, float wx, float wy, ref float __result) {
      var obj = __instance;
      float wx2 = wx;
      float wy2 = wy;
      float baseHeight = obj.GetBaseHeight(wx, wy, false);
      wx += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      wy += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      float num = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
      num += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num * 0.5f;
      float num2 = baseHeight;
      num2 += num * 0.1f;
      float num3 = 0.15f;
      float num4 = num2 - num3;
      float num5 = Mathf.Clamp01(baseHeight / 0.4f);
      if (num4 > 0f) {
        num2 -= num4 * (1f - num5) * 0.75f;
      }
      num2 = obj.AddRivers(wx2, wy2, num2);
      num2 += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
      __result = num2 + Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
      return false;
    }
  }
  [HarmonyPatch(typeof(WorldGenerator), "GetSnowMountainHeight")]
  public class GetSnowMountainHeight {
    public static bool Prefix(WorldGenerator __instance, float wx, float wy, bool menu, ref float __result) {
      if (menu) return true;
      var obj = __instance;
      float wx2 = wx;
      float wy2 = wy;
      float num = obj.GetBaseHeight(wx, wy, false);
      float num2 = obj.BaseHeightTilt(wx, wy);
      wx += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      wy += 100000f + (Settings.UseHeightSeed ? Settings.HeightSeed : obj.m_offset3);
      float num3 = num - 0.4f;
      num += num3;
      float num4 = Mathf.PerlinNoise(wx * 0.01f, wy * 0.01f) * Mathf.PerlinNoise(wx * 0.02f, wy * 0.02f);
      num4 += Mathf.PerlinNoise(wx * 0.05f, wy * 0.05f) * Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * num4 * 0.5f;
      num += num4 * 0.2f;
      num = obj.AddRivers(wx2, wy2, num);
      num += Mathf.PerlinNoise(wx * 0.1f, wy * 0.1f) * 0.01f;
      num += Mathf.PerlinNoise(wx * 0.4f, wy * 0.4f) * 0.003f;
      __result = num + Mathf.PerlinNoise(wx * 0.2f, wy * 0.2f) * 2f * num2;
      return false;
    }
  }
}