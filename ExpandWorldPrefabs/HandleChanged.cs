using System.Collections.Generic;
using HarmonyLib;

namespace ExpandWorld.Prefab;

public class HandleChanged
{
  public static void Patch(Harmony harmony)
  {
    var method = AccessTools.Method(typeof(ZDO), nameof(ZDO.Deserialize));
    var patch = AccessTools.Method(typeof(HandleChanged), nameof(Handle));
    harmony.Patch(method, postfix: new HarmonyMethod(patch));
    method = AccessTools.Method(typeof(ZDO), nameof(ZDO.IncreaseDataRevision));
    patch = AccessTools.Method(typeof(HandleChanged), nameof(Handle));
    harmony.Patch(method, postfix: new HarmonyMethod(patch));
  }


  private static readonly HashSet<ZDO> ChangedZDOs = [];

  public static void Execute()
  {
    foreach (var zdo in ChangedZDOs)
    {
      Manager.Handle(ActionType.Change, "", zdo);
    }
    ChangedZDOs.Clear();
  }
  private static void Handle(ZDO __instance)
  {
    ChangedZDOs.Add(__instance);
  }
}
