
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;


[HarmonyPatch(typeof(Player), nameof(Player.UpdateEnvStatusEffects))]
public class StatusManager
{
  private static float DamageTimer = 0f;
  private static readonly float TickRate = 1f;

  private static string PreviousWeather = "";
  private static bool PreviousDay = false;
  private static Heightmap.Biome PreviousBiome = Heightmap.Biome.None;

  static void Postfix(Player __instance, float dt)
  {
    if (__instance != Player.m_localPlayer) return;
    var seman = __instance.GetSEMan();
    DamageTimer += dt;
    var weather = EnvMan.instance.GetCurrentEnvironment()?.m_name ?? "";
    var day = EnvMan.instance.IsDay();
    var biome = EnvMan.instance.GetBiome();

    RemoveBiomeEffects(seman, day, biome);
    RemoveWeatherEffects(seman, day, weather);
    ApplyBiomeEffects(seman, day, biome);
    ApplyWeatherEffects(seman, day, weather);

    if (DamageTimer >= TickRate) DamageTimer = 0f;
    PreviousWeather = weather;
    PreviousDay = day;
    PreviousBiome = biome;
  }

  private static void RemoveBiomeEffects(SEMan seman, bool day, Heightmap.Biome biome)
  {
    if (!BiomeManager.TryGetData(PreviousBiome, out var data)) return;
    if (biome != PreviousBiome)
    {
      Remove(seman, data.statusEffects);
      Remove(seman, data.dayStatusEffects);
      Remove(seman, data.nightStatusEffects);
    }
    if (day != PreviousDay)
    {
      if (day) Remove(seman, data.nightStatusEffects);
      else Remove(seman, data.dayStatusEffects);
    }
  }
  private static void RemoveWeatherEffects(SEMan seman, bool day, string weather)
  {
    if (!EnvironmentManager.Extra.TryGetValue(PreviousWeather, out var data)) return;
    if (weather != PreviousWeather)
    {
      Remove(seman, data.statusEffects);
      Remove(seman, data.dayStatusEffects);
      Remove(seman, data.nightStatusEffects);
    }
    if (day != PreviousDay)
    {
      if (day) Remove(seman, data.nightStatusEffects);
      else Remove(seman, data.dayStatusEffects);
    }
  }
  private static void ApplyBiomeEffects(SEMan seman, bool day, Heightmap.Biome biome)
  {
    if (!BiomeManager.TryGetData(biome, out var data)) return;
    Add(seman, data.statusEffects);
    if (day) Add(seman, data.dayStatusEffects);
    else Add(seman, data.nightStatusEffects);
  }
  private static void ApplyWeatherEffects(SEMan seman, bool day, string weather)
  {
    if (!EnvironmentManager.Extra.TryGetValue(weather, out var data)) return;
    Add(seman, data.statusEffects);
    if (day) Add(seman, data.dayStatusEffects);
    else Add(seman, data.nightStatusEffects);
  }

  private static void Remove(SEMan seman, List<Status> es)
  {
    foreach (var statusEffect in es)
      Remove(seman, statusEffect);
  }
  private static void Remove(SEMan seman, Status es)
  {
    var statusEffect = seman.GetStatusEffect(es.hash);
    if (statusEffect == null) return;
    // Expiring status effects should expire their own.
    // But permanent ones should be removed.
    if (statusEffect.m_ttl > 0f) return;
    ExpandWorld.Log.LogInfo($"Removing {statusEffect.name}");
    seman.RemoveStatusEffect(es.hash);
  }
  private static void Add(SEMan seman, List<Status> es)
  {
    foreach (var statusEffect in es)
      Add(seman, statusEffect);
  }

  private static void Add(SEMan seman, Status es)
  {
    seman.AddStatusEffect(es.hash, es.reset, es.itemLevel, es.skillLevel);
    if (es.reset) return;
    var se = seman.GetStatusEffect(es.hash);
    // To avoid spamming damage calculations, only tick once per second.
    var addDamage = DamageTimer >= TickRate;
    if (se is SE_Burning burning)
    {
      if (!addDamage) return;
      var hasDamage = (burning.m_fireDamageLeft + burning.m_spiritDamageLeft) > 0;
      // Heuristic to try detect the damage type.
      if (burning.NameHash() == Character.s_statusEffectSpirit || burning.m_spiritDamageLeft > 0f)
      {
        var damage = CalculateDamage(seman, es, HitData.DamageType.Spirit);
        // Fire stacks, so the damage must match the tick rate.
        if (hasDamage) damage *= TickRate / se.m_ttl;
        ExpandWorld.Log.LogDebug($"Adding {damage} spirit damage to {burning.name}");
        burning.AddSpiritDamage(damage);
      }
      else
      {
        var damage = CalculateDamage(seman, es, HitData.DamageType.Fire);
        // Fire stacks, so the damage must match the tick rate.
        if (hasDamage) damage *= TickRate / se.m_ttl;
        ExpandWorld.Log.LogDebug($"Adding {damage} fire damage to {burning.name}");
        burning.AddFireDamage(damage);
      }
    }
    else if (se is SE_Poison poison)
    {
      if (!addDamage) return;
      var damage = CalculateDamage(seman, es, HitData.DamageType.Poison);
      // Poison doesn't stack so full damage can always be added.
      ExpandWorld.Log.LogDebug($"Adding {damage} poison damage to {poison.name}");
      poison.AddDamage(damage);
    }
    else
      se.m_time = se.m_ttl - es.duration;
  }

  private static float CalculateDamage(SEMan seman, Status es, HitData.DamageType damageType)
  {
    var damage = es.damage;
    var damageIgnoreArmor = es.damageIgnoreArmor;
    if (seman.m_character)
    {
      var mod = seman.m_character.GetDamageModifier(damageType);
      var multi = ModToMultiplier(mod);
      damage *= multi;
      damageIgnoreArmor *= multi;
      if (damage > 0)
      {
        var armor = seman.m_character.GetBodyArmor();
        damage = ApplyArmor(damage, armor);
      }
    }
    return damage + damageIgnoreArmor + es.damageIgnoreAll;
  }
  private static float ApplyArmor(float dmg, float ac)
  {
    float num = Mathf.Clamp01(dmg / (ac * 4f)) * dmg;
    if ((double)ac < (double)dmg / 2.0)
      num = dmg - ac;
    return num;
  }
  private static float ModToMultiplier(HitData.DamageModifier mod)
  {
    if (mod == HitData.DamageModifier.Resistant) return 0.5f;
    if (mod == HitData.DamageModifier.VeryResistant) return 0.25f;
    if (mod == HitData.DamageModifier.Weak) return 1.5f;
    if (mod == HitData.DamageModifier.VeryWeak) return 2f;
    if (mod == HitData.DamageModifier.Immune) return 0f;
    if (mod == HitData.DamageModifier.Ignore) return 0f;
    return 1f;
  }
}


public class Status
{
  public int hash;
  public float duration;
  public float damage;
  public float damageIgnoreAll;
  public float damageIgnoreArmor;
  public int itemLevel;
  public float skillLevel;
  public bool reset;
  public Status(string str)
  {
    var split = str.Split(':');
    hash = split[0].GetStableHashCode();
    var amount1 = Parse.Float(split, 1, 0f);
    var amount2 = Parse.Float(split, 2, 0f);
    var amount3 = Parse.Float(split, 3, 0f);
    duration = amount1;
    damage = amount1;
    damageIgnoreArmor = amount2;
    damageIgnoreAll = amount3;
    itemLevel = (int)amount2;
    skillLevel = amount3;
    // Custom duration is handled manually.
    // Also damage effects shouldn't be reseted (since it messed up the damage calculation).
    reset = amount1 == 0f && amount2 == 0f && amount3 == 0f;
  }
}