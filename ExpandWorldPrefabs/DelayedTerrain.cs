using System.Collections.Generic;
using UnityEngine;

namespace ExpandWorld.Prefab;

public class DelayedTerrain(float delay, long source, Vector3 pos, ZPackage pkg, bool retry)
{
  private static readonly List<DelayedTerrain> Terrains = [];
  public static void Add(float delay, long source, Vector3 pos, ZPackage pkg, bool retry)
  {
    if (delay <= 0f)
    {
      Manager.ModifyTerrain(source, pos, pkg, retry);
      return;
    }
    Terrains.Add(new(delay, source, pos, pkg, retry));
  }
  public static void Execute(float dt)
  {
    // Two loops to preserve order.
    for (var i = 0; i < Terrains.Count; i++)
    {
      var terrain = Terrains[i];
      terrain.Delay -= dt;
      if (terrain.Delay > -0.001) continue;
      terrain.Execute();
    }
    for (var i = Terrains.Count - 1; i >= 0; i--)
    {
      if (Terrains[i].Delay > -0.001) continue;
      Terrains.RemoveAt(i);
    }
  }
  public float Delay = delay;
  private readonly Vector3 Pos = pos;
  private readonly ZPackage Pkg = pkg;
  private readonly bool Retry = retry;
  private readonly long Source = source;
  public void Execute()
  {
    Manager.ModifyTerrain(Source, Pos, Pkg, Retry);
  }
}