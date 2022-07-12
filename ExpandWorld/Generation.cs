
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
    if (__instance.m_world.m_menu) return true;
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
    if (Minimap.instance && ZNet.instance && !ZNet.instance.IsDedicated())
      Minimap.instance.GenerateWorldMap();
  }
}


[HarmonyPatch(typeof(Minimap), nameof(Minimap.GenerateWorldMap))]
public class MapGeneration {
  // Some map mods may do stuff after generation which won't work with async.
  // So do one "fake" generate call to trigger those.
  static bool DoFakeGenerate = false;
  static bool Prefix(Minimap __instance) {
    if (DoFakeGenerate) {
      DoFakeGenerate = false;
      return false;
    }
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
  public static void UpdateTextureSize(Minimap map, int textureSize) {
    if (map.m_textureSize == textureSize) return;
    map.m_textureSize = textureSize;
    map.m_mapTexture = new Texture2D(map.m_textureSize, map.m_textureSize, TextureFormat.RGBA32, false);
    map.m_mapTexture.wrapMode = TextureWrapMode.Clamp;
    map.m_forestMaskTexture = new Texture2D(map.m_textureSize, map.m_textureSize, TextureFormat.RGBA32, false);
    map.m_forestMaskTexture.wrapMode = TextureWrapMode.Clamp;
    map.m_heightTexture = new Texture2D(map.m_textureSize, map.m_textureSize, TextureFormat.RFloat, false);
    map.m_heightTexture.wrapMode = TextureWrapMode.Clamp;
    map.m_fogTexture = new Texture2D(map.m_textureSize, map.m_textureSize, TextureFormat.RGBA32, false);
    map.m_fogTexture.wrapMode = TextureWrapMode.Clamp;
    map.m_explored = new bool[map.m_textureSize * map.m_textureSize];
    map.m_exploredOthers = new bool[map.m_textureSize * map.m_textureSize];
    map.m_mapImageLarge.material.SetTexture("_MainTex", map.m_mapTexture);
    map.m_mapImageLarge.material.SetTexture("_MaskTex", map.m_forestMaskTexture);
    map.m_mapImageLarge.material.SetTexture("_HeightTex", map.m_heightTexture);
    map.m_mapImageLarge.material.SetTexture("_FogTex", map.m_fogTexture);
    map.m_mapImageSmall.material.SetTexture("_MainTex", map.m_mapTexture);
    map.m_mapImageSmall.material.SetTexture("_MaskTex", map.m_forestMaskTexture);
    map.m_mapImageSmall.material.SetTexture("_HeightTex", map.m_heightTexture);
    map.m_mapImageSmall.material.SetTexture("_FogTex", map.m_fogTexture);
    map.Reset();
  }
  public static bool Generating => CTS != null;
  static CancellationTokenSource? CTS = null;
  static IEnumerator Coroutine(Minimap map) {
    Cancel();

    ZLog.Log($"Starting map generation.");
    Stopwatch stopwatch = Stopwatch.StartNew();

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
      // Some map mods may do stuff after generation which won't work with async.
      // So do one "fake" generate call to trigger those.
      DoFakeGenerate = true;
      map.GenerateWorldMap();
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
            var textureSize = map.m_textureSize; // default 2048
            var halfTextureSize = textureSize / 2;
            var pixelSize = map.m_pixelSize;   // default 12
            var halfPixelSize = pixelSize / 2f;

            for (var i = 0; i < textureSize; i++) {
              for (var j = 0; j < textureSize; j++) {
                var wx = (j - halfTextureSize) * pixelSize + halfPixelSize;
                var wy = (i - halfTextureSize) * pixelSize + halfPixelSize;

                var biome = wg.GetBiome(wx, wy);
                var terrain = BiomeManager.GetTerrain(biome);
                var biomeHeight = wg.GetBiomeHeight(biome, wx, wy);
                if (terrain != Heightmap.Biome.Mountain && terrain != Heightmap.Biome.DeepNorth && biomeHeight > 70f)
                  biomeHeight = Mathf.Min(85f, 70f + Mathf.Sqrt(biomeHeight - 70f));

                mapTexture[i * textureSize + j] = GetPixelColor32(biome);
                forestMaskTexture[i * textureSize + j] = GetMaskColor32(wx, wy, biomeHeight, biome, terrain);
                heightTexture[i * textureSize + j] = new Color(biomeHeight, 0f, 0f);
                if (ct.IsCancellationRequested)
                  ct.ThrowIfCancellationRequested();
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

  static Color32 GetMaskColor32(float wx, float wy, float height, Heightmap.Biome biome, Heightmap.Biome terrain) {
    if (height < Configuration.WaterLevel) {
      return NoForestColor;
    }
    
    return terrain switch
    {
      Heightmap.Biome.Meadows => GetForestFactor(biome, wx, wy) < 1.15f ? ForestColor : NoForestColor,
      Heightmap.Biome.Plains => GetForestFactor(biome, wx, wy) < 0.8f ? ForestColor : NoForestColor,
      Heightmap.Biome.BlackForest => ForestColor,
      Heightmap.Biome.Mistlands => ForestColor,
      _ => NoForestColor,
    };
  }

  static float GetForestFactor(Heightmap.Biome biome, float vx, float vz) {
    var multiplier = Configuration.ForestMultiplier;
    if (BiomeManager.TryGetData(biome, out var data)) {
      multiplier *= data.forestMultiplier;
    }
    if (multiplier == 0f) return 10f;
    return Fbm(vx * 0.004f, vz * 0.004f, 3, 1.6f, 0.7f) / multiplier;
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
