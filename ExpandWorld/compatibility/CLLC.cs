using System;
using System.Reflection;
using BepInEx.Bootstrap;
namespace ExpandWorld;

public class CLLCWrapper {
  public const string GUID = "org.bepinex.plugins.creaturelevelcontrol";
  private static void Call(Type type, string name) => type.GetMethod(name, BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[0]);
  public static void UpdateBiomes() {
    if (CLLC == null) return;
    var type = CLLC.GetType("CreatureLevelControl.CreatureConfig");
    if (type == null) return;
    var field = type.GetField("knownBiomes", BindingFlags.Static | BindingFlags.NonPublic);
    if (field == null) return;
    var values = (Heightmap.Biome[])field.GetValue(null);
    field.SetValue(null, BiomeManager.Biomes);
  }
  private static Assembly? CLLC;
  public static void Run() {
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var info)) return;
    CLLC = info.Instance.GetType().Assembly;
    if (CLLC == null) return;
    ExpandWorld.Log.LogInfo("\"CLLC\" detected. Adding compatibility for custom biomes.");
  }
}
