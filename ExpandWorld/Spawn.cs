using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

 using DataOverride = System.Func<ZDO?, string, string, ZDO?>;  

namespace ExpandWorld;

// Most of the custom object spawning should be done here because there are many pitfalls:
// 1. If custom data is used, ghost init is not used and must be done manually.
// 2. On ghost spawning mode, created objects must be either stored or instantly destroyed.
public class Spawn
{
  public static Vector3? DungeonGeneratorPos = null;
  public static bool IgnoreHealth = false;
  public static void Blueprint(string source, string name, Vector3 pos, Quaternion rot, DataOverride dataOverride, List<GameObject>? spawned)
  {
    if (BlueprintManager.TryGet(name, out var bp))
      Blueprint(source, bp, pos, rot, dataOverride, spawned);
  }
  public static void Blueprint(string source, Blueprint bp, Vector3 pos, Quaternion rot, DataOverride dataOverride, List<GameObject>? spawned)
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
  public static void BPO(string source, BlueprintObject obj, Vector3 pos, Quaternion rot, DataOverride dataOverride, List<GameObject>? spawned)
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
    var data = dataOverride(obj.Data, source, obj.Prefab);
    SetData(prefab, pos, rot, obj.Scale, data);
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

  // Spawning a object should support following scenarions:
  // 1. Adding an object with random data.
  // 2. Adding an object with random prefab.
  // 3. Adding an object with a specific data, regardless of other configuration.
  // 4. Adding an object with a specific prefab, regardless of other configuration.
  //
  // 1. can be achieved by applying objectData to every object.
  // 2. can be achieved by applying objectSwap to every object.
  // 3. can be achieved by skipping objectData if the object has already custom data.
  // 4. can be achieved by using a dummy object and then objectSwap to replace it.
  public static GameObject? BPO(string source, BlueprintObject obj, DataOverride dataOverride, Func<string, string, string> prefabOverride, List<GameObject>? spawned)
  {
    var pos = obj.Pos;
    var rot = obj.Rot;
    obj.Prefab = prefabOverride(source, obj.Prefab);
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
    var data = dataOverride(obj.Data, source, obj.Prefab);
    SetData(prefab, pos, rot, obj.Scale, data);
    var go = UnityEngine.Object.Instantiate<GameObject>(prefab, pos, rot);
    // This is used for the top level objects so dungeon generation is done by vanilal code.
    Data.CleanGhostInit(go);
    return go;
  }

  private static List<Tuple<float, string>> ParseSwap(IEnumerable<string> items, float baseWeight)
  {
    var total = 0f;
    // Parse.Split not used to keep empty objects.
    return items.Select(s => s.Trim().Split(':')).Select(s =>
    {
      var weight = Parse.Float(s, 1, 1f);
      total += weight * baseWeight;
      return Tuple.Create(weight, s[0]);
    }).ToList().Select(t => Tuple.Create(t.Item1 / total, t.Item2)).ToList();
  }

  public static Dictionary<string, List<Tuple<float, string>>> LoadSwaps(string[] objectSwap)
  {
    Dictionary<string, List<Tuple<float, string>>> swaps = new();
    // Empty items are kept to support spawning nothing.
    var list = objectSwap.Select(s => Data.ToList(s, false)).ToList();
    // Complicated logic to support:
    // 1. Multiple rows for the same object.
    // 2. Multiple swaps in the same row.
    foreach (var row in list)
    {
      var split = Parse.Split(row[0], true, ':');
      var name = split[0];
      var weight = Parse.Float(split, 1, 1f);
      var items = row.Skip(1);
      if (!swaps.ContainsKey(name))
        swaps[name] = new();
      swaps[name].AddRange(ParseSwap(items, weight));
    }
    foreach (var kvp in swaps)
    {
      foreach (var swap in kvp.Value)
        BlueprintManager.Load(swap.Item2, "");
    }
    return swaps;
  }

  public static Dictionary<string, ZDO?> LoadData(string[] objectData)
  {
    return objectData.Select(s => Data.ToList(s)).ToDictionary(arr => arr[0], arr => Data.ToZDO(arr[1]));
  }

  public static string RandomizeSwap(List<Tuple<float, string>> swaps)
  {
    if (swaps.Count == 0)
      return "";
    if (swaps.Count == 1)
      return swaps[0].Item2;
    var rng = UnityEngine.Random.value;
    foreach (var swap in swaps)
    {
      rng -= swap.Item1;
      if (rng <= 0f) return swap.Item2;
    }
    return swaps[swaps.Count - 1].Item2;
  }
}