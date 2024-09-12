using System.Collections.Generic;
using System.Linq;
using Data;
using Service;
using UnityEngine;

namespace ExpandWorld.Prefab;


public class ObjectsFiltering
{
  // Note: Can include the object itself.
  public static ZDO[] GetNearby(int limit, Object[] objects, Vector3 pos, Parameters parameters)
  {
    if (objects.Length == 0) return [];
    foreach (var o in objects) o.Roll(parameters);
    var maxRadius = objects.Max(o => o.MaxDistance);
    if (maxRadius > 10000)
    {
      var zdos = ZDOMan.instance.m_objectsByID.Values;
      return GetObjects(limit, zdos, objects, pos, parameters);
    }
    var zdoLists = GetSectorIndices(pos, maxRadius);
    return GetObjects(limit, zdoLists, objects, pos, parameters);
  }
  public static ZDO[] GetNearby(int limit, Object objects, Vector3 pos, Parameters parameters)
  {
    objects.Roll(parameters);
    var maxRadius = objects.MaxDistance;
    if (maxRadius > 10000)
    {
      var zdos = ZDOMan.instance.m_objectsByID.Values;
      return GetObjects(limit, zdos, objects, pos, parameters);
    }
    var zdoLists = GetSectorIndices(pos, maxRadius);
    return GetObjects(limit, zdoLists, objects, pos, parameters);
  }
  private static ZDO[] GetObjects(int limit, List<List<ZDO>> zdoLists, Object objects, Vector3 pos, Parameters parameters)
  {
    var zm = ZDOMan.instance;
    var query = zdoLists.SelectMany(z => z).Where(z => objects.IsValid(z, pos, parameters));
    if (limit > 0)
      query = query.OrderBy(z => Utils.DistanceXZ(z.m_position, pos)).Take(limit);
    return query.ToArray();
  }
  private static ZDO[] GetObjects(int limit, Dictionary<ZDOID, ZDO>.ValueCollection zdos, Object objects, Vector3 pos, Parameters parameters)
  {
    var zm = ZDOMan.instance;
    var query = zdos.Where(z => objects.IsValid(z, pos, parameters));
    if (limit > 0)
      query = query.OrderBy(z => Utils.DistanceXZ(z.m_position, pos)).Take(limit);
    return query.ToArray();
  }
  private static ZDO[] GetObjects(int limit, List<List<ZDO>> zdoLists, Object[] objects, Vector3 pos, Parameters parameters)
  {
    var zm = ZDOMan.instance;
    var query = zdoLists.SelectMany(z => z).Where(z => objects.Any(o => o.IsValid(z, pos, parameters)));
    if (limit > 0)
      query = query.OrderBy(z => Utils.DistanceXZ(z.m_position, pos)).Take(limit);
    return query.ToArray();
  }
  private static ZDO[] GetObjects(int limit, Dictionary<ZDOID, ZDO>.ValueCollection zdos, Object[] objects, Vector3 pos, Parameters parameters)
  {
    var zm = ZDOMan.instance;
    var query = zdos.Where(z => objects.Any(o => o.IsValid(z, pos, parameters)));
    if (limit > 0)
      query = query.OrderBy(z => Utils.DistanceXZ(z.m_position, pos)).Take(limit);
    return query.ToArray();
  }



  public static bool HasNearby(Range<int>? limit, Object[] objects, ZDO zdo, Parameters parameters)
  {
    if (objects.Length == 0) return true;
    foreach (var o in objects) o.Roll(parameters);
    var maxRadius = objects.Max(o => o.MaxDistance);
    if (maxRadius > 10000)
    {
      var zdos = ZDOMan.instance.m_objectsByID.Values;
      if (limit == null)
        return HasAllObjects(zdos, objects, zdo, parameters);
      else
        return HasLimitObjects(zdos, limit, objects, zdo, parameters);
    }
    var zdoLists = GetSectorIndices(zdo.m_position, maxRadius);
    if (limit == null)
      return HasAllObjects(zdoLists, objects, zdo, parameters);
    else
      return HasLimitObjects(zdoLists, limit, objects, zdo, parameters);
  }
  public static bool HasNotNearby(Range<int>? limit, Object[] objects, ZDO zdo, Parameters parameters)
  {
    if (objects.Length == 0) return true;
    foreach (var o in objects) o.Roll(parameters);
    var maxRadius = objects.Max(o => o.MaxDistance);
    var zdoLists = GetSectorIndices(zdo.m_position, maxRadius);
    if (limit == null)
      return !HasAllObjects(zdoLists, objects, zdo, parameters);
    else
      return !HasLimitObjects(zdoLists, limit, objects, zdo, parameters);
  }

  private static bool HasAllObjects(List<List<ZDO>> zdoLists, Object[] objects, ZDO zdo, Parameters parameters)
  {
    var pos = zdo.m_position;
    var zm = ZDOMan.instance;
    return objects.All(o => zdoLists.Any(z => z.Any(z => o.IsValid(z, pos, parameters) && z != zdo)));
  }
  private static bool HasAllObjects(Dictionary<ZDOID, ZDO>.ValueCollection zdos, Object[] objects, ZDO zdo, Parameters parameters)
  {
    var pos = zdo.m_position;
    var zm = ZDOMan.instance;
    return objects.All(o => zdos.Any(z => o.IsValid(z, pos, parameters) && z != zdo));
  }
  private static bool HasLimitObjects(List<List<ZDO>> zdoLists, Range<int> limit, Object[] objects, ZDO zdo, Parameters parameters)
  {
    var pos = zdo.m_position;
    var counter = 0;
    var useMax = limit.Max > 0;
    foreach (var list in zdoLists)
    {
      foreach (var z in list)
      {
        var valid = objects.FirstOrDefault(o => o.IsValid(z, pos, parameters) && z != zdo);
        if (valid == null) continue;
        counter += valid.Weight;
        if (useMax && limit.Max < counter) return false;
        if (limit.Min <= counter && !useMax) return true;
      }
    }
    return limit.Min <= counter && counter <= limit.Max;
  }

  private static bool HasLimitObjects(Dictionary<ZDOID, ZDO>.ValueCollection zdos, Range<int> limit, Object[] objects, ZDO zdo, Parameters parameters)
  {
    var pos = zdo.m_position;
    var counter = 0;
    var useMax = limit.Max > 0;
    foreach (var z in zdos)
    {
      var valid = objects.FirstOrDefault(o => o.IsValid(z, pos, parameters) && z != zdo);
      if (valid == null) continue;
      counter += valid.Weight;
      if (useMax && limit.Max < counter) return false;
      if (limit.Min <= counter && !useMax) return true;
    }
    return limit.Min <= counter && counter <= limit.Max;
  }
  private static List<List<ZDO>> GetSectorIndices(Vector3 pos, float radius)
  {
    List<List<ZDO>> zdoLists = [];
    HashSet<int> indices = [];
    var corner1 = ZoneSystem.instance.GetZone(pos + new Vector3(-radius, 0, -radius));
    var corner2 = ZoneSystem.instance.GetZone(pos + new Vector3(radius, 0, radius));
    var zm = ZDOMan.instance;
    for (var x = corner1.x; x <= corner2.x; x++)
    {
      for (var y = corner1.y; y <= corner2.y; y++)
      {
        var zone = new Vector2i(x, y);
        var index = zm.SectorToIndex(zone);
        if (index < 0 || index >= zm.m_objectsBySector.Length)
        {
          if (zm.m_objectsByOutsideSector.TryGetValue(zone, out var list) && list != null)
            zdoLists.Add(list);
          continue;
        }
        if (indices.Contains(index)) continue;
        indices.Add(index);
        if (zm.m_objectsBySector[index] != null)
          zdoLists.Add(zm.m_objectsBySector[index]);
      }
    }
    return zdoLists;
  }
}
