using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PlaceVegetation))]
public class VegetationSpawning
{
  public static Dictionary<ZoneSystem.ZoneVegetation, VegetationExtra> Extra = new();
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
    if (!Extra.TryGetValue(CurrentVegetation, out var extra)) return null;
    return extra.data;
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
    if (Extra.TryGetValue(CurrentVegetation, out var extra) && extra.scale != null)
      scale = Helper.RandomValue(extra.scale);
    view.SetLocalScale(scale);
  }
  private static bool InsideClearArea(List<ZoneSystem.ClearArea> areas, Vector3 point)
  {
    var size = Extra.TryGetValue(CurrentVegetation, out var extra) ? extra.clearRadius : 0;
    foreach (var clearArea in areas)
    {
      var distance = Utils.DistanceXZ(point, clearArea.m_center);
      if (distance < clearArea.m_radius + size) return true;
    }
    return false;
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    var instantiator = typeof(Object).GetMethods().First(m => m.Name == nameof(Object.Instantiate) && m.IsGenericMethodDefinition && m.GetParameters().Skip(1).Select(p => p.ParameterType).SequenceEqual(new[] { typeof(Vector3), typeof(Quaternion) })).MakeGenericMethod(typeof(GameObject));
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ZoneSystem.ZoneVegetation), nameof(ZoneSystem.ZoneVegetation.m_enable))))
      .Insert(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(SetVeg).operand))
      .MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZoneSystem), nameof(ZoneSystem.InsideClearArea))))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(InsideClearArea).operand)
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
      if (!Extra.TryGetValue(veg, out var extra)) continue;
      if (extra.forbiddenGlobalKeys == null && extra.requiredGlobalKeys == null) continue;
      // Spawn condition only for enabled vegs.
      veg.m_enable = true;
      if (extra.forbiddenGlobalKeys != null && Helper.HasAnyGlobalKey(extra.forbiddenGlobalKeys)) veg.m_enable = false;
      if (extra.requiredGlobalKeys != null && !Helper.HasEveryGlobalKey(extra.requiredGlobalKeys)) veg.m_enable = false;
    }
  }
}


[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.ValidateVegetation))]
public class ValidateVegetation
{
  static bool Prefix() => false;
}
