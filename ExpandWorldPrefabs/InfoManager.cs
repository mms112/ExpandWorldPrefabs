using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Service;
using UnityEngine;

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
  Event,
  Change,
  Key
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
  public static readonly GlobalInfo KeyDatas = new();
  public static readonly GlobalInfo EventDatas = new();
  public static readonly PrefabInfo ChangeDatas = new();

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
    KeyDatas.Clear();
    EventDatas.Clear();
    ChangeDatas.Clear();
  }
  public static void Add(Info info)
  {
    if (info.Type == ActionType.GlobalKey)
    {
      GlobalKeyDatas.Add(info);
      return;
    }
    if (info.Type == ActionType.Key)
    {
      KeyDatas.Add(info);
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
    if (Helper.IsClient())
      return;
    EWP.Harmony.PatchAll();
    if (CreateDatas.Exists)
      HandleCreated.Patch(EWP.Harmony);
    if (RemoveDatas.Exists)
      HandleDestroyed.Patch(EWP.Harmony);
    if (RepairDatas.Exists || DamageDatas.Exists || StateDatas.Exists || CommandDatas.Exists || SayDatas.Exists)
      HandleRPC.Patch(EWP.Harmony);
    if (CommandDatas.Exists || SayDatas.Exists)
      ServerClient.Patch(EWP.Harmony);
    if (GlobalKeyDatas.Exists)
      HandleGlobalKey.Patch(EWP.Harmony);
    if (EventDatas.Exists)
      HandleEvent.Patch(EWP.Harmony);
    if (ChangeDatas.Exists)
      HandleChanged.Patch(EWP.Harmony);
    DataStorage.OnSet = KeyDatas.Exists ? OnKeySet : null;
  }

  private static void OnKeySet(string key, string value)
  {
    Manager.HandleGlobal(ActionType.Key, key + " " + value, Vector3.zero, value == "");
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
    ActionType.Change => ChangeDatas,
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
    ActionType.Key => KeyDatas,
    ActionType.Event => EventDatas,
    _ => ErrorGlobal(type),
  };
  private static GlobalInfo ErrorGlobal(ActionType type)
  {
    Log.Error($"Unknown entry type {type}");
    return new();
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
    var prefabs = PrefabHelper.GetPrefabs(info.Prefabs).ToList();
    foreach (var hash in prefabs)
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
