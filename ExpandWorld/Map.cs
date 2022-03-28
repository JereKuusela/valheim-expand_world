using HarmonyLib;
using UnityEngine;
namespace ExpandWorld {

  [HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake))]
  public class MinimapAwake {
    // Applies the map parameter changes.
    public static float OriginalPixelSize;
    public static int OriginalTextureSize;
    public static void Postfix(Minimap __instance) {
      OriginalTextureSize = __instance.m_textureSize;
      __instance.m_textureSize *= Settings.MapSize;
      __instance.m_minZoom /= Settings.MapSize;
      OriginalPixelSize = __instance.m_pixelSize;
      __instance.m_pixelSize *= Settings.MapPixelSize;
      __instance.m_mapImageLarge.rectTransform.localScale = new Vector3(Settings.MapSize, Settings.MapSize, Settings.MapSize);
    }
  }

  [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapMode))]
  public class SetMapMode {
    public static bool ForceRegen = false;
    public static bool TextureSizeChanged = false;
    public static void Prefix(Minimap __instance, Minimap.MapMode mode) {
      var obj = __instance;
      if (mode == obj.m_mode || mode != Minimap.MapMode.Large) return;
      if (TextureSizeChanged) {
        obj.m_explored = new bool[obj.m_textureSize * obj.m_textureSize];
        obj.m_exploredOthers = new bool[obj.m_textureSize * obj.m_textureSize];
        obj.Start();
      }
      if (ForceRegen || TextureSizeChanged) {
        obj.ForceRegen();
        SetupMaterial.Refresh();
      }
      TextureSizeChanged = false;
      ForceRegen = false;
    }
  }

  [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapData))]
  public class InitializeWhenDimensionsChange {
    public static bool Prefix(Minimap __instance, byte[] data) {
      var obj = __instance;
      ZPackage zpackage = new ZPackage(data);
      var num = zpackage.ReadInt();
      if (num >= 7) zpackage = zpackage.ReadCompressedPackage();
      int num2 = zpackage.ReadInt();
      if (obj.m_textureSize == num2) return true;
      // Base game code would stop initializxing.
      obj.Reset();
      obj.ClearPins();
      obj.m_fogTexture.Apply();
      return false;
    }
  }
}
