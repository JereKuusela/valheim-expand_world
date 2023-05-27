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
  public static Dictionary<ZoneSystem.ZoneVegetation, ZDO> ZDO = new();
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

  private static ZDO? DataOverride(ZDO? data, string dummy, string prefab)
  {
    if (data != null) return data;
    if (!ZDO.TryGetValue(Veg, out data)) return null;
    return data;
  }
  static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion rot)
  {
    return Data.Instantiate(prefab, pos, rot, DataOverride(null, "", ""));
  }
  static GameObject InstantiateBlueprint(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    if (Mode == ZoneSystem.SpawnMode.Ghost)
      ZNetView.StartGhostInit();
    Spawn.Blueprint("", prefab.name, position, rotation, DataOverride, SpawnedObjects);
    if (Mode == ZoneSystem.SpawnMode.Ghost)
      ZNetView.FinishGhostInit();
    // Blueprints spawn a dummy non-ZNetView object, so no extra stuff is needed.
    return UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
  }
  static void SetScale(ZNetView view, Vector3 scale)
  {
    if (Scale.TryGetValue(Veg, out var randomScale))
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
