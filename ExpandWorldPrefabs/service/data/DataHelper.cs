using System;
using System.Collections.Generic;
using System.Linq;
using Service;
using UnityEngine;

namespace Data;

public class DataHelper
{
  public static DataEntry? Merge(params DataEntry?[] datas)
  {
    var nonNull = datas.Where(d => d != null).ToArray();
    if (nonNull.Length == 0) return null;
    if (nonNull.Length == 1) return nonNull[0];
    DataEntry result = new();
    foreach (var data in nonNull)
      result.Load(data!);
    return result;
  }
  public static bool Exists(int hash) => DataLoading.Data.ContainsKey(hash);

  public static bool Match(int hash, ZDO zdo, Parameters pars)
  {
    if (DataLoading.Data.TryGetValue(hash, out var data))
    {
      return data.Match(pars, zdo);
    }
    return false;
  }
  public static DataEntry? Get(string name) => name == "" ? null : DataLoading.Get(name);

  public static List<string>? GetValuesFromGroup(string group)
  {
    var hash = group.ToLowerInvariant().GetStableHashCode();
    if (DataLoading.ValueGroups.TryGetValue(hash, out var values))
      return values;
    return null;
  }
  // This is mainly used to simplify code.
  // Not very efficient because usually only a single prefab is used.
  // So only use when the result is cached.
  public static List<string> ResolvePrefabs(string values)
  {
    HashSet<string> prefabs = [];
    ResolvePrefabsSub(prefabs, values);
    return [.. prefabs];
  }
  private static void ResolvePrefabsSub(HashSet<string> prefabs, string value)
  {
    if (value == "all" || ZNetScene.instance.m_namedPrefabs.ContainsKey(value.GetStableHashCode()))
    {
      prefabs.Add(value);
      return;
    }
    var values = GetValuesFromGroup(value);
    if (values != null)
    {
      foreach (var v in values)
        ResolvePrefabsSub(prefabs, v);
      return;
    }
    Log.Warning($"Failed to resolve prefab: {value}");
  }

  public static string GetGlobalKey(string key)
  {
    var lower = key.ToLowerInvariant();
    return ZoneSystem.instance.m_globalKeysValues.FirstOrDefault(kvp => kvp.Key.ToLowerInvariant() == lower).Value ?? "0";
  }

  // Parameter value could be a value group, so that has to be resolved.
  public static string ResolveValue(string value)
  {
    if (!value.StartsWith("<", StringComparison.OrdinalIgnoreCase)) return value;
    if (!value.EndsWith(">", StringComparison.OrdinalIgnoreCase)) return value;
    var sub = value.Substring(1, value.Length - 2);
    if (TryGetValueFromGroup(sub, out var valueFromGroup))
      return valueFromGroup;
    return value;
  }

  public static bool TryGetValueFromGroup(string group, out string value)
  {
    var hash = group.ToLowerInvariant().GetStableHashCode();
    if (!DataLoading.ValueGroups.ContainsKey(hash))
    {
      value = group;
      return false;
    }
    var roll = UnityEngine.Random.Range(0, DataLoading.ValueGroups[hash].Count);
    // Value from group could be another group, so yet another resolve is needed.
    value = ResolveValue(DataLoading.ValueGroups[hash][roll]);
    return true;
  }
}