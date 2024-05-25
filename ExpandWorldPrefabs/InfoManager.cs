using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Service;

namespace ExpandWorld.Prefab;

public enum ActionType
{
  Create,
  Destroy,
  Repair,
  Damage,
  State,
  Command,
  Say,
  Poke,
  GlobalKey,
  Event
}
public class InfoManager
{
  public static readonly PrefabInfo CreateDatas = new();
  public static readonly PrefabInfo RemoveDatas = new();
  public static readonly PrefabInfo RepairDatas = new();
  public static readonly PrefabInfo DamageDatas = new();
  public static readonly PrefabInfo StateDatas = new();
  public static readonly PrefabInfo CommandDatas = new();
  public static readonly PrefabInfo SayDatas = new();
  public static readonly PrefabInfo PokeDatas = new();
  public static readonly GlobalInfo GlobalKeyDatas = new();
  public static readonly GlobalInfo EventDatas = new();

  public static void Clear()
  {
    CreateDatas.Clear();
    RemoveDatas.Clear();
    RepairDatas.Clear();
    DamageDatas.Clear();
    StateDatas.Clear();
    CommandDatas.Clear();
    SayDatas.Clear();
    PokeDatas.Clear();
    GlobalKeyDatas.Clear();
    EventDatas.Clear();
  }
  public static void Add(Info info)
  {
    if (info.Type == ActionType.GlobalKey)
    {
      GlobalKeyDatas.Add(info);
      return;
    }
    if (info.Type == ActionType.Event)
    {
      EventDatas.Add(info);
      return;
    }
    Select(info.Type).Add(info);
    if (info.Type == ActionType.Say)
      Select(ActionType.Command).Add(info);
  }
  public static void Patch()
  {
    EWP.Harmony.UnpatchSelf();
    EWP.Harmony.PatchAll();
    if (CreateDatas.Exists)
      HandleCreated.Patch(EWP.Harmony);
    if (RemoveDatas.Exists)
      HandleDestroyed.Patch(EWP.Harmony);
    if (RepairDatas.Exists || DamageDatas.Exists || StateDatas.Exists || CommandDatas.Exists || SayDatas.Exists)
      HandleRPC.Patch(EWP.Harmony);
    if (GlobalKeyDatas.Exists)
      HandleGlobalKey.Patch(EWP.Harmony);
    if (EventDatas.Exists)
      HandleEvent.Patch(EWP.Harmony);
  }


  public static PrefabInfo Select(ActionType type) => type switch
  {
    ActionType.Destroy => RemoveDatas,
    ActionType.Repair => RepairDatas,
    ActionType.Damage => DamageDatas,
    ActionType.State => StateDatas,
    ActionType.Command => CommandDatas,
    ActionType.Say => SayDatas,
    ActionType.Poke => PokeDatas,
    ActionType.Create => CreateDatas,
    _ => Error(type),
  };
  private static PrefabInfo Error(ActionType type)
  {
    Log.Error($"Unknown entry type {type}");
    return new();
  }
  public static GlobalInfo SelectGlobal(ActionType type) => type switch
  {
    ActionType.GlobalKey => GlobalKeyDatas,
    ActionType.Event => EventDatas,
    _ => ErrorGlobal(type),
  };
  private static GlobalInfo ErrorGlobal(ActionType type)
  {
    Log.Error($"Unknown entry type {type}");
    return new();
  }

  private static Dictionary<string, int> PrefabCache = [];
  public static IEnumerable<int> GetPrefabs(string prefab)
  {
    var p = prefab.ToLowerInvariant();
    if (PrefabCache.Count == 0)
      PrefabCache = ZNetScene.instance.m_namedPrefabs.ToDictionary(pair => pair.Value.name, pair => pair.Key);
    if (p == "*")
      return PrefabCache.Values;
    if (p[0] == '*' && p[p.Length - 1] == '*')
    {
      p = p.Substring(1, p.Length - 2);
      return PrefabCache.Where(pair => pair.Key.ToLowerInvariant().Contains(p)).Select(pair => pair.Value);
    }
    if (p[0] == '*')
    {
      p = p.Substring(1);
      return PrefabCache.Where(pair => pair.Key.EndsWith(p, StringComparison.OrdinalIgnoreCase)).Select(pair => pair.Value);
    }
    if (p[p.Length - 1] == '*')
    {
      p = p.Substring(0, p.Length - 1);
      return PrefabCache.Where(pair => pair.Key.StartsWith(p, StringComparison.OrdinalIgnoreCase)).Select(pair => pair.Value);
    }
    return [];
  }
}

public class PrefabInfo
{
  public readonly Dictionary<int, List<Info>> Info = [];
  public readonly Dictionary<int, List<Info>> Fallback = [];
  public bool Exists => Info.Count > 0 || Fallback.Count > 0;


  public void Clear()
  {
    Info.Clear();
    Fallback.Clear();
  }
  public void Add(Info info)
  {
    var prefabs = Parse.ToList(info.Prefabs);
    HashSet<int> hashes = [];
    // Resolving dynamic values and caching hashes helps with performance.
    // Downside is that rules must be reloaded manually when changing value groups.
    ParsePrefabs(prefabs, hashes);
    foreach (var hash in hashes)
    {
      if (info.Fallback)
      {
        if (!Fallback.TryGetValue(hash, out var list))
          Fallback[hash] = list = [];
        list.Add(info);
      }
      else
      {
        if (!Info.TryGetValue(hash, out var list))
          Info[hash] = list = [];
        list.Add(info);
      }
    }
  }
  private void ParsePrefabs(List<string> prefabs, HashSet<int> hashes)
  {
    var scene = ZNetScene.instance;
    foreach (var prefab in prefabs)
    {
      if (prefab.Contains("*"))
        hashes.UnionWith(InfoManager.GetPrefabs(prefab));
      else
      {
        var hash = prefab.GetStableHashCode();
        if (scene.m_namedPrefabs.ContainsKey(hash))
          hashes.Add(hash);
        else
        {
          var values = DataHelper.GetValuesFromGroup(prefab);
          if (values != null)
            ParsePrefabs(values, hashes);
          else
            Log.Warning($"Prefab {prefab} not found");
        }
      }
    }
  }
  public bool TryGetValue(int prefab, out List<Info> list) => Info.TryGetValue(prefab, out list);
  public bool TryGetFallbackValue(int prefab, out List<Info> list) => Fallback.TryGetValue(prefab, out list);

}


public class GlobalInfo
{
  public readonly List<Info> Info = [];
  public readonly List<Info> Fallback = [];
  public bool Exists => Info.Count > 0 || Fallback.Count > 0;


  public void Clear()
  {
    Info.Clear();
    Fallback.Clear();
  }
  public void Add(Info info)
  {
    if (info.Fallback)
      Fallback.Add(info);
    else
      Info.Add(info);
  }
}
