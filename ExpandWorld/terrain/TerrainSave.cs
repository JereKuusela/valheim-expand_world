using UnityEngine;
namespace ExpandWorld;
public partial class Terrain
{
  public static void Save(TerrainComp compiler)
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
