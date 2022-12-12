using BepInEx.Bootstrap;
namespace ExpandWorld;

public class SpawnThatPatcher
{
  public const string GUID = "asharppen.valheim.spawn_that";
  public static void Run()
  {
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var info)) return;
    ExpandWorld.Log.LogInfo("\"Spawn That\" detected. Disabling spawns data.");
    Configuration.configDataSpawns.Value = false;
  }
}
