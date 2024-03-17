using System.Collections.Generic;
using Service;
namespace ExpandWorld.Prefab;

public class DelayedRemove(float delay, ZDO zdo, bool triggerRules)
{
  private static readonly List<DelayedRemove> Removes = [];
  public static void Add(float delay, ZDO zdo, bool triggerRules)
  {
    if (delay <= 0f)
    {
      Manager.RemoveZDO(zdo, triggerRules);
      return;
    }
    Removes.Add(new(delay, zdo, triggerRules));
  }
  public static void Execute(float dt)
  {
    // Two loops to preserve order.
    foreach (var remove in Removes)
    {
      remove.Delay -= dt;
      if (remove.Delay > -0.001) continue;
      remove.Execute();
    }
    for (var i = Removes.Count - 1; i >= 0; i--)
    {
      if (Removes[i].Delay > -0.001) continue;
      Removes.RemoveAt(i);
    }
  }
  private readonly ZDO Zdo = zdo;
  public float Delay = delay;
  private readonly bool TriggerRules = triggerRules;

  public void Execute()
  {
    Manager.RemoveZDO(Zdo, TriggerRules);
  }
}