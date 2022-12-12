using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace ExpandWorld;
public class DebugCommands
{
  public DebugCommands()
  {
    new Terminal.ConsoleCommand("ew_lakes", "Pings lakes", args =>
    {
      foreach (Minimap.PinData pin in args.Context.m_findPins)
        Minimap.instance.RemovePin(pin);
      args.Context.m_findPins.Clear();
      foreach (var lake in WorldGenerator.instance.m_lakes)
      {
        Vector3 pos = new(lake.x, 0f, lake.y);
        args.Context.m_findPins.Add(Minimap.instance.AddPin(pos, Minimap.PinType.Icon3, "", false, true, Player.m_localPlayer.GetPlayerID()));
      }
      args.Context.AddString($"Found {WorldGenerator.instance.m_lakes.Count} lakes.");
    }, true);
    new Terminal.ConsoleCommand("ew_rivers", "Pings rivers", args =>
    {
      foreach (Minimap.PinData pin in args.Context.m_findPins)
        Minimap.instance.RemovePin(pin);
      args.Context.m_findPins.Clear();
      foreach (var river in WorldGenerator.instance.m_rivers)
      {
        Vector3 pos = new(river.center.x, 0f, river.center.y);
        args.Context.m_findPins.Add(Minimap.instance.AddPin(pos, Minimap.PinType.Icon3, "", false, true, Player.m_localPlayer.GetPlayerID()));
      }
      args.Context.AddString($"Found {WorldGenerator.instance.m_rivers.Count} rivers.");
    }, true);
    new Terminal.ConsoleCommand("ew_streams", "Pings streams", args =>
    {
      foreach (Minimap.PinData pin in args.Context.m_findPins)
        Minimap.instance.RemovePin(pin);
      args.Context.m_findPins.Clear();
      foreach (var stream in WorldGenerator.instance.m_streams)
      {
        Vector3 pos = new(stream.center.x, 0f, stream.center.y);
        args.Context.m_findPins.Add(Minimap.instance.AddPin(pos, Minimap.PinType.Icon3, "", false, true, Player.m_localPlayer.GetPlayerID()));
      }
      args.Context.AddString($"Found {WorldGenerator.instance.m_streams.Count} streams.");
    }, true);
    new Terminal.ConsoleCommand("ew_spawns", "Forces spawn file creation.", (args) =>
    {
      SpawnManager.Save();
    }, true);
    new Terminal.ConsoleCommand("ew_biomes", "[precision] - Counts biomes by sampling points with a given precision (meters).", args =>
    {
      var precision = 100f;
      if (args.Length > 1 && int.TryParse(args[1], out var value)) precision = (float)value;
      var r = Configuration.WorldRadius;
      var start = -(float)Math.Ceiling(r / precision) * precision;
      Dictionary<Heightmap.Biome, int> biomes = new();
      for (var x = start; x <= r; x += precision)
      {
        for (var y = start; y <= r; y += precision)
        {
          var distance = new Vector2(x, y).magnitude;
          if (distance > r) continue;
          var biome = WorldGenerator.instance.GetBiome(x, y);
          if (!biomes.ContainsKey(biome)) biomes[biome] = 0;
          biomes[biome]++;
        }
      }
      float total = biomes.Values.Sum();
      var text = biomes.OrderBy(kvp => kvp.Key.ToString()).Select(kvp => kvp.Key.ToString() + ": " + kvp.Value + "/" + total + " (" + (kvp.Value / total).ToString("P2", CultureInfo.InvariantCulture) + ")");
      args.Context.AddString(string.Join("\n", text));
    }, true);
    new Terminal.ConsoleCommand("ew_musics", "- Prints available musics.", args =>
    {
      var mm = MusicMan.instance;
      if (!mm) return;
      var names = mm.m_music.Where(music => music.m_enabled).Select(music => music.m_name);
      args.Context.AddString(string.Join("\n", names));
    }, true);
    new Terminal.ConsoleCommand("ew_seeds", "- Prints different seeds.", args =>
    {
      var wg = WorldGenerator.m_instance;
      if (wg == null) return;
      List<string> lines = new() {
        "Main: " + wg.m_world.m_seedName,
        "Generator: " + wg.m_world.m_worldGenVersion,
        "World: " + wg.m_world.m_seed,
        "Offset X: " + wg.m_offset0,
        "Offset Y: " + wg.m_offset1,
        "Height: " + wg.m_offset3,
        "Meadows: " + wg.m_offset3,
        "Black forest: " + wg.m_offset2,
        "Swamp: "+ wg.m_offset0,
        "Plains: "+ wg.m_offset1,
        "Mistlands: " + wg.m_offset4
      };
      args.Context.AddString(string.Join("\n", lines));
    }, true);
  }
}
