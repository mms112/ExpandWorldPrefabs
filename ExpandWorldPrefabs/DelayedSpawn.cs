using System.Collections.Generic;
using Data;
using UnityEngine;

namespace ExpandWorld.Prefab;

public class DelayedSpawn(float delay, ZdoEntry zdoEntry, bool triggerRules)
{
  private static readonly List<DelayedSpawn> Spawns = [];
  public static void Add(float delay, ZdoEntry zdoEntry, bool triggerRules)
  {
    if (delay <= 0f)
    {
      Manager.CreateObject(zdoEntry, triggerRules);
      return;
    }
    Spawns.Add(new(delay, zdoEntry, triggerRules));
  }
  public static void Execute(float dt)
  {
    // Two loops to preserve order.
    for (var i = 0; i < Spawns.Count; i++)
    {
      var spawn = Spawns[i];
      spawn.Delay -= dt;
      if (spawn.Delay > -0.001) continue;
      spawn.Execute();
    }
    for (var i = Spawns.Count - 1; i >= 0; i--)
    {
      if (Spawns[i].Delay > -0.001) continue;
      Spawns.RemoveAt(i);
    }
  }
  public float Delay = delay;
  public void Execute()
  {
    Manager.CreateObject(zdoEntry, triggerRules);
  }
}