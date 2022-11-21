using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ExpandWorld;
using CompilerIndices = Dictionary<TerrainComp, Indices>;
public class BaseIndex
{
  public int Index;
  public Vector3 Position;
}
public class HeightIndex : BaseIndex
{
  public float DistanceWidth;
  public float DistanceDepth;
  public float Distance;
}

public class PaintIndex : BaseIndex
{
}
public class Indices
{
  public HeightIndex[] HeightIndices = new HeightIndex[0];
  public PaintIndex[] PaintIndices = new PaintIndex[0];
}

public enum BlockCheck
{
  Off,
  On,
  Inverse
}

public partial class Terrain
{
  public static Func<TerrainComp, Indices> CreateIndexer(Vector3 centerPos, float radius)
  {
    return (TerrainComp comp) =>
    {
      return new()
      {
        HeightIndices = GetHeightIndicesWithCircle(comp, centerPos, radius).ToArray(),
        PaintIndices = GetPaintIndicesWithCircle(comp, centerPos, radius).ToArray()
      };
    };
  }

  public static TerrainComp[] GetCompilers(Vector3 position, float radius)
  {
    List<Heightmap> heightMaps = new();
    Heightmap.FindHeightmap(position, radius + 1, heightMaps);
    var pos = ZNet.instance.IsDedicated() ? position : ZNet.instance.GetReferencePosition();
    var zs = ZoneSystem.instance;
    var ns = ZNetScene.instance;
    return heightMaps.Where(hmap => ns.InActiveArea(zs.GetZone(hmap.transform.position), pos)).Select(hmap => hmap.GetAndCreateTerrainCompiler()).ToArray();
  }
  private static CompilerIndices FilterEmpty(CompilerIndices indices)
  {
    return indices.Where(kvp => kvp.Value.HeightIndices.Count() + kvp.Value.PaintIndices.Count() > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
  }
  public static CompilerIndices GetIndices(IEnumerable<TerrainComp> compilers, Func<TerrainComp, Indices> indexer)
  {
    return FilterEmpty(compilers.ToDictionary(compiler => compiler, indexer));
  }
  private static IEnumerable<HeightIndex> GetHeightIndicesWithCircle(TerrainComp compiler, Vector3 centerPos, float radius)
  {
    List<HeightIndex> indices = new();
    compiler.m_hmap.WorldToVertex(centerPos, out var cx, out var cy);
    var distanceLimit = radius / compiler.m_hmap.m_scale;
    var max = compiler.m_width + 1;
    Vector2 center = new((float)cx, (float)cy);
    for (int i = 0; i < max; i++)
    {
      for (int j = 0; j < max; j++)
      {
        var distance = Vector2.Distance(center, new((float)j, (float)i));
        if (distance > distanceLimit) continue;
        var distanceX = j - cx;
        var distanceY = i - cy;
        indices.Add(new()
        {
          Index = i * max + j,
          Position = VertexToWorld(compiler.m_hmap, j, i),
          DistanceWidth = distanceX / distanceLimit,
          DistanceDepth = distanceY / distanceLimit,
          Distance = distance / distanceLimit
        });
      }
    }
    return indices;
  }
  private static Vector3 VertexToWorld(Heightmap hmap, int x, int y)
  {
    var vector = hmap.transform.position;
    vector.x += (x - hmap.m_width / 2 + 0.5f) * hmap.m_scale;
    vector.z += (y - hmap.m_width / 2 + 0.5f) * hmap.m_scale;
    return vector;
  }


  private static IEnumerable<PaintIndex> GetPaintIndicesWithCircle(TerrainComp compiler, Vector3 centerPos, float radius)
  {
    centerPos = new(centerPos.x - 0.5f, centerPos.y, centerPos.z - 0.5f);
    List<PaintIndex> indices = new();
    compiler.m_hmap.WorldToVertex(centerPos, out var cx, out var cy);
    var distanceLimit = radius / compiler.m_hmap.m_scale;
    var max = compiler.m_width;
    Vector2 center = new(cx, cy);
    for (int i = 0; i < max; i++)
    {
      for (int j = 0; j < max; j++)
      {
        var distance = Vector2.Distance(center, new(j, i));
        if (distance > distanceLimit) continue;
        indices.Add(new()
        {
          Index = i * max + j,
          Position = VertexToWorld(compiler.m_hmap, j, i)
        });
      }
    }
    return indices;
  }
}
