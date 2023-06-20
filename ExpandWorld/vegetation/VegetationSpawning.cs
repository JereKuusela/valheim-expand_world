using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PlaceVegetation))]
public class VegetationSpawning
{
  public static Dictionary<ZoneSystem.ZoneVegetation, Range<Vector3>> Scale = new();
  public static Dictionary<ZoneSystem.ZoneVegetation, ZPackage?> ZDO = new();
  public static Dictionary<ZoneSystem.ZoneVegetation, VegetationSpawnCondition> SpawnCondition = new();
  private static ZoneSystem.ZoneVegetation CurrentVegetation = new();
  private static ZoneSystem.SpawnMode Mode = ZoneSystem.SpawnMode.Client;
  private static List<GameObject> SpawnedObjects = new();
  static void Prefix(ZoneSystem.SpawnMode mode, List<GameObject> spawnedObjects)
  {
    Mode = mode;
    SpawnedObjects = spawnedObjects;
  }
  static ZoneSystem.ZoneVegetation SetVeg(ZoneSystem.ZoneVegetation veg)
  {
    CurrentVegetation = veg;
    return veg;
  }

  private static ZPackage? DataOverride(ZPackage? data, string prefab)
  {
    if (data != null) return data;
    if (!ZDO.TryGetValue(CurrentVegetation, out data)) return null;
    return data;
  }

  private static string PrefabOverride(string prefab)
  {
    return prefab;
  }
  static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion rot)
  {
    return DataManager.Instantiate(prefab, pos, rot, DataOverride(null, ""));
  }
  static GameObject InstantiateBlueprint(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    if (Mode == ZoneSystem.SpawnMode.Ghost)
      ZNetView.StartGhostInit();
    Spawn.Blueprint(prefab.name, position, rotation, DataOverride, PrefabOverride, SpawnedObjects);
    if (Mode == ZoneSystem.SpawnMode.Ghost)
      ZNetView.FinishGhostInit();
    // Blueprints spawn a dummy non-ZNetView object, so no extra stuff is needed.
    return Object.Instantiate(prefab, position, rotation);
  }
  static void SetScale(ZNetView view, Vector3 scale)
  {
    if (Scale.TryGetValue(CurrentVegetation, out var randomScale))
      scale = Helper.RandomValue(randomScale);
    view.SetLocalScale(scale);
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var instantiator = typeof(Object).GetMethods().First(m => m.Name == nameof(Object.Instantiate) && m.IsGenericMethodDefinition && m.GetParameters().Skip(1).Select(p => p.ParameterType).SequenceEqual(new[] { typeof(Vector3), typeof(Quaternion) })).MakeGenericMethod(typeof(GameObject));
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
  static void Prefix(ZoneSystem __instance)
  {
    var vegs = __instance.m_vegetation;
    foreach (var veg in vegs)
    {
      if (!SpawnCondition.TryGetValue(veg, out var data)) continue;
      // Spawn condition only for enabled vegs.
      veg.m_enable = true;
      if (Helper.HasAnyGlobalKey(data.forbiddenGlobalKeys)) veg.m_enable = false;
      if (!Helper.HasEveryGlobalKey(data.requiredGlobalKeys)) veg.m_enable = false;
    }
  }
}


[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.ValidateVegetation))]
public class ValidateVegetation
{
  static bool Prefix() => false;
}
