using System;
using System.Collections.Generic;
using UnityEngine;
namespace ExpandWorld;
using CompilerIndices = Dictionary<TerrainComp, Indices>;

public partial class Terrain
{
  private static float CalculateSmooth(float smooth, float distance) => (1f - distance) >= smooth ? 1f : (1f - distance) / smooth;
  public static void ResetTerrain(Dictionary<TerrainComp, Indices> compilerIndices, Vector3 pos, float radius)
  {
    List<TerrainModifier> modifiers = new List<TerrainModifier>();
    TerrainModifier.GetModifiers(pos, radius + 1f, modifiers);
    foreach (TerrainModifier modifier in modifiers)
    {
      if (modifier.m_nview == null) continue;
      modifier.m_nview.ClaimOwnership();
      ZNetScene.instance.Destroy(modifier.gameObject);
    }
    Action<TerrainComp, HeightIndex> action = (compiler, heightIndex) =>
    {
      var index = heightIndex.Index;
      compiler.m_levelDelta[index] = 0f;
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = false;
    };
    DoHeightOperation(compilerIndices, pos, radius, action);
    ClearPaint(compilerIndices, pos, radius);
  }
  public static void LevelTerrain(CompilerIndices compilerIndices, Vector3 pos, float radius, float smooth, float altitude)
  {
    Action<TerrainComp, HeightIndex> action = (compiler, heightIndex) =>
    {
      var multiplier = CalculateSmooth(smooth, heightIndex.Distance);
      var index = heightIndex.Index;
      compiler.m_levelDelta[index] += multiplier * (altitude - compiler.m_hmap.m_heights[index]);
      compiler.m_smoothDelta[index] = 0f;
      compiler.m_modifiedHeight[index] = compiler.m_levelDelta[index] != 0f;
    };
    DoHeightOperation(compilerIndices, pos, radius, action);
  }
  public static void ClearPaint(CompilerIndices compilerIndices, Vector3 pos, float radius)
  {
    Action<TerrainComp, int> action = (compiler, index) =>
    {
      compiler.m_modifiedPaint[index] = false;
    };
    DoPaintOperation(compilerIndices, pos, radius, action);
  }
  private static void DoHeightOperation(CompilerIndices compilerIndices, Vector3 pos, float radius, Action<TerrainComp, HeightIndex> action)
  {
    foreach (var kvp in compilerIndices)
    {
      var compiler = kvp.Key;
      var indices = kvp.Value.HeightIndices;
      foreach (var heightIndex in indices) action(compiler, heightIndex);
      Save(compiler);
    }
    ClutterSystem.instance?.ResetGrass(pos, radius);
  }
  private static void DoPaintOperation(CompilerIndices compilerIndices, Vector3 pos, float radius, Action<TerrainComp, int> action)
  {
    foreach (var kvp in compilerIndices)
    {
      var compiler = kvp.Key;
      var indices = kvp.Value.PaintIndices;
      foreach (var index in indices) action(compiler, index.Index);
      Save(compiler);
    }
    ClutterSystem.instance?.ResetGrass(pos, radius);
  }
}
