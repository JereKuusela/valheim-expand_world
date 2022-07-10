
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;

public class Generate {
  public static int LastTaskIndex = 0;
  public static bool Generating => WorldGeneration.Generating || MapGeneration.Generating;

  private static float Debouncer = 10f;
  public static void World() {
    Debouncer = 0f;
    MapOnly = false;
  }
  private static bool MapOnly = true;
  public static void Map() {
    Debouncer = 0f;
  }
  public static void Cancel() {
    Debouncer = 10f;
    WorldGeneration.Cancel();
    MapGeneration.Cancel();
  }
  public static void CheckRegen(float dt) {
    if (Debouncer > 2f) return;
    Debouncer += dt;
    if (Debouncer > 2f) {
      if (MapOnly)
        Minimap.instance?.GenerateWorldMap();
      else
        WorldGenerator.instance?.Pregenerate();
      MapOnly = true;
    } 
  }
}

[HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.Pregenerate))]
public class WorldGeneration {
  public static bool HasLoaded = false;
  static bool Prefix(WorldGenerator __instance) {
    Game.m_instance.StartCoroutine(Coroutine(__instance));
    return false;
  }
  public static void Cancel() {
    if (CTS != null) {
      ZLog.Log("Cancelling previous world generation.");
      CTS.Cancel();
      CTS = null;
    }
  }
  public static bool Generating => CTS != null;
  static CancellationTokenSource? CTS = null;
  static IEnumerator Coroutine(WorldGenerator wg) {
    int taskIndex = global::ExpandWorld.Generate.LastTaskIndex++;
    Cancel();
    MapGeneration.Cancel();

    ZLog.Log($"Started world generation.");
    var stopwatch = Stopwatch.StartNew();

    CancellationTokenSource cts = new();
    var ct = cts.Token;

    CTS = cts;
    wg.m_riverPoints.Clear();
    wg.m_cachedRiverGrid = new Vector2i(-999999, -999999);
		wg.m_cachedRiverPoints = null;
    wg.FindLakes();
    if (ct.IsCancellationRequested)
      yield break;
    yield return null;
    wg.m_rivers = wg.PlaceRivers();
    if (ct.IsCancellationRequested)
      yield break;
    yield return null;
    wg.m_streams = wg.PlaceStreams();
    if (ct.IsCancellationRequested)
      yield break;
    yield return null;
    foreach (var heightmap in Heightmap.m_heightmaps) {
      heightmap.m_buildData = null;
      heightmap.Regenerate();
    }
    if (ct.IsCancellationRequested)
      yield break;
    yield return null;
    if (ClutterSystem.instance)
      ClutterSystem.instance.m_forceRebuild = true;
    SetupMaterial.Refresh();
    if (ct.IsCancellationRequested)
      yield break;
    yield return null;
    HasLoaded = true;
    ZLog.Log($"Finished world generation ({stopwatch.Elapsed.TotalSeconds.ToString("F0")} seconds).");
    stopwatch.Stop();
    cts.Dispose();

    if (CTS == cts)
      CTS = null;
    if (Minimap.instance)
      Minimap.instance.GenerateWorldMap();
  }
}


[HarmonyPatch(typeof(Minimap), nameof(Minimap.GenerateWorldMap))]
public class MapGeneration {
  static bool Prefix(Minimap __instance) {
    if (!WorldGeneration.HasLoaded || WorldGeneration.Generating) return false;
    Game.instance.StartCoroutine(Coroutine(__instance));
    return false;
  }
  public static void Cancel() {
    if (CTS != null) {
      ZLog.Log($"Cancelling previous map generation.");
      CTS.Cancel();
      CTS = null;
    }
  }
  public static int TextureSize;
  public static bool Generating => CTS != null;
  static CancellationTokenSource? CTS = null;
  static IEnumerator Coroutine(Minimap map) {
    Cancel();

    ZLog.Log($"Starting map generation.");
    Stopwatch stopwatch = Stopwatch.StartNew();
    if (map.m_textureSize != TextureSize) {
      map.m_explored = new bool[map.m_textureSize * map.m_textureSize];
      map.m_exploredOthers = new bool[map.m_textureSize * map.m_textureSize];
      map.Start();
      TextureSize = map.m_textureSize;
    }

    int size = map.m_textureSize * map.m_textureSize;
    var mapTexture = new Color32[size];
    var forestMaskTexture = new Color32[size];
    var heightTexture = new Color[size];

    CancellationTokenSource cts = new();
    var ct = cts.Token;

    var task = Generate(map, mapTexture, forestMaskTexture, heightTexture, ct);
    CTS = cts;
    while (!task.IsCompleted)
      yield return null;
    
    if (task.IsFaulted)
      ZLog.LogError($"Map generation failed!\n{task.Exception}");
    else if (!ct.IsCancellationRequested) {
      map.m_mapTexture.SetPixels32(mapTexture);
      yield return null;
      map.m_mapTexture.Apply();
      yield return null;

      map.m_forestMaskTexture.SetPixels32(forestMaskTexture);
      yield return null;
      map.m_forestMaskTexture.Apply();
      yield return null;

      map.m_heightTexture.SetPixels(heightTexture);
      yield return null;
      map.m_heightTexture.Apply();
      yield return null;
      ZLog.Log($"Map generation finished ({stopwatch.Elapsed.TotalSeconds.ToString("F0")} seconds).");
    }
    stopwatch.Stop();
    cts.Dispose();

    if (CTS == cts)
      CTS = null;
  }

  static async Task Generate(
      Minimap map, Color32[] mapTexture, Color32[] forestMaskTexture, Color[] heightTexture, CancellationToken ct) {
    await Task
        .Run(
          () => {
            if (ct.IsCancellationRequested)
              ct.ThrowIfCancellationRequested();

            var wg = WorldGenerator.m_instance;
            int textureSize = map.m_textureSize; // default 2048
            int halfTextureSize = textureSize / 2;
            float pixelSize = map.m_pixelSize;   // default 12
            float halfPixelSize = pixelSize / 2f;

            for (int i = 0; i < textureSize; i++) {
              for (int j = 0; j < textureSize; j++) {
                float wx = (j - halfTextureSize) * pixelSize + halfPixelSize;
                float wy = (i - halfTextureSize) * pixelSize + halfPixelSize;

                Heightmap.Biome biome = wg.GetBiome(wx, wy);
                float biomeHeight = wg.GetBiomeHeight(biome, wx, wy);
                if (biome != Heightmap.Biome.Mountain && biome != Heightmap.Biome.DeepNorth && biomeHeight > 70f)
                  biomeHeight = Mathf.Min(85f, 70f + Mathf.Sqrt(biomeHeight - 70f));

                mapTexture[i * textureSize + j] = GetPixelColor32(biome);
                forestMaskTexture[i * textureSize + j] = GetMaskColor32(wx, wy, biomeHeight, biome);
                heightTexture[i * textureSize + j] = new Color(biomeHeight, 0f, 0f);
                if (ct.IsCancellationRequested) {
                  ct.ThrowIfCancellationRequested();
                }
              }
            }
          })
        .ConfigureAwait(continueOnCapturedContext: false);
  }


  static readonly Color32 AshlandsColor = new Color(0.6903f, 0.192f, 0.192f);
  static readonly Color32 BlackForestColor = new Color(0.4190f, 0.4548f, 0.2467f);
  static readonly Color32 DeepNorthColor = new Color(1f, 1f, 1f);
  static readonly Color32 HeathColor = new Color(0.9062f, 0.6707f, 0.4704f);
  static readonly Color32 MeadowsColor = new Color(0.5725f, 0.6551f, 0.3605f);
  static readonly Color32 MistlandsColor = new Color(0.3254f, 0.3254f, 0.3254f);
  static readonly Color32 MountainColor = new Color(1f, 1f, 1f);
  static readonly Color32 SwampColor = new Color(0.6395f, 0.447f, 0.3449f);
  static readonly Color32 WhiteColor32 = Color.white;

  public static Color32 GetPixelColor32(Heightmap.Biome biome) {
    if (BiomeManager.TryGetData(biome, out var data))
      return data.mapColor;
    return biome switch
    {
      Heightmap.Biome.Meadows => MeadowsColor,
      Heightmap.Biome.Swamp => SwampColor,
      Heightmap.Biome.Mountain => MountainColor,
      Heightmap.Biome.BlackForest => BlackForestColor,
      Heightmap.Biome.Plains => HeathColor,
      Heightmap.Biome.DeepNorth => DeepNorthColor,
      Heightmap.Biome.AshLands => AshlandsColor,
      Heightmap.Biome.Mistlands => MistlandsColor,
      Heightmap.Biome.Ocean => WhiteColor32,
      _ => WhiteColor32,
    };
  }

  static readonly Color32 ForestColor = new Color(1f, 0f, 0f, 0f);
  static readonly Color32 NoForestColor = new Color(0f, 0f, 0f, 0f);

  static Color32 GetMaskColor32(float wx, float wy, float height, Heightmap.Biome biome) {
    if (height < Configuration.WaterLevel) {
      return NoForestColor;
    }

    return biome switch
    {
      Heightmap.Biome.Meadows => GetForestFactor(wx, wy) < 1.15f ? ForestColor : NoForestColor,
      Heightmap.Biome.Plains => GetForestFactor(wx, wy) < 0.8f ? ForestColor : NoForestColor,
      Heightmap.Biome.BlackForest => ForestColor,
      Heightmap.Biome.Mistlands => ForestColor,
      _ => NoForestColor,
    };
  }

  static float GetForestFactor(float vx, float vz) {
    if (Configuration.ForestMultiplier == 0f) return 10f;
    return Fbm(vx * 0.004f, vz * 0.004f, 3, 1.6f, 0.7f) / Configuration.ForestMultiplier;
  }

  static float Fbm(float vx, float vz, int octaves, float lacunarity, float gain) {
    float result = 0f;
    float multiplier = 1f;

    for (int i = 0; i < octaves; i++) {
      result += multiplier * Mathf.PerlinNoise(vx, vz);
      multiplier *= gain;
      vx *= lacunarity;
      vz *= lacunarity;
    }

    return result;
  }
}

[HarmonyPatch(typeof(Game), nameof(Game.FindSpawnPoint))]
public class FindSpawnPoint {
  static bool Prefix() => WorldGeneration.HasLoaded;
}
[HarmonyPatch(typeof(Game), nameof(Game.Logout))]
public class CancelOnLogout {
  static void Prefix() {
    Generate.Cancel();
    WorldGeneration.HasLoaded = false;
  }
}
