using System.Collections.Generic;
namespace ExpandWorld.Prefab;


public interface IPokeable
{
  public float Delay { get; set; }
  void Execute();
}
public class DelayedSinglePoke(float delay, ZDO zdo, string parameter) : IPokeable
{
  private readonly ZDO Zdo = zdo;
  private readonly string Parameter = parameter;

  float IPokeable.Delay { get => delay; set => delay = value; }
  public void Execute() => Manager.Poke(Zdo, Parameter);

}

public class DelayedPoke(float delay, ZDO[] zdos, string parameter) : IPokeable
{
  private static readonly List<IPokeable> Pokes = [];
  public static void Add(float delay, ZDO[] zdos, string parameter)
  {
    if (delay <= 0f)
    {
      Manager.Poke(zdos, parameter);
      return;
    }
    Pokes.Add(new DelayedPoke(delay, zdos, parameter));
  }
  public static void Add(float delay, ZDO zdo, string parameter)
  {
    if (delay <= 0f)
    {
      Manager.Poke(zdo, parameter);
      return;
    }
    Pokes.Add(new DelayedSinglePoke(delay, zdo, parameter));
  }
  public static void Execute(float dt)
  {
    // Two loops to preserve order.
    for (var i = 0; i < Pokes.Count; i++)
    {
      var poke = Pokes[i];
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
  private readonly ZDO[] Zdos = zdos;
  private readonly string Parameter = parameter;

  float IPokeable.Delay { get => delay; set => delay = value; }

  public void Execute() => Manager.Poke(Zdos, Parameter);
}