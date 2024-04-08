using System.Collections.Generic;
using System.Linq;
using Service;

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
    var args = Parse.Split(Helper.ReplaceParameters(pars, parameters));
    for (var i = 0; i < Types.Length; i++)
    {
      var type = Types[i];
      var arg = args[i];
      if (type == "int") result.Add(Parse.Int(arg));
      if (type == "float") result.Add(Parse.Float(arg));
      if (type == "bool") result.Add(Parse.BooleanTrue(arg));
      if (type == "string") result.Add(arg);
      if (type == "vec")
      {
        result.Add(Parse.VectorXZY(args, i));
        i += 2;
      }
      if (type == "quat")
      {
        result.Add(Parse.AngleYXZ(args, i));
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


public class RpcInfo
{
  private readonly string Parameters = "";
  private readonly float Delay;
  private readonly RpcMapping? Mapping;

  public RpcInfo(string line, float delay)
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
    DelayedRpc.Add(Delay, zdo.GetOwner(), zdo.m_uid, Mapping.GetHash(), pars);
  }
}


public class CustomRpcInfo
{
  private static readonly HashSet<string> Types = ["int", "float", "bool", "string", "vec", "quat", "hash", "hit", "enum_reason", "enum_message", "enum_trap", "zdo"];
  public static bool IsType(string line) => Types.Contains(Parse.Kvp(line).Key);
  private readonly int Hash;
  private readonly long? FixedTarget;
  private readonly string? VariableTarget;
  private readonly string[] Parameters;
  private readonly float Delay;

  public CustomRpcInfo(string[] lines, float delay)
  {
    var first = Parse.Split(lines[0]);
    Hash = first[0].GetStableHashCode();
    if (first.Length > 1)
    {
      if (first[1] == "all")
        FixedTarget = ZNetView.Everybody;
      else if (first[1] == "owner")
        FixedTarget = null;
      else if (Parse.TryLong(first[1], out var target))
        FixedTarget = target;
      else
        VariableTarget = first[1];
    }
    Delay = first.Length < 3 ? delay : Parse.Float(first[2], delay);
    Parameters = lines.Skip(1).ToArray();
  }
  public void Invoke(ZDO zdo, Dictionary<string, string> parameters)
  {
    var target = zdo.GetOwner();
    if (FixedTarget.HasValue) target = FixedTarget.Value;
    if (VariableTarget != null) target = Parse.Long(Helper.ReplaceParameters(VariableTarget, parameters));
    DelayedRpc.Add(Delay, target, zdo.m_uid, Hash, GetParameters(zdo, parameters));
  }
  private object[] GetParameters(ZDO zdo, Dictionary<string, string> parameters)
  {
    var pars = Parameters.Select(p => Helper.ReplaceParameters(p, parameters)).ToArray<object>();
    for (var i = 0; i < pars.Length; i++)
    {
      var split = Parse.Kvp((string)pars[i]);
      var type = split.Key;
      var arg = split.Value;
      if (type == "int") pars[i] = Parse.Int(arg);
      if (type == "float") pars[i] = Parse.Float(arg);
      if (type == "bool") pars[i] = Parse.Boolean(arg);
      if (type == "string") pars[i] = arg;
      if (type == "vec") pars[i] = Parse.VectorXZY(arg);
      if (type == "quat") pars[i] = Parse.AngleYXZ(arg);
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