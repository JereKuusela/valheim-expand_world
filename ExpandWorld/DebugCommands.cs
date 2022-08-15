using UnityEngine;

namespace ExpandWorld;
public class DebugCommands {
  public DebugCommands() {
    new Terminal.ConsoleCommand("find_lakes", "Pings lakes", (args) => {
      foreach (Minimap.PinData pin in args.Context.m_findPins)
        Minimap.instance.RemovePin(pin);
      args.Context.m_findPins.Clear();
      foreach (var lake in WorldGenerator.instance.m_lakes) {
        Vector3 pos = new(lake.x, 0f, lake.y);
        args.Context.m_findPins.Add(Minimap.instance.AddPin(pos, Minimap.PinType.Icon3, "", false, true, Player.m_localPlayer.GetPlayerID()));
      }
      args.Context.AddString($"Found {WorldGenerator.instance.m_lakes.Count} lakes.");
    }, true);
    new Terminal.ConsoleCommand("find_rivers", "Pings rivers", (args) => {
      foreach (Minimap.PinData pin in args.Context.m_findPins)
        Minimap.instance.RemovePin(pin);
      args.Context.m_findPins.Clear();
      foreach (var river in WorldGenerator.instance.m_rivers) {
        Vector3 pos = new(river.center.x, 0f, river.center.y);
        args.Context.m_findPins.Add(Minimap.instance.AddPin(pos, Minimap.PinType.Icon3, "", false, true, Player.m_localPlayer.GetPlayerID()));
      }
      args.Context.AddString($"Found {WorldGenerator.instance.m_rivers.Count} rivers.");
    }, true);
    new Terminal.ConsoleCommand("find_streams", "Pings streams", (args) => {
      foreach (Minimap.PinData pin in args.Context.m_findPins)
        Minimap.instance.RemovePin(pin);
      args.Context.m_findPins.Clear();
      foreach (var stream in WorldGenerator.instance.m_streams) {
        Vector3 pos = new(stream.center.x, 0f, stream.center.y);
        args.Context.m_findPins.Add(Minimap.instance.AddPin(pos, Minimap.PinType.Icon3, "", false, true, Player.m_localPlayer.GetPlayerID()));
      }
      args.Context.AddString($"Found {WorldGenerator.instance.m_streams.Count} streams.");
    }, true);
    new Terminal.ConsoleCommand("debug_spawns", "Forces spawn file creation.", (args) => {
      SpawnManager.Save();
    }, true);
  }
}
