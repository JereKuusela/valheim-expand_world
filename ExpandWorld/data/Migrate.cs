using System;
using System.Collections.Generic;

namespace ExpandWorld;

public class Migrate
{
  public static bool DictionaryToList(List<string> lines, string field)
  {
    var migrated = false;
    for (var i = lines.Count - 1; i >= 0; i -= 1)
    {
      var line = lines[i];
      if (!line.Contains(field + ":")) continue;
      var intend = line.Length - line.TrimStart().Length;
      var j = i + 1;
      while (j < lines.Count)
      {
        line = lines[j];
        var subIntend = line.Length - line.TrimStart().Length;
        if (intend == subIntend) break;
        if (line.Contains("-") || !line.Contains(":")) break;
        migrated = true;
        var split = line.Split(':');
        lines[j] = line = "".PadLeft(subIntend, ' ') + "- " + split[0].Trim() + ", " + split[1].Trim();
        j += 1;
      }
    }
    return migrated;
  }
  public static bool BiomeAreas(List<string> lines)
  {
    var migrated = false;
    for (var i = lines.Count - 1; i >= 0; i -= 1)
    {
      var line = lines[i];
      if (!line.Contains("biomeArea:")) continue;
      var value = line.Split(':')[1].Trim();
      if (value == "median" || value == "edge") continue;
      migrated = true;
      var entry = "";
      var multipleEntries = false;
      var j = i + 1;
      while (j < lines.Count)
      {
        line = lines[j];
        if (line.Contains(":") || !line.Contains("-")) break;
        lines.RemoveAt(j);
        var newEntry = line.Split('-')[1].Trim();
        // Ignore numbers like 4 since they don't mean anything.
        if (newEntry != "median" && newEntry != "edge") continue;
        if (entry != "")
          multipleEntries = true;
        entry = newEntry;
      }
      // Default value is both biome areas so no data is needed in that case.
      if (multipleEntries)
        lines.RemoveAt(i);
      // No entries makes no sense because the spawn condition would always be false.
      // So better convert that to the default (both biome areas).
      else if (entry == "")
        lines.RemoveAt(i);
      else
        lines[i] = $"{lines[i].Split(':')[0]}: {entry}";
    }
    return migrated;
  }
  public static bool Biomes(List<string> lines) => ListMerger(lines, "biome", str => Data.FromBiomes(Data.ToBiomes(str)));
  public static bool Environments(List<string> lines) => ListMerger(lines, "requiredEnvironments");
  public static bool GlobalKeys(List<string> lines) => ListMerger(lines, "requiredGlobalKeys");
  public static bool NotGlobalKeys(List<string> lines) => ListMerger(lines, "notRequiredGlobalKeys");
  private static bool ListMerger(List<string> lines, string key, Func<string, string>? post = null)
  {
    var migrated = false;
    for (var i = lines.Count - 1; i >= 0; i -= 1)
    {
      var line = lines[i];
      if (!line.Contains($"{key}:")) continue;
      var value = line.Split(':')[1].Trim();
      if (value == "[]")
      {
        lines.RemoveAt(i);
        migrated = true;
      }
      if (value != "") continue;
      migrated = true;
      List<string> entries = new();
      var j = i + 1;
      while (j < lines.Count)
      {
        line = lines[j];
        if (line.Contains(":") || !line.Contains("-")) break;
        lines.RemoveAt(j);
        entries.Add(line.Split('-')[1].Trim());
      }
      var str = string.Join(", ", entries);
      if (post != null)
        str = post(str);
      if (str == "")
        lines.RemoveAt(i);
      else
        lines[i] = $"{lines[i].Split(':')[0]}: {str}";
    }
    return migrated;
  }
}