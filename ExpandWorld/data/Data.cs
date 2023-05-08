
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ExpandWorld;


[HarmonyPatch(typeof(DungeonDB), nameof(DungeonDB.Start)), HarmonyPriority(Priority.VeryLow)]
public class InitializeRooms
{
  static void Postfix() => RoomLoading.Initialize();
}

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.VersionSetup)), HarmonyPriority(Priority.VeryLow)]
public class InitializeWorld
{
  // River generation requires biome and world data being loaded.
  // Saving is done later because that requires environments.
  static void Postfix()
  {
    // Only called for server so no need to check.
    BiomeManager.FromFile();
    WorldManager.FromFile();
  }
}


[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start)), HarmonyPriority(Priority.VeryLow)]
public class InitializeContent
{
  static void Postfix()
  {
    if (ZNet.instance.IsServer())
    {
      EventManager.ToFile();
      DungeonManager.ToFile();
      EnvironmentManager.ToFile();
      ClutterManager.ToFile();

      EnvironmentManager.FromFile();
      BiomeManager.LoadEnvironments();
      BiomeManager.ToFile();
      WorldManager.ToFile();

      // These are here to not have to clear location lists (slightly better compatibility).
      VegetationLoading.Initialize();
      // Clutter must be here because since SetupLocations adds prefabs to the list.
      ClutterManager.FromFile();

      EventManager.FromFile();
      SpawnManager.FromFile();
      DungeonManager.FromFile();
      // Room data is handled elsewhere.
      // Spawn data is handled elsewhere.
    }
    ZoneSystem.instance.m_locations = LocationSetup.CleanUpLocations(ZoneSystem.instance.m_locations);
    LocationSetup.SetupExtraLocations(ZoneSystem.instance.m_locations);
    LocationLoading.Initialize();
  }
}
[HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.Awake))]
public class HandleSpawnData
{
  // File exist check might be bit too slow for constant checking.
  static bool Done = false;
  public static List<SpawnSystem.SpawnData>? Override = null;
  static void Postfix(SpawnSystem __instance)
  {
    if (ZNet.instance.IsServer() && !Done)
    {
      SpawnManager.ToFile();
      Done = true;
    }
    if (Override != null)
    {
      __instance.m_spawnLists.Clear();
      __instance.m_spawnLists.Add(new() { m_spawners = Override });
    }
  }
}

[HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.UpdateSpawning))]
public class Spawn_WaitForConfigSync
{
  static bool Prefix() => Data.IsReady;
}
public class Data : MonoBehaviour
{
  public static bool IsReady => ExpandWorld.ConfigSync.IsSourceOfTruth || ExpandWorld.ConfigSync.InitialSyncDone;

  private static void SetupWatcher(string folder, string pattern, Action<string> action)
  {
    FileSystemWatcher watcher = new(folder, pattern);
    watcher.Created += (s, e) => action(e.Name);
    watcher.Changed += (s, e) => action(e.Name);
    watcher.Renamed += (s, e) => action(e.Name);
    watcher.Deleted += (s, e) => action(e.Name);
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }
  private static void ReloadBlueprint(string name)
  {
    BlueprintManager.Reload(Path.GetFileNameWithoutExtension(name));
  }
  public static void SetupBlueprintWatcher()
  {
    if (!Directory.Exists(Configuration.BlueprintGlobalFolder))
      Directory.CreateDirectory(Configuration.BlueprintGlobalFolder);
    if (!Directory.Exists(Configuration.BlueprintLocalFolder))
      Directory.CreateDirectory(Configuration.BlueprintLocalFolder);
    SetupWatcher(Configuration.BlueprintGlobalFolder, "*.blueprint", ReloadBlueprint);
    SetupWatcher(Configuration.BlueprintGlobalFolder, "*.vbuild", ReloadBlueprint);
    if (Path.GetFullPath(Configuration.BlueprintLocalFolder) != Path.GetFullPath(Configuration.BlueprintGlobalFolder))
    {
      SetupWatcher(Configuration.BlueprintLocalFolder, "*.blueprint", ReloadBlueprint);
      SetupWatcher(Configuration.BlueprintLocalFolder, "*.vbuild", ReloadBlueprint);
    }
  }
  public static void SetupWatcher(string pattern, Action action) => SetupWatcher(ExpandWorld.YamlDirectory, pattern, _ => action());
  public static IDeserializer Deserializer() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
    .WithTypeConverter(new FloatConverter()).Build();
  public static IDeserializer DeserializerUnSafe() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
  .WithTypeConverter(new FloatConverter()).IgnoreUnmatchedProperties().Build();
  public static ISerializer Serializer() => new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases()
    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults).WithTypeConverter(new FloatConverter())
      .WithAttributeOverride<Color>(c => c.gamma, new YamlIgnoreAttribute())
      .WithAttributeOverride<Color>(c => c.grayscale, new YamlIgnoreAttribute())
      .WithAttributeOverride<Color>(c => c.linear, new YamlIgnoreAttribute())
      .WithAttributeOverride<Color>(c => c.maxColorComponent, new YamlIgnoreAttribute())
      .Build();

  public static List<T> Deserialize<T>(string raw, string fileName)
  {
    try
    {
      return Deserializer().Deserialize<List<T>>(raw) ?? new();
    }
    catch (Exception ex1)
    {
      ExpandWorld.Log.LogError($"{fileName}: {ex1.Message}");
      try
      {
        return DeserializerUnSafe().Deserialize<List<T>>(raw) ?? new();
      }
      catch (Exception)
      {
        return new();
      }
    }
  }
  private static Heightmap.Biome DefaultMax =
    Heightmap.Biome.AshLands | Heightmap.Biome.BlackForest | Heightmap.Biome.DeepNorth |
    Heightmap.Biome.Meadows | Heightmap.Biome.Mistlands | Heightmap.Biome.Mountain |
    Heightmap.Biome.Ocean | Heightmap.Biome.Plains | Heightmap.Biome.Swamp;

  public static string FromList(IEnumerable<string> array) => string.Join(", ", array);
  public static List<string> ToList(string str) => Parse.Split(str).ToList();
  public static Dictionary<string, string> ToDict(string str) => ToList(str).Select(s => s.Split('=')).Where(s => s.Length == 2).ToDictionary(s => s[0].Trim(), s => s[1].Trim());
  public static ZDO? ToZDO(string data)
  {
    if (data == "") return null;
    ZPackage pkg = new(data);
    ZDO zdo = new();
    Data.Deserialize(zdo, pkg);
    return zdo;
  }
  public static string FromBiomes(Heightmap.Biome biome)
  {
    // Unused biome.
    biome &= (Heightmap.Biome)~128;
    if (biome == DefaultMax) return "";
    if (biome == Heightmap.Biome.None) return "None";
    List<string> biomes = new();
    var number = 1;
    var biomeNumber = (int)biome;
    while (number <= biomeNumber)
    {
      if ((number & biomeNumber) > 0)
      {
        if (BiomeManager.TryGetDisplayName((Heightmap.Biome)number, out var name))
          biomes.Add(name);
        else
          biomes.Add(number.ToString());
      }
      number *= 2;
    }
    return string.Join(", ", biomes);
  }
  public static string FromBiomeAreas(Heightmap.BiomeArea biomeArea)
  {
    var edge = (biomeArea & Heightmap.BiomeArea.Edge) > 0;
    var median = (biomeArea & Heightmap.BiomeArea.Median) > 0;
    if (edge && median) return "";
    if (edge) return "edge";
    if (median) return "median";
    return "";
  }
  public static string FromEnum<T>(T value) where T : struct, Enum
  {
    List<string> names = new();
    var number = 1;
    var maxNumber = (int)(object)value;
    while (number <= maxNumber)
    {
      if ((number & maxNumber) > 0)
      {
        names.Add(((T)(object)(number)).ToString());
      }
      number *= 2;
    }
    return FromList(names);
  }
  public static T ToEnum<T>(string str) where T : struct, Enum
  {
    var list = ToList(str);
    int value = 0;
    foreach (var item in list)
    {
      if (Enum.TryParse<T>(item, true, out var parsed))
        value = value + (int)(object)parsed;
      else
        ExpandWorld.Log.LogWarning($"Failed to parse value {item} as {nameof(T)}.");
    }
    return (T)(object)value;
  }
  public static Heightmap.Biome ToBiomes(string biomeStr)
  {
    Heightmap.Biome result = 0;
    if (biomeStr == "")
    {
      foreach (var biome in BiomeManager.BiomeToDisplayName.Keys)
        result |= biome;
    }
    else
    {
      var biomes = Parse.Split(biomeStr);
      foreach (var biome in biomes)
      {
        if (BiomeManager.TryGetBiome(biome, out var number))
          result |= number;
        else
        {
          if (int.TryParse(biome, out var value)) result += value;
          else throw new InvalidOperationException($"Invalid biome {biome}.");
        }
      }
    }
    return result;
  }
  public static Heightmap.BiomeArea ToBiomeAreas(string m_biome)
  {
    if (m_biome == "") return Heightmap.BiomeArea.Edge | Heightmap.BiomeArea.Median;
    var biomes = Parse.Split(m_biome);
    var biomeAreas = biomes.Select(s => Enum.TryParse<Heightmap.BiomeArea>(s, true, out var area) ? area : 0);
    Heightmap.BiomeArea result = 0;
    foreach (var biome in biomeAreas) result |= biome;
    return result;
  }
  public static GameObject? ToPrefab(string str)
  {
    if (ZNetScene.instance.m_namedPrefabs.TryGetValue(str.GetStableHashCode(), out var obj))
      return obj;
    else
      ExpandWorld.Log.LogWarning($"Prefab {str} not found!");
    return null;
  }
  public static void CopyData(ZDO from, ZDO to)
  {
    to.m_floats = from.m_floats;
    to.m_vec3 = from.m_vec3;
    to.m_quats = from.m_quats;
    to.m_ints = from.m_ints;
    to.m_longs = from.m_longs;
    to.m_strings = from.m_strings;
    to.m_byteArrays = from.m_byteArrays;
    to.IncreseDataRevision();
  }
  public static void Deserialize(ZDO zdo, ZPackage pkg)
  {
    int num = pkg.ReadInt();
    if ((num & 1) != 0)
    {
      zdo.InitFloats();
      int num2 = (int)pkg.ReadByte();
      for (int i = 0; i < num2; i++)
      {
        int key = pkg.ReadInt();
        zdo.m_floats[key] = pkg.ReadSingle();
      }
    }
    else
    {
      zdo.ReleaseFloats();
    }
    if ((num & 2) != 0)
    {
      zdo.InitVec3();
      int num3 = (int)pkg.ReadByte();
      for (int j = 0; j < num3; j++)
      {
        int key2 = pkg.ReadInt();
        zdo.m_vec3[key2] = pkg.ReadVector3();
      }
    }
    else
    {
      zdo.ReleaseVec3();
    }
    if ((num & 4) != 0)
    {
      zdo.InitQuats();
      int num4 = (int)pkg.ReadByte();
      for (int k = 0; k < num4; k++)
      {
        int key3 = pkg.ReadInt();
        zdo.m_quats[key3] = pkg.ReadQuaternion();
      }
    }
    else
    {
      zdo.ReleaseQuats();
    }
    if ((num & 8) != 0)
    {
      zdo.InitInts();
      int num5 = (int)pkg.ReadByte();
      for (int l = 0; l < num5; l++)
      {
        int key4 = pkg.ReadInt();
        zdo.m_ints[key4] = pkg.ReadInt();
      }
    }
    else
    {
      zdo.ReleaseInts();
    }
    if ((num & 64) != 0)
    {
      zdo.InitLongs();
      int num6 = (int)pkg.ReadByte();
      for (int m = 0; m < num6; m++)
      {
        int key5 = pkg.ReadInt();
        zdo.m_longs[key5] = pkg.ReadLong();
      }
    }
    else
    {
      zdo.ReleaseLongs();
    }
    if ((num & 16) != 0)
    {
      zdo.InitStrings();
      int num7 = (int)pkg.ReadByte();
      for (int n = 0; n < num7; n++)
      {
        int key6 = pkg.ReadInt();
        zdo.m_strings[key6] = pkg.ReadString();
      }
    }
    else
    {
      zdo.ReleaseStrings();
    }
    if ((num & 128) != 0)
    {
      zdo.InitByteArrays();
      int num8 = (int)pkg.ReadByte();
      for (int num9 = 0; num9 < num8; num9++)
      {
        int key7 = pkg.ReadInt();
        zdo.m_byteArrays[key7] = pkg.ReadByteArray();
      }
      return;
    }
    zdo.ReleaseByteArrays();
  }
  public static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion rot, Vector3 sc, ZDO? data)
  {
    Data.InitZDO(pos, rot, sc, data, prefab);
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, pos, rot);
    Data.CleanGhostInit(obj);
    return obj;
  }
  public static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion rot, ZDO? data)
  {
    Data.InitZDO(pos, rot, Vector3.one, data, prefab);
    var obj = UnityEngine.Object.Instantiate<GameObject>(prefab, pos, rot);
    Data.CleanGhostInit(obj);
    return obj;
  }
  public static void InitZDO(Vector3 pos, Quaternion rot, Vector3 sc, ZDO? data, GameObject obj)
  {
    if (data == null) return;
    if (!obj.TryGetComponent<ZNetView>(out var view)) return;
    InitZDO(pos, rot, sc, data, view);
  }
  public static void InitZDO(Vector3 pos, Quaternion rot, Vector3 sc, ZDO data, ZNetView view)
  {
    ZNetView.m_initZDO = ZDOMan.instance.CreateNewZDO(pos);
    Data.CopyData(data.Clone(), ZNetView.m_initZDO);
    ZNetView.m_initZDO.m_rotation = rot;
    ZNetView.m_initZDO.m_type = view.m_type;
    ZNetView.m_initZDO.m_distant = view.m_distant;
    ZNetView.m_initZDO.m_persistent = view.m_persistent;
    ZNetView.m_initZDO.m_prefab = view.GetPrefabName().GetStableHashCode();
    if (view.m_syncInitialScale && sc != Vector3.one)
      ZNetView.m_initZDO.Set("scale", sc);
    ZNetView.m_initZDO.m_dataRevision = 1;
  }
  public static void CleanGhostInit(GameObject obj)
  {
    if (ZNetView.m_ghostInit) CleanGhostInit(obj.GetComponent<ZNetView>());
  }
  public static void CleanGhostInit(ZNetView view)
  {
    if (ZNetView.m_ghostInit && view)
    {
      view.m_ghost = true;
      ZNetScene.instance.m_instances.Remove(view.GetZDO());
    }
  }
  public static string Read(string pattern)
  {
    if (!Directory.Exists(ExpandWorld.YamlDirectory))
      Directory.CreateDirectory(ExpandWorld.YamlDirectory);
    var data = Directory.GetFiles(ExpandWorld.YamlDirectory, pattern, SearchOption.AllDirectories).Reverse().Select(name =>
      string.Join("\n", File.ReadAllLines(name).ToList())
    );
    return string.Join("\n", data) ?? "";
  }
  public static void Sanity(ref Color color)
  {
    if (color.r > 1.0f) color.r /= 255f;
    if (color.g > 1.0f) color.g /= 255f;
    if (color.b > 1.0f) color.b /= 255f;
    if (color.a > 1.0f) color.a /= 255f;
  }
  public static Color Sanity(Color color)
  {
    if (color.r > 1.0f) color.r /= 255f;
    if (color.g > 1.0f) color.g /= 255f;
    if (color.b > 1.0f) color.b /= 255f;
    if (color.a > 1.0f) color.a /= 255f;
    return color;
  }

}
#nullable disable
public class FloatConverter : IYamlTypeConverter
{
  public bool Accepts(Type type) => type == typeof(float);

  public object ReadYaml(IParser parser, Type type)
  {
    var scalar = (YamlDotNet.Core.Events.Scalar)parser.Current;
    var number = float.Parse(scalar.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
    parser.MoveNext();
    return number;
  }

  public void WriteYaml(IEmitter emitter, object value, Type type)
  {
    var number = (float)value;
    emitter.Emit(new YamlDotNet.Core.Events.Scalar(number.ToString("0.###", CultureInfo.InvariantCulture)));
  }
}
#nullable enable
