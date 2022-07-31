using System;
using System.Reflection;
using BepInEx.Bootstrap;
namespace ExpandWorld;

public class JotunnWrapper {
  public const string GUID = "com.jotunn.jotunn";
  private static void Call(Type type, string name) => type.GetMethod(name, BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[0]);
  public static bool IsCustomlocation(string name) {
    if (Jotunn == null) return false;
    var type = Jotunn.GetType("Jotunn.Entities.CustomLocation");
    if (type == null) return false;
    var method = type.GetMethod("IsCustomLocation", BindingFlags.Static | BindingFlags.Public);
    if (method == null) return false;
    return (bool)method.Invoke(null, new object[] { name });
  }
  private static Assembly? Jotunn;
  public static void Run() {
    if (!Chainloader.PluginInfos.TryGetValue(GUID, out var info)) return;
    Jotunn = info.Instance.GetType().Assembly;
    if (Jotunn == null) return;
    ExpandWorld.Log.LogInfo("\"Jotunn\" detected. Adding compatibility for custom locations.");
  }
}
