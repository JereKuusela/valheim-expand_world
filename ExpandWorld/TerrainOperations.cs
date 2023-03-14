using System;
using System.Collections.Generic;
using UnityEngine;
namespace ExpandWorld;
using Operation = Action<TerrainComp, int, TerrainNode>;
public abstract class TerrainNode
{
  public int Index;
  public Vector3 Position;
  public float DistanceWidth;
  public float DistanceDepth;
  public float Distance;

  public TerrainComp? Compiler;
}

public class HeightNode : TerrainNode { }
public class PaintNode : TerrainNode { }
public partial class Terrain
{
  public static Dictionary<string, Color> Paints = new() {
    {"grass", Color.black},
    {"patches", new(0f, 0.75f, 0f)},
    {"grass_dark", new(0.6f, 0.5f, 0f)},
    {"dirt", Color.red},
    {"cultivated", Color.green},
    {"paved", Color.blue},
    {"paved_moss", new(0f, 0f, 0.5f)},
    {"paved_dirt", new(1f, 0f, 0.5f)},
    {"paved_dark", new(0f, 1f, 0.5f)},
  };
  public static Color ParsePaint(string paint)
  {
    return ParsePaintColor(paint);
  }
  private static Color ParsePaintColor(string paint)
  {
    var split = paint.Split(',');
    if (split.Length < 3 && Paints.TryGetValue(paint, out var color)) return color;
    return new(Parse.Float(split, 0), Parse.Float(split, 1), Parse.Float(split, 2), Parse.Float(split, 3, 1f));
  }
  private static float CalculateSmooth(float smooth, float distance) => (1f - distance) >= smooth ? 1f : (1f - distance) / smooth;

  private static void GetHeightNodes(List<HeightNode> nodes, TerrainComp compiler, Vector3 centerPos, float radius)
  {
    if (radius == 0f) return;
    var max = compiler.m_width + 1;
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(compiler.m_hmap, x, z);
        var dx = nodePos.x - centerPos.x;
        var dz = nodePos.z - centerPos.z;
        var distance = Utils.DistanceXZ(centerPos, nodePos);
        if (distance > radius) continue;
        nodes.Add(new()
        {
          Index = z * max + x,
          Position = nodePos,
          DistanceWidth = dx / radius,
          DistanceDepth = dz / radius,
          Distance = distance / radius,
          Compiler = compiler
        });
      }
    }
  }
  private static void GetPaintNodes(List<PaintNode> nodes, TerrainComp compiler, Vector3 centerPos, float radius)
  {
    var max = compiler.m_width;
    for (int x = 0; x < max; x++)
    {
      for (int z = 0; z < max; z++)
      {
        var nodePos = VertexToWorld(compiler.m_hmap, x, z);
        // Painting is applied from the corner of the node, not the center.
        nodePos.x += 0.5f;
        nodePos.z += 0.5f;
        var dx = nodePos.x - centerPos.x;
        var dz = nodePos.z - centerPos.z;
        var distance = Utils.DistanceXZ(centerPos, nodePos);
        if (distance > radius) continue;
        nodes.Add(new()
        {
          Index = z * max + x,
          Position = nodePos,
          DistanceWidth = dx / radius,
          DistanceDepth = dz / radius,
          Distance = distance / radius,
          Compiler = compiler
        });
      }
    }
  }

  private static Vector3 VertexToWorld(Heightmap hmap, int x, int y)
  {
    var vector = hmap.transform.position;
    vector.x += (x - hmap.m_width / 2 + 0.5f) * hmap.m_scale;
    vector.z += (y - hmap.m_width / 2 + 0.5f) * hmap.m_scale;
    return vector;
  }
  public static void ChangeTerrain(Vector3 pos, Action<TerrainComp> action)
  {
    var compiler = Heightmap.FindHeightmap(pos)?.GetAndCreateTerrainCompiler();
    if (compiler == null) return;
    action(compiler);
    Save(compiler);
  }

  public static void Level(TerrainComp compiler, Vector3 pos, float radius, float border)
  {
    radius += border;
    if (radius == 0f) return;
    var smooth = border / radius;
    List<HeightNode> nodes = new();
    GetHeightNodes(nodes, compiler, pos, radius);
    Operation action = (compiler, index, node) =>
    {
      var multiplier = CalculateSmooth(smooth, node.Distance);
      compiler.m_levelDelta[index] = multiplier * (pos.y - compiler.m_hmap.m_heights[index]);
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoOperation(nodes, pos, radius, action);
  }
  public static void Paint(TerrainComp compiler, Vector3 pos, string paint, float radius, float border)
  {
    radius += border;
    if (radius == 0f) return;
    var smooth = border / radius;
    List<PaintNode> nodes = new();
    Color color = ParsePaint(paint);
    GetPaintNodes(nodes, compiler, pos, radius);
    Operation action = (compiler, index, node) =>
    {
      var multiplier = CalculateSmooth(smooth, node.Distance);
      var newColor = Color.Lerp(compiler.m_paintMask[index], color, multiplier);
      newColor.a = color.a;
      compiler.m_paintMask[index] = newColor;
      compiler.m_modifiedPaint[index] = true;
    };
    DoOperation(nodes, pos, radius, action);
  }

  private static void Save(TerrainComp compiler)
  {
    compiler.GetComponent<ZNetView>()?.ClaimOwnership();
    compiler.m_operations++;
    // These are only used to remove grass which isn't really needed.
    compiler.m_lastOpPoint = Vector3.zero;
    compiler.m_lastOpRadius = 0f;
    compiler.Save();
    compiler.m_hmap.Poke(false);
  }
  private static void DoOperation(List<PaintNode> nodes, Vector3 pos, float radius, Operation action)
  {
    foreach (var node in nodes)
      action(node.Compiler!, node.Index, node);
    ClutterSystem.instance?.ResetGrass(pos, radius);
  }
  private static void DoOperation(List<HeightNode> nodes, Vector3 pos, float radius, Operation action)
  {
    foreach (var node in nodes)
      action(node.Compiler!, node.Index, node);
    ClutterSystem.instance?.ResetGrass(pos, radius);
  }
}
