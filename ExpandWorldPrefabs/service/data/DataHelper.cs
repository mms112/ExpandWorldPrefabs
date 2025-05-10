using System.Collections.Generic;
using System.Linq;
using Service;

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
  public static DataEntry? Get(IStringValue? name, Parameters parameters)
  {
    if (name == null) return null;
    var dataStr = name.GetWhole(parameters);
    if (dataStr == null) return null;
    var hash = dataStr.GetStableHashCode();
    // Usually there is a single data entry, so makes sense to check the cache first.
    // This also works for the type, key, value format.
    if (DataLoading.TryGet(hash, out var data))
      return data;
    if (!dataStr.Contains(',')) return Get(dataStr);
    // Need to detect multiple data entries from the type, key, value format.
    // Bit hacky but no idea of a better way to do it.
    var tkv = dataStr.Split([','], 3).Select(s => s.Trim()).ToArray();
    if (tkv.Length > 2 && DataEntry.SupportedTypes.Contains(tkv[0]))
    {
      var entry = new DataEntry(tkv);
      DataLoading.Add(hash, entry);
      return entry;
    }
    return Get(name.Get(parameters) ?? "");
  }
  public static int GetHash(string name)
  {
    var hash = name.GetStableHashCode();
    if (name.Contains(','))
    {
      var tkv = name.Split([','], 3).Select(s => s.Trim()).ToArray();
      if (tkv.Length > 2 && DataEntry.SupportedTypes.Contains(tkv[0]))
      {
        var entry = new DataEntry(tkv);
        DataLoading.Add(hash, entry);
        return hash;
      }
    }
    Get(name);
    return hash;
  }

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
}