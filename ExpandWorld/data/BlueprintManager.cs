using System.Collections.Generic;

namespace ExpandWorld;

//<summary>External blueprint data from configs (not directly from the blueprint file).
public class BlueprintMetaData
{
  public string CenterPiece = "";
  public string[] SnapPieces = new string[0];
  public BlueprintMetaData(string centerPiece, string[] snapPieces)
  {
    CenterPiece = centerPiece;
    SnapPieces = snapPieces;
  }
}
public class BlueprintManager
{
  public static bool Has(string name) => BlueprintFiles.ContainsKey(Parse.Name(name));
  public static bool TryGet(string name, out Blueprint bp) => BlueprintFiles.TryGetValue(Parse.Name(name), out bp);
  public static Dictionary<string, Blueprint> BlueprintFiles = new();
  private static Dictionary<string, BlueprintMetaData> MetaData = new();
  public static bool Load(string name, string centerPiece) => Load(name, centerPiece, new string[0]);
  public static bool Load(string name, string centerPiece, string[] snapPieces)
  {
    var hash = name.GetStableHashCode();
    if (ZNetScene.instance.m_namedPrefabs.ContainsKey(hash)) return true;
    if (BlueprintFiles.ContainsKey(name) && MetaData.TryGetValue(name, out var data))
    {
      // Already loaded so no point to check again.
      if (data.CenterPiece == centerPiece && data.SnapPieces == snapPieces) return true;
    }
    if (Blueprints.TryGetBluePrint(name, out var bp))
    {
      MetaData[name] = new(centerPiece, snapPieces);
      bp.LoadSnapPoints(snapPieces);
      bp.Center(centerPiece);
      BlueprintFiles[name] = bp;
      foreach (var obj in bp.Objects)
      {
        if (obj.Chance == 0) continue;
        Load(obj.Prefab, centerPiece, snapPieces);
      }
      return true;
    }
    ExpandWorld.Log.LogWarning($"Object / blueprint {name} not found!");
    return false;
  }
  public static void Reload(string name)
  {
    if (!Has(name)) return;
    if (!Blueprints.TryGetBluePrint(name, out var bp)) return;
    ExpandWorld.Log.LogInfo($"Reloading blueprint {name}.");
    BlueprintFiles.Remove(name);
    if (MetaData.TryGetValue(name, out var data))
      Load(name, data.CenterPiece, data.SnapPieces);
    else
      Load(name, "", new string[0]);
  }
}