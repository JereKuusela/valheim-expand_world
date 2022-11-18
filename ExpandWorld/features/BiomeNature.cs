using HarmonyLib;
namespace ExpandWorld;

[HarmonyPatch(typeof(Beehive), nameof(Beehive.CheckBiome))]
public class BeehiveCheckBiome
{
  static void Prefix() => HeightmapFindBiome.Nature = true;
  static void Finalizer() => HeightmapFindBiome.Nature = false;
}

[HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
public class PlayerUpdatePlacementGhost
{
  static void Prefix() => HeightmapFindBiome.Nature = true;
  static void Finalizer() => HeightmapFindBiome.Nature = false;
}
[HarmonyPatch(typeof(Plant), nameof(Plant.UpdateHealth))]
public class PlantUpdateHealth
{
  static void Prefix() => HeightmapGetBiome.Nature = true;
  static void Finalizer() => HeightmapGetBiome.Nature = false;
}
[HarmonyPatch(typeof(FootStep), nameof(FootStep.GetGroundMaterial))]
public class FootStepGetGroundMaterial
{
  static void Prefix() => HeightmapGetBiome.Nature = true;
  static void Finalizer() => HeightmapGetBiome.Nature = false;
}
