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
  public TerrainData[]? terrain;

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

  [DefaultValue(null)]
  public string? owner;
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
  public Object[]? LegacyPokes;
  public Poke[]? Pokes;
  public Terrain[]? Terrains;
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
  public ILongValue? Owner;
  public IBoolValue? Cancel;
}

public class SpawnData
{
  [DefaultValue(null)]
  public string? prefab;
  [DefaultValue(null)]
  public string? snap;
  [DefaultValue(null)]
  public string? pos;
  [DefaultValue(null)]
  public string? position;
  [DefaultValue(null)]
  public string? rot;
  [DefaultValue(null)]
  public string? rotation;
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
    Pos = data.pos != null ? DataValue.Vector3(data.pos) : data.position != null ? DataValue.Vector3(data.position) : null;
    Snap = data.snap == null ? null : DataValue.Bool(data.snap);
    Rot = data.rot != null ? DataValue.Quaternion(data.rot) : data.rotation != null ? DataValue.Quaternion(data.rotation) : null;
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
  public IBoolValue? Self = data.self == null ? null : DataValue.Bool(data.self);
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
  public string? self;
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


public class TerrainData
{
  [DefaultValue(null)]
  public string? delay;
  [DefaultValue(null)]
  public string? pos;
  [DefaultValue(null)]
  public string? position;
  [DefaultValue(null)]
  public string? square;
  [DefaultValue(null)]
  public string? levelRadius;
  [DefaultValue(null)]
  public string? levelOffset;
  [DefaultValue(null)]
  public string? raiseRadius;
  [DefaultValue(null)]
  public string? raisePower;
  [DefaultValue(null)]
  public string? raiseDelta;
  [DefaultValue(null)]
  public string? smoothRadius;
  [DefaultValue(null)]
  public string? smoothPower;
  [DefaultValue(null)]
  public string? paintRadius;
  [DefaultValue(null)]
  public string? paintHeightCheck;
  [DefaultValue(null)]
  public string? paint;
}

public class Terrain(TerrainData data)
{
  public readonly IFloatValue? Delay = data.delay == null ? null : DataValue.Float(data.delay);
  public readonly IVector3Value? Position = data.pos != null ? DataValue.Vector3(data.pos) : data.position != null ? DataValue.Vector3(data.position) : null;
  public readonly IBoolValue? Square = data.square == null ? null : DataValue.Bool(data.square);
  public readonly IFloatValue? LevelRadius = data.levelRadius == null ? null : DataValue.Float(data.levelRadius);
  public readonly IFloatValue? LevelOffset = data.levelOffset == null ? null : DataValue.Float(data.levelOffset);
  public readonly IFloatValue? RaiseRadius = data.raiseRadius == null ? null : DataValue.Float(data.raiseRadius);
  public readonly IFloatValue? RaisePower = data.raisePower == null ? null : DataValue.Float(data.raisePower);
  public readonly IFloatValue? RaiseDelta = data.raiseDelta == null ? null : DataValue.Float(data.raiseDelta);
  public readonly IFloatValue? SmoothRadius = data.smoothRadius == null ? null : DataValue.Float(data.smoothRadius);
  public readonly IFloatValue? SmoothPower = data.smoothPower == null ? null : DataValue.Float(data.smoothPower);
  public readonly IFloatValue? PaintRadius = data.paintRadius == null ? null : DataValue.Float(data.paintRadius);
  public readonly IBoolValue? PaintHeightCheck = data.paintHeightCheck == null ? null : DataValue.Bool(data.paintHeightCheck);
  public readonly IStringValue? Paint = data.paint == null ? null : DataValue.String(data.paint);

  public void Get(Parameters pars, Vector3 basePosition, Quaternion baseRotation, out Vector3 pos, out float size, out ZPackage pkg)
  {
    pos = basePosition;
    pos += baseRotation * (Position?.Get(pars) ?? Vector3.zero);
    pkg = new ZPackage();
    pkg.Write(pos);
    pkg.Write(LevelOffset?.Get(pars) ?? 0f);
    var levelRadius = LevelRadius?.Get(pars) ?? 0f;
    pkg.Write(levelRadius > 0f);
    pkg.Write(levelRadius);
    pkg.Write(Square?.GetBool(pars) == true);
    var raiseRadius = RaiseRadius?.Get(pars) ?? 0f;
    pkg.Write(raiseRadius > 0f);
    pkg.Write(raiseRadius);
    pkg.Write(RaisePower?.Get(pars) ?? 0f);
    pkg.Write(RaiseDelta?.Get(pars) ?? 0f);
    var smoothRadius = SmoothRadius?.Get(pars) ?? 0f;
    pkg.Write(smoothRadius > 0f);
    pkg.Write(smoothRadius);
    pkg.Write(SmoothPower?.Get(pars) ?? 0f);
    var paintRadius = PaintRadius?.Get(pars) ?? 0f;
    pkg.Write(paintRadius > 0f);
    pkg.Write(PaintHeightCheck?.GetBool(pars) == true);
    var paint = Paint?.Get(pars) ?? "Reset";
    var paintEnum =
      Enum.TryParse(paint, true, out TerrainModifier.PaintType paintType) ? paintType :
      int.TryParse(paint, out var paintInt) ? (TerrainModifier.PaintType)paintInt :
      TerrainModifier.PaintType.Reset;
    pkg.Write((int)paintEnum);
    pkg.Write(paintRadius);
    size = Mathf.Max(levelRadius, raiseRadius, smoothRadius, paintRadius);
  }
}