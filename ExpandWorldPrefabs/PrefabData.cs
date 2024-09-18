using System;
using System.Collections.Generic;
using System.ComponentModel;
using Data;
using Service;
using UnityEngine;

namespace ExpandWorld.Prefab;

public class Data
{
  [DefaultValue("")]
  public string prefab = "";
  public string type = "";
  [DefaultValue(null)]
  public string[]? types;
  [DefaultValue(false)]
  public bool fallback = false;
  [DefaultValue(null)]
  public string? weight;
  [DefaultValue(null)]
  public SpawnData[]? swap;
  [DefaultValue(null)]
  public string[]? swaps;
  [DefaultValue(null)]
  public SpawnData[]? spawn;
  [DefaultValue(null)]
  public string[]? spawns;
  [DefaultValue(null)]
  public float? spawnDelay;
  [DefaultValue(null)]
  public string? remove;
  [DefaultValue(null)]
  public string? removeDelay;
  [DefaultValue("")]
  public string drops = "";
  [DefaultValue("")]
  public string data = "";
  [DefaultValue(null)]
  public string? command;
  [DefaultValue(null)]
  public string[]? commands;
  [DefaultValue(null)]
  public string? day;
  [DefaultValue(null)]
  public string? night;
  [DefaultValue("")]
  public string biomes = "";
  [DefaultValue(null)]
  public string? minDistance;
  [DefaultValue(null)]
  public string? maxDistance;
  [DefaultValue(null)]
  public string? minAltitude;
  [DefaultValue(null)]
  public string? maxAltitude;
  [DefaultValue(null)]
  public string? minY;
  [DefaultValue(null)]
  public string? maxY;
  [DefaultValue("")]
  public string environments = "";
  [DefaultValue("")]
  public string bannedEnvironments = "";
  [DefaultValue("")]
  public string globalKeys = "";
  [DefaultValue("")]
  public string bannedGlobalKeys = "";
  [DefaultValue("")]
  public string events = "";
  [DefaultValue(null)]
  public float? eventDistance;
  [DefaultValue(null)]
  public PokeData[]? poke;
  [DefaultValue(null)]
  public string[]? pokes;
  [DefaultValue(0)]
  public int pokeLimit = 0;
  [DefaultValue("")]
  public string pokeParameter = "";
  [DefaultValue(0f)]
  public float pokeDelay = 0f;

  [DefaultValue(null)]
  public ObjectData[]? objects;
  [DefaultValue(null)]
  public string? objectsLimit;
  [DefaultValue(null)]
  public ObjectData[]? bannedObjects;
  [DefaultValue(null)]
  public string? bannedObjectsLimit;
  [DefaultValue("")]
  public string locations = "";
  [DefaultValue(0f)]
  public float locationDistance = 0f;
  [DefaultValue(null)]
  public string? filter = null;
  [DefaultValue(null)]
  public string[]? filters = null;
  [DefaultValue(null)]
  public string? bannedFilter = null;
  [DefaultValue(null)]
  public string[]? bannedFilters = null;
  [DefaultValue(null)]
  public float? delay;

  [DefaultValue(null)]
  public bool? triggerRules = null;
  [DefaultValue(null)]
  public Dictionary<string, string>[]? objectRpc = null;
  [DefaultValue(null)]
  public Dictionary<string, string>[]? clientRpc = null;

  [DefaultValue("")]
  public string minPaint = "";
  [DefaultValue("")]
  public string maxPaint = "";
  [DefaultValue("")]
  public string paint = "";

  [DefaultValue(false)]
  public bool injectData = false;
  [DefaultValue("")]
  public string addItems = "";
  [DefaultValue("")]
  public string removeItems = "";
  [DefaultValue(null)]
  public string? cancel;
}


public class Info
{
  public string Prefabs = "";
  public ActionType Type = ActionType.Create;
  public bool Fallback = false;
  public string[] Args = [];
  public IFloatValue? Weight;
  public Spawn[]? Swaps;
  public Spawn[]? Spawns;
  public IBoolValue? Remove;
  public bool Regenerate = false;
  public IFloatValue? RemoveDelay;
  public IBoolValue? Drops;
  public IStringValue? Data;
  public bool InjectData = false;
  public string[] Commands = [];
  public IBoolValue? Day;
  public IBoolValue? Night;
  public IFloatValue? MinDistance;
  public IFloatValue? MaxDistance;
  public IFloatValue? MinY;
  public IFloatValue? MaxY;
  public IFloatValue? MinAltitude;
  public IFloatValue? MaxAltitude;
  public Heightmap.Biome Biomes = Heightmap.Biome.None;
  public float EventDistance = 0f;
  public HashSet<string> Events = [];
  public HashSet<string> Environments = [];
  public HashSet<string> BannedEnvironments = [];
  public List<string> GlobalKeys = [];
  public List<string> BannedGlobalKeys = [];
  public Object[] LegacyPokes = [];
  public Poke[] Pokes = [];
  public int PokeLimit = 0;
  public string PokeParameter = "";
  public float PokeDelay = 0f;
  public Range<int>? ObjectsLimit;
  public Object[]? Objects;
  public Range<int>? BannedObjectsLimit;
  public Object[]? BannedObjects;
  public HashSet<string> Locations = [];
  public float LocationDistance = 0f;
  public DataEntry? Filter;
  public DataEntry? BannedFilter;
  public bool TriggerRules;
  public ObjectRpcInfo[]? ObjectRpcs;
  public ClientRpcInfo[]? ClientRpcs;
  public Color? MinPaint;
  public Color? MaxPaint;
  public DataEntry? AddItems;
  public DataEntry? RemoveItems;
  public IBoolValue? Cancel;
}

public class SpawnData
{
  [DefaultValue(null)]
  public string? prefab;
  [DefaultValue(null)]
  public string? pos;
  [DefaultValue(null)]
  public string? rot;
  [DefaultValue(null)]
  public string? data;
  [DefaultValue(null)]
  public string? delay;
  [DefaultValue(null)]
  public string? triggerRules;

}
public class Spawn
{
  private readonly IPrefabValue Prefab;
  public readonly IVector3Value? Pos;
  public readonly IBoolValue? Snap;
  public readonly IQuaternionValue? Rot;
  public readonly IStringValue? Data;
  public readonly IFloatValue? Delay;
  public readonly IBoolValue? TriggerRules;

  public Spawn(SpawnData data, float? delay, bool? triggerRules)
  {
    Prefab = data.prefab == null ? new SimplePrefabValue(0) : DataValue.Prefab(data.prefab);
    Pos = data.pos == null ? null : DataValue.Vector3(data.pos);
    Snap = data.pos == null ? null : new SimpleBoolValue(false);
    Rot = data.rot == null ? null : DataValue.Quaternion(data.rot);
    Data = data.data == null ? null : DataValue.String(data.data);
    Delay = data.delay == null ? delay == null ? null : new SimpleFloatValue(delay.Value) : DataValue.Float(data.delay);
    TriggerRules = data.triggerRules == null ? triggerRules == null ? null : new SimpleBoolValue(triggerRules.Value) : DataValue.Bool(data.triggerRules);
  }

  public Spawn(string line, float? delay, bool? triggerRules)
  {
    Delay = delay == null ? null : new SimpleFloatValue(delay.Value);
    TriggerRules = triggerRules == null ? null : new SimpleBoolValue(triggerRules.Value);
    var split = Parse.ToList(line);
    Prefab = DataValue.Prefab(split[0]);
    var posParsed = false;
    for (var i = 1; i < split.Count; i++)
    {
      var value = split[i];
      if (Parse.TryBoolean(value, out var boolean))
        TriggerRules = new SimpleBoolValue(boolean);
      else if (Parse.TryFloat(value, out var number1))
      {
        if (split.Count <= i + 2)
          Delay = new SimpleFloatValue(number1);
        else if (Parse.TryFloat(split[i + 1], out var number2))
        {
          var number3 = Parse.Float(split[i + 2]);
          if (posParsed)
          {
            Rot = new SimpleQuaternionValue(Quaternion.Euler(number2, number1, number3));
          }
          else
          {
            Pos = new SimpleVector3Value(new Vector3(number1, number3, number2));
            if (split[i + 2] == "snap")
              Snap = new SimpleBoolValue(true);
            posParsed = true;
          }
          i += 2;
        }
        else
          Delay = new SimpleFloatValue(number1);
      }
      else
        Data = new SimpleStringValue(value);
    }

  }
  public int GetPrefab(Parameters pars) => Prefab.Get(pars) ?? 0;
}

public class Poke(PokeData data)
{
  public Object Filter = new(data);
  public IStringValue? Parameter = data.parameter == null ? null : DataValue.String(data.parameter);
  public IIntValue? Limit = data.limit == null ? null : DataValue.Int(data.limit);
  public IFloatValue? Delay = data.delay == null ? null : DataValue.Float(data.delay);
}
public class Object
{
  private readonly IPrefabValue PrefabsValue;
  private readonly IFloatValue? MinDistanceValue;
  private readonly IFloatValue MaxDistanceValue;
  private readonly IFloatValue? MinHeightValue;
  private readonly IFloatValue? MaxHeightValue;
  private readonly int Data = 0;
  private readonly IIntValue WeightValue = new SimpleIntValue(1);

  public Object(ObjectData data)
  {

    PrefabsValue = DataValue.Prefab(data.prefab);
    if (data.minDistance != null)
      MinDistanceValue = DataValue.Float(data.minDistance);
    if (data.maxDistance != null)
      MaxDistanceValue = DataValue.Float(data.maxDistance);
    else
      MaxDistanceValue = new SimpleFloatValue(100);
    if (data.minHeight != null)
      MinHeightValue = DataValue.Float(data.minHeight);
    if (data.maxHeight != null)
      MaxHeightValue = DataValue.Float(data.maxHeight);
    if (data.data != null)
    {
      Data = data.data.GetStableHashCode();
      if (!DataHelper.Exists(Data))
      {
        Log.Error($"Invalid object filter data: {data.data}");
        Data = 0;
      }
    }
  }
  public Object(string line)
  {
    var split = Parse.ToList(line);
    PrefabsValue = DataValue.Prefab(split[0]);
    MaxDistanceValue = new SimpleFloatValue(100f);

    if (split.Count > 1)
    {
      var range = Parse.StringRange(split[1]);
      if (range.Min != range.Max)
        MinDistanceValue = DataValue.Float(range.Min);
      MaxDistanceValue = DataValue.Float(range.Max);
    }
    if (split.Count > 2)
    {
      Data = split[2].GetStableHashCode();
      if (!DataHelper.Exists(Data))
      {
        Log.Error($"Invalid object filter data: {split[2]}");
        Data = 0;
      }
    }
    if (split.Count > 3)
    {
      WeightValue = DataValue.Int(split[3]);
    }
    if (split.Count > 4)
    {
      var range = Parse.StringRange(split[4]);
      if (range.Min != range.Max)
        MinHeightValue = DataValue.Float(range.Min);
      MaxHeightValue = DataValue.Float(range.Max);
    }
  }
  private float? MinDistance;
  public float MaxDistance;
  private float? MinHeight;
  private float? MaxHeight;
  public int Weight;

  public void Roll(Parameters pars)
  {
    MinDistance = MinDistanceValue?.Get(pars);
    MaxDistance = MaxDistanceValue.Get(pars) ?? 100f;
    MinHeight = MinHeightValue?.Get(pars);
    MaxHeight = MaxHeightValue?.Get(pars);
    Weight = WeightValue.Get(pars) ?? 1;
  }

  public bool IsValid(ZDO zdo, Vector3 pos, Parameters pars)
  {
    if (PrefabsValue.Match(pars, zdo.GetPrefab()) == false) return false;
    var d = Utils.DistanceXZ(pos, zdo.GetPosition());
    if (MinDistance != null && d < MinDistance) return false;
    if (d > MaxDistance) return false;
    var dy = Mathf.Abs(pos.y - zdo.GetPosition().y);
    if (MinHeight != null && dy < MinHeight) return false;
    if (MaxHeight != null && dy > MaxHeight) return false;

    if (Data == 0) return true;
    return DataHelper.Match(Data, zdo, pars);
  }
}

public class PokeData : ObjectData
{
  [DefaultValue(null)]
  public string? delay;
  [DefaultValue(null)]
  public string? parameter;
  [DefaultValue(null)]
  public string? limit;
}
public class ObjectData
{
  [DefaultValue("")]
  public string prefab = "";
  [DefaultValue(null)]
  public string? maxDistance;
  [DefaultValue(null)]
  public string? minDistance;
  [DefaultValue(null)]
  public string? maxHeight;
  [DefaultValue(null)]
  public string? minHeight;
  [DefaultValue(null)]
  public string? data;
}


public class InfoType
{
  public readonly ActionType Type;
  public readonly string[] Parameters;
  public InfoType(string prefab, string line)
  {
    var types = Parse.ToList(line);
    if (types.Count == 0 || !Enum.TryParse(types[0], true, out Type))
    {
      if (line == "")
        Log.Warning($"Missing type for prefab {prefab}.");
      else
        Log.Error($"Failed to parse type {prefab}.");
      Type = ActionType.Create;
    }
    Parameters = types.Count > 1 ? types[1].Split(' ') : [];
  }
}