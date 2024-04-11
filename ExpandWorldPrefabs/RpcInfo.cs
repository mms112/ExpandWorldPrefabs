using System.Collections.Generic;
using System.Linq;
using Data;
using Service;
using UnityEngine;

namespace ExpandWorld.Prefab;


public class RpcMapping(string call, string[] types, object[]? baseParameters = null)
{
  private static readonly int DamageHash = "Damage".GetStableHashCode();
  private static readonly int WNTDamageHash = "WNTDamage".GetStableHashCode();
  private static readonly Dictionary<int, bool> IsWearNTear = [];
  private readonly int CallHash = call.GetStableHashCode();
  public int GetHash()
  {
    if (CallHash != DamageHash) return CallHash;
    // Currently only damage hash is dynamic so the logic can be hardcoded.
    if (IsWearNTear.TryGetValue(CallHash, out var value)) return value ? WNTDamageHash : CallHash;
    var isWNT = ZNetScene.instance.GetPrefab(call)?.GetComponent<WearNTear>() != null;
    IsWearNTear[CallHash] = isWNT;
    return isWNT ? WNTDamageHash : CallHash;
  }
  private readonly string[] Types = types;
  private readonly object[] BaseParameters = baseParameters ?? [];

  public object[] GetParameters(string pars, ZDO zdo, Dictionary<string, string> parameters)
  {
    var result = BaseParameters.ToList();
    var args = Parse.Split(Helper.ReplaceParameters(pars, parameters, zdo));
    for (var i = 0; i < Types.Length; i++)
    {
      var type = Types[i];
      var arg = args.Length <= i ? "" : args[i];
      if (type == "int") result.Add(Calculator.EvaluateInt(arg) ?? 0);
      if (type == "float") result.Add(Calculator.EvaluateFloat(arg) ?? 0f);
      if (type == "bool") result.Add(Parse.BooleanTrue(arg));
      if (type == "long") result.Add(Calculator.EvaluateLong(arg) ?? 0L);
      if (type == "string") result.Add(arg);
      if (type == "vec")
      {
        result.Add(Calculator.EvaluateVector3(args, i));
        i += 2;
      }
      if (type == "quat")
      {
        result.Add(Calculator.EvaluateQuaternion(args, i));
        i += 2;
      }
      if (type == "hash") result.Add(arg.GetStableHashCode());
      if (type == "hit") result.Add(Parse.Hit(zdo, arg));
      if (type == "zdo") result.Add(Parse.ZDOID(arg));
      if (type == "enum_message") result.Add(Parse.EnumMessage(arg));
      if (type == "enum_reason") result.Add(Parse.EnumReason(arg));
      if (type == "enum_trap") result.Add(Parse.EnumTrap(arg));
    }
    return [.. result];
  }


  public static readonly Dictionary<int, RpcMapping> Mappings = new(){
    { "damage".GetStableHashCode(), new RpcMapping("Damage", ["hit"]) },
    { "heal".GetStableHashCode(), new RpcMapping("Heal", ["float", "bool"]) },
    { "stamina".GetStableHashCode(), new RpcMapping("UseStamina", ["float"]) },
    { "message".GetStableHashCode(), new RpcMapping("Message", ["string", "int"], [1]) },
    { "broadcast".GetStableHashCode(), new RpcMapping("Message", ["string", "int"], [2]) },
    { "forward".GetStableHashCode(), new RpcMapping("Forward", []) },
    { "backward".GetStableHashCode(), new RpcMapping("Backward", []) },
    { "rudder".GetStableHashCode(), new RpcMapping("Rudder", ["float"]) },
    { "stop".GetStableHashCode(), new RpcMapping("Stop", []) },
    { "trap".GetStableHashCode(), new RpcMapping("RPC_RequestStateChange", ["enum_trap"]) },
    { "ammo".GetStableHashCode(), new RpcMapping("RPC_AddAmmo", ["string"]) },
    { "repair".GetStableHashCode(), new RpcMapping("WNTRepair", []) },
    { "remove".GetStableHashCode(), new RpcMapping("WNTRemove", []) },
    { "stagger".GetStableHashCode(), new RpcMapping("Stagger", ["vec"]) },
    { "aggravate".GetStableHashCode(), new RpcMapping("SetAggravated", [], [true]) },
    { "calm".GetStableHashCode(), new RpcMapping("SetAggravated", [], [false]) },
    { "pick".GetStableHashCode(), new RpcMapping("Pick", []) },
    { "alert".GetStableHashCode(), new RpcMapping("Alert", []) },
    { "door".GetStableHashCode(), new RpcMapping("UseDoor", ["bool"]) },
    { "teleport".GetStableHashCode(), new RpcMapping("RPC_TeleportTo", ["vec", "quat", "bool"]) },
    { "status".GetStableHashCode(), new RpcMapping("RPC_AddStatusEffect", ["hash", "bool", "int", "float"]) },
  };
}


public class SimpleRpcInfo
{
  private readonly string Parameters = "";
  private readonly float Delay;
  private readonly RpcMapping? Mapping;

  public SimpleRpcInfo(string line, float delay)
  {
    Delay = delay;
    var split = Parse.SplitWithEscape(line);
    var hash = split[0].ToLowerInvariant().GetStableHashCode();
    if (RpcMapping.Mappings.TryGetValue(hash, out var mapping))
    {
      Mapping = mapping;
      Parameters = string.Join(",", split.Skip(1));
    }
    else
    {
      Log.Warning($"Unknown RPC: {split[0]}");
    }
  }
  public void Invoke(ZDO zdo, Dictionary<string, string> parameters)
  {
    if (Mapping == null) return;
    if (zdo.GetOwner() == 0) return;
    var pars = Mapping.GetParameters(Parameters, zdo, parameters);
    DelayedRpc.Add(Delay, ZRoutedRpc.instance.m_id, zdo.GetOwner(), zdo.m_uid, Mapping.GetHash(), pars);
  }
}
public enum RpcTarget
{
  All,
  Owner,
  Target,
  ZDO
}

public abstract class RpcInfo
{
  protected abstract ZDOID GetId(ZDO zdo);
  private static readonly HashSet<string> Types = ["int", "long", "float", "bool", "string", "vec", "quat", "hash", "hit", "enum_reason", "enum_message", "enum_trap", "zdo"];
  public static bool IsType(string line) => Types.Contains(Parse.Kvp(line).Key);
  private readonly int Hash;
  private readonly RpcTarget Target;
  private readonly string? TargetParameter;
  private readonly string? SourceParameter;
  private readonly string[] Parameters;
  private readonly float Delay;
  public bool IsTarget => Target == RpcTarget.Target;

  public RpcInfo(string[] lines, float delay, string? rpcSource)
  {
    var first = Parse.Split(lines[0]);
    Hash = first[0].GetStableHashCode();
    Target = RpcTarget.Owner;
    SourceParameter = rpcSource;
    if (first.Length > 1)
    {
      if (first[1] == "all")
        Target = RpcTarget.All;
      else if (first[1] == "target")
        Target = RpcTarget.Target;
      else if (first[1] == "owner")
        Target = RpcTarget.Owner;
      else if (first[1] == "zdo")
      {
        Target = RpcTarget.ZDO;
        TargetParameter = first[1];
      }
    }
    Delay = first.Length < 3 ? delay : Parse.Float(first[2], delay);
    Parameters = lines.Skip(1).ToArray();
  }
  public void Invoke(ZDO zdo, Dictionary<string, string> parameters, PlayerInfo[]? players)
  {
    var source = ZRoutedRpc.instance.m_id;
    if (SourceParameter != null)
    {
      var id = Parse.ZDOID(Helper.ReplaceParameters(SourceParameter, parameters, zdo));
      source = ZDOMan.instance.GetZDO(id)?.GetOwner() ?? 0;
    }
    var pars = GetParameters(zdo, parameters);
    if (Target == RpcTarget.Owner)
      DelayedRpc.Add(Delay, source, zdo.GetOwner(), GetId(zdo), Hash, pars);
    else if (Target == RpcTarget.All)
      DelayedRpc.Add(Delay, source, ZRoutedRpc.Everybody, GetId(zdo), Hash, pars);
    else if (Target == RpcTarget.ZDO)
    {
      var id = Parse.ZDOID(Helper.ReplaceParameters(TargetParameter ?? "", parameters, zdo));
      var peerId = ZDOMan.instance.GetZDO(id)?.GetOwner();
      if (peerId.HasValue)
        DelayedRpc.Add(Delay, source, peerId.Value, GetId(zdo), Hash, pars);
    }
    else if (Target == RpcTarget.Target && players != null)
      foreach (var player in players)
        DelayedRpc.Add(Delay, source, player.PeerId, GetId(zdo), Hash, pars);
  }
  private object[] GetParameters(ZDO zdo, Dictionary<string, string> parameters)
  {
    var pars = Parameters.Select(p => Helper.ReplaceParameters(p, parameters, zdo)).ToArray<object>();
    for (var i = 0; i < pars.Length; i++)
    {
      var split = Parse.Kvp((string)pars[i]);
      var type = split.Key;
      var arg = split.Value;
      if (type == "int") pars[i] = Calculator.EvaluateInt(arg) ?? 0;
      if (type == "long") pars[i] = Calculator.EvaluateLong(arg) ?? 0;
      if (type == "float") pars[i] = Calculator.EvaluateFloat(arg) ?? 0f;
      if (type == "bool") pars[i] = Parse.Boolean(arg);
      if (type == "string") pars[i] = arg;
      if (type == "vec") pars[i] = Calculator.EvaluateVector3(arg);
      if (type == "quat") pars[i] = Calculator.EvaluateQuaternion(arg);
      if (type == "hash") pars[i] = arg.GetStableHashCode();
      if (type == "hit") pars[i] = Parse.Hit(zdo, arg);
      if (type == "zdo") pars[i] = Parse.ZDOID(arg);
      if (type == "enum_message") pars[i] = Parse.EnumMessage(arg);
      if (type == "enum_reason") pars[i] = Parse.EnumReason(arg);
      if (type == "enum_trap") pars[i] = Parse.EnumTrap(arg);
    }
    return pars;
  }
}


public class ObjectRpcInfo(string[] lines, float delay, string? rpcSource) : RpcInfo(lines, delay, rpcSource)
{
  protected override ZDOID GetId(ZDO zdo) => zdo.m_uid;
}
public class ClientRpcInfo(string[] lines, float delay, string? rpcSource) : RpcInfo(lines, delay, rpcSource)
{
  protected override ZDOID GetId(ZDO zdo) => ZDOID.None;
}