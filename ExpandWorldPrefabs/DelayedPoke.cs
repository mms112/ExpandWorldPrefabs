using System.Collections.Generic;
namespace ExpandWorld.Prefab;

public class DelayedPoke(float delay, ZDO[] zdos, string parameter)
{
  private static readonly List<DelayedPoke> Pokes = [];
  public static void Add(float delay, ZDO[] zdos, string parameter)
  {
    if (delay <= 0f)
    {
      Manager.Poke(zdos, parameter);
      return;
    }
    Pokes.Add(new(delay, zdos, parameter));
  }
  public static void Execute(float dt)
  {
    // Two loops to preserve order.
    foreach (var poke in Pokes)
    {
      poke.Delay -= dt;
      if (poke.Delay > -0.001) continue;
      poke.Execute();
    }
    for (var i = Pokes.Count - 1; i >= 0; i--)
    {
      if (Pokes[i].Delay > -0.001) continue;
      Pokes.RemoveAt(i);
    }
  }
  public float Delay = delay;
  private readonly ZDO[] Zdos = zdos;
  private readonly string Parameter = parameter;

  public void Execute()
  {
    Manager.Poke(Zdos, Parameter);
  }
}