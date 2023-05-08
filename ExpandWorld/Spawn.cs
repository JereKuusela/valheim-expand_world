using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExpandWorld;

// Most of the custom object spawning should be done here because there are many pitfalls:
// 1. If custom data is used, ghost init is not used and must be done manually.
// 2. On ghost spawning mode, created objects must be either stored or instantly destroyed.
public class Spawn
{
  public static Vector3? DungeonGeneratorPos = null;
  public static void Blueprint(string source, string name, Vector3 pos, Quaternion rot, Func<string, string, ZDO?> dataOverride, List<GameObject>? spawned)
  {
    if (BlueprintManager.TryGet(name, out var bp))
      Blueprint(source, bp, pos, rot, dataOverride, spawned);
  }
  public static void Blueprint(string source, Blueprint bp, Vector3 pos, Quaternion rot, Func<string, string, ZDO?> dataOverride, List<GameObject>? spawned)
  {
    foreach (var obj in bp.Objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      BPO(source, obj, pos, rot, dataOverride, spawned);
    }
  }
  private static void SetData(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, ZDO? data = null)
  {
    if (data == null) return;
    if (!prefab.TryGetComponent<ZNetView>(out var view)) return;
    Data.InitZDO(position, rotation, scale, data, view);
  }
  public static void BPO(string source, BlueprintObject obj, Vector3 pos, Quaternion rot, Func<string, string, ZDO?> dataOverride, List<GameObject>? spawned)
  {
    pos += rot * obj.Pos;
    rot = rot * obj.Rot;
    var prefab = ZNetScene.instance.GetPrefab(obj.Prefab);
    if (!prefab)
    {
      if (BlueprintManager.TryGet(obj.Prefab, out var bp))
      {
        Blueprint(source, bp, pos, rot, dataOverride, spawned);
        return;
      }
      ExpandWorld.Log.LogWarning($"Blueprint prefab {obj.Prefab} not found!");
      return;
    }
    var data = dataOverride(source, obj.Prefab);
    SetData(prefab, pos, rot, obj.Scale, data ?? obj.Data);
    var go = UnityEngine.Object.Instantiate<GameObject>(prefab, pos, rot);
    if (go.TryGetComponent<DungeonGenerator>(out var dg))
    {
      if (DungeonGeneratorPos.HasValue)
        dg.m_originalPosition = DungeonGeneratorPos.Value;
      dg.Generate(ZNetView.m_ghostInit ? ZoneSystem.SpawnMode.Ghost : ZoneSystem.SpawnMode.Full);
    }
    Data.CleanGhostInit(go);
    if (ZNetView.m_ghostInit)
    {
      if (spawned != null)
        spawned.Add(go);
      else
        UnityEngine.Object.Destroy(go);
    }
  }

  public static GameObject? BPO(string source, BlueprintObject obj, Func<string, string, ZDO?> dataOverride, List<GameObject>? spawned)
  {
    var pos = obj.Pos;
    var rot = obj.Rot;
    var prefab = ZNetScene.instance.GetPrefab(obj.Prefab);
    if (!prefab)
    {
      if (BlueprintManager.TryGet(obj.Prefab, out var bp))
      {
        Blueprint(source, bp, pos, rot, dataOverride, spawned);
        return null;
      }
      ExpandWorld.Log.LogWarning($"Blueprint prefab {obj.Prefab} not found!");
      return null;
    }
    var data = dataOverride(source, obj.Prefab);
    SetData(prefab, pos, rot, obj.Scale, data ?? obj.Data);
    var go = UnityEngine.Object.Instantiate<GameObject>(prefab, pos, rot);
    // This is used for the top level objects so dungeon generation is done by vanilal code.
    Data.CleanGhostInit(go);
    return go;
  }
}