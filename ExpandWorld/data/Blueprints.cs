
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Service;
using UnityEngine;
namespace ExpandWorld;

public class BlueprintObject
{
  public string Prefab = "";
  public Vector3 Pos;
  public Quaternion Rot;
  public ZPackage? Data;
  public float Chance = 1f;
  public Vector3? Scale;
  public bool SnapToGround = false;
  public BlueprintObject(string name, Vector3 pos, Quaternion rot, Vector3 scale, ZPackage? data, float chance, bool snapToGround = false)
  {
    Prefab = name;
    Pos = pos;
    Rot = rot.normalized;
    Data = data;
    Chance = chance;
    SnapToGround = snapToGround;
    // Some blueprints have the scale only in the data, so don't override it with the default.
    if (scale != Vector3.one)
      Scale = scale;
  }
}

public class SnapPoint
{
  public Vector3 Pos;
  public Quaternion Rot;
  public SnapPoint(Vector3 pos, Quaternion rot)
  {
    Pos = pos;
    Rot = rot;
  }
}
public class Blueprint
{
  public string Name;
  public List<BlueprintObject> Objects = new();
  public string CenterPiece = "piece_bpcenterpoint";
  public float Radius = 0f;
  public List<SnapPoint> SnapPoints = new();
  public Vector3 Size = Vector3.one;

  public Blueprint(string name)
  {
    Name = name;
  }
  private void AddSnapPoint(Vector3 pos, Quaternion rot, int index)
  {
    if (SnapPoints.Count <= index)
      SnapPoints.Add(new(pos, rot));
    else
      SnapPoints[index] = new(pos, rot);
  }
  // Provides a way to override or load snap points from pieces or coordinates.
  // Snap point system is used for dungeon room connections.
  public void LoadSnapPoints(string[] snapPieces)
  {
    for (var i = 0; i < snapPieces.Length; i++)
    {
      var piece = snapPieces[i];
      if (piece.Split(',').Length == 3)
      {
        var pos = Parse.VectorXZY(piece);
        AddSnapPoint(pos, Quaternion.identity, i);
        continue;
      }
      var success = false;
      foreach (var obj in Objects)
      {
        if (obj.Prefab != piece) continue;
        if (obj.Chance == 0f) continue;
        obj.Chance = 0f;
        AddSnapPoint(obj.Pos, obj.Rot, i);
        success = true;
        break;
      }
      if (!success)
        ExpandWorld.Log.LogWarning($"Snap point piece {piece} not found in blueprint {Name}.");
    }
  }
  public void Center(string centerPiece)
  {
    if (centerPiece == "")
      centerPiece = CenterPiece;
    Bounds bounds = new();
    var y = float.MaxValue;
    Quaternion rot = Quaternion.identity;
    foreach (var obj in Objects)
    {
      y = Mathf.Min(y, obj.Pos.y);
      bounds.Encapsulate(obj.Pos);
    }
    // Slightly towards the ground to prevent gaps.
    y += 0.05f;
    Size = bounds.size;
    Vector3 center = new(bounds.center.x, y, bounds.center.z);
    foreach (var obj in Objects)
    {
      if (obj.Prefab == centerPiece)
      {
        center = obj.Pos;
        rot = Quaternion.Inverse(obj.Rot);
        // Bit hacky way to prevent it from being spawned.
        obj.Chance = 0f;
        break;
      }
    }
    Radius = Utils.LengthXZ(bounds.extents);
    foreach (var obj in Objects)
      obj.Pos -= center;
    if (rot != Quaternion.identity)
    {
      foreach (var obj in Objects)
      {
        obj.Pos = rot * obj.Pos;
        obj.Rot = rot * obj.Rot;
      }
    }
  }
}
public class Blueprints
{
  private static IEnumerable<string> LoadFiles(string folder, IEnumerable<string> bps)
  {
    if (Directory.Exists(folder))
    {
      var blueprints = Directory.EnumerateFiles(folder, "*.blueprint", SearchOption.AllDirectories);
      var vbuilds = Directory.EnumerateFiles(folder, "*.vbuild", SearchOption.AllDirectories);
      return bps.Concat(blueprints).Concat(vbuilds);
    }
    return bps;
  }
  private static IEnumerable<string> Files()
  {
    IEnumerable<string> bps = new List<string>();
    bps = LoadFiles(Configuration.BlueprintGlobalFolder, bps);
    if (Path.GetFullPath(Configuration.BlueprintLocalFolder) != Path.GetFullPath(Configuration.BlueprintGlobalFolder))
      bps = LoadFiles(Configuration.BlueprintLocalFolder, bps);
    return bps.Distinct().OrderBy(s => s);
  }
  public static bool TryGetBluePrint(string name, out Blueprint blueprint)
  {
    blueprint = new("Invalid");
    var bp = GetBluePrint(name);
    if (bp == null) return false;
    blueprint = bp;
    return true;
  }
  public static Blueprint? GetBluePrint(string name)
  {
    name = name.Replace(" ", "_");
    var path = Files().FirstOrDefault(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_") == name);
    if (path == null) return null;
    var rows = File.ReadAllLines(path);
    var extension = Path.GetExtension(path);
    Blueprint bp = new(name);
    if (extension == ".vbuild") return GetBuildShare(bp, rows);
    if (extension == ".blueprint") return GetPlanBuild(bp, rows);
    throw new InvalidOperationException("Unknown file format.");
  }
  private static Blueprint? GetPlanBuild(Blueprint bp, string[] rows)
  {
    var piece = true;
    var currentRow = -1;
    try
    {
      foreach (var row in rows)
      {
        currentRow += 1;
        if (row.StartsWith("#snappoints", StringComparison.OrdinalIgnoreCase))
          piece = false;
        else if (row.StartsWith("#pieces", StringComparison.OrdinalIgnoreCase))
          piece = true;
        else if (row.StartsWith("#center:", StringComparison.OrdinalIgnoreCase))
          bp.CenterPiece = row.Split(':')[1];
        else if (row.StartsWith("#", StringComparison.Ordinal))
          continue;
        else if (piece)
          bp.Objects.Add(GetPlanBuildObject(row));
        else
          bp.SnapPoints.Add(new(GetPlanBuildSnapPoint(row), Quaternion.identity));
      }
    }
    catch
    {
      ExpandWorld.Log.LogError($"Failed to load blueprint {bp.Name} at row {currentRow}: {rows[currentRow]}.");
      return null;
    }
    return bp;
  }
  private static BlueprintObject GetPlanBuildObject(string row)
  {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(';');
    var name = split[0];
    var posX = InvariantFloat(split, 2);
    var posY = InvariantFloat(split, 3);
    var posZ = InvariantFloat(split, 4);
    var rotX = InvariantFloat(split, 5);
    var rotY = InvariantFloat(split, 6);
    var rotZ = InvariantFloat(split, 7);
    var rotW = InvariantFloat(split, 8);
    // Info is not supported.
    var scaleX = InvariantFloat(split, 10, 1f);
    var scaleY = InvariantFloat(split, 11, 1f);
    var scaleZ = InvariantFloat(split, 12, 1f);
    var data = split.Length > 13 ? split[13] : "";
    var chance = split.Length > 14 ? InvariantFloat(split, 14, 1f) : 1f;
    return new BlueprintObject(name, new(posX, posY, posZ), new(rotX, rotY, rotZ, rotW), new(scaleX, scaleY, scaleZ), DataHelper.Deserialize(data), chance);
  }
  private static Vector3 GetPlanBuildSnapPoint(string row)
  {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(';');
    var x = InvariantFloat(split, 0);
    var y = InvariantFloat(split, 1);
    var z = InvariantFloat(split, 2);
    return new Vector3(x, y, z);
  }
  private static Blueprint GetBuildShare(Blueprint bp, string[] rows)
  {
    bp.Objects = rows.Select(GetBuildShareObject).ToList();
    return bp;
  }
  private static BlueprintObject GetBuildShareObject(string row)
  {
    if (row.IndexOf(',') > -1) row = row.Replace(',', '.');
    var split = row.Split(' ');
    var name = split[0];
    var rotX = InvariantFloat(split, 1);
    var rotY = InvariantFloat(split, 2);
    var rotZ = InvariantFloat(split, 3);
    var rotW = InvariantFloat(split, 4);
    var posX = InvariantFloat(split, 5);
    var posY = InvariantFloat(split, 6);
    var posZ = InvariantFloat(split, 7);
    var data = split.Length > 8 ? split[8] : "";
    var chance = split.Length > 9 ? InvariantFloat(split, 9, 1f) : 1f;
    return new BlueprintObject(name, new(posX, posY, posZ), new(rotX, rotY, rotZ, rotW), Vector3.one, DataHelper.Deserialize(data), chance);
  }
  private static float InvariantFloat(string[] row, int index, float defaultValue = 0f)
  {
    if (index >= row.Length) return defaultValue;
    var s = row[index];
    if (string.IsNullOrEmpty(s)) return defaultValue;
    return float.Parse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
  }
}