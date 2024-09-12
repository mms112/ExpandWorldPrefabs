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
  public string[]? types = null;
  [DefaultValue(false)]
  public bool fallback = false;
  [DefaultValue("1f")]
  public string weight = "1";
  [DefaultValue(null)]
  public string? swap = null;
  [DefaultValue(null)]
  public string[]? swaps = null;
  [DefaultValue(null)]
  public string? spawn = null;
  [DefaultValue(null)]
  public string[]? spawns = null;
  [DefaultValue(0f)]
  public float spawnDelay = 0f;
  [DefaultValue("")]
  public string remove = "";
  [DefaultValue("")]
  public string removeDelay = "";
  [DefaultValue("")]
  public string drops = "";
  [DefaultValue("")]
  public string data = "";
  [DefaultValue(null)]
  public string? command = null;
  [DefaultValue(null)]
  public string[]? commands = null;
  [DefaultValue("true")]
  public string day = "true";
  [DefaultValue("true")]
  public string night = "true";
  [DefaultValue("")]
  public string biomes = "";
  [DefaultValue("")]
  public string minDistance = "";
  [DefaultValue("")]
  public string maxDistance = "";
  [DefaultValue("")]
  public string minAltitude = "";
  [DefaultValue("")]
  public string maxAltitude = "";
  [DefaultValue("")]
  public string minY = "";
  [DefaultValue("")]
  public string maxY = "";
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
  public float? eventDistance = null;
  [DefaultValue(null)]
  public PokeData[]? poke = null;
  [DefaultValue(null)]
  public string[]? pokes = null;
  [DefaultValue(0)]
  public int pokeLimit = 0;
  [DefaultValue("")]
  public string pokeParameter = "";
  [DefaultValue(0f)]
  public float pokeDelay = 0f;

  [DefaultValue(null)]
  public string[]? objects = null;
  [DefaultValue("")]
  public string objectsLimit = "";
  [DefaultValue(null)]
  public string[]? bannedObjects = null;
  [DefaultValue("")]
  public string bannedObjectsLimit = "";
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
  [DefaultValue(0f)]
  public float delay = 0f;

  [DefaultValue(false)]
  public bool triggerRules = false;
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
  [DefaultValue(false)]
  public bool cancel = false;
}


public class Info
{
  public string Prefabs = "";
  public ActionType Type = ActionType.Create;
  public bool Fallback = false;
  public string[] Args = [];
  public IFloatValue Weight = new SimpleFloatValue(1f);
  public Spawn[] Swaps = [];
  public Spawn[] Spawns = [];
  public IBoolValue Remove = new SimpleBoolValue(false);
  public bool Regenerate = false;
  public IFloatValue RemoveDelay = new SimpleFloatValue(0f);
  public IBoolValue Drops = new SimpleBoolValue(false);
  public IStringValue Data = new SimpleStringValue("");
  public bool InjectData = false;
  public string[] Commands = [];
  public IBoolValue Day = new SimpleBoolValue(true);
  public IBoolValue Night = new SimpleBoolValue(true);
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
  public Range<int>? ObjectsLimit = null;
  public Object[] Objects = [];
  public Range<int>? BannedObjectsLimit = null;
  public Object[] BannedObjects = [];
  public HashSet<string> Locations = [];
  public float LocationDistance = 0f;
  public DataEntry? Filter;
  public DataEntry? BannedFilter;
  public bool TriggerRules = false;
  public ObjectRpcInfo[]? ObjectRpcs;
  public ClientRpcInfo[]? ClientRpcs;
  public Color? MinPaint;
  public Color? MaxPaint;
  public DataEntry? AddItems;
  public DataEntry? RemoveItems;
  public bool Cancel;
}
public class Spawn
{
  private readonly IPrefabValue Prefab;
  public readonly Vector3 Pos = Vector3.zero;
  public readonly bool Snap = false;
  public readonly Quaternion Rot = Quaternion.identity;
  public readonly string Data = "";
  public readonly float Delay = 0;
  public readonly bool TriggerRules = false;
  public Spawn(string line, float delay, bool triggerRules)
  {
    Delay = delay;
    TriggerRules = triggerRules;
    var split = Parse.ToList(line);
    Prefab = DataValue.Prefab(split[0]);
    var posParsed = false;
    for (var i = 1; i < split.Count; i++)
    {
      var value = split[i];
      if (Parse.TryBoolean(value, out var boolean))
        TriggerRules = boolean;
      else if (Parse.TryFloat(value, out var number1))
      {
        if (split.Count <= i + 2)
          Delay = number1;
        else if (Parse.TryFloat(split[i + 1], out var number2))
        {
          var number3 = Parse.Float(split[i + 2]);
          if (posParsed)
          {
            Rot = Quaternion.Euler(number2, number1, number3);
          }
          else
          {
            Pos = new Vector3(number1, number3, number2);
            if (split[i + 2] == "snap")
              Snap = true;
            posParsed = true;
          }
          i += 2;
        }
        else
          Delay = number1;
      }
      else
        Data = value;
    }

  }
  public int GetPrefab(Parameters pars) => Prefab.Get(pars) ?? 0;
}

public class Poke(PokeData data)
{
  public Object Filter = new(data.prefab, data.minDistance, data.maxDistance, data.minHeight, data.maxHeight, data.data);
  public IStringValue Parameter = DataValue.String(data.parameter);
  public IIntValue Limit = DataValue.Int(data.limit);
  public IFloatValue Delay = DataValue.Float(data.delay);
}
public class Object
{
  private readonly IPrefabValue PrefabsValue;
  private readonly IFloatValue MinDistanceValue;
  private readonly IFloatValue MaxDistanceValue;
  private readonly IFloatValue? MinHeightValue;
  private readonly IFloatValue? MaxHeightValue;
  private readonly int Data = 0;
  private readonly IIntValue WeightValue = new SimpleIntValue(1);

  public Object(string prefab, string minDistance, string maxDistance, string minHeight, string maxHeight, string data)
  {
    PrefabsValue = DataValue.Prefab(prefab);
    MinDistanceValue = DataValue.Float(minDistance);
    if (maxDistance != "")
      MaxDistanceValue = DataValue.Float(maxDistance);
    else
      MaxDistanceValue = new SimpleFloatValue(100);
    if (minHeight != "")
      MinHeightValue = DataValue.Float(minHeight);
    if (maxHeight != "")
      MaxHeightValue = DataValue.Float(maxHeight);
    if (data != "")
    {
      Data = data.GetStableHashCode();
      if (!DataHelper.Exists(Data))
      {
        Log.Error($"Invalid object filter data: {data}");
        Data = 0;
      }
    }
  }
  public Object(string line)
  {
    var split = Parse.ToList(line);
    PrefabsValue = DataValue.Prefab(split[0]);
    MinDistanceValue = new SimpleFloatValue(0f);
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
    MinDistance = MinDistanceValue.Get(pars);
    MaxDistance = MaxDistanceValue.Get(pars) ?? 100f;
    MinHeight = MinHeightValue?.Get(pars);
    MaxHeight = MaxHeightValue?.Get(pars);
    Weight = WeightValue.Get(pars) ?? 1;
  }

  public bool IsValid(ZDO zdo, Vector3 pos, Parameters pars)
  {
    var d = Utils.DistanceXZ(pos, zdo.GetPosition());
    if (MinDistance != null && d < MinDistance) return false;
    if (d > MaxDistance) return false;
    var dy = Mathf.Abs(pos.y - zdo.GetPosition().y);
    if (MinHeight != null && dy < MinHeight) return false;
    if (MaxHeight != null && dy > MaxHeight) return false;

    if (PrefabsValue.Match(pars, zdo.GetPrefab()) == false) return false;
    if (Data == 0) return true;
    return DataHelper.Match(Data, zdo, pars);
  }
}

public class PokeData
{
  [DefaultValue("")]
  public string prefab = "";
  [DefaultValue("0f")]
  public string delay = "0f";
  [DefaultValue("")]
  public string parameter = "";
  [DefaultValue("")]
  public string maxDistance = "";
  [DefaultValue("")]
  public string minDistance = "";
  [DefaultValue("")]
  public string maxHeight = "";
  [DefaultValue("")]
  public string minHeight = "";
  [DefaultValue("0")]
  public string limit = "0";
  [DefaultValue("")]
  public string data = "";
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