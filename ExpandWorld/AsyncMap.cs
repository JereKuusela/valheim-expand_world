
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld;
[HarmonyPatch(typeof(Minimap))]
public class MinimapAsync {
  [HarmonyPatch(nameof(Minimap.GenerateWorldMap)), HarmonyPrefix]
  static bool GenerateWorldMapPrefix(Minimap __instance) {
    _waterLevel = Configuration.WaterLevel;
    __instance.StartCoroutine(GenerateWorldMapCoroutine(__instance));
    return false;
  }
  static int _lastTaskIndex = 0;
  public static void Cancel(string source) {
    if (_lastCancellationTokenSource != null) {
      ZLog.Log($"{source}: Cancelling already running GenerateWorldMapAsync task...");
      _lastCancellationTokenSource.Cancel();
      _lastCancellationTokenSource = null;
    }
  }
  public static bool Generating => _lastCancellationTokenSource != null;
  static CancellationTokenSource? _lastCancellationTokenSource = null;
  static IEnumerator GenerateWorldMapCoroutine(Minimap minimap) {
    int taskIndex = _lastTaskIndex++;
    Cancel($"Task {taskIndex}");

    ZLog.Log($"Starting GenerateWorldMapAsync...");
    Stopwatch stopwatch = Stopwatch.StartNew();
    var obj = minimap;
    if (SetMapMode.TextureSizeChanged) {
      obj.m_explored = new bool[obj.m_textureSize * obj.m_textureSize];
      obj.m_exploredOthers = new bool[obj.m_textureSize * obj.m_textureSize];
      obj.Start();
    }
    // Some water stuff probably.
    SetupMaterial.Refresh();
    SetMapMode.TextureSizeChanged = false;
    SetMapMode.ForceRegen = false;


    int size = minimap.m_textureSize * minimap.m_textureSize;
    ZLog.Log($"Task {taskIndex}: Populating texture arrays of size: {size:G}");

    Color32[] mapTexture = new Color32[size];
    Color32[] forestMaskTexture = new Color32[size];
    Color[] heightTexture = new Color[size];

    CancellationTokenSource cancellationTokenSource = new();
    var cancellationToken = cancellationTokenSource.Token;

    Task task = GenerateWorldMapAsync(minimap, mapTexture, forestMaskTexture, heightTexture, cancellationToken);
    _lastCancellationTokenSource = cancellationTokenSource;
    while (!task.IsCompleted) {
      yield return null;
    }

    if (task.IsFaulted) {
      ZLog.LogError($"Task {taskIndex}: Failed to generate world map!\n{task.Exception}");
    } else if (cancellationToken.IsCancellationRequested) {
      ZLog.LogWarning($"Task {taskIndex}: Generate world map cancelled.");
    } else {
      minimap.m_mapTexture.SetPixels32(mapTexture);
      yield return null;
      minimap.m_mapTexture.Apply();
      yield return null;

      minimap.m_forestMaskTexture.SetPixels32(forestMaskTexture);
      yield return null;
      minimap.m_forestMaskTexture.Apply();
      yield return null;

      minimap.m_heightTexture.SetPixels(heightTexture);
      yield return null;
      minimap.m_heightTexture.Apply();
      yield return null;
    }
    stopwatch.Stop();
    ZLog.Log($"Task {taskIndex}: Finished GenerateWorldMapAsync in: {stopwatch.Elapsed:G}");
    cancellationTokenSource.Dispose();

    if (_lastCancellationTokenSource == cancellationTokenSource) {
      _lastCancellationTokenSource = null;
    }
  }

  static async Task GenerateWorldMapAsync(
      Minimap minimap, Color32[] mapTexture, Color32[] forestMaskTexture, Color[] heightTexture, CancellationToken cancellationToken) {
    await Task
        .Run(
          () => {
            if (cancellationToken.IsCancellationRequested)
              cancellationToken.ThrowIfCancellationRequested();

            WorldGenerator generator = WorldGenerator.m_instance;

            int textureSize = minimap.m_textureSize; // 2048
            int halfTextureSize = textureSize / 2;
            float pixelSize = minimap.m_pixelSize;   // 12
            float halfPixelSize = pixelSize / 2f;

            for (int i = 0; i < textureSize; i++) {
              for (int j = 0; j < textureSize; j++) {
                float wx = (j - halfTextureSize) * pixelSize + halfPixelSize;
                float wy = (i - halfTextureSize) * pixelSize + halfPixelSize;

                Heightmap.Biome biome = generator.GetBiome(wx, wy);
                float biomeHeight = generator.GetBiomeHeight(biome, wx, wy);
                if (biome != Heightmap.Biome.Mountain && biome != Heightmap.Biome.DeepNorth && biomeHeight > 70f)
                  biomeHeight = Mathf.Min(85f, 70f + Mathf.Sqrt(biomeHeight - 70f));

                mapTexture[i * textureSize + j] = GetPixelColor32(biome);
                forestMaskTexture[i * textureSize + j] = GetMaskColor32(wx, wy, biomeHeight, biome);
                heightTexture[i * textureSize + j] = new Color(biomeHeight, 0f, 0f);
                if (cancellationToken.IsCancellationRequested) {
                  cancellationToken.ThrowIfCancellationRequested();
                }
              }
            }
          })
        .ConfigureAwait(continueOnCapturedContext: false);
  }

  static float _waterLevel = 30f;

  static readonly Color32 AshlandsColor = new Color(0.6903f, 0.192f, 0.192f);
  static readonly Color32 BlackForestColor = new Color(0.4190f, 0.4548f, 0.2467f);
  static readonly Color32 DeepNorthColor = new Color(1f, 1f, 1f);
  static readonly Color32 HeathColor = new Color(0.9062f, 0.6707f, 0.4704f);
  static readonly Color32 MeadowsColor = new Color(0.5725f, 0.6551f, 0.3605f);
  static readonly Color32 MistlandsColor = new Color(0.3254f, 0.3254f, 0.3254f);
  static readonly Color32 MountainColor = new Color(1f, 1f, 1f);
  static readonly Color32 SwampColor = new Color(0.6395f, 0.447f, 0.3449f);
  static readonly Color32 WhiteColor32 = Color.white;

  static Color32 GetPixelColor32(Heightmap.Biome biome) {
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
    if (height < _waterLevel) {
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

[HarmonyPatch(typeof(Game), nameof(Game.Logout))]
public class CancelOnLogout {
  static void Prefix() {
    MinimapAsync.Cancel("Logout");
  }
}
