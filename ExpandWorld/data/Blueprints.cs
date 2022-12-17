
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ExpandWorld;

public class BlueprintObject
{
  public string Prefab = "";
  public Vector3 Pos;
  public Quaternion Rot;
  public Vector3 Scale;
  public string ExtraInfo;
  public ZDO? Data;
  public BlueprintObject(string name, Vector3 pos, Quaternion rot, Vector3 scale, string info, ZDO? data)
  {
    Prefab = name;
    Pos = pos;
    Rot = rot.normalized;
    Scale = scale;
    ExtraInfo = info;
    Data = data;
  }
  public BlueprintObject(string name, Vector3 pos, Quaternion rot)
  {
    Prefab = name;
    Pos = pos;
    Rot = rot.normalized;
    Scale = Vector3.one;
    ExtraInfo = "";
    Data = null;
  }
}
public class Blueprint
{
  public List<BlueprintObject> Objects = new();
  public float Radius = 0f;
  public void Center(Vector2? offset)
  {
    Bounds bounds = new();
    foreach (var obj in Objects)
      bounds.Encapsulate(obj.Pos);
    Radius = Utils.LengthXZ(bounds.extents);
    var center = offset ?? new(bounds.center.x, bounds.center.z);
    foreach (var obj in Objects)
    {
      obj.Pos.x -= center.x;
      obj.Pos.z -= center.y;
    }
  }
  public void Offset(float? offset)
  {
    var y = float.MaxValue;
    if (offset.HasValue) y = -offset.Value;
    else
    {
      foreach (var obj in Objects) y = Mathf.Min(y, obj.Pos.y);
      // Slightly towards the ground to prevent gaps.
      y += 0.05f;
    }
    foreach (var obj in Objects)
      obj.Pos.y -= y;
  }
}
public class Blueprints
{
  private static IEnumerable<string> Files()
  {
    if (!Directory.Exists(Configuration.PlanBuildFolder)) Directory.CreateDirectory(Configuration.PlanBuildFolder);
    if (!Directory.Exists(Configuration.BuildShareFolder)) Directory.CreateDirectory(Configuration.BuildShareFolder);
    var planBuild = Directory.EnumerateFiles(Configuration.PlanBuildFolder, "*.blueprint", SearchOption.AllDirectories);
    var buildShare = Directory.EnumerateFiles(Configuration.BuildShareFolder, "*.vbuild", SearchOption.AllDirectories);
    return planBuild.Concat(buildShare).OrderBy(s => s);
  }
  private static List<string> GetBlueprints() => Files().Select(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_")).ToList();
  public static bool Exists(string name)
  {
    var path = Files().FirstOrDefault(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_") == name);
    return path != null;
  }
  public static bool TryGetBluePrint(string name, out Blueprint blueprint)
  {
    blueprint = new();
    var bp = GetBluePrint(name);
    if (bp == null) return false;
    blueprint = bp;
    return true;
  }
  public static Blueprint? GetBluePrint(string name)
  {
    var path = Files().FirstOrDefault(path => Path.GetFileNameWithoutExtension(path).Replace(" ", "_") == name);
    if (path == null) return null;
    var rows = File.ReadAllLines(path);
    var extension = Path.GetExtension(path);
    Blueprint bp = new();
    if (extension == ".vbuild") return GetBuildShare(bp, rows);
    if (extension == ".blueprint") return GetPlanBuild(bp, rows);
    throw new InvalidOperationException("Unknown file format.");
  }
  private static Blueprint GetPlanBuild(Blueprint bp, string[] rows)
  {
    var piece = true;
    foreach (var row in rows)
    {
      if (row.StartsWith("#snappoints", StringComparison.OrdinalIgnoreCase))
        piece = false;
      else if (row.StartsWith("#pieces", StringComparison.OrdinalIgnoreCase))
        piece = true;
      else if (row.StartsWith("#", StringComparison.Ordinal))
        continue;
      else if (piece)
        bp.Objects.Add(GetPlanBuildObject(row));
    }
    return bp;
  }
  private static void Deserialize(ZDO zdo, ZPackage pkg)
  {
    int num = pkg.ReadInt();
    if ((num & 1) != 0)
    {
      zdo.InitFloats();
      int num2 = (int)pkg.ReadByte();
      for (int i = 0; i < num2; i++)
      {
        int key = pkg.ReadInt();
        zdo.m_floats[key] = pkg.ReadSingle();
      }
    }
    else
    {
      zdo.ReleaseFloats();
    }
    if ((num & 2) != 0)
    {
      zdo.InitVec3();
      int num3 = (int)pkg.ReadByte();
      for (int j = 0; j < num3; j++)
      {
        int key2 = pkg.ReadInt();
        zdo.m_vec3[key2] = pkg.ReadVector3();
      }
    }
    else
    {
      zdo.ReleaseVec3();
    }
    if ((num & 4) != 0)
    {
      zdo.InitQuats();
      int num4 = (int)pkg.ReadByte();
      for (int k = 0; k < num4; k++)
      {
        int key3 = pkg.ReadInt();
        zdo.m_quats[key3] = pkg.ReadQuaternion();
      }
    }
    else
    {
      zdo.ReleaseQuats();
    }
    if ((num & 8) != 0)
    {
      zdo.InitInts();
      int num5 = (int)pkg.ReadByte();
      for (int l = 0; l < num5; l++)
      {
        int key4 = pkg.ReadInt();
        zdo.m_ints[key4] = pkg.ReadInt();
      }
    }
    else
    {
      zdo.ReleaseInts();
    }
    if ((num & 64) != 0)
    {
      zdo.InitLongs();
      int num6 = (int)pkg.ReadByte();
      for (int m = 0; m < num6; m++)
      {
        int key5 = pkg.ReadInt();
        zdo.m_longs[key5] = pkg.ReadLong();
      }
    }
    else
    {
      zdo.ReleaseLongs();
    }
    if ((num & 16) != 0)
    {
      zdo.InitStrings();
      int num7 = (int)pkg.ReadByte();
      for (int n = 0; n < num7; n++)
      {
        int key6 = pkg.ReadInt();
        zdo.m_strings[key6] = pkg.ReadString();
      }
    }
    else
    {
      zdo.ReleaseStrings();
    }
    if ((num & 128) != 0)
    {
      zdo.InitByteArrays();
      int num8 = (int)pkg.ReadByte();
      for (int num9 = 0; num9 < num8; num9++)
      {
        int key7 = pkg.ReadInt();
        zdo.m_byteArrays[key7] = pkg.ReadByteArray();
      }
      return;
    }
    zdo.ReleaseByteArrays();
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
    var info = split.Length > 9 ? split[9] : "";
    var scaleX = InvariantFloat(split, 10, 1f);
    var scaleY = InvariantFloat(split, 11, 1f);
    var scaleZ = InvariantFloat(split, 12, 1f);
    var data = split.Length > 13 ? split[13] : "";
    ZDO? zdo = null;
    if (data != "")
    {
      zdo = new();
      ZPackage pkg = new(data);
      Deserialize(zdo, pkg);
    }
    return new BlueprintObject(name, new(posX, posY, posZ), new(rotX, rotY, rotZ, rotW), new(scaleX, scaleY, scaleZ), info, zdo);
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
    return new BlueprintObject(name, new(posX, posY, posZ), new(rotX, rotY, rotZ, rotW), Vector3.one, "", null);
  }
  private static float InvariantFloat(string[] row, int index, float defaultValue = 0f)
  {
    if (index >= row.Length) return defaultValue;
    var s = row[index];
    if (string.IsNullOrEmpty(s)) return defaultValue;
    return float.Parse(s, NumberStyles.Any, NumberFormatInfo.InvariantInfo);
  }

}