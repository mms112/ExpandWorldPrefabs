using System.Collections.Generic;
namespace ExpandWorld.Prefab;

public class DelayedRpc(float delay, long target, ZDOID id, string name, object[] parameters)
{
  private static readonly List<DelayedRpc> Rpcs = [];
  public static void Add(float delay, long target, ZDOID id, string name, object[] parameters)
  {
    if (delay <= 0f)
    {
      Manager.Rpc(target, id, name, parameters);
      return;
    }
    Rpcs.Add(new(delay, target, id, name, parameters));
  }
  public static void Execute(float dt)
  {
    // Two loops to preserve order.
    foreach (var rpc in Rpcs)
    {
      rpc.Delay -= dt;
      if (rpc.Delay > -0.001) continue;
      rpc.Execute();
    }
    for (var i = Rpcs.Count - 1; i >= 0; i--)
    {
      if (Rpcs[i].Delay > -0.001) continue;
      Rpcs.RemoveAt(i);
    }
  }
  public float Delay = delay;
  private readonly long Target = target;
  private readonly ZDOID Id = id;
  private readonly string Name = name;
  private readonly object[] Parameters = parameters;

  public void Execute()
  {
    Manager.Rpc(Target, Id, Name, Parameters);
  }
}