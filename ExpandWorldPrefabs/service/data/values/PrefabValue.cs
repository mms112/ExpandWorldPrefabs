
using System.Collections.Generic;
using Service;
using UnityEngine;

namespace Data;

public class PrefabValue(string[] values) : AnyValue(values), IPrefabValue
{
  // Caching makes sense because parameters and wildcards makes it slow.
  // Also prefab is often checked many times.
  private List<int>? Cache;
  private Parameters? LastParameters;
  public int? Get(Parameters pars)
  {
    if (pars != LastParameters)
    {
      var values = GetAllValues(pars);
      Cache = PrefabHelper.GetPrefabs(values);
      LastParameters = pars;
    }
    if (Cache == null || Cache.Count == 0) return null;
    if (Cache.Count == 1) return Cache[0];
    return Cache[Random.Range(0, Cache.Count)];
  }

  public bool? Match(Parameters pars, int value)
  {
    if (pars != LastParameters)
    {
      var values = GetAllValues(pars);
      Cache = PrefabHelper.GetPrefabs(values);
      LastParameters = pars;
    }
    if (Cache == null || Cache.Count == 0) return null;
    return Cache.Contains(value);
  }
}
public class SimpleWildPrefabValue : IPrefabValue
{
  private readonly List<int> Values;
  public SimpleWildPrefabValue(string value)
  {
    Values = PrefabHelper.GetPrefabs(value);
    if (Values.Count == 0)
      Log.Warning($"No prefabs found for {value}");
  }

  public int? Get(Parameters pars) => RollValue();
  public bool? Match(Parameters pars, int value) => Values.Contains(value);
  private int RollValue()
  {
    if (Values.Count == 1)
      return Values[0];
    return Values[Random.Range(0, Values.Count)];
  }
}
public class SimplePrefabValue : IPrefabValue
{
  private readonly int Value;
  public SimplePrefabValue(string value)
  {
    Value = value.GetStableHashCode();
    if (ZNetScene.instance.m_namedPrefabs.ContainsKey(Value))
      return;
    Value = 0;
    Log.Warning($"Prefab {value} not found");
  }

  public int? Get(Parameters pars) => Value;
  public bool? Match(Parameters pars, int value) => Value == value;
}
public interface IPrefabValue
{
  int? Get(Parameters pars);
  bool? Match(Parameters pars, int value);
}