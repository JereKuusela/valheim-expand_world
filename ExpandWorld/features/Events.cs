using HarmonyLib;
namespace ExpandWorld;

[HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.InValidBiome))]
public class RandomEventEnvironment {
  static bool Postfix(bool result, RandomEvent ev, ZDO zdo) {
    if (!result) return false;
    if (!EventManager.EventToRequirentEnvironment.TryGetValue(ev.m_name, out var required)) return true;
    if (required.Count == 0) return true;
    var biome = WorldGenerator.instance.GetBiome(zdo.GetPosition());
    var em = EnvMan.instance;
    var availableEnvironments = em.GetAvailableEnvironments(biome);
    if (availableEnvironments == null || availableEnvironments.Count == 0) return false;
    UnityEngine.Random.State state = UnityEngine.Random.state;
    var num = (long)ZNet.instance.GetTimeSeconds() / em.m_environmentDuration;
    UnityEngine.Random.InitState((int)num);
    var env = em.SelectWeightedEnvironment(availableEnvironments);
    UnityEngine.Random.state = state;
    return required.Contains(env.m_name.ToLower());
  }
}

[HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.Awake))]
public class RandomEventSystem {

  public static void Setup(RandEventSystem rs) {
    if (!rs) return;
    rs.m_eventChance = Configuration.EventChance;
    rs.m_eventIntervalMin = Configuration.EventInterval;
  }
  static void Postfix(RandEventSystem __instance) => Setup(__instance);
}