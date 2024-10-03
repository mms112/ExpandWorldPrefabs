using System.Collections.Generic;
using UnityEngine;

namespace ExpandWorld.Prefab;

public class DelayedTerrain(float delay, long source, Vector3 pos, float size, ZPackage pkg)
{
  private static readonly List<DelayedTerrain> Terrains = [];
  public static void Add(float delay, long source, Vector3 pos, float size, ZPackage pkg)
  {
    var created = Manager.GenerateTerrainCompilers(source, pos, size);
    // One second should be enough to deliver the compiler to the client.
    if (created) delay = Mathf.Max(delay, 1f);
    if (delay <= 0f)
    {
      Manager.ModifyTerrain(source, pos, size, pkg);
      return;
    }
    Terrains.Add(new(delay, source, pos, size, pkg));
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
  private readonly float Size = size;
  private readonly ZPackage Pkg = pkg;
  private readonly long Source = source;
  public void Execute()
  {
    Manager.ModifyTerrain(Source, Pos, Size, Pkg);
  }
}