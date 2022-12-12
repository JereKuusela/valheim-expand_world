using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
namespace ExpandWorld;

public class CustomRaidsPatcher
{
  public const string GUID = "asharppen.valheim.custom_raids";
  public static void Run()
  {
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var info)) return;
    ExpandWorld.Log.LogInfo("\"Custom Raids\" detected. Disabling event data.");
    Configuration.configDataEvents.Value = false;
    Harmony harmony = new("expand_world.custom_raids");
    PatchCreateEvent(harmony, info.Instance.GetType().Assembly);
  }

  private static void PatchCreateEvent(Harmony harmony, Assembly assembly)
  {
    var mOriginal = AccessTools.Method(assembly.GetType("Valheim.CustomRaids.Raids.Managers.RaidConfigManager"), "CreateEvent");
    if (mOriginal == null)
    {
      ExpandWorld.Log.LogWarning("\"Custom Raids\" detected. Unable to patch \"CreateEvent\" for biome compatibility.");
      return;
    }
    ExpandWorld.Log.LogInfo("\"Custom Raids\" detected. Patching \"CreateEvent\" for biome compatibility.");
    var mPostfix = SymbolExtensions.GetMethodInfo((RandomEvent ev) => Postfix(ev));
    harmony.Patch(mOriginal, null, new(mPostfix));
  }
  static RandomEvent Postfix(RandomEvent ev)
  {
    if (ev.m_biome == (Heightmap.Biome)1023)
      ev.m_biome = BiomeManager.MaxBiome;
    foreach (var spawn in ev.m_spawn)
    {
      if (spawn.m_biome == (Heightmap.Biome)1023)
        spawn.m_biome = BiomeManager.MaxBiome;
    }
    return ev;
  }
}
