using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace ExpandWorld;

public class Marketplace
{
  public const string GUID = "MarketplaceAndServerNPCs";
  private static Assembly? MarketplaceAssembly;
  private static FieldInfo? IsLoadingField;
  public static void Run()
  {
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var info)) return;
    MarketplaceAssembly = info.Instance.GetType().Assembly;
    var type = MarketplaceAssembly.GetType("API.ClientSide");
    if (type == null) return;
    IsLoadingField = AccessTools.Field(type, "FillingTerritoryData");
    if (IsLoadingField == null) return;
    ExpandWorld.Log.LogInfo("\"Marketplace\" detected. Applying compatibility.");
  }

  public static bool IsLoading()
  {
    if (IsLoadingField == null) return false;
    return (bool)IsLoadingField.GetValue(null);
  }
}
