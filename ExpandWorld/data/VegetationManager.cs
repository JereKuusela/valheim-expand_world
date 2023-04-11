using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

public class VegetationManager
{
  public static string FileName = "expand_vegetation.yaml";
  public static string FilePath = Path.Combine(ExpandWorld.YamlDirectory, FileName);
  public static string Pattern = "expand_vegetation*.yaml";
  public static Dictionary<string, Blueprint> BlueprintFiles = new();

  public static void ReloadBlueprint(string name)
  {
    if (!Blueprints.TryGetBluePrint(name, out var bp)) return;
    name = Path.GetFileNameWithoutExtension(name);
    var vegs = ZoneSystem.instance.m_vegetation.Where(l => Parse.Name(l.m_prefab.name) == name).ToList();
    if (vegs.Count == 0) return;
    ExpandWorld.Log.LogInfo($"Reloading blueprint {name} used by {vegs.Count} vegetationaw  .");
    foreach (var veg in vegs)
      BlueprintFiles[veg.m_prefab.name] = bp;
  }
  public static ZoneSystem.ZoneVegetation FromData(VegetationData data)
  {
    var veg = new ZoneSystem.ZoneVegetation();
    var hash = data.prefab.GetStableHashCode();
    Range<Vector3> scale = new(Parse.Scale(data.scaleMin), Parse.Scale(data.scaleMax));
    scale.Uniform = data.scaleUniform;
    // Minor optimization to skip RNG calls if there is nothing to randomize.
    if (Helper.IsMultiAxis(scale))
      Scale[veg] = scale;
    if (data.data != "")
    {
      ZPackage pkg = new(data.data);
      ZDO zdo = new();
      Data.Deserialize(zdo, pkg);
      ZDO[veg] = zdo;
    }
    if (ZNetScene.instance.m_namedPrefabs.TryGetValue(hash, out var obj))
      veg.m_prefab = obj;
    else if (Blueprints.TryGetBluePrint(data.prefab, out var bp))
    {
      veg.m_prefab = new GameObject();
      veg.m_prefab.name = data.prefab;
      BlueprintFiles[data.prefab] = bp;
    }
    else
      ExpandWorld.Log.LogWarning($"Vegetation prefab {data.prefab} not found!");
    veg.m_enable = data.enabled;
    veg.m_min = data.min;
    veg.m_max = data.max;
    veg.m_forcePlacement = data.forcePlacement;
    veg.m_scaleMin = Parse.Scale(data.scaleMin).x;
    veg.m_scaleMax = Parse.Scale(data.scaleMax).x;
    veg.m_randTilt = data.randTilt;
    veg.m_chanceToUseGroundTilt = data.chanceToUseGroundTilt;
    veg.m_biome = Data.ToBiomes(data.biome);
    veg.m_biomeArea = Data.ToBiomeAreas(data.biomeArea);
    veg.m_blockCheck = data.blockCheck;
    veg.m_minAltitude = data.minAltitude;
    veg.m_maxAltitude = data.maxAltitude;
    veg.m_minOceanDepth = data.minOceanDepth;
    veg.m_maxOceanDepth = data.maxOceanDepth;
    veg.m_minVegetation = data.minVegetation;
    veg.m_maxVegetation = data.maxVegetation;
    veg.m_minTilt = data.minTilt;
    veg.m_maxTilt = data.maxTilt;
    veg.m_terrainDeltaRadius = data.terrainDeltaRadius;
    veg.m_maxTerrainDelta = data.maxTerrainDelta;
    veg.m_minTerrainDelta = data.minTerrainDelta;
    veg.m_snapToWater = data.snapToWater;
    veg.m_snapToStaticSolid = data.snapToStaticSolid;
    veg.m_groundOffset = data.groundOffset;
    veg.m_groupSizeMin = data.groupSizeMin;
    veg.m_groupSizeMax = data.groupSizeMax;
    veg.m_groupRadius = data.groupRadius;
    veg.m_inForest = data.inForest;
    veg.m_forestTresholdMin = data.forestTresholdMin;
    veg.m_forestTresholdMax = data.forestTresholdMax;
    return veg;
  }
  public static bool IsValid(ZoneSystem.ZoneVegetation veg) => veg.m_prefab && veg.m_prefab.GetComponent<ZNetView>() != null;
  public static VegetationData ToData(ZoneSystem.ZoneVegetation veg)
  {
    VegetationData data = new();
    data.enabled = veg.m_enable;
    data.prefab = veg.m_prefab.name;
    data.min = veg.m_min;
    data.max = veg.m_max;
    data.forcePlacement = veg.m_forcePlacement;
    data.scaleMin = veg.m_scaleMin.ToString(CultureInfo.InvariantCulture);
    data.scaleMax = veg.m_scaleMax.ToString(CultureInfo.InvariantCulture);
    data.randTilt = veg.m_randTilt;
    data.chanceToUseGroundTilt = veg.m_chanceToUseGroundTilt;
    data.biome = Data.FromBiomes(veg.m_biome);
    data.biomeArea = Data.FromBiomeAreas(veg.m_biomeArea);
    data.blockCheck = veg.m_blockCheck;
    data.minAltitude = veg.m_minAltitude;
    data.maxAltitude = veg.m_maxAltitude;
    data.minOceanDepth = veg.m_minOceanDepth;
    data.maxOceanDepth = veg.m_maxOceanDepth;
    data.minVegetation = veg.m_minVegetation;
    data.maxVegetation = veg.m_maxVegetation;
    data.minTilt = veg.m_minTilt;
    data.maxTilt = veg.m_maxTilt;
    data.terrainDeltaRadius = veg.m_terrainDeltaRadius;
    data.maxTerrainDelta = veg.m_maxTerrainDelta;
    data.minTerrainDelta = veg.m_minTerrainDelta;
    data.snapToWater = veg.m_snapToWater;
    data.snapToStaticSolid = veg.m_snapToStaticSolid;
    data.groundOffset = veg.m_groundOffset;
    data.groupSizeMin = veg.m_groupSizeMin;
    data.groupSizeMax = veg.m_groupSizeMax;
    data.groupRadius = veg.m_groupRadius;
    data.inForest = veg.m_inForest;
    data.forestTresholdMin = veg.m_forestTresholdMin;
    data.forestTresholdMax = veg.m_forestTresholdMax;
    return data;
  }

  public static void ToFile()
  {
    if (!Helper.IsServer() || !Configuration.DataVegetation) return;
    if (File.Exists(FilePath)) return;
    var yaml = Data.Serializer().Serialize(ZoneSystem.instance.m_vegetation.Where(IsValid).Select(ToData).ToList());
    File.WriteAllText(FilePath, yaml);
    Configuration.valueVegetationData.Value = yaml;
  }
  public static void FromFile()
  {
    if (!Helper.IsServer()) return;
    var yaml = Configuration.DataVegetation ? Data.Read(Pattern) : "";
    Configuration.valueVegetationData.Value = yaml;
    Set(yaml);
  }
  public static void FromSetting(string yaml)
  {
    if (Helper.IsClient()) Set(yaml);
  }
  public static Dictionary<ZoneSystem.ZoneVegetation, Range<Vector3>> Scale = new();
  public static Dictionary<ZoneSystem.ZoneVegetation, ZDO> ZDO = new();
  private static void Set(string yaml)
  {
    BlueprintFiles.Clear();
    Scale.Clear();
    ZDO.Clear();
    if (yaml == "" || !Configuration.DataVegetation) return;
    try
    {
      var data = Data.Deserialize<VegetationData>(yaml, FileName)
      .Select(FromData).Where(veg => veg.m_prefab).ToList();
      if (data.Count == 0)
      {
        ExpandWorld.Log.LogWarning($"Failed to load any vegetation data.");
        return;
      }
      ExpandWorld.Log.LogInfo($"Reloading {data.Count} vegetation data.");
      ZoneSystem.instance.m_vegetation = data;
      ZoneSystem.instance.ValidateVegetation();
    }
    catch (Exception e)
    {
      ExpandWorld.Log.LogError(e.StackTrace);
    }
  }
  public static void SetupWatcher()
  {
    Data.SetupWatcher(Pattern, FromFile);
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PlaceVegetation))]
public class PlaceVegetation
{
  private static ZoneSystem.ZoneVegetation Veg = new();
  private static ZoneSystem.SpawnMode Mode = ZoneSystem.SpawnMode.Client;
  private static List<GameObject> SpawnedObjects = new();
  static void Prefix(ZoneSystem.SpawnMode mode, List<GameObject> spawnedObjects)
  {
    Mode = mode;
    SpawnedObjects = spawnedObjects;
  }
  static ZoneSystem.ZoneVegetation SetVeg(ZoneSystem.ZoneVegetation veg)
  {
    Veg = veg;
    return veg;
  }
  static void SetData(GameObject prefab, Vector3 position, Quaternion rotation, ZDO? data = null)
  {
    if (data == null)
    {
      if (!VegetationManager.ZDO.TryGetValue(Veg, out data)) return;
    }
    if (data == null) return;
    if (!prefab.TryGetComponent<ZNetView>(out var view)) return;
    Data.InitZDO(position, rotation, data, view);
  }
  static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    SetData(prefab, position, rotation);
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    Data.CleanGhostInit(obj);
    return obj;
  }
  static GameObject InstantiateWithData(GameObject prefab, Vector3 position, Quaternion rotation, ZDO? data = null)
  {
    SetData(prefab, position, rotation, data);
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    Data.CleanGhostInit(obj);
    return obj;
  }
  static GameObject InstantiateBlueprint(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    SetData(prefab, position, rotation);
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    Data.CleanGhostInit(obj);
    if (VegetationManager.BlueprintFiles.TryGetValue(prefab.name, out var bp))
      SpawnBlueprint(bp, position, rotation);
    return obj;
  }
  static void SpawnBlueprint(Blueprint bp, Vector3 pos, Quaternion rot)
  {
    var zs = ZoneSystem.instance;
    foreach (var obj in bp.Objects)
    {
      if (obj.Chance < 1f && UnityEngine.Random.value > obj.Chance) continue;
      SpawnBPO(zs, pos, rot, Mode, SpawnedObjects, obj);
    }
  }
  static void SpawnBPO(ZoneSystem zs, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects, BlueprintObject obj)
  {
    var objPos = pos + rot * obj.Pos;
    var objRot = rot * obj.Rot;
    var prefab = ZNetScene.instance.GetPrefab(obj.Prefab);
    if (!prefab)
    {
      if (VegetationManager.BlueprintFiles.TryGetValue(obj.Prefab, out var bp))
      {
        SpawnBlueprint(bp, objPos, objRot);
        return;
      }
      ExpandWorld.Log.LogWarning($"Blueprint prefab {obj.Prefab} not found!");
      return;
    }
    if (mode == ZoneSystem.SpawnMode.Ghost)
    {
      ZNetView.StartGhostInit();
    }
    var go = InstantiateWithData(prefab, objPos, objRot, obj.Data);
    if (mode == ZoneSystem.SpawnMode.Ghost)
    {
      spawnedGhostObjects.Add(go);
      ZNetView.FinishGhostInit();
    }
  }
  static void SetScale(ZNetView view, Vector3 scale)
  {
    if (VegetationManager.Scale.TryGetValue(Veg, out var randomScale))
      scale = Helper.RandomValue(randomScale);
    view.SetLocalScale(scale);
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var instantiator = typeof(UnityEngine.Object).GetMethods().First(m => m.Name == nameof(UnityEngine.Object.Instantiate) && m.IsGenericMethodDefinition && m.GetParameters().Skip(1).Select(p => p.ParameterType).SequenceEqual(new[] { typeof(Vector3), typeof(Quaternion) })).MakeGenericMethod(typeof(GameObject));
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ZoneSystem.ZoneVegetation), nameof(ZoneSystem.ZoneVegetation.m_enable))))
      .Insert(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(SetVeg).operand))
      .MatchForward(false, new CodeMatch(OpCodes.Call, instantiator))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(Instantiate).operand)
      .MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(ZNetView), nameof(ZNetView.SetLocalScale))))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(SetScale).operand)
      .MatchForward(false, new CodeMatch(OpCodes.Call, instantiator))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(InstantiateBlueprint).operand)
      .InstructionEnumeration();
  }
}


[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.ValidateVegetation))]
public class ValidateVegetation
{
  static bool Prefix() => false;
}
