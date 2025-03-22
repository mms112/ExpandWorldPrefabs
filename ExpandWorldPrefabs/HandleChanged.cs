using System.Collections.Generic;
using System.Globalization;
using Data;
using HarmonyLib;
using Service;

namespace ExpandWorld.Prefab;

public class HandleChanged
{
  public static void Patch(Harmony harmony, PrefabInfo changeDatas)
  {
    var method = AccessTools.Method(typeof(ZDOExtraData), nameof(ZDOExtraData.Set), [typeof(ZDOID), typeof(int), typeof(int)]);
    var patch = AccessTools.Method(typeof(HandleChanged), nameof(HandleInt));
    harmony.Patch(method, prefix: new HarmonyMethod(patch));
    method = AccessTools.Method(typeof(ZDOExtraData), nameof(ZDOExtraData.Set), [typeof(ZDOID), typeof(int), typeof(float)]);
    patch = AccessTools.Method(typeof(HandleChanged), nameof(HandleFloat));
    harmony.Patch(method, prefix: new HarmonyMethod(patch));
    method = AccessTools.Method(typeof(ZDOExtraData), nameof(ZDOExtraData.Set), [typeof(ZDOID), typeof(int), typeof(string)]);
    patch = AccessTools.Method(typeof(HandleChanged), nameof(HandleString));
    harmony.Patch(method, prefix: new HarmonyMethod(patch));
    method = AccessTools.Method(typeof(ZDOExtraData), nameof(ZDOExtraData.Set), [typeof(ZDOID), typeof(int), typeof(long)]);
    patch = AccessTools.Method(typeof(HandleChanged), nameof(HandleLong));
    harmony.Patch(method, prefix: new HarmonyMethod(patch));

    TrackedHashes.Clear();
    AddTracks(changeDatas.Info);
    AddTracks(changeDatas.Fallback);
  }

  private static void AddTracks(Dictionary<int, List<Info>> datas)
  {
    foreach (var kvp in datas)
    {
      var prefab = kvp.Key;
      foreach (var info in kvp.Value)
      {
        if (info.Args.Length == 0) continue;
        var hash = ZdoHelper.Hash(info.Args[0]);
        if (!TrackedHashes.ContainsKey(hash)) TrackedHashes[hash] = [];
        TrackedHashes[hash].Add(prefab);
      }
    }
  }

  private static readonly List<ChangedZdo> ChangedZDOs = [];

  public static void Execute()
  {
    foreach (var changed in ChangedZDOs)
    {
      Manager.Handle(ActionType.Change, changed.Key + " " + changed.Value, changed.Zdo);
    }
    ChangedZDOs.Clear();
  }
  private static readonly Dictionary<int, HashSet<int>> TrackedHashes = [];
  private static void HandleInt(ZDOID zid, int hash, int value)
  {
    if (!TrackedHashes.TryGetValue(hash, out var tracked)) return;
    if (!ZDOMan.instance.m_objectsByID.TryGetValue(zid, out var zdo)) return;
    if (!tracked.Contains(zdo.m_prefab)) return;
    var prev = zdo.GetInt(hash);
    if (prev == value) return;
    ChangedZDOs.Add(new(zdo, ZdoHelper.ReverseHash(hash), value.ToString()));
    ChangedZDOs.Add(new(zdo, ZdoHelper.ReverseHash(hash), value != 0 ? "true" : "false"));
    var prefab = ZNetScene.instance.GetPrefab(value);
    if (prefab)
      ChangedZDOs.Add(new(zdo, ZdoHelper.ReverseHash(hash), prefab.name));
  }
  private static void HandleFloat(ZDOID zid, int hash, float value)
  {
    if (!TrackedHashes.TryGetValue(hash, out var tracked)) return;
    if (!ZDOMan.instance.m_objectsByID.TryGetValue(zid, out var zdo)) return;
    if (!tracked.Contains(zdo.m_prefab)) return;
    var prev = zdo.GetFloat(hash);
    if (prev == value) return;
    ChangedZDOs.Add(new(zdo, ZdoHelper.ReverseHash(hash), value.ToString(NumberFormatInfo.InvariantInfo)));
  }
  private static void HandleString(ZDOID zid, int hash, string value)
  {
    if (!TrackedHashes.TryGetValue(hash, out var tracked)) return;
    if (!ZDOMan.instance.m_objectsByID.TryGetValue(zid, out var zdo)) return;
    if (!tracked.Contains(zdo.m_prefab)) return;
    var prev = zdo.GetString(hash);
    if (prev == value) return;
    ChangedZDOs.Add(new(zdo, ZdoHelper.ReverseHash(hash), value == "" ? "none" : value));
  }
  private static void HandleLong(ZDOID zid, int hash, long value)
  {
    if (!TrackedHashes.TryGetValue(hash, out var tracked)) return;
    if (!ZDOMan.instance.m_objectsByID.TryGetValue(zid, out var zdo)) return;
    if (!tracked.Contains(zdo.m_prefab)) return;
    var prev = zdo.GetLong(hash);
    if (prev == value) return;
    ChangedZDOs.Add(new(zdo, ZdoHelper.ReverseHash(hash), value.ToString()));
  }
}

public class ChangedZdo(ZDO zdo, string key, string value)
{
  public ZDO Zdo = zdo;
  public string Key = key;
  public string Value = value;
}