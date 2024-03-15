using System;
using System.Collections.Generic;

namespace ExpandWorld.Prefab;

public class Helper2
{

  public static string ReplaceParameters(string str, Dictionary<string, string> parameters)
  {
    foreach (var pair in parameters)
      str = str.Replace(pair.Key, pair.Value);
    return str;
  }
  public static Dictionary<string, string> CreateParameters(string prefab, string args)
  {
    var split = args.Split(' ');
    return new Dictionary<string, string> {
      { "<prefab>", prefab },
      { "<par0>", split.Length > 0 ? split[0] : "" },
      { "<par1>", split.Length > 1 ? split[1] : "" },
      { "<par2>", split.Length > 2 ? split[2] : "" },
      { "<par3>", split.Length > 3 ? split[3] : "" },
      { "<par4>", split.Length > 4 ? split[4] : "" },
      { "<par>", args }
    };
  }
  public static bool CheckWild(string wild, string str)
  {
    if (wild == "*")
      return true;
    if (wild[0] == '*' && wild[wild.Length - 1] == '*')
      return str.ToLowerInvariant().Contains(wild.Substring(1, wild.Length - 2).ToLowerInvariant());
    if (wild[0] == '*')
      return str.EndsWith(wild.Substring(1), StringComparison.OrdinalIgnoreCase);
    else if (wild[wild.Length - 1] == '*')
      return str.StartsWith(wild.Substring(0, wild.Length - 1), StringComparison.OrdinalIgnoreCase);
    else
      return str.Equals(wild, StringComparison.OrdinalIgnoreCase);
  }
}