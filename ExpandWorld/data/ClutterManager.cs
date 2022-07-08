using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;
namespace ExpandWorld;

[HarmonyPatch]
public class ClutterManager {
  public static string FileName = Path.Combine(ExpandWorld.ConfigPath, "expand_clutter.yaml");
  private static Dictionary<string, GameObject> Prefabs = new();
  [HarmonyPatch(typeof(ClutterSystem), nameof(ClutterSystem.Awake)), HarmonyPriority(Priority.VeryLow), HarmonyFinalizer]
  public static void LoadPrefabs() {
    if (!ZNet.instance) return;
    Prefabs = ClutterSystem.instance.m_clutter
      .ToLookup(item => item.m_prefab.name, item => item.m_prefab)
      .ToDictionary(kvp => kvp.Key, kvp => kvp.First());
  }
  public static ClutterSystem.Clutter FromData(ClutterData data) {
    ClutterSystem.Clutter clutter = new();
    if (Prefabs.TryGetValue(data.prefab, out var prefab))
      clutter.m_prefab = prefab;
    else
      ExpandWorld.Log.LogWarning($"Failed to find clutter prefab {data.prefab}.");
    clutter.m_enabled = data.enabled;
    clutter.m_amount = data.amount;
    clutter.m_biome = Data.ToBiomes(data.biome);
    clutter.m_instanced = data.instanced;
    clutter.m_onUncleared = data.onUncleared;
    clutter.m_onCleared = data.onCleared;
    clutter.m_scaleMin = data.scaleMin;
    clutter.m_scaleMax = data.scaleMax;
    clutter.m_maxTilt = data.maxTilt;
    clutter.m_maxAlt = data.maxAltitude;
    clutter.m_minAlt = data.minAltitude;
    clutter.m_snapToWater = data.snapToWater;
    clutter.m_terrainTilt = data.terrainTilt;
    clutter.m_randomOffset = data.randomOffset;
    clutter.m_minOceanDepth = data.minOceanDepth;
    clutter.m_maxOceanDepth = data.maxOceanDepth;
    clutter.m_inForest = data.inForest;
    clutter.m_forestTresholdMin = data.forestTresholdMin;
    clutter.m_forestTresholdMax = data.forestTresholdMax;
    clutter.m_fractalScale = data.fractalScale;
    clutter.m_fractalOffset = data.fractalOffset;
    clutter.m_fractalTresholdMin = data.fractalThresholdMin;
    clutter.m_fractalTresholdMax = data.fractalThresholdMax;
    return clutter;
  }
  public static ClutterData ToData(ClutterSystem.Clutter clutter) {
    ClutterData data = new();
    data.prefab = clutter.m_prefab.name;
    data.enabled = clutter.m_enabled;
    data.amount = clutter.m_amount;
    data.biome = Data.FromBiomes(clutter.m_biome);
    data.instanced = clutter.m_instanced;
    data.onUncleared = clutter.m_onUncleared;
    data.onCleared = clutter.m_onCleared;
    data.scaleMin = clutter.m_scaleMin;
    data.scaleMax = clutter.m_scaleMax;
    data.maxTilt = clutter.m_maxTilt;
    data.maxAltitude = clutter.m_maxAlt;
    data.minAltitude = clutter.m_minAlt;
    data.snapToWater = clutter.m_snapToWater;
    data.terrainTilt = clutter.m_terrainTilt;
    data.randomOffset = clutter.m_randomOffset;
    data.minOceanDepth = clutter.m_minOceanDepth;
    data.maxOceanDepth = clutter.m_maxOceanDepth;
    data.inForest = clutter.m_inForest;
    data.forestTresholdMin = clutter.m_forestTresholdMin;
    data.forestTresholdMax = clutter.m_forestTresholdMax;
    data.fractalScale = clutter.m_fractalScale;
    data.fractalOffset = clutter.m_fractalOffset;
    data.fractalThresholdMin = clutter.m_fractalTresholdMin;
    data.fractalThresholdMax = clutter.m_fractalTresholdMax;
    return data;
  }

  public static void ToFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataClutter) return;
    if (File.Exists(FileName)) return;
    var yaml = Data.Serializer().Serialize(ClutterSystem.instance.m_clutter.Select(ToData).ToList());
    Configuration.valueClutterData.Value = yaml;
    File.WriteAllText(FileName, yaml);
  }
  public static void FromFile() {
    if (!ZNet.instance.IsServer() || !Configuration.DataClutter) return;
    if (!File.Exists(FileName)) return;
    var yaml = File.ReadAllText(FileName);
    Configuration.valueClutterData.Value = yaml;
    if (Data.IsLoading) Set(yaml);
  }
  public static void FromSetting(string raw) {
    if (!Data.IsLoading) Set(raw);
  }
  private static void Set(string raw) {
    if (raw == "" || !Configuration.DataClutter) return;
    var data = Data.Deserializer().Deserialize<List<ClutterData>>(raw)
      .Select(FromData).Where(clutter => clutter.m_prefab).ToList();
    if (data.Count == 0) {
      ExpandWorld.Log.LogWarning($"Failed to load any clutter data.");
      return;
    }
    ExpandWorld.Log.LogInfo($"Reloading {data.Count} clutter data.");
    ClutterSystem.instance.m_clutter.Clear();
    foreach (var clutter in data)
      ClutterSystem.instance.m_clutter.Add(clutter);
    ClutterSystem.instance.ClearAll();
  }
  public static void SetupWatcher() {
    Data.SetupWatcher(FileName, FromFile);
  }
}
