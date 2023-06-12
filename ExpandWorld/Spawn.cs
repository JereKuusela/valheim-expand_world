using System;
using System.Collections.Generic;
using System.Linq;
using Service;
using UnityEngine;

using DataOverride = System.Func<ZPackage?, string, string, ZPackage?>;

namespace ExpandWorld;

// Most of the custom object spawning should be done here because there are many pitfalls:
// 1. If custom data is used, ghost init is not used and must be done manually.
// 2. On ghost spawning mode, created objects must be either stored or instantly destroyed.
public class Spawn
{
  public static Vector3? DungeonGeneratorPos = null;
  public static bool IgnoreHealth = false;
  public static void Blueprint(string source, string name, Vector3 pos, Quaternion rot, DataOverride dataOverride, Func<string, string, string> prefabOverride, List<GameObject>? spawned)
  {
    if (BlueprintManager.TryGet(name, out var bp))
      Blueprint(source, bp, pos, rot, dataOverride, prefabOverride, spawned);
  }
  public static void Blueprint(string source, Blueprint bp, Vector3 pos, Quaternion rot, DataOverride dataOverride, Func<string, string, string> prefabOverride, List<GameObject>? spawned)
  {
    foreach (var obj in bp.Objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      BPO(source, obj, pos, rot, dataOverride, prefabOverride, spawned);
    }
  }
  private static void SetData(GameObject prefab, Vector3 position, Quaternion rotation, Vector3? scale, ZPackage? data = null)
  {
    if (data == null) return;
    if (!prefab.TryGetComponent<ZNetView>(out var view)) return;
    var zdo = DataHelper.InitZDO(position, rotation, scale, data, view);
    // Users very easily might have creator on their blueprints or copied data.
    // This causes enemies to attack them because they are considered player built.
    // So far no reason to keep this data.
    ZNetView.m_initZDO.RemoveLong(ZDOVars.s_creator);
    // For random damage, health is not needed.
    if (IgnoreHealth)
      zdo?.RemoveFloat(ZDOVars.s_health);
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

  // This should be only called for custom objects and blueprints so not returning anything.
  public static void BPO(string source, BlueprintObject obj, Vector3 pos, Quaternion rot, DataOverride dataOverride, Func<string, string, string> prefabOverride, List<GameObject>? spawned)
  {
    var go = SharedBPO(source, obj, pos, rot, dataOverride, prefabOverride, spawned);
    if (go != null && go.TryGetComponent<DungeonGenerator>(out var dg))
    {
      if (DungeonGeneratorPos.HasValue)
        dg.m_originalPosition = DungeonGeneratorPos.Value;
      dg.Generate(ZNetView.m_ghostInit ? ZoneSystem.SpawnMode.Ghost : ZoneSystem.SpawnMode.Full);
    }
  }

  // This is called for vanilla objects so it must be returned to the original code.
  public static GameObject? BPO(string source, BlueprintObject obj, DataOverride dataOverride, Func<string, string, string> prefabOverride, List<GameObject>? spawned)
  {
    // Dungeon generate not called here because it's called in the original location code.
    return SharedBPO(source, obj, Vector3.zero, Quaternion.identity, dataOverride, prefabOverride, spawned);
  }
  private static GameObject? SharedBPO(string source, BlueprintObject obj, Vector3 pos, Quaternion rot, DataOverride dataOverride, Func<string, string, string> prefabOverride, List<GameObject>? spawned)
  {
    pos += rot * obj.Pos;
    rot *= obj.Rot;
    obj.Prefab = prefabOverride(source, obj.Prefab);
    var prefab = ZNetScene.instance.GetPrefab(obj.Prefab);
    if (!prefab)
    {
      if (BlueprintManager.TryGet(obj.Prefab, out var bp))
      {
        Blueprint(source, bp, pos, rot, dataOverride, prefabOverride, spawned);
        return null;
      }
      ExpandWorld.Log.LogWarning($"Blueprint / object prefab {obj.Prefab} not found!");
      return null;
    }
    var data = dataOverride(obj.Data, source, obj.Prefab);
    SetData(prefab, pos, rot, obj.Scale, data);

    //ExpandWorld.Log.LogDebug($"Spawning {obj.Prefab} at {Helper.Print(pos)} {source}");
    var go = UnityEngine.Object.Instantiate(prefab, pos, rot);
    DataManager.CleanGhostInit(go);
    if (ZNetView.m_ghostInit)
    {
      if (spawned != null)
        spawned.Add(go);
      // Vanilla code also calls Destroy in some cases but this doesn't matter.
      else
        UnityEngine.Object.Destroy(go);
    }
    return go;
  }

  private static List<Tuple<float, string>> ParseSwapItems(IEnumerable<string> items, float weight) => items.Select(s => Parse.Split(s, false, ':')).Select(s => Tuple.Create(Parse.Float(s, 1, 1f) * weight, s[0])).ToList();


  public static Dictionary<string, List<Tuple<float, string>>> LoadSwaps(string[] objectSwap)
  {
    Dictionary<string, List<Tuple<float, string>>> swaps = new();
    // Empty items are kept to support spawning nothing.
    var list = objectSwap.Select(s => DataManager.ToList(s, false)).Where(l => l.Count > 0).ToList();
    // Complicated logic to support:
    // 1. Multiple rows for the same object.
    // 2. Multiple swaps in the same row.
    foreach (var row in list)
    {
      var s = Parse.Split(row[0], true, ':');
      var name = s[0];
      var weight = Parse.Float(s, 1, 1f);
      if (!swaps.ContainsKey(name))
        swaps[name] = new();
      swaps[name].AddRange(ParseSwapItems(row.Skip(1), weight));
    }
    foreach (var kvp in swaps)
    {
      var total = kvp.Value.Sum(t => t.Item1);
      for (var i = 0; i < kvp.Value.Count; ++i)
        kvp.Value[i] = Tuple.Create(kvp.Value[i].Item1 / total, kvp.Value[i].Item2);
      foreach (var swap in kvp.Value)
        BlueprintManager.Load(swap.Item2, "");
    }
    return swaps;
  }

  private static List<Tuple<float, ZPackage?>> ParseDataItems(IEnumerable<string> items, float weight) => items.Select(s => Parse.Split(s, false, ':')).Select(s => Tuple.Create(Parse.Float(s, 1, 1f) * weight, DataManager.Deserialize(s[0]))).ToList();
  public static Dictionary<string, List<Tuple<float, ZPackage?>>> LoadData(string[] objectData)
  {
    Dictionary<string, List<Tuple<float, ZPackage?>>> swaps = new();
    // Empty items are kept to support spawning nothing.
    var list = objectData.Select(s => DataManager.ToList(s, false)).Where(l => l.Count > 0).ToList();
    // Complicated logic to support:
    // 1. Multiple rows for the same object.
    // 2. Multiple swaps in the same row.
    foreach (var row in list)
    {
      var s = Parse.Split(row[0], true, ':');
      var name = s[0];
      var weight = Parse.Float(s, 1, 1f);
      if (!swaps.ContainsKey(name))
        swaps[name] = new();
      swaps[name].AddRange(ParseDataItems(row.Skip(1), weight));
    }
    foreach (var kvp in swaps)
    {
      var total = kvp.Value.Sum(t => t.Item1);
      for (var i = 0; i < kvp.Value.Count; ++i)
        kvp.Value[i] = Tuple.Create(kvp.Value[i].Item1 / total, kvp.Value[i].Item2);
    }
    return swaps;
  }
  public static string RandomizeSwap(List<Tuple<float, string>> swaps)
  {
    if (swaps.Count == 0)
      return "";
    if (swaps.Count == 1)
      return swaps[0].Item2;
    var rng = UnityEngine.Random.value;
    //ExpandWorld.Log.LogDebug($"RandomizeSwap: Roll {Helper.Print(rng)} for {string.Join(", ", swaps.Select(s => s.Item2 + ":" + Helper.Print(s.Item1)))}");
    foreach (var swap in swaps)
    {
      rng -= swap.Item1;
      if (rng <= 0f) return swap.Item2;
    }
    return swaps[swaps.Count - 1].Item2;
  }
  public static ZPackage? RandomizeData(List<Tuple<float, ZPackage?>> swaps)
  {
    if (swaps.Count == 0)
      return null;
    if (swaps.Count == 1)
      return swaps[0].Item2;
    var rng = UnityEngine.Random.value;
    //ExpandWorld.Log.LogDebug($"RandomizeData: Roll {Helper.Print(rng)} for weigths {string.Join(", ", swaps.Select(s => Helper.Print(s.Item1)))}");
    foreach (var swap in swaps)
    {
      rng -= swap.Item1;
      if (rng <= 0f) return swap.Item2;
    }
    return swaps[swaps.Count - 1].Item2;
  }
}