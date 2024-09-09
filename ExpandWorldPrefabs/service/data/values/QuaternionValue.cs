using System.Linq;
using Service;
using UnityEngine;

namespace Data;

public class QuaternionValue(string[] values) : AnyValue(values), IQuaternionValue
{
  public Quaternion? Get(Parameters pars) => Parse.AngleYXZNull(GetValue(pars));
  public bool? Match(Parameters pars, Quaternion value)
  {
    var values = GetAllValues(pars);
    if (values.Count == 0) return null;
    return values.Any(v => Parse.AngleYXZNull(v) == value);
  }
}

public class SimpleQuaternionValue(Quaternion value) : IQuaternionValue
{
  private readonly Quaternion Value = value;
  public Quaternion? Get(Parameters pars) => Value;
  public bool? Match(Parameters pars, Quaternion value) => Value == value;
}

public interface IQuaternionValue
{
  Quaternion? Get(Parameters pars);
  bool? Match(Parameters pars, Quaternion value);
}
