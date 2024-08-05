
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data;

public class PrefabHelper
{

  public static int? GetPrefab(string value)
  {
    var prefabs = GetPrefabs(value);
    if (prefabs.Count == 0) return null;
    if (prefabs.Count == 1) return prefabs[0];
    return prefabs[UnityEngine.Random.Range(0, prefabs.Count)];
  }
  private static readonly Dictionary<string, List<int>> Prefabs = [];
  public static List<int> GetPrefabs(string value)
  {
    if (Prefabs.ContainsKey(value)) return Prefabs[value];

    var prefabs = ParsePrefabs(value).ToList();
    Prefabs[value] = prefabs;
    return prefabs;
  }
  public static List<int>? GetPrefabs(string[] values)
  {
    if (values.Length == 0) return null;
    if (values.Length == 1) return GetPrefabs(values[0]);
    var value = values.Select(GetPrefabs).Aggregate((a, b) => a.Intersect(b).ToList());
    return value.Count == 0 ? null : value;
  }

  private static Dictionary<string, int> PrefabCache = [];
  private static IEnumerable<int> ParsePrefabs(string prefab)
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
    if (PrefabCache.ContainsKey(p))
      return [PrefabCache[p]];
    return PrefabCache.Where(pair => pair.Key.ToLowerInvariant().Contains(p)).Select(pair => pair.Value);
  }
}