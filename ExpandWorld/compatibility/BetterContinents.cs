using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace ExpandWorld;

public class BetterContinents
{
  public const string GUID = "BetterContinents";
  private static Assembly? BetterContinentsAssembly;
  private static FieldInfo? SettingsField;
  private static FieldInfo? IsEnabledField;
  public static void Run()
  {
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var info)) return;
    BetterContinentsAssembly = info.Instance.GetType().Assembly;
    var type = BetterContinentsAssembly.GetType("BetterContinents.BetterContinents");
    if (type == null) return;
    SettingsField = AccessTools.Field(type, "Settings");
    if (SettingsField == null) return;
    type = BetterContinentsAssembly.GetType("BetterContinents.BetterContinents+BetterContinentsSettings");
    IsEnabledField = AccessTools.Field(type, "EnabledForThisWorld");
    if (IsEnabledField == null) return;
    ExpandWorld.Log.LogInfo("\"Better Continents\" detected. Applying compatibility.");
  }

  public static bool IsEnabled()
  {
    if (SettingsField == null) return false;
    if (IsEnabledField == null) return false;
    var settings = SettingsField.GetValue(null);
    if (settings == null) return false;
    return (bool)IsEnabledField.GetValue(settings);
  }
}
