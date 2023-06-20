using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Service;
using UnityEngine;

namespace ExpandWorld;

public class LocationSpawning
{
  public static string CurrentLocation = "";
  public static ZPackage? DataOverride(ZPackage? pkg, string prefab)
  {
    if (pkg == null)
    {
      if (!LocationLoading.ObjectData.TryGetValue(CurrentLocation, out var objectData)) return null;
      if (!objectData.TryGetValue(prefab, out var data)) return null;
      pkg = Spawn.RandomizeData(data);
      //ExpandWorld.Log.LogDebug($"Replaced data for {prefab} in {location}");
    }
    return pkg;
  }
  public static string PrefabOverride(string prefab)
  {
    if (!LocationLoading.ObjectSwaps.TryGetValue(CurrentLocation, out var objectSwaps)) return prefab;
    if (!objectSwaps.TryGetValue(prefab, out var swaps)) return prefab;
    return Spawn.RandomizeSwap(swaps);
  }
  static readonly string DummyObj = "vfx_auto_pickup";
  public static GameObject DummySpawn => UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab(DummyObj), Vector3.zero, Quaternion.identity);
  public static GameObject Object(GameObject prefab, Vector3 pos, Quaternion rot, List<GameObject> spawnedGhostObjects)
  {
    BlueprintObject bpo = new(Utils.GetPrefabName(prefab), pos, rot, prefab.transform.localScale, null, 1f);
    var obj = Spawn.BPO(bpo, DataOverride, PrefabOverride, spawnedGhostObjects);
    return obj ?? DummySpawn;
  }


  public static void CustomObjects(ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, List<GameObject> spawnedGhostObjects)
  {
    if (!LocationLoading.Objects.TryGetValue(location.m_prefabName, out var objects)) return;
    //ExpandWorld.Log.LogDebug($"Spawning {objects.Count} custom objects in {location.m_prefabName}");
    foreach (var obj in objects)
    {
      if (obj.Chance < 1f && Random.value > obj.Chance) continue;
      Spawn.BPO(obj, pos, rot, DataOverride, PrefabOverride, spawnedGhostObjects);
    }
  }


}
[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.CreateLocationProxy))]
public class LocationZDO
{
  static void Prefix(ZoneSystem __instance, ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rotation)
  {
    if (!LocationLoading.ZDO.TryGetValue(location.m_prefabName, out var data)) return;
    if (!__instance.m_locationProxyPrefab.TryGetComponent<ZNetView>(out var view)) return;
    if (data != null) DataHelper.InitZDO(pos, rotation, null, data, view);
  }
}
[HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.SetLocation))]
public class FixGhostInit
{
  static void Prefix(LocationProxy __instance, ref bool spawnNow)
  {
    if (ZNetView.m_ghostInit)
    {
      spawnNow = false;
      DataManager.CleanGhostInit(__instance.m_nview);
    }
  }
}


[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SpawnLocation))]
public class LocationObjectDataAndSwap
{
  static bool Prefix(ZoneSystem.ZoneLocation location, ZoneSystem.SpawnMode mode, ref Vector3 pos)
  {
    var loc = location.m_location;
    var flag = loc && loc.m_useCustomInteriorTransform && loc.m_interiorTransform && loc.m_generator;
    if (flag)
      Spawn.DungeonGeneratorPos = location.m_generatorPosition;
    Spawn.IgnoreHealth = location.m_location.m_applyRandomDamage;
    LocationSpawning.CurrentLocation = "";
    if (mode != ZoneSystem.SpawnMode.Client)
    {
      LocationSpawning.CurrentLocation = location.m_prefabName;
      if (LocationLoading.LocationData.TryGetValue(location.m_prefabName, out var data))
        pos.y += data.groundOffset;
    }
    // Blueprints won't have any znetviews to spawn or other logic to handle.
    return !BlueprintManager.Has(location.m_prefabName);
  }

  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var instantiator = typeof(Object).GetMethods().First(m => m.Name == nameof(UnityEngine.Object.Instantiate) && m.IsGenericMethodDefinition && m.GetParameters().Skip(1).Select(p => p.ParameterType).SequenceEqual(new[] { typeof(Vector3), typeof(Quaternion) })).MakeGenericMethod(typeof(GameObject));
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Call, instantiator))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 6))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(LocationSpawning.Object).operand)
      .InstructionEnumeration();
  }


  static void Postfix(ZoneSystem.ZoneLocation location, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects)
  {
    if (mode != ZoneSystem.SpawnMode.Client)
    {
      var isBluePrint = BlueprintManager.TryGet(location.m_prefabName, out var bp);
      if (LocationLoading.LocationData.TryGetValue(location.m_prefabName, out var data))
      {
        // Remove the applied offset.
        var surface = pos with { y = pos.y - data.groundOffset };
        HandleTerrain(surface, location.m_exteriorRadius, isBluePrint, data);
      }
      if (mode == ZoneSystem.SpawnMode.Ghost)
        ZNetView.StartGhostInit();
      if (isBluePrint)
        Spawn.Blueprint(bp, pos, rot, LocationSpawning.DataOverride, LocationSpawning.PrefabOverride, spawnedGhostObjects);
      LocationSpawning.CustomObjects(location, pos, rot, spawnedGhostObjects);
      if (mode == ZoneSystem.SpawnMode.Ghost)
        ZNetView.FinishGhostInit();
    }
    LocationSpawning.CurrentLocation = "";
    Spawn.DungeonGeneratorPos = null;
    Spawn.IgnoreHealth = false;
  }

  static void HandleTerrain(Vector3 pos, float radius, bool isBlueprint, LocationData data)
  {
    var level = false;
    if (data.levelArea == "") level = isBlueprint;
    else if (data.levelArea == "false") level = false;
    else level = true;
    if (!level && data.paint == "") return;

    Terrain.ChangeTerrain(pos, compiler =>
    {
      if (level)
      {
        var levelRadius = data.levelRadius;
        var levelBorder = data.levelBorder;
        if (levelRadius == 0f && levelBorder == 0f)
        {
          var multiplier = Parse.Float(data.levelArea, 0.5f);
          levelRadius = multiplier * radius;
          levelBorder = (1 - multiplier) * radius;
        }
        Terrain.Level(compiler, pos, levelRadius, levelBorder);
      }
      if (data.paint != "")
      {
        var paintRadius = data.paintRadius ?? radius;
        var paintBorder = data.paintBorder ?? 5f;
        Terrain.Paint(compiler, pos, data.paint, paintRadius, paintBorder);
      }
    });
  }



}
