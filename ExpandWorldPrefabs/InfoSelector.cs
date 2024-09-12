using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

namespace ExpandWorld.Prefab;


public class InfoSelector
{
  public static Info? Select(ActionType type, ZDO zdo, string arg, Parameters parameters, ZDO? source)
  {
    var infos = InfoManager.Select(type);
    return SelectDefault(infos, zdo, arg, parameters, source) ?? SelectFallback(infos, zdo, arg, parameters, source);
  }
  private static Info? SelectDefault(PrefabInfo infos, ZDO zdo, string arg, Parameters parameters, ZDO? source)
  {
    var prefab = zdo.m_prefab;
    if (!infos.TryGetValue(prefab, out var data)) return null;
    return SelectInfo(data, zdo, arg, parameters, source);
  }
  private static Info? SelectFallback(PrefabInfo infos, ZDO zdo, string arg, Parameters parameters, ZDO? source)
  {
    var prefab = zdo.m_prefab;
    if (!infos.TryGetFallbackValue(prefab, out var data)) return null;
    return SelectInfo(data, zdo, arg, parameters, source);
  }
  private static Info? SelectInfo(List<Info> data, ZDO zdo, string arg, Parameters parameters, ZDO? source)
  {
    if (data.Count == 0) return null;
    var pos = zdo.m_position;
    var biome = WorldGenerator.instance.GetBiome(pos);
    var distance = Utils.LengthXZ(pos);
    var day = EnvMan.IsDay();
    var args = arg.Split(' ');
    var waterY = pos.y - ZoneSystem.instance.m_waterLevel;
    var linq = data
      .Where(d => CheckArgs(d, args))
      .Where(d => (d.Biomes & biome) == biome)
      .Where(d => d.Day.GetBool(parameters) != false || !day)
      .Where(d => d.Night.GetBool(parameters) != false || day)
      .Where(d => d.MinDistance == null || !d.MinDistance.TryGet(parameters, out var v) || v < distance)
      .Where(d => d.MaxDistance == null || !d.MaxDistance.TryGet(parameters, out var v) || v >= distance)
      .Where(d => d.MinY == null || !d.MinY.TryGet(parameters, out var v) || v < pos.y)
      .Where(d => d.MaxY == null || !d.MaxY.TryGet(parameters, out var v) || v >= pos.y)
      .Where(d => d.MinAltitude == null || !d.MinAltitude.TryGet(parameters, out var v) || v < waterY)
      .Where(d => d.MaxAltitude == null || !d.MaxAltitude.TryGet(parameters, out var v) || v >= waterY)
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
    var checkPaint = linq.Any(d => d.MinPaint != null || d.MaxPaint != null);
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
      if (checkFilters)
      {
        linq = linq.Where(d => d.Filter == null || d.Filter.Match(parameters, zdo) || (source != null && d.Filter.Match(parameters, source))).ToArray();
      }
      if (checkBannedFilters)
      {
        linq = linq.Where(d => d.BannedFilter == null || d.BannedFilter.Unmatch(parameters, zdo) || (source != null && d.BannedFilter.Unmatch(parameters, source))).ToArray();
      }
    }
    if (checkPaint)
    {
      var paint = Paint.GetPaint(pos, biome);
      linq = linq.Where(d =>
        (d.MinPaint == null || (d.MinPaint.Value.b <= paint.b && d.MinPaint.Value.g <= paint.g && d.MinPaint.Value.r <= paint.r)) &&
        (d.MaxPaint == null || (d.MaxPaint.Value.b >= paint.b && d.MaxPaint.Value.g >= paint.g && d.MaxPaint.Value.r >= paint.r))).ToArray();
    }
    return Randomize(linq.ToArray(), parameters);
  }
  private static Info? Randomize(Info[] valid, Parameters parameters)
  {
    if (valid.Length == 0) return null;
    var weights = valid.Select(d => d.Weight.Get(parameters) ?? 1f).ToArray();
    if (valid.Length == 1 && weights[0] >= 1f) return valid[0];
    var totalWeight = Mathf.Max(1f, weights.Sum());
    var random = Random.Range(0f, totalWeight);
    for (int i = 0; i < valid.Length; i++)
    {
      random -= weights[i];
      if (random <= 0f) return valid[i];
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
  public static Info? SelectGlobal(ActionType type, string arg, Parameters parameters, Vector3 pos, bool remove)
  {
    var infos = InfoManager.SelectGlobal(type);
    return SelectGlobalInfo(infos.Info, arg, parameters, pos, remove) ?? SelectGlobalInfo(infos.Fallback, arg, parameters, pos, remove);
  }

  private static Info? SelectGlobalInfo(List<Info> data, string arg, Parameters parameters, Vector3 pos, bool remove)
  {
    if (data.Count == 0) return null;
    var day = EnvMan.IsDay();
    var args = arg.Split(' ');
    var distance = Utils.LengthXZ(pos);
    var waterY = pos.y - ZoneSystem.instance.m_waterLevel;
    var linq = data
      .Where(d => CheckArgs(d, args))
      .Where(d => remove == d.Remove.GetBool(parameters))
      .Where(d => d.Day.GetBool(parameters) != false || !day)
      .Where(d => d.Night.GetBool(parameters) != false || day)
      .Where(d => d.MinDistance == null || distance >= d.MinDistance.Get(parameters))
      .Where(d => d.MaxDistance == null || distance < d.MaxDistance.Get(parameters))
      .Where(d => d.MinY == null || !d.MinY.TryGet(parameters, out var v) || v < pos.y)
      .Where(d => d.MaxY == null || !d.MaxY.TryGet(parameters, out var v) || v >= pos.y)
      .Where(d => d.MinAltitude == null || !d.MinAltitude.TryGet(parameters, out var v) || v < waterY)
      .Where(d => d.MaxAltitude == null || !d.MaxAltitude.TryGet(parameters, out var v) || v >= waterY)
      .Where(d => Helper.HasEveryGlobalKey(d.GlobalKeys, parameters))
      .Where(d => !Helper.HasAnyGlobalKey(d.BannedGlobalKeys, parameters));

    return Randomize(linq.ToArray(), parameters);
  }
}