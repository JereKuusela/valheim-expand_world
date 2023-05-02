using System.Collections.Generic;
using System.IO;

namespace ExpandWorld;

public class BlueprintManager
{
  public static bool Has(string name) => BlueprintFiles.ContainsKey(Parse.Name(name));
  public static bool TryGet(string name, out Blueprint bp) => BlueprintFiles.TryGetValue(Parse.Name(name), out bp);
  public static Dictionary<string, Blueprint> BlueprintFiles = new();
  private static Dictionary<string, string> CenterPieces = new();
  public static bool Load(string name, string centerPiece)
  {
    var hash = name.GetStableHashCode();
    if (ZNetScene.instance.m_namedPrefabs.ContainsKey(hash)) return true;
    if (Blueprints.TryGetBluePrint(name, out var bp))
    {
      // Already loaded so no point to check again.
      // Center piece could be different but that would cause problems anyway.
      if (BlueprintFiles.ContainsKey(name)) return true;
      CenterPieces[name] = centerPiece;
      bp.Center(centerPiece);
      BlueprintFiles[name] = bp;
      foreach (var obj in bp.Objects)
      {
        if (obj.Chance == 0) continue;
        Load(obj.Prefab, centerPiece);
      }
      return true;
    }
    ExpandWorld.Log.LogWarning($"Object / blueprint {name} not found!");
    return false;
  }

  public static void Reload(string name)
  {
    if (!Blueprints.TryGetBluePrint(name, out var bp)) return;
    name = Path.GetFileNameWithoutExtension(name);
    if (!Has(name)) return;
    ExpandWorld.Log.LogInfo($"Reloading blueprint {name}.");
    Load(name, CenterPieces.TryGetValue(name, out var centerPiece) ? centerPiece : "");
  }
}