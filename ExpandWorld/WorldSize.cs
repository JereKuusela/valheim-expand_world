using HarmonyLib;
using UnityEngine;
namespace ExpandWorld {

  [HarmonyPatch(typeof(Ship), "ApplyEdgeForce")]
  public class ApplyEdgeForce {
    // Copy paste from the base game code.
    public static bool Prefix(Ship __instance, float dt) {
      var obj = __instance;
      float magnitude = obj.transform.position.magnitude;
      float num = Settings.WorldTotalRadius - 80;
      if (magnitude > num) {
        Vector3 a = Vector3.Normalize(obj.transform.position);
        float d = Utils.LerpStep(num, Settings.WorldTotalRadius, magnitude) * 8f;
        Vector3 a2 = a * d;
        obj.m_body.AddForce(a2 * dt, ForceMode.VelocityChange);
      }
      return false;
    }
  }

  [HarmonyPatch(typeof(Player), "EdgeOfWorldKill")]
  public class EdgeOfWorldKill {
    // Copy paste from the base game code.
    public static bool Prefix(Player __instance, float dt) {
      var obj = __instance;
      if (obj.IsDead()) {
        return false;
      }
      float magnitude = obj.transform.position.magnitude;
      float num = Settings.WorldTotalRadius - 80;
      if (magnitude > num && (obj.IsSwiming() || obj.transform.position.y < ZoneSystem.instance.m_waterLevel)) {
        Vector3 a = Vector3.Normalize(obj.transform.position);
        float d = Utils.LerpStep(num, Settings.WorldTotalRadius, magnitude) * 10f;
        obj.m_body.MovePosition(obj.m_body.position + a * d * dt);
      }
      if (magnitude > num && obj.transform.position.y < ZoneSystem.instance.m_waterLevel - 40f) {
        HitData hitData = new HitData();
        hitData.m_damage.m_damage = 99999f;
        obj.Damage(hitData);
      }
      return false;
    }
  }

  [HarmonyPatch(typeof(WorldGenerator), "GetBaseHeight")]
  public class GetBaseHeight {
    // Copy paste from the base game code.
    public static bool Prefix(WorldGenerator __instance, float wx, float wy, bool menuTerrain, ref float __result) {
      if (menuTerrain) return true;
      var obj = __instance;
      float num2 = Utils.Length(wx, wy);
      wx += 100000f + (Settings.UseOffsetX ? Settings.OffsetX : obj.m_offset0);
      wy += 100000f + (Settings.UseOffsetY ? Settings.OffsetY : obj.m_offset0);
      float num3 = 0f;
      num3 += Mathf.PerlinNoise(wx * 0.002f * 0.5f, wy * 0.002f * 0.5f) * Mathf.PerlinNoise(wx * 0.003f * 0.5f, wy * 0.003f * 0.5f) * 1f;
      num3 += Mathf.PerlinNoise(wx * 0.002f * 1f, wy * 0.002f * 1f) * Mathf.PerlinNoise(wx * 0.003f * 1f, wy * 0.003f * 1f) * num3 * 0.9f;
      num3 += Mathf.PerlinNoise(wx * 0.005f * 1f, wy * 0.005f * 1f) * Mathf.PerlinNoise(wx * 0.01f * 1f, wy * 0.01f * 1f) * 0.5f * num3;
      num3 -= 0.07f;
      float num4 = Mathf.PerlinNoise(wx * 0.002f * 0.25f + 0.123f, wy * 0.002f * 0.25f + 0.15123f);
      float num5 = Mathf.PerlinNoise(wx * 0.002f * 0.25f + 0.321f, wy * 0.002f * 0.25f + 0.231f);
      float v = Mathf.Abs(num4 - num5);
      float num6 = 1f - Utils.LerpStep(0.02f, 0.12f, v);
      num6 *= Utils.SmoothStep(744f, 1000f, num2);
      num3 *= 1f - num6;
      if (num2 > Settings.WorldRadius) {
        float t = Utils.LerpStep(Settings.WorldRadius, Settings.WorldTotalRadius, num2);
        num3 = Mathf.Lerp(num3, -0.2f, t);
        float num7 = Settings.WorldTotalRadius - 10;
        if (num2 > num7) {
          float t2 = Utils.LerpStep(num7, Settings.WorldTotalRadius, num2);
          num3 = Mathf.Lerp(num3, -2f, t2);
        }
      }
      if (num2 < obj.m_minMountainDistance && num3 > 0.28f) {
        float t3 = Mathf.Clamp01((num3 - 0.28f) / 0.099999994f);
        num3 = Mathf.Lerp(Mathf.Lerp(0.28f, 0.38f, t3), num3, Utils.LerpStep(obj.m_minMountainDistance - 400f, obj.m_minMountainDistance, num2));
      }
      __result = num3;
      return false;
    }
  }

  [HarmonyPatch(typeof(WorldGenerator), "GetEdgeHeight")]
  public class GetEdgeHeight {
    // Copy paste from the base game code.
    public static bool Prefix(WorldGenerator __instance, float wx, float wy, ref float __result) {
      var obj = __instance;
      float magnitude = new Vector2(wx, wy).magnitude;
      float num = Settings.WorldTotalRadius - 10;
      if (magnitude > num) {
        float num2 = Utils.LerpStep(num, Settings.WorldTotalRadius, magnitude);
        __result = -2f * num2;
        return false;
      }
      float t = Utils.LerpStep(Settings.WorldRadius, Settings.WorldRadius + 100, magnitude);
      float num3 = obj.GetBaseHeight(wx, wy, false);
      num3 = Mathf.Lerp(num3, 0f, t);
      __result = obj.AddRivers(wx, wy, num3);
      return false;
    }
  }
  [HarmonyPatch(typeof(WaterVolume), "GetWaterSurface")]
  public class GetWaterSurface {
    // Copy paste from the base game code.
    public static bool Prefix(WaterVolume __instance, Vector3 point, float waveFactor, ref float __result) {
      var obj = __instance;
      float wrappedDayTimeSeconds = ZNet.instance.GetWrappedDayTimeSeconds();
      float depth = obj.Depth(point);
      float num = obj.CalcWave(point, depth, wrappedDayTimeSeconds, waveFactor);
      float num2 = obj.transform.position.y + num + obj.m_surfaceOffset;
      if (Utils.LengthXZ(point) > Settings.WorldTotalRadius && obj.m_forceDepth < 0f) {
        num2 -= 100f;
      }
      __result = num2;
      return false;
    }
  }
  [HarmonyPatch(typeof(WaterVolume), "SetupMaterial")]
  public class SetupMaterial {
    public static void Refresh() {
      var objects = Object.FindObjectsOfType<WaterVolume>();
      foreach (var water in objects) {
        water.m_waterSurface.material.SetFloat("_WaterEdge", Settings.WorldTotalRadius);
      }
    }
    // Copy paste from the base game code.
    public static void Prefix(WaterVolume __instance) {
      var obj = __instance;
      obj.m_waterSurface.material.SetFloat("_WaterEdge", Settings.WorldTotalRadius);
    }
  }
  [HarmonyPatch(typeof(EnvMan), "UpdateWind")]
  public class UpdateWind {
    // Copy paste from the base game code.
    public static bool Prefix(EnvMan __instance, long timeSec, float dt) {
      var obj = __instance;
      if (obj.m_debugWind) return true;

      EnvSetup currentEnvironment = obj.GetCurrentEnvironment();
      if (currentEnvironment == null) return true;

      UnityEngine.Random.State state = UnityEngine.Random.state;
      float f2 = 0f;
      float num = 0.5f;
      obj.AddWindOctave(timeSec, 1, ref f2, ref num);
      obj.AddWindOctave(timeSec, 2, ref f2, ref num);
      obj.AddWindOctave(timeSec, 4, ref f2, ref num);
      obj.AddWindOctave(timeSec, 8, ref f2, ref num);
      UnityEngine.Random.state = state;
      Vector3 dir2 = new Vector3(Mathf.Sin(f2), 0f, Mathf.Cos(f2));
      num = Mathf.Lerp(currentEnvironment.m_windMin, currentEnvironment.m_windMax, num);
      if (Player.m_localPlayer) {
        float magnitude = Player.m_localPlayer.transform.position.magnitude;
        if (magnitude > Settings.WorldRadius) {
          float num2 = Utils.LerpStep(Settings.WorldRadius, Settings.WorldTotalRadius, magnitude);
          num2 = 1f - Mathf.Pow(1f - num2, 2f);
          dir2 = Player.m_localPlayer.transform.position.normalized;
          num = Mathf.Lerp(num, 1f, num2);
        } else {
          Ship localShip = Ship.GetLocalShip();
          if (localShip && localShip.IsWindControllActive()) {
            dir2 = localShip.transform.forward;
          }
        }
      }
      obj.SetTargetWind(dir2, num);


      obj.UpdateWindTransition(dt);
      return false;
    }
  }
}