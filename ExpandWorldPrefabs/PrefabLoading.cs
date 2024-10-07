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
      Load();
    }
  }

  private static void Load()
  {
    InfoManager.Clear();
    if (Helper.IsClient()) return;

    var data = Yaml.Read<Data>(Pattern, true);
    if (data.Count == 0)
    {
      Log.Warning($"Failed to load any prefab data.");
      return;
    }
    Log.Info($"Reloading prefab rules ({data.Count} entries).");
    var items = data.SelectMany(FromData).ToList();
    foreach (var item in items)
    {
      InfoManager.Add(item);
    }
    InfoManager.Patch();
  }

  private static Info[] FromData(Data data)
  {
    var waterLevel = ZoneSystem.instance.m_waterLevel;
    float? spawnDelay = data.delay == null && data.spawnDelay == null ? null : Math.Max(data.delay ?? 0f, data.spawnDelay ?? 0f);
    bool? triggerRules = data.triggerRules;
    var swaps = data.swap == null ? data.swaps == null ? null : ParseSpawns(data.swaps, spawnDelay, triggerRules) : ParseSpawns(data.swap, spawnDelay, triggerRules);
    var spawns = data.spawn == null ? data.spawns == null ? null : ParseSpawns(data.spawns, spawnDelay, triggerRules) : ParseSpawns(data.spawn, spawnDelay, triggerRules);
    var types = (data.types ?? [data.type]).Select(s => new InfoType(data.prefab, s)).ToArray();
    if (data.prefab == "" && types.Any(t => t.Type != ActionType.GlobalKey && t.Type != ActionType.Event))
      Log.Warning($"Prefab missing for type {data.type}");
    HashSet<string> events = [.. Parse.ToList(data.events)];
    var commands = data.commands ?? (data.command == null ? [] : [data.command]);
    HashSet<string> environments = [.. Parse.ToList(data.environments).Select(s => s.ToLower())];
    HashSet<string> bannedEnvironments = [.. Parse.ToList(data.bannedEnvironments).Select(s => s.ToLower())];
    HashSet<string> locations = [.. Parse.ToList(data.locations)];
    var objectsLimit = ParseObjectsLimit(data.objectsLimit);
    var objects = data.objects == null ? null : ParseObjects(data.objects);
    var bannedObjects = data.bannedObjects == null ? null : ParseObjects(data.bannedObjects);
    var bannedObjectsLimit = ParseObjectsLimit(data.bannedObjectsLimit);
    var filters = ParseFilters(data.filters ?? (data.filter == null ? [] : [data.filter]));
    var bannedFilters = ParseFilters(data.bannedFilters ?? (data.bannedFilter == null ? [] : [data.bannedFilter]));
    var legacyPokes = data.pokes == null ? null : ParseObjects(data.pokes);
    var pokes = data.poke == null ? null : ParsePokes(data.poke);
    var terrains = data.terrain == null ? null : data.terrain.Select(s => new Terrain(s)).ToArray();
    var objectRpcs = ParseObjectRpcs(data);
    var clientRpcs = ParseClientRpcs(data);
    var minPaint = data.minPaint != "" ? Parse.Color(data.minPaint) : data.paint != "" ? Parse.Color(data.paint) : null;
    var maxPaint = data.maxPaint != "" ? Parse.Color(data.maxPaint) : data.paint != "" ? Parse.Color(data.paint) : null;
    var addItems = HandleItems(data.addItems);
    var removeItems = HandleItems(data.removeItems);
    return types.Select(t =>
    {
      var d = t.Type != ActionType.Destroy ? data.data : "";
      bool? remove = t.Type == ActionType.Destroy ? false : swaps != null ? true : data.remove == "" ? false : null;
      return new Info()
      {
        Prefabs = data.prefab,
        Type = t.Type,
        Fallback = data.fallback,
        Args = t.Parameters,
        Remove = remove == null ? data.remove == null ? null : DataValue.Bool(data.remove) : new SimpleBoolValue(remove.Value),
        Regenerate = (d != "" || data.addItems != "" || data.removeItems != "") && !data.injectData,
        RemoveDelay = data.removeDelay == null ? null : DataValue.Float(data.removeDelay),
        Drops = t.Type == ActionType.Destroy || data.drops == null ? null : DataValue.Bool(data.drops),
        Spawns = spawns,
        Swaps = swaps,
        Data = DataValue.String(d),
        InjectData = data.injectData,
        Commands = commands,
        Weight = data.weight == null ? null : DataValue.Float(data.weight),
        Day = data.day == null ? null : DataValue.Bool(data.day),
        Night = data.night == null ? null : DataValue.Bool(data.night),
        MinDistance = data.minDistance == null ? null : Parse.TryFloat(data.minDistance, out var minDistance) ? minDistance < 1f ? new SimpleFloatValue(minDistance * 10000f) : new SimpleFloatValue(minDistance) : DataValue.Float(data.minDistance),
        MaxDistance = data.maxDistance == null ? null : Parse.TryFloat(data.maxDistance, out var maxDistance) ? maxDistance < 1f ? new SimpleFloatValue(maxDistance * 10000f) : new SimpleFloatValue(maxDistance) : DataValue.Float(data.maxDistance),
        MinY = data.minY == null ? null : DataValue.Float(data.minY),
        MaxY = data.maxY == null ? null : DataValue.Float(data.maxY),
        MinAltitude = data.minAltitude == null ? null : DataValue.Float(data.minAltitude),
        MaxAltitude = data.maxAltitude == null ? null : DataValue.Float(data.maxAltitude),
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
        Terrains = terrains,
        LegacyPokes = legacyPokes,
        PokeDelay = data.pokeDelay,
        ObjectsLimit = objectsLimit,
        Objects = objects,
        BannedObjects = bannedObjects,
        BannedObjectsLimit = bannedObjectsLimit,
        Filter = filters,
        BannedFilter = bannedFilters,
        TriggerRules = triggerRules ?? false,
        ObjectRpcs = objectRpcs,
        ClientRpcs = clientRpcs,
        MinPaint = minPaint,
        MaxPaint = maxPaint,
        AddItems = addItems,
        RemoveItems = removeItems,
        Cancel = data.cancel == null ? null : DataValue.Bool(data.cancel),
        Owner = data.owner == null ? null : DataValue.Long(data.owner),
      };
    }).ToArray();
  }

  private static Spawn[] ParseSpawns(string[] spawns, float? delay, bool? triggerRules) => spawns.Select(s => new Spawn(s, delay, triggerRules)).ToArray();
  private static Spawn[] ParseSpawns(SpawnData[] spawns, float? delay, bool? triggerRules) => spawns.Select(s => new Spawn(s, delay, triggerRules)).ToArray();

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
    var value = string.Join(",", split.Skip(2));
    if (type == "hash")
    {
      entry.Hashes ??= [];
      entry.Hashes.Add(key, DataValue.Hash(value));
      return;
    }
    if (type == "string")
    {
      entry.Strings ??= [];
      entry.Strings.Add(key, DataValue.String(value));
      return;
    }
    if (type == "bool")
    {
      entry.Bools ??= [];
      entry.Bools.Add(key, DataValue.Bool(value));
      return;
    }
    if (type == "int")
    {
      entry.Ints ??= [];
      entry.Ints.Add(key, DataValue.Int(value));
      return;
    }
    if (type == "float")
    {
      entry.Floats ??= [];
      entry.Floats.Add(key, DataValue.Float(value));
      return;
    }
    if (type == "long")
    {
      entry.Longs ??= [];
      entry.Longs.Add(key, DataValue.Long(value));
      return;
    }
    Log.Error($"Invalid filter type: {type}");
  }
  private static Object[] ParseObjects(string[] objects) => objects.Select(s => new Object(s)).ToArray();
  private static Object[] ParseObjects(ObjectData[] objects) => objects.Select(s => new Object(s)).ToArray();
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

  private static Range<int>? ParseObjectsLimit(string? str) => str == null ?
    null : int.TryParse(str, out var limit) ?
      new Range<int>(limit, 0) : Parse.IntRange(str);

  public static void SetupWatcher()
  {
    if (!Directory.Exists(Yaml.BaseDirectory))
      Directory.CreateDirectory(Yaml.BaseDirectory);
    Yaml.SetupWatcher(Pattern, FromFile);
  }
  private static DataEntry? HandleItems(string items)
  {
    var split = Parse.Kvp(items);
    if (split.Value == "") return DataHelper.Get(items);
    DataEntry data = new()
    {
      Items = []
    };
    ItemData itemData = new()
    {
      prefab = split.Key,
      stack = split.Value
    };
    ItemValue item = new(itemData);
    data.Items.Add(item);
    return data;
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