using System.Collections.Generic;
using HarmonyLib;

namespace ExpandWorld.Prefab;

[HarmonyPatch(typeof(ZDOExtraData), nameof(ZDOExtraData.SetConnection))]
public class PrefabConnector
{
  private static Dictionary<ZDOID, ZDOID> SwappedZDOs = [];
  private static Dictionary<ZDOID, ZDOID> ReverseConnectionTable = [];

  public static void AddSwap(ZDOID from, ZDOID to)
  {
    SwappedZDOs[from] = to;
    // If the swapped ZDO is connected, update the connection.
    if (ZDOExtraData.s_connections.TryGetValue(from, out var conn))
    {
      ZDOExtraData.s_connections.Remove(from);
      ZDOExtraData.s_connections[to] = conn;
      // No need to use RPC here because the new ZDO was just created.
    }
    // If some other ZDO is connected to the swapped ZDO, update the connection.
    if (ReverseConnectionTable.TryGetValue(from, out var target))
    {
      if (ZDOExtraData.s_connections.TryGetValue(target, out var otherConn))
      {
        ZDOExtraData.s_connections[target] = new(otherConn.m_type, to);
        var zdo = ZDOMan.instance.GetZDO(target);
        // This should guarantee that the change gets through even when not the owner.
        zdo.DataRevision += 100;
        ZDOMan.instance.ForceSendZDO(target);
      }
    }
  }
  // The idea is that if something tries to connect to a swapped ZDO, it will instead connect to the new ZDO.
  static void Prefix(ZDOID zid, ref ZDOID target)
  {
    if (SwappedZDOs.TryGetValue(target, out var newZid))
    {
      target = newZid;
      var zdo = ZDOMan.instance.GetZDO(zid);
      // This should guarantee that the change gets through even when not the owner.
      zdo.DataRevision += 100;
      ZDOMan.instance.ForceSendZDO(zid);
    }
    ReverseConnectionTable[target] = zid;
  }
}