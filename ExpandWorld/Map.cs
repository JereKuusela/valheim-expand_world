using HarmonyLib;
using UnityEngine;
namespace ExpandWorld {

  [HarmonyPatch(typeof(Minimap), "Awake")]
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

  [HarmonyPatch(typeof(Minimap), "OnMapLeftClick")]
  public class OnMapLeftClick {
    public static bool Prefix(Minimap __instance) {
      if (Input.GetKey(KeyCode.LeftControl)) {
        Vector3 pos = __instance.ScreenToWorldPoint(Input.mousePosition);
        Console.instance.TryRunCommand("goto " + pos.x + " " + pos.z);
        return false;
      }
      return true;
    }
  }

  [HarmonyPatch(typeof(Minimap), "SetMapMode")]
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
        obj.ForceRegen();
      }
      if (ForceRegen) {
        obj.ForceRegen();
      }
      TextureSizeChanged = false;
      ForceRegen = false;
    }
  }

  [HarmonyPatch(typeof(Minimap), "SetMapData")]
  public class SetMapData {
    // Copy paste from the base code.
    // Changed that explored is not loaded if texture size is different.
    public static bool Prefix(Minimap __instance, byte[] data) {
      var obj = __instance;
      ZPackage zpackage = new ZPackage(data);
      int num = zpackage.ReadInt();
      if (num >= 7) {
        ZLog.Log("Unpacking compressed mapdata " + zpackage.Size());
        zpackage = zpackage.ReadCompressedPackage();
      }
      obj.Reset();
      int num2 = zpackage.ReadInt();
      if (obj.m_textureSize == num2) {
        for (int i = 0; i < obj.m_explored.Length; i++) {
          if (zpackage.ReadBool()) {
            int x = i % num2;
            int y = i / num2;
            obj.Explore(x, y);
          }
        }
        if (num >= 5) {
          for (int j = 0; j < obj.m_exploredOthers.Length; j++) {
            if (zpackage.ReadBool()) {
              int x2 = j % num2;
              int y2 = j / num2;
              obj.ExploreOthers(x2, y2);
            }
          }
        }
      }
      if (num >= 2) {
        int num3 = zpackage.ReadInt();
        obj.ClearPins();
        for (int k = 0; k < num3; k++) {
          string name = zpackage.ReadString();
          Vector3 pos = zpackage.ReadVector3();
          Minimap.PinType type = (Minimap.PinType)zpackage.ReadInt();
          bool isChecked = num >= 3 && zpackage.ReadBool();
          long ownerID = (num >= 6) ? zpackage.ReadLong() : 0L;
          obj.AddPin(pos, type, name, true, isChecked, ownerID);
        }
      }
      if (num >= 4) {
        bool publicReferencePosition = zpackage.ReadBool();
        ZNet.instance.SetPublicReferencePosition(publicReferencePosition);
      }
      obj.m_fogTexture.Apply();
      return false;
    }
  }


  [HarmonyPatch(typeof(Minimap), "UpdateBiome")]

  public class Minimap_ShowPos {
    // Text doesn't always get updated so extra stuff must be reseted manually.
    private static string previousText = "";
    public static void Prefix(Minimap __instance) {
      __instance.m_biomeNameLarge.text = previousText;

    }
    public static void Postfix(Minimap __instance, Player player) {
      var obj = __instance;
      previousText = obj.m_biomeNameLarge.text;
      if (Terminal.m_cheat && obj.m_mode == Minimap.MapMode.Large) {
        var position = obj.ScreenToWorldPoint(ZInput.IsMouseActive() ? Input.mousePosition : new Vector3((float)(Screen.width / 2), (float)(Screen.height / 2)));
        var zone = ZoneSystem.instance.GetZone(position);
        var zoneText = "zone: " + zone.x + "/" + zone.y;
        var positionText = "x: " + position.x.ToString("F0") + " z: " + position.z.ToString("F0");
        var distanceText = "distance: " + Utils.DistanceXZ(position, player.transform.position).ToString("P0") + " meters";
        var text = "\n\n" + previousText + "\n" + zoneText + "\n" + positionText + "\n" + distanceText;
        obj.m_biomeNameLarge.text = text;
      }
    }
  }
}