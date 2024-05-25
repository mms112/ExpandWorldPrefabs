using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExpandWorld.Prefab;


public class InfoSelector
{
  public static Info? Select(ActionType type, ZDO zdo, string arg, Dictionary<string, string> parameters, ZDO? source)
  {
    var infos = InfoManager.Select(type);
    return SelectDefault(infos, zdo, arg, parameters, source) ?? SelectFallback(infos, zdo, arg, parameters, source);
  }
  private static Info? SelectDefault(PrefabInfo infos, ZDO zdo, string arg, Dictionary<string, string> parameters, ZDO? source)
  {
    var prefab = zdo.m_prefab;
    if (!infos.TryGetValue(prefab, out var data)) return null;
    return SelectInfo(data, zdo, arg, parameters, source);
  }
  private static Info? SelectFallback(PrefabInfo infos, ZDO zdo, string arg, Dictionary<string, string> parameters, ZDO? source)
  {
    var prefab = zdo.m_prefab;
    if (!infos.TryGetFallbackValue(prefab, out var data)) return null;
    return SelectInfo(data, zdo, arg, parameters, source);
  }
  private static Info? SelectInfo(List<Info> data, ZDO zdo, string arg, Dictionary<string, string> parameters, ZDO? source)
  {
    if (data.Count == 0) return null;
    var pos = zdo.m_position;
    var biome = WorldGenerator.instance.GetBiome(pos);
    var distance = Utils.DistanceXZ(pos, Vector3.zero);
    var day = EnvMan.IsDay();
    var args = arg.Split(' ');
    var linq = data
      .Where(d => CheckArgs(d, args))
      .Where(d => (d.Biomes & biome) == biome)
      .Where(d => d.Day || !day)
      .Where(d => d.Night || day)
      .Where(d => distance >= d.MinDistance)
      .Where(d => distance < d.MaxDistance)
      .Where(d => pos.y >= d.MinY)
      .Where(d => pos.y < d.MaxY)
      .Where(d => Helper.HasEveryGlobalKey(d.GlobalKeys, parameters))
      .Where(d => !Helper.HasAnyGlobalKey(d.BannedGlobalKeys, parameters));
    // Minor optimization to resolve simpler checks first (not measured).
    linq = linq.ToArray();
    var checkEnvironments = linq.Any(d => d.Environments.Count > 0) || linq.Any(d => d.BannedEnvironments.Count > 0);
    var checkEvents = linq.Any(d => d.Events.Count > 0);
    var checkObjects = linq.Any(d => d.Objects.Length > 0);
    var checkBannedObjects = linq.Any(d => d.BannedObjects.Length > 0);
    var checkLocations = linq.Any(d => d.Locations.Count > 0);
    var checkFilters = linq.Any(d => d.Filter != null);
    var checkBannedFilters = linq.Any(d => d.BannedFilter != null);
    if (checkEnvironments)
    {
      var environment = GetEnvironment(biome);
      linq = linq
        .Where(d => d.Environments.Count == 0 || d.Environments.Contains(environment))
        .Where(d => !d.BannedEnvironments.Contains(environment)).ToArray();
    }
    if (checkEvents)
    {
      var ev = EWP.GetCurrentEvent(pos);
      // Three cases:
      // 1. Nothing set, always true.
      // 2. Only event distance set, any event is fine.
      // 3. Event name set, only that event is fine.
      // Event distance is zero only if nothing is set.
      linq = linq.Where(d => d.EventDistance == 0f || (ev != null && (d.Events.Contains(ev.m_name) || d.Events.Count == 0) && d.EventDistance >= Utils.DistanceXZ(pos, ev.m_pos))).ToArray();
    }
    if (checkObjects)
    {
      linq = linq.Where(d => ObjectsFiltering.HasNearby(d.ObjectsLimit, d.Objects, zdo, parameters)).ToArray();
    }
    if (checkBannedObjects)
    {
      linq = linq.Where(d => ObjectsFiltering.HasNotNearby(d.BannedObjectsLimit, d.BannedObjects, zdo, parameters)).ToArray();
    }
    if (checkLocations)
    {
      var zone = ZoneSystem.instance.GetZone(pos);
      linq = linq.Where(d =>
      {
        if (d.Locations.Count == 0) return true;
        // +1 because the location can be at zone edge, so any distance can be on the next zone.
        var di = (int)(d.LocationDistance / 64f) + 1;
        var dj = (int)(d.LocationDistance / 64f) + 1;
        var minI = zone.x - di;
        var maxI = zone.x + di;
        var minJ = zone.y - dj;
        var maxJ = zone.y + dj;

        for (int i = minI; i <= maxI; i++)
        {
          for (int j = minJ; j <= maxJ; j++)
          {
            var key = new Vector2i(i, j);
            if (!ZoneSystem.instance.m_locationInstances.TryGetValue(key, out var loc)) continue;
            if (!d.Locations.Contains(loc.m_location.m_prefabName)) continue;
            var dist = d.LocationDistance == 0 ? loc.m_location.m_exteriorRadius : d.LocationDistance;
            if (Utils.DistanceXZ(loc.m_position, pos) > dist) continue;
            return true;
          }
        }
        return false;
      }).ToArray();
    }
    if (checkFilters || checkBannedFilters)
    {
      Dictionary<string, string> pars = [];
      Dictionary<string, string> sourcePars = [];
      if (checkFilters)
      {
        linq = linq.Where(d => d.Filter == null || d.Filter.Match(pars, zdo) || (source != null && d.Filter.Match(sourcePars, source))).ToArray();
      }
      if (checkBannedFilters)
      {
        linq = linq.Where(d => d.BannedFilter == null || d.BannedFilter.Unmatch(pars, zdo) || (source != null && d.BannedFilter.Unmatch(sourcePars, source))).ToArray();
      }
    }
    var valid = linq.ToArray();
    if (valid.Length == 0) return null;
    if (valid.Length == 1 && valid[0].Weight >= 1f) return valid[0];
    var totalWeight = Mathf.Max(1f, valid.Sum(d => d.Weight));
    var random = Random.Range(0f, totalWeight);
    foreach (var item in valid)
    {
      random -= item.Weight;
      if (random <= 0f) return item;
    }
    return null;
  }
  private static bool CheckArgs(Info info, string[] args)
  {
    if (info.Args.Length == 0) return true;
    if (info.Args.Length > args.Length) return false;
    for (int i = 0; i < info.Args.Length; i++)
      if (!Helper.CheckWild(info.Args[i], args[i])) return false;
    return true;

  }
  private static string GetEnvironment(Heightmap.Biome biome)
  {
    var em = EnvMan.instance;
    var availableEnvironments = em.GetAvailableEnvironments(biome);
    if (availableEnvironments == null || availableEnvironments.Count == 0) return "";
    Random.State state = Random.state;
    var num = (long)ZNet.instance.GetTimeSeconds() / em.m_environmentDuration;
    Random.InitState((int)num);
    var env = em.SelectWeightedEnvironment(availableEnvironments);
    Random.state = state;
    return env.m_name.ToLower();
  }
  public static Info? SelectGlobal(ActionType type, string arg, Dictionary<string, string> parameters, bool remove)
  {
    var infos = InfoManager.SelectGlobal(type);
    return SelectGlobalInfo(infos.Info, arg, parameters, remove) ?? SelectGlobalInfo(infos.Fallback, arg, parameters, remove);
  }

  private static Info? SelectGlobalInfo(List<Info> data, string arg, Dictionary<string, string> parameters, bool remove)
  {
    if (data.Count == 0) return null;
    var day = EnvMan.IsDay();
    var args = arg.Split(' ');
    var linq = data
      .Where(d => CheckArgs(d, args))
      .Where(d => remove == d.Remove)
      .Where(d => d.Day || !day)
      .Where(d => d.Night || day)
      .Where(d => Helper.HasEveryGlobalKey(d.GlobalKeys, parameters))
      .Where(d => !Helper.HasAnyGlobalKey(d.BannedGlobalKeys, parameters));

    var valid = linq.ToArray();
    if (valid.Length == 0) return null;
    if (valid.Length == 1 && valid[0].Weight >= 1f) return valid[0];
    var totalWeight = Mathf.Max(1f, valid.Sum(d => d.Weight));
    var random = Random.Range(0f, totalWeight);
    foreach (var item in valid)
    {
      random -= item.Weight;
      if (random <= 0f) return item;
    }
    return null;
  }
}