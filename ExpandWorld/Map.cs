using HarmonyLib;
using UnityEngine;
namespace ExpandWorld;

[HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake))]
public class MinimapAwake {
  // Applies the map parameter changes.
  public static float OriginalPixelSize;
  public static int OriginalTextureSize;
  public static float OriginalMinZoom;
  static void Postfix(Minimap __instance) {
    OriginalTextureSize = __instance.m_textureSize;
    __instance.m_textureSize = (int)(__instance.m_textureSize * Configuration.MapSize);
    OriginalMinZoom = __instance.m_maxZoom;
    __instance.m_maxZoom = MinimapAwake.OriginalMinZoom * Configuration.MapSize;
    OriginalPixelSize = __instance.m_pixelSize;
    __instance.m_pixelSize *= Configuration.MapPixelSize;
    __instance.m_mapImageLarge.rectTransform.localScale = new(Configuration.MapSize, Configuration.MapSize, Configuration.MapSize);
  }
}

[HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapData))]
public class InitializeWhenDimensionsChange {
  static bool Prefix(Minimap __instance, byte[] data) {
    var obj = __instance;
    ZPackage zpackage = new(data);
    var num = zpackage.ReadInt();
    if (num >= 7) zpackage = zpackage.ReadCompressedPackage();
    int num2 = zpackage.ReadInt();
    if (obj.m_textureSize == num2) return true;
    // Base game code would stop initializxing.
    obj.Reset();
    obj.m_fogTexture.Apply();
    return false;
  }
}

[HarmonyPatch(typeof(Minimap), nameof(Minimap.Update))]
public class Map_WaitForConfigSync {
  static bool Prefix() => ExpandWorld.ConfigSync.IsSourceOfTruth || ExpandWorld.ConfigSync.InitialSyncDone;
}

[HarmonyPatch(typeof(Minimap), nameof(Minimap.Explore), new[] { typeof(int), typeof(int) })]
public class PreventExploreWhenDirty1 {
  static bool Prefix(Minimap __instance) => __instance.m_textureSize == MapGeneration.TextureSize;
}
[HarmonyPatch(typeof(Minimap), nameof(Minimap.Explore), new[] { typeof(Vector3), typeof(float) })]
public class PreventExploreWhenDirty2 {
  static bool Prefix(Minimap __instance) => __instance.m_textureSize == MapGeneration.TextureSize;
}
[HarmonyPatch(typeof(Minimap), nameof(Minimap.ExploreOthers))]
public class PreventExploreOthersWhenDirty {
  static bool Prefix(Minimap __instance) => __instance.m_textureSize == MapGeneration.TextureSize;
}
[HarmonyPatch(typeof(Minimap), nameof(Minimap.ExploreAll))]
public class PreventExploreAllWhenDirty {
  static bool Prefix(Minimap __instance) => __instance.m_textureSize == MapGeneration.TextureSize;
}
[HarmonyPatch(typeof(Minimap), nameof(Minimap.IsExplored))]
public class PreventIsExploredWhenDirty {
  static bool Prefix(Minimap __instance) => __instance.m_textureSize == MapGeneration.TextureSize;
}