using System.Collections.Generic;
using ExpandWorld;
using HarmonyLib;
using UnityEngine;

namespace ServerSync;
public class VersionCheck {
  public string Name;
  public string DisplayName;
  public string CurrentVersion;
  // Tracks which clients have passed the version check (only for servers).
  private readonly List<ZRpc> ValidatedClients = new();

  private ConfigSync ConfigSync;
  public VersionCheck(ConfigSync configSync) {
    ConfigSync = configSync;
    Name = ConfigSync.Name;
    DisplayName = ConfigSync.DisplayName;
    CurrentVersion = ConfigSync.CurrentVersion;
    VersionCheckPatches.VersionCheck = this;
  }
  private string? ReceivedVersion = null;

  public bool IsVersionOk() => CurrentVersion == ReceivedVersion;

  public string ErrorMessage(ZRpc? rpc = null) {
    if (rpc != null)
      return $"Disconnect: The client ({rpc.m_socket.GetHostName()}) doesn't have the correct {DisplayName} version {ReceivedVersion}";
    if (ReceivedVersion == null)
      return $"Mod {DisplayName} must be installed on the server (without Server only mode).";
    return $"Mod {DisplayName} requires version {ReceivedVersion}. Installed is version {CurrentVersion}.";
  }
  private static void Logout() {
    Game.instance.Logout();
    AccessTools.DeclaredField(typeof(ZNet), "m_connectionStatus").SetValue(null, ZNet.ConnectionStatus.ErrorVersion);
  }
  private static void DisconnectClient(ZRpc rpc) => rpc.Invoke("Error", new object[] { (int)ZNet.ConnectionStatus.ErrorVersion });
  public void CheckVersion(ZRpc rpc, ZPackage pkg) {
    var currentVersion = pkg.ReadString();
    var target = ZNet.instance.IsServer() ? "client" : "server";
    Debug.Log($"Received {DisplayName} version {currentVersion} from the ${target}.");
    ReceivedVersion = currentVersion;
    if (ZNet.instance.IsServer() && IsVersionOk())
      ValidatedClients.Add(rpc);
  }
  public bool IsServerValid(ZNet znet, ZRpc rpc) {
    if (znet.IsServer()) return true;
    if (IsVersionOk()) return true;
    Debug.LogWarning(ErrorMessage());
    Logout();
    return false;
  }
  public bool IsClientValid(ZNet znet, ZRpc rpc) {
    if (!znet.IsServer()) return true;
    if (ValidatedClients.Contains(rpc)) return true;
    Debug.LogWarning(ErrorMessage(rpc));
    DisconnectClient(rpc);
    return false;
  }

  public void Initialize(ZNetPeer peer) {
    ReceivedVersion = null;
    peer.m_rpc.Register<ZPackage>($"VersionCheck_{Name}", CheckVersion);
    var target = ZNet.instance.IsServer() ? "client" : "server";
    Debug.Log($"Sending {DisplayName} version {CurrentVersion} to the ${target}.");
    ZPackage zpackage = new ZPackage();
    zpackage.Write(CurrentVersion);
    peer.m_rpc.Invoke($"VersionCheck_{Name}", new object[] { zpackage });
  }
  public void Disconnect(ZRpc rpc) => ValidatedClients.Remove(rpc);
}

[HarmonyPatch]
public class VersionCheckPatches {
  public static VersionCheck? VersionCheck;
  [HarmonyPatch(typeof(ZNet), nameof(ZNet.RPC_PeerInfo)), HarmonyPrefix]
  private static bool RPC_PeerInfo(ZRpc rpc, ZNet __instance) {
    if (VersionCheck == null) return true;
    return VersionCheck.IsServerValid(__instance, rpc) && VersionCheck.IsClientValid(__instance, rpc);
  }

  [HarmonyPatch(typeof(ZNet), nameof(ZNet.OnNewConnection)), HarmonyPrefix]
  private static void RegisterAndCheckVersion(ZNetPeer peer, ZNet __instance) {
    // No need to do anything. Client will fail the check.
    if (ZNet.instance.IsServer() && Configuration.ServerOnly) return;
    VersionCheck?.Initialize(peer);
  }

  [HarmonyPatch(typeof(ZNet), nameof(ZNet.Disconnect)), HarmonyPrefix]
  private static void RemoveDisconnected(ZNetPeer peer, ZNet __instance) {
    if (!__instance.IsServer()) return;
    VersionCheck?.Disconnect(peer.m_rpc);
  }

  [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.ShowConnectError)), HarmonyPostfix]
  private static void ShowConnectionError(FejdStartup __instance) {
    if (!__instance.m_connectionFailedPanel.activeSelf) return;
    if (VersionCheck == null) return;
    if (VersionCheck.IsVersionOk()) return;
    __instance.m_connectionFailedError.text += "\n" + VersionCheck.ErrorMessage();
  }
}