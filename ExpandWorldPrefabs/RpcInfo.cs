using System.Collections.Generic;
using System.Linq;
using Data;
using Service;

namespace ExpandWorld.Prefab;

public enum RpcTarget
{
  All,
  Owner,
  Search,
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
  private readonly KeyValuePair<string, string>[] Parameters;
  private readonly float Delay;
  public bool IsTarget => Target == RpcTarget.Search;

  public RpcInfo(Dictionary<string, string> lines)
  {
    Target = RpcTarget.Owner;
    if (lines.TryGetValue("name", out var name))
      Hash = name.GetStableHashCode();

    if (lines.TryGetValue("source", out var source))
      SourceParameter = source;

    if (lines.TryGetValue("target", out var target))
    {
      if (target == "all")
        Target = RpcTarget.All;
      else if (target == "search")
        Target = RpcTarget.Search;
      else if (target == "owner")
        Target = RpcTarget.Owner;
      else
      {
        Target = RpcTarget.ZDO;
        TargetParameter = target;
      }
    }
    Delay = 0f;
    if (lines.TryGetValue("delay", out var d))
      Delay = Parse.Float(d, 0f);
    Parameters = lines.OrderBy(p => p.Key).Where(p => Parse.TryInt(p.Key, out var _)).Select(p => Parse.Kvp(p.Value)).ToArray();
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
    else if (Target == RpcTarget.Search && players != null)
      foreach (var player in players)
        DelayedRpc.Add(Delay, source, player.PeerId, GetId(zdo), Hash, pars);
  }
  private object[] GetParameters(ZDO zdo, Dictionary<string, string> parameters)
  {
    var pars = Parameters.Select(p => Helper.ReplaceParameters(p.Value, parameters, zdo)).ToArray<object>();
    for (var i = 0; i < pars.Length; i++)
    {
      var type = Parameters[i].Key;
      var arg = (string)pars[i];
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


public class ObjectRpcInfo(Dictionary<string, string> lines) : RpcInfo(lines)
{
  protected override ZDOID GetId(ZDO zdo) => zdo.m_uid;
}
public class ClientRpcInfo(Dictionary<string, string> lines) : RpcInfo(lines)
{
  protected override ZDOID GetId(ZDO zdo) => ZDOID.None;
}