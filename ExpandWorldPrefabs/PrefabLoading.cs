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
      var yaml = "# Write your entries here. Good luck!";
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
    var triggerRules = data.triggerRules;
    var swaps = ParseSpawns(data.swaps ?? (data.swap == null ? [] : [data.swap]), spawnDelay, triggerRules);
    var spawns = ParseSpawns(data.spawns ?? (data.spawn == null ? [] : [data.spawn]), spawnDelay, triggerRules);
    var types = (data.types ?? [data.type]).Select(s => new InfoType(data.prefab, s)).ToArray();
    if (data.prefab == "" && types.Any(t => t.Type != ActionType.GlobalKey && t.Type != ActionType.Event))
      Log.Warning($"Prefab missing for type {data.type}");
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
    var objectRpcs = ParseObjectRpcs(data);
    var clientRpcs = ParseClientRpcs(data);
    var minPaint = data.minPaint != "" ? Parse.Color(data.minPaint) : data.paint != "" ? Parse.Color(data.paint) : null;
    var maxPaint = data.maxPaint != "" ? Parse.Color(data.maxPaint) : data.paint != "" ? Parse.Color(data.paint) : null;
    return types.Select(t =>
    {
      var d = t.Type != ActionType.Destroy ? data.data : "";
      return new Info()
      {
        Prefabs = data.prefab,
        Type = t.Type,
        Fallback = data.fallback,
        Args = t.Parameters,
        Remove = t.Type != ActionType.Destroy && (data.remove || swaps.Length > 0),
        Regenerate = (d != "" || data.addItems != "" || data.removeItems != "") && !data.injectData,
        RemoveDelay = data.removeDelay,
        Drops = t.Type != ActionType.Destroy && data.drops,
        Spawns = [.. spawns],
        Swaps = [.. swaps],
        Data = d,
        InjectData = data.injectData,
        Commands = commands,
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
        GlobalKeys = Parse.ToList(data.globalKeys).Select(s => s.ToLowerInvariant()).ToList(),
        BannedGlobalKeys = Parse.ToList(data.bannedGlobalKeys).Select(s => s.ToLowerInvariant()).ToList(),
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
        Filter = filters,
        BannedFilter = bannedFilters,
        TriggerRules = triggerRules,
        ObjectRpcs = objectRpcs,
        ClientRpcs = clientRpcs,
        MinPaint = minPaint,
        MaxPaint = maxPaint,
        AddItems = data.addItems,
        RemoveItems = data.removeItems
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

  private static Spawn[] ParseSpawns(string[] spawns, float delay, bool triggerRules) => spawns.Select(s => new Spawn(s, delay, triggerRules)).ToArray();

  private static DataEntry? ParseFilters(string[] filters)
  {
    if (filters.Length == 0)
      return null;
    if (filters.Length == 1 && Parse.ToList(filters[0]).Count == 1)
      return DataLoading.Get(filters[0]);
    DataEntry entry = new();
    foreach (var filter in filters)
      AddFilter(entry, filter);
    return entry;
  }
  private static void AddFilter(DataEntry entry, string filter)
  {
    var split = Parse.ToList(filter);
    if (split.Count < 3)
    {
      Log.Error($"Invalid filter: {filter}");
      return;
    }
    var type = split[0].ToLowerInvariant();
    var key = split[1].GetStableHashCode();
    var value = split[2];
    if (type == "hash")
    {
      entry.Hashes ??= [];
      entry.Hashes.Add(key, DataValue.Hash(value, entry.RequiredParameters));
      return;
    }
    if (type == "string")
    {
      entry.Strings ??= [];
      entry.Strings.Add(key, DataValue.String(value, entry.RequiredParameters));
      return;
    }
    if (type == "bool")
    {
      entry.Bools ??= [];
      entry.Bools.Add(key, DataValue.Bool(value, entry.RequiredParameters));
      return;
    }
    if (type == "int")
    {
      entry.Ints ??= [];
      entry.Ints.Add(key, DataValue.Int(value, entry.RequiredParameters));
      return;
    }
    if (type == "float")
    {
      entry.Floats ??= [];
      entry.Floats.Add(key, DataValue.Float(value, entry.RequiredParameters));
      return;
    }
    Log.Error($"Invalid filter type: {type}");
  }
  private static Object[] ParseObjects(string[] objects) => objects.Select(s => new Object(s)).ToArray();
  private static Poke[] ParsePokes(PokeData[] objects) => objects.Select(s => new Poke(s)).ToArray();
  private static ObjectRpcInfo[]? ParseObjectRpcs(Data data)
  {
    if (data.objectRpc != null && data.objectRpc.Length > 0) return data.objectRpc.Select(s => new ObjectRpcInfo(s)).ToArray();
    return null;
  }
  private static ClientRpcInfo[]? ParseClientRpcs(Data data)
  {
    if (data.clientRpc != null && data.clientRpc.Length > 0) return data.clientRpc.Select(s => new ClientRpcInfo(s)).ToArray();
    return null;
  }

  private static Range<int>? ParseObjectsLimit(string str) => str == "" ?
    null : int.TryParse(str, out var limit) ?
      new Range<int>(limit, 0) : Parse.IntRange(str);

  public static void SetupWatcher()
  {
    if (!Directory.Exists(Yaml.BaseDirectory))
      Directory.CreateDirectory(Yaml.BaseDirectory);
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