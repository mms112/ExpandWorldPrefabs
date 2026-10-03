using HarmonyLib;
using Service;

namespace ExpandWorld.Prefab;

public class HandleRpcFailure
{
  private static bool IsPatched = false;

  public static void Patch(Harmony harmony, bool shouldPatch)
  {
    if (shouldPatch && !IsPatched)
      DoPatch(harmony);
    if (!shouldPatch && IsPatched)
      DoUnpatch(harmony);
  }

  private static void DoPatch(Harmony harmony)
  {
    IsPatched = true;
    var objectRpc = AccessTools.Method(typeof(ZNetView), nameof(ZNetView.HandleRoutedRPC));
    var objectPrefix = AccessTools.Method(typeof(HandleRpcFailure), nameof(ObjectRpcPrefix));
    harmony.Patch(objectRpc, prefix: new HarmonyMethod(objectPrefix));

    var clientRpc = AccessTools.Method(typeof(ZRoutedRpc), nameof(ZRoutedRpc.HandleRoutedRPC));
    var clientPostfix = AccessTools.Method(typeof(HandleRpcFailure), nameof(ClientRpcPostfix));
    harmony.Patch(clientRpc, postfix: new HarmonyMethod(clientPostfix));
  }

  private static void DoUnpatch(Harmony harmony)
  {
    IsPatched = false;
    var objectRpc = AccessTools.Method(typeof(ZNetView), nameof(ZNetView.HandleRoutedRPC));
    var objectPrefix = AccessTools.Method(typeof(HandleRpcFailure), nameof(ObjectRpcPrefix));
    harmony.Unpatch(objectRpc, objectPrefix);

    var clientRpc = AccessTools.Method(typeof(ZRoutedRpc), nameof(ZRoutedRpc.HandleRoutedRPC));
    var clientPostfix = AccessTools.Method(typeof(HandleRpcFailure), nameof(ClientRpcPostfix));
    harmony.Unpatch(clientRpc, clientPostfix);
  }

  private static bool ObjectRpcPrefix(ZNetView __instance, ZRoutedRpc.RoutedRPCData rpcData)
  {
    if (__instance.m_functions.ContainsKey(rpcData.m_methodHash)) return true;
    LogFailure(rpcData.m_methodHash);
    return false;
  }

  private static void ClientRpcPostfix(ZRoutedRpc __instance, ZRoutedRpc.RoutedRPCData data)
  {
    if (!data.m_targetZDO.IsNone()) return;
    if (__instance.m_functions.ContainsKey(data.m_methodHash)) return;
    if (!RpcInfo.TryGetName(data.m_methodHash, out var name)) return;
    Log.Warning($"Failed to find rpc method {data.m_methodHash} ('{name}')");
  }

  private static void LogFailure(int hash)
  {
    if (RpcInfo.TryGetName(hash, out var name))
      Log.Warning($"Failed to find rpc method {hash} ('{name}')");
    else
      Log.Warning($"Failed to find rpc method {hash}");
  }
}
