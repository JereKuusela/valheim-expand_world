using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using ServerSync;

namespace Service;

public class ConfigWrapper
{

  private readonly ConfigFile ConfigFile;
  private readonly ConfigSync ConfigSync;
  private readonly Action Regenerate;
  public ConfigWrapper(string command, ConfigFile configFile, ConfigSync configSync, Action regenerate)
  {
    ConfigFile = configFile;
    ConfigSync = configSync;
    Regenerate = regenerate;

    new Terminal.ConsoleCommand(command, "[key] [value] - Toggles or sets a config value.", (Terminal.ConsoleEventArgs args) =>
    {
      if (configSync.IsLocked && !configSync.IsAdmin)
      {
        args.Context.AddString("Error: Unable to edit locked config.");
        return;
      }
      if (args.Length < 2)
      {
        args.Context.AddString("Error: Missing the key.");
        return;
      }
      if (!SettingHandlers.TryGetValue(args[1].ToLower(), out var handler))
      {
        args.Context.AddString("Error: Key not found.");
        return;
      }
      if (args.Length == 2)
        handler(args.Context, "");
      else
        handler(args.Context, string.Join(" ", args.Args.Skip(2)));
    }, optionsFetcher: () => SettingHandlers.Keys.ToList());
  }
  public CustomSyncedValue<string> AddValue(string identifier) => new(ConfigSync, identifier);
  public ConfigEntry<bool> BindLocking(string group, string name, bool value, ConfigDescription description)
  {
    var configEntry = ConfigFile.Bind(group, name, value, description);
    Register(configEntry);
    var syncedConfigEntry = ConfigSync.AddLockingConfigEntry(configEntry);
    syncedConfigEntry.SynchronizedConfig = true;
    return configEntry;
  }
  public ConfigEntry<bool> BindLocking(string group, string name, bool value, string description) => BindLocking(group, name, value, new ConfigDescription(description));
  public ConfigEntry<T> Bind<T>(string group, string name, T value, bool automaticRegenerate, ConfigDescription description, bool synchronizedSetting = true)
  {
    var configEntry = ConfigFile.Bind(group, name, value, description);
    if (automaticRegenerate)
      configEntry.SettingChanged += ForceRegen;
    if (configEntry is ConfigEntry<bool> boolEntry)
      Register(boolEntry);
    else if (configEntry is ConfigEntry<string> stringEntry)
      Register(stringEntry);
    else throw new NotImplementedException($"Config type not implemented for {name}.");
    var syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
    syncedConfigEntry.SynchronizedConfig = synchronizedSetting;
    return configEntry;
  }
  public ConfigEntry<T> Bind<T>(string group, string name, T value, bool forceRegen, string description = "", bool synchronizedSetting = true) => Bind(group, name, value, forceRegen, new ConfigDescription(description), synchronizedSetting);
  private void ForceRegen(object e, EventArgs s) => Regenerate();
  public static Dictionary<ConfigEntry<string>, float> Floats = new();
  public static Dictionary<ConfigEntry<string>, int?> Ints = new();
  public static Dictionary<ConfigEntry<int>, float> Amounts = new();

  public ConfigEntry<string> BindFloat(string group, string name, float value, bool forceRegen, string description = "", bool synchronizedSetting = true)
  {
    var entry = Bind(group, name, value.ToString(CultureInfo.InvariantCulture), forceRegen, description, synchronizedSetting);
    entry.SettingChanged += (s, e) => Floats[entry] = TryParseFloat(entry);
    Floats[entry] = TryParseFloat(entry);
    return entry;
  }
  public ConfigEntry<string> BindInt(string group, string name, int? value, bool forceRegen, string description = "", bool synchronizedSetting = true)
  {
    var entry = Bind(group, name, value.HasValue ? value.ToString() : "", forceRegen, description, synchronizedSetting);
    entry.SettingChanged += (s, e) => Ints[entry] = TryParseInt(entry);
    Ints[entry] = TryParseInt(entry);
    return entry;
  }
  private static void AddMessage(Terminal context, string message)
  {
    context.AddString(message);
    Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, message);
  }
  private readonly Dictionary<string, Action<Terminal, string>> SettingHandlers = new();
  private string ToKey(string name) => name.ToLower().Replace(' ', '_').Replace("(", "").Replace(")", "");
  private void Register(ConfigEntry<bool> setting)
  {
    var name = setting.Definition.Key;
    var key = ToKey(name);
    SettingHandlers.Add(key, (Terminal terminal, string value) => Toggle(terminal, setting, name, value));
  }
  private void Register(ConfigEntry<string> setting)
  {
    var name = setting.Definition.Key;
    var key = ToKey(name);
    SettingHandlers.Add(key, (Terminal terminal, string value) => SetValue(terminal, setting, name, value));
  }
  private static string State(bool value) => value ? "enabled" : "disabled";
  private static readonly HashSet<string> Truthies = new() {
    "1",
    "true",
    "yes",
    "on"
  };
  private static bool IsTruthy(string value) => Truthies.Contains(value);
  private static readonly HashSet<string> Falsies = new() {
    "0",
    "false",
    "no",
    "off"
  };
  private static bool IsFalsy(string value) => Falsies.Contains(value);
  private static void Toggle(Terminal context, ConfigEntry<bool> setting, string name, string value)
  {
    if (value == "") setting.Value = !setting.Value;
    else if (IsTruthy(value)) setting.Value = true;
    else if (IsFalsy(value)) setting.Value = false;
    AddMessage(context, $"{name} {State(setting.Value)}.");
  }
  private static int? TryParseInt(ConfigEntry<string> setting)
  {
    if (int.TryParse(setting.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)) return result;
    return null;
  }
  private static float TryParseFloat(string value, float defaultValue)
  {
    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return result;
    return defaultValue;
  }
  private static float TryParseFloat(ConfigEntry<string> setting)
  {
    if (float.TryParse(setting.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return result;
    return TryParseFloat((string)setting.DefaultValue, 0f);
  }
  private static void SetValue(Terminal context, ConfigEntry<string> setting, string name, string value)
  {
    if (value == "")
    {
      AddMessage(context, $"{name}: {setting.Value}.");
      return;
    }
    setting.Value = value;
    AddMessage(context, $"{name} set to {value}.");
  }
}
