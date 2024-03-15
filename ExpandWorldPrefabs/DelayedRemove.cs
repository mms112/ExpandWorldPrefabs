using System.Collections.Generic;
using Service;
using UnityEngine;

namespace ExpandWorld.Prefab;

public class DelayedRemove(float delay, ZDO zdo)
{
  private static readonly List<DelayedRemove> Removes = [];
  public static void Add(float delay, ZDO zdo)
  {
    if (delay <= 0f)
    {
      Manager.RemoveZDO(zdo);
      return;
    }
    Removes.Add(new(delay, zdo));
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

  public void Execute()
  {
    Manager.RemoveZDO(Zdo);
  }
}