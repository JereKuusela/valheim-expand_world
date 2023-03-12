using BepInEx.Bootstrap;
namespace ExpandWorld;

public class CustomRaids
{
  public const string GUID = "asharppen.valheim.custom_raids";
  public static void Run()
  {
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var info)) return;
    ExpandWorld.Log.LogInfo("\"Custom Raids\" detected. Disabling event data.");
    Configuration.configDataEvents.Value = false;
  }
}
