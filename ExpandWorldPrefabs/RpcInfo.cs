using System;
using System.Collections.Generic;
using System.Linq;
using Service;

namespace ExpandWorld.Prefab;


public class RpcInfo
{
  private readonly string Name;
  private readonly string[] Parameters;
  private readonly float Delay;

  public RpcInfo(string line, float delay)
  {
    Delay = delay;
    var split = Parse.SplitWithEscape(line);
    Name = split[0];
    Parameters = split.Count() > 1 ? split.Skip(1).ToArray() : [];
    // TODO: Get amount of pars to get delay.
  }
  public void Invoke(ZDO zdo, Dictionary<string, string> parameters)
  {
    if (zdo.GetOwner() == 0) return;
    DelayedRpc.Add(Delay, zdo.GetOwner(), zdo.m_uid, Name, GetParameters(zdo, parameters));
  }
  private object[] GetParameters(ZDO zdo, Dictionary<string, string> parameters)
  {
    var pars = Parameters.Select(p => Helper.ReplaceParameters(p, parameters)).ToArray();
    if (Name == "AddItem") return [Parse.String(pars, 0)];
    if (Name == "SetAggravated") return [Parse.Boolean(pars, 0), Parse.Int(pars, 1)];
    if (Name == "Damage")
    {
      var hit = Parse.Hit(pars[0]);
      hit.m_point = zdo.m_position;
      return [hit, Parse.Int(pars, 1)];
    }
    if (Name == "Heal") return [Parse.Float(pars, 0), Parse.Boolean(pars, 1)];
    if (Name == "UseDoor") return [Parse.Boolean(pars, 0)];
    if (Name == "Pick") return [];
    if (Name == "Alert") return [];
    if (Name == "UseStamina") return [Parse.Float(pars, 0)];
    if (Name == "RPC_TeleportTo") return [Parse.VectorXZY(pars, 0), Parse.AngleYXZ(pars, 3), Parse.Boolean(pars, 6)];
    if (Name == "OnTargeted") return [Parse.Boolean(pars, 0), Parse.Boolean(pars, 1)];
    if (Name == "Message") return [Parse.Int(pars, 0), Parse.String(pars, 1), Parse.Int(pars, 2)];
    if (Name == "RPC_AddStatusEffect") return [Parse.Hash(pars, 0), Parse.Boolean(pars, 1), Parse.Int(pars, 2), Parse.Float(pars, 3)];
    if (Name == "Forward") return [];
    if (Name == "Backward") return [];
    if (Name == "Rudder") return [Parse.Float(pars, 0)];
    if (Name == "Stop") return [];
    if (Name == "SetTag") return [Parse.String(pars, 0), Parse.String(pars, 1)];
    if (Name == "RPC_RequestStateChange") return [Parse.Int(pars, 0)];
    if (Name == "RPC_AddAmmo") return [Parse.String(pars, 0)];
    if (Name == "WNTRepair") return [];
    if (Name == "WNTRemove") return [];
    if (Name == "Stagger") return [Parse.VectorXZY(pars, 0)];
    return pars;
  }
}


public class CustomRpcInfo
{
  private static readonly HashSet<string> Types = ["int", "float", "bool", "string", "vec", "quat", "hash", "hit", "enum_reason", "enum_message", "zdo"];
  public static bool IsType(string line) => Types.Contains(Parse.Kvp(line).Key);
  private readonly string Name;
  private readonly bool OnlyOwner = true;
  private readonly string[] Parameters;
  private readonly float Delay;

  public CustomRpcInfo(string[] lines, float delay)
  {
    var first = Parse.Split(lines[0]);
    Name = first[0];
    OnlyOwner = first.Length < 2 || first[3].ToLowerInvariant() != "all";
    Delay = first.Length < 3 ? delay : Parse.Float(first[2], delay);
    Parameters = lines.Skip(1).ToArray();
  }
  public void Invoke(ZDO zdo, Dictionary<string, string> parameters)
  {
    if (OnlyOwner && zdo.GetOwner() == 0) return;
    var target = OnlyOwner ? zdo.GetOwner() : 0;
    DelayedRpc.Add(Delay, target, zdo.m_uid, Name, GetParameters(parameters));
  }
  private object[] GetParameters(Dictionary<string, string> parameters)
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
      if (type == "hit") pars[i] = Parse.Hit(arg);
      if (type == "zdo") pars[i] = Parse.ZDOID(arg);
      if (type == "enum_message") pars[i] = Enum.TryParse(arg, true, out MessageHud.MessageType message) ? (int)message : 2;
      if (type == "enum_reason") pars[i] = Enum.TryParse(arg, true, out BaseAI.AggravatedReason message) ? (int)message : 0;

    }
    return pars;
  }
}