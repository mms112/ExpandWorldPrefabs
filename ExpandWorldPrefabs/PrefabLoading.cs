using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using HarmonyLib;
using Service;
namespace ExpandWorld.Prefab;

public class Loading
{
  private static readonly string FileName = "expand_prefabs.yaml";
  private static readonly string FilePath = Path.Combine(Yaml.BaseDirectory, FileName);
  private static readonly string Pattern = "expand_prefabs*.yaml";

  private static void Load(string yaml)
  {
    InfoManager.Clear();
    if (Helper.IsClient()) return;

    var data = ParseYaml(yaml);
    if (data.Count == 0)
    {
      Log.Warning($"Failed to load any prefab data.");
      return;
    }
    Log.Info($"Reloading prefab rules ({data.Count} entries).");
    foreach (var item in data)
    {
      InfoManager.Add(item);
    }
    InfoManager.Patch();
  }

  public static void FromSetting()
  {
    //if (Helper.IsClient()) Load(EWP.valuePrefabData.Value);
  }
  public static void FromFile()
  {
    if (Helper.IsClient()) return;
    if (!File.Exists(FilePath))
    {
      var yaml = Yaml.Serializer().Serialize(new Data[]{new(){
        prefab = "Example",
        type = "Create",
        swap = "Surtling",
        biomes = "Meadows",
      }});
      File.WriteAllText(FilePath, yaml);
      // Watcher triggers reload.
      return;
    }
    else
    {
      var yaml = Yaml.Read(Pattern);
      Load(yaml);
    }
  }
  private static List<Info> ParseYaml(string yaml)
  {
    try
    {
      return Yaml.Deserialize<Data>(yaml, FileName).SelectMany(FromData).ToList();
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
      Log.Error(e.StackTrace);
    }
    return [];
  }
  private static Info[] FromData(Data data)
  {
    var waterLevel = ZoneSystem.instance.m_waterLevel;
    var spawnDelay = Math.Max(data.delay, data.spawnDelay);
    var swaps = ParseSpawns(data.swaps ?? (data.swap == null ? [] : [data.swap]), spawnDelay);
    var spawns = ParseSpawns(data.spawns ?? (data.spawn == null ? [] : [data.spawn]), spawnDelay);
    var playerSearch = Parse.ToList(data.playerSearch).ToArray();
    var types = (data.types ?? [data.type]).Select(s => new InfoType(data.prefab, s)).ToArray();
    HashSet<string> events = [.. Parse.ToList(data.events)];
    var commands = ParseCommands(data.commands ?? (data.command == null ? [] : [data.command]));
    HashSet<string> environments = [.. Parse.ToList(data.environments).Select(s => s.ToLower())];
    HashSet<string> bannedEnvironments = [.. Parse.ToList(data.bannedEnvironments).Select(s => s.ToLower())];
    HashSet<string> locations = [.. Parse.ToList(data.locations)];
    var objectsLimit = ParseObjectsLimit(data.objectsLimit);
    var objects = ParseObjects(data.objects ?? []);
    var bannedObjects = ParseObjects(data.bannedObjects ?? []);
    var bannedObjectsLimit = ParseObjectsLimit(data.bannedObjectsLimit);
    var filters = ParseFilters(data.filters ?? (data.filter == null ? [] : [data.filter]));
    var bannedFilters = ParseFilters(data.bannedFilters ?? (data.bannedFilter == null ? [] : [data.bannedFilter]));
    var legacyPokes = ParseObjects(data.pokes ?? []);
    var pokes = ParsePokes(data.poke ?? []);
    var rpcs = ParseRpcs(data);
    var objectRpcs = ParseObjectRpcs(data);
    var clientRpcs = ParseClientRpcs(data);
    return types.Select(t =>
    {
      return new Info()
      {
        Prefabs = data.prefab,
        Type = t.Type,
        Fallback = data.fallback,
        Args = t.Parameters,
        Remove = t.Type != ActionType.Destroy && (data.remove || swaps.Length > 0),
        RemoveDelay = data.removeDelay,
        Drops = t.Type != ActionType.Destroy && data.drops,
        Spawns = [.. spawns],
        Swaps = [.. swaps],
        Data = t.Type != ActionType.Destroy ? data.data : "",
        Commands = commands,
        PlayerSearch = playerSearch.Length > 0 && Enum.TryParse(playerSearch[0], true, out PlayerSearch mode) ? mode : PlayerSearch.None,
        PlayerSearchDistance = Parse.Float(playerSearch, 1, 0f),
        PlayerSearchHeight = Parse.Float(playerSearch, 2, 0f),
        Weight = data.weight,
        Day = data.day,
        Night = data.night,
        MinDistance = data.minDistance < 1f ? data.minDistance * 10000f : data.minDistance,
        MaxDistance = data.maxDistance < 1f ? data.maxDistance * 10000f : data.maxDistance,
        MinY = data.minY ?? data.minAltitude - waterLevel,
        MaxY = data.maxY ?? data.maxAltitude - waterLevel,
        Biomes = Yaml.ToBiomes(data.biomes),
        Environments = environments,
        BannedEnvironments = bannedEnvironments,
        GlobalKeys = Parse.ToList(data.globalKeys),
        BannedGlobalKeys = Parse.ToList(data.bannedGlobalKeys),
        Events = events,
        // Distance can be set without events for any event.
        // However if event is set, there must be a distance (the default value).
        // Zero distance means no check at all.
        EventDistance = data.eventDistance ?? (events.Count > 0 ? 100f : 0f),
        LocationDistance = data.locationDistance,
        Locations = locations,
        PokeLimit = data.pokeLimit,
        PokeParameter = data.pokeParameter,
        Pokes = pokes,
        LegacyPokes = legacyPokes,
        PokeDelay = data.pokeDelay,
        ObjectsLimit = objectsLimit,
        Objects = objects,
        BannedObjects = bannedObjects,
        BannedObjectsLimit = bannedObjectsLimit,
        Filters = filters,
        BannedFilters = bannedFilters,
        TriggerRules = data.triggerRules,
        Rpcs = rpcs,
        ObjectRpcs = objectRpcs,
        ClientRpcs = clientRpcs,
      };
    }).ToArray();
  }
  private static string[] ParseCommands(string[] commands) => commands.Select(s =>
  {
    if (s.Contains("$$"))
    {
      Log.Warning($"Command \"{s}\" contains $$ which is obsolete. Use {"<>"} instead.");
      return s.Replace("$$x", "<x>").Replace("$$y", "<y>").Replace("$$z", "<z>").Replace("$$a", "<a>").Replace("$$i", "<i>").Replace("$$j", "<j>");
    }
    if (s.Contains("{") && s.Contains("}"))
    {
      Log.Warning($"Command \"{s}\" contains {{}} which is obsolete. Use {"<>"} instead.");
      return s.Replace("{", "<").Replace("}", ">");
    }
    return s;
  }).ToArray();

  private static Spawn[] ParseSpawns(string[] spawns, float delay) => spawns.Select(s => new Spawn(s, delay)).ToArray();

  private static Filter[] ParseFilters(string[] filters) => filters.Select(Filter.Create).Where(s => s != null).ToArray();
  private static Object[] ParseObjects(string[] objects) => objects.Select(s => new Object(s)).ToArray();
  private static Poke[] ParsePokes(PokeData[] objects) => objects.Select(s => new Poke(s)).ToArray();
  private static SimpleRpcInfo[]? ParseRpcs(Data data)
  {
    if (data.rpcs != null && data.rpcs.Length > 0) return ParseRpcs(data.rpcs, data.rpcDelay);
    if (data.rpc != null) return ParseRpcs([data.rpc], data.rpcDelay);
    return null;
  }
  private static SimpleRpcInfo[] ParseRpcs(string[] objects, float delay) => objects.Select(s => new SimpleRpcInfo(s, delay)).ToArray();
  private static ObjectRpcInfo[]? ParseObjectRpcs(Data data)
  {
    if (data.objectRpc != null && data.objectRpc.Length > 0) return ParseObjectRpcs(data.objectRpc, data.rpcDelay, data.rpcSource);
    return null;
  }
  private static ObjectRpcInfo[] ParseObjectRpcs(string[] objects, float delay, string? rpcSource)
  {
    List<ObjectRpcInfo> rpcs = [];
    var start = 0;
    for (var i = 1; i < objects.Length; i++)
    {
      if (RpcInfo.IsType(objects[i])) continue;
      rpcs.Add(new(objects.Skip(start).Take(i - start).ToArray(), delay, rpcSource));
      start = i;
    }
    rpcs.Add(new(objects.Skip(start).ToArray(), delay, rpcSource));
    return [.. rpcs];
  }
  private static ClientRpcInfo[]? ParseClientRpcs(Data data)
  {
    if (data.clientRpc != null && data.clientRpc.Length > 0) return ParseClientRpcs(data.clientRpc, data.rpcDelay, data.rpcSource);
    return null;
  }
  private static ClientRpcInfo[] ParseClientRpcs(string[] objects, float delay, string? rpcSource)
  {
    List<ClientRpcInfo> rpcs = [];
    var start = 0;
    for (var i = 1; i < objects.Length; i++)
    {
      if (RpcInfo.IsType(objects[i])) continue;
      rpcs.Add(new(objects.Skip(start).Take(i - start).ToArray(), delay, rpcSource));
      start = i;
    }
    rpcs.Add(new(objects.Skip(start).ToArray(), delay, rpcSource));
    return [.. rpcs];
  }
  private static Range<int>? ParseObjectsLimit(string str) => str == "" ?
    null : int.TryParse(str, out var limit) ?
      new Range<int>(limit, 0) : Parse.IntRange(str);

  public static void SetupWatcher()
  {
    Yaml.SetupWatcher(Pattern, FromFile);
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start)), HarmonyPriority(Priority.VeryLow)]
public class InitializeContent
{
  static void Postfix()
  {
    if (Helper.IsServer())
    {
      DataLoading.LoadEntries();
      Loading.FromFile();
    }

  }
}