using System;
using System.Collections.Generic;
using UnityEngine;
namespace ExpandWorld;
public class HeightIndex
{
  public int Index;
  public Vector3 Position;
  public float Distance;
}
public partial class Terrain
{
  private static float CalculateSmooth(float smooth, float distance) => (1f - distance) >= smooth ? 1f : (1f - distance) / smooth;

  private static List<HeightIndex> GetIndices(TerrainComp compiler, Vector3 centerPos, float radius)
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

  public static void Level(Vector3 pos, float radius, float smooth)
  {
    var compiler = Heightmap.FindHeightmap(pos)?.GetAndCreateTerrainCompiler();
    if (compiler == null) return;
    var indices = GetIndices(compiler, pos, radius);
    Action<TerrainComp, HeightIndex> action = (compiler, heightIndex) =>
    {
      var multiplier = CalculateSmooth(smooth, heightIndex.Distance);
      var index = heightIndex.Index;
      compiler.m_levelDelta[index] = multiplier * (pos.y - compiler.m_hmap.m_heights[index]);
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    foreach (var index in indices) action(compiler, index);
    Save(compiler);
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
}
