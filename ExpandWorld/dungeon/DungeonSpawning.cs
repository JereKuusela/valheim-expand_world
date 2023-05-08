using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

///<summary>Dungeon level customization</summary>
[HarmonyPatch(typeof(DungeonGenerator))]
public class DungeonSpawning
{
  ///<summary>Dungeon doesn't know its location so it must be tracked manually.</summary>
  public static ZoneSystem.ZoneLocation? Location = null;

  ///<summary>Implements object data and swapping from location data.</summary>
  static GameObject CustomObject(GameObject prefab, Vector3 position, Quaternion rotation)
  {
    // Some mods cause client side dungeon reloading.
    // In this case location information is not available.
    // Revert to the default behaviour as fail safe (no swaps or blueprint won't be available).
    if (Location == null) return UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
    BlueprintObject bpo = new(Utils.GetPrefabName(prefab), position, rotation, prefab.transform.localScale, "", null, 1f);
    var locName = Location?.m_prefabName ?? "";
    if (LocationSpawning.TryGetSwap(locName, bpo.Prefab, out var objName))
      bpo.Prefab = objName;

    var obj = Spawn.BPO(locName, bpo, LocationSpawning.DataOverride, null);
    return obj ?? LocationSpawning.DummySpawn;
  }

  static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
  {
    var instantiator = typeof(UnityEngine.Object).GetMethods().First(m => m.Name == nameof(UnityEngine.Object.Instantiate) && m.IsGenericMethodDefinition && m.GetParameters().Skip(1).Select(p => p.ParameterType).SequenceEqual(new[] { typeof(Vector3), typeof(Quaternion) })).MakeGenericMethod(typeof(GameObject));
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Call, instantiator))
      .Set(OpCodes.Call, Transpilers.EmitDelegate(CustomObject).operand)
      .InstructionEnumeration();
  }

  // Room objects in the dungeon.
  [HarmonyPatch(nameof(DungeonGenerator.PlaceRoom), typeof(DungeonDB.RoomData), typeof(Vector3), typeof(Quaternion), typeof(RoomConnection), typeof(ZoneSystem.SpawnMode)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DungeonObjects(IEnumerable<CodeInstruction> instructions) => Transpile(instructions);

  // Doors in the dungeon.
  [HarmonyPatch(nameof(DungeonGenerator.PlaceDoors)), HarmonyTranspiler]
  static IEnumerable<CodeInstruction> DungeonDoors(IEnumerable<CodeInstruction> instructions) => Transpile(instructions);

}