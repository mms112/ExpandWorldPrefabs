using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Data;
using Service;
using UnityEngine;

namespace ExpandWorld.Prefab;

public class Helper
{
  public static string ReplaceParameters(string str, Dictionary<string, string> parameters, ZDO? zdo)
  {
    for (int i = str.Length; i >= 0; i--)
    {
      var end = str.LastIndexOf(">", i);
      if (end == -1) break;
      var start = str.LastIndexOf("<", end);
      if (start == -1) break;
      i = start;
      var key = str.Substring(start, end - start + 1);
      if (parameters.ContainsKey(key))
      {
        str = str.Remove(start, end - start + 1);
        str = str.Insert(start, parameters[key]);
        continue;
      }
      if (zdo == null) continue;
      var kvp = Parse.Kvp(key, '_');
      if (kvp.Value != "")
      {
        var zdoKey = kvp.Key.Substring(1);
        var zdoValue = kvp.Value.Substring(0, kvp.Value.Length - 1);
        str = str.Remove(start, end - start + 1);
        var value = GetZdoValue(zdo, zdoKey, zdoValue);
        str = str.Insert(start, value);
      }
    }
    return str;
  }
  public static Dictionary<string, string> CreateParameters(string prefab, string args, ZDO zdo)
  {
    var split = args.Split(' ');
    var zone = ZoneSystem.instance.GetZone(zdo.m_position);
    return new Dictionary<string, string> {
      { "<zdo>", zdo.m_uid.ToString() },
      { "<prefab>", prefab },
      { "<par0>", split.Length > 0 ? split[0] : "" },
      { "<par1>", split.Length > 1 ? split[1] : "" },
      { "<par2>", split.Length > 2 ? split[2] : "" },
      { "<par3>", split.Length > 3 ? split[3] : "" },
      { "<par4>", split.Length > 4 ? split[4] : "" },
      { "<par>", args },
      { "<x>", Format(zdo.m_position.x) },
      { "<y>", Format(zdo.m_position.y) },
      { "<z>", Format(zdo.m_position.z) },
      { "<i>", zone.x.ToString() },
      { "<j>", zone.y.ToString() },
      { "<a>", Format(zdo.m_rotation.y) },
      { "<time>", ZNet.instance.GetTime().Ticks.ToString() },
    };
  }
  public static Dictionary<string, string> CreateParameters(string args, Vector3 pos)
  {
    var split = args.Split(' ');
    var zone = ZoneSystem.instance.GetZone(pos);
    return new Dictionary<string, string> {
      { "<par0>", split.Length > 0 ? split[0] : "" },
      { "<par1>", split.Length > 1 ? split[1] : "" },
      { "<par2>", split.Length > 2 ? split[2] : "" },
      { "<par3>", split.Length > 3 ? split[3] : "" },
      { "<par4>", split.Length > 4 ? split[4] : "" },
      { "<par>", args },
      { "<x>", Format(pos.x) },
      { "<y>", Format(pos.y) },
      { "<z>", Format(pos.z) },
      { "<i>", zone.x.ToString() },
      { "<j>", zone.y.ToString() },
      { "<time>", ZNet.instance.GetTime().Ticks.ToString() },
    };
  }

  public static string GetZdoValue(ZDO zdo, string key, string value)
  {
    if (key == "key")
      return DataHelper.GetGlobalKey(value);
    if (key == "string")
      return zdo.GetString(value);
    else if (key == "float")
      return zdo.GetFloat(value).ToString(CultureInfo.InvariantCulture);
    else if (key == "int")
      return zdo.GetInt(value).ToString(CultureInfo.InvariantCulture);
    else if (key == "long")
      return zdo.GetLong(value).ToString(CultureInfo.InvariantCulture);
    else if (key == "bool")
      return zdo.GetBool(value) ? "true" : "false";
    else if (key == "hash")
      return ZNetScene.instance.GetPrefab(zdo.GetInt(value))?.name ?? "";
    else if (key == "vec")
      return DataEntry.PrintVectorXZY(zdo.GetVec3(value, Vector3.zero));
    else if (key == "quat")
      return DataEntry.PrintAngleYXZ(zdo.GetQuaternion(value, Quaternion.identity));
    else if (key == "byte")
      return Convert.ToBase64String(zdo.GetByteArray(value));
    else if (key == "zdo")
      return zdo.GetZDOID(value).ToString();
    return "";
  }
  private static string Format(float value) => value.ToString("0.#####", NumberFormatInfo.InvariantInfo);
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

  public static bool IsServer() => ZNet.instance && ZNet.instance.IsServer();
  // Note: Intended that is client when no Znet instance (so stuff isn't loaded in the main menu).
  public static bool IsClient() => !IsServer();

  public static bool IsZero(float a) => Mathf.Abs(a) < 0.001f;
  public static bool Approx(float a, float b) => Mathf.Abs(a - b) < 0.001f;
  public static bool ApproxBetween(float a, float min, float max) => min - 0.001f <= a && a <= max + 0.001f;

  public static bool HasAnyGlobalKey(List<string> keys, Dictionary<string, string> parameters)
  {
    foreach (var key in keys)
    {
      if (key.Contains("<"))
      {
        if (ZoneSystem.instance.m_globalKeys.Contains(ReplaceParameters(key, parameters, null))) return true;
      }
      else
      {
        if (ZoneSystem.instance.m_globalKeys.Contains(key)) return true;
      }
    }
    return false;
  }
  public static bool HasEveryGlobalKey(List<string> keys, Dictionary<string, string> parameters)
  {
    foreach (var key in keys)
    {
      if (key.Contains("<"))
      {
        if (!ZoneSystem.instance.m_globalKeys.Contains(ReplaceParameters(key, parameters, null))) return false;
      }
      else
      {
        if (!ZoneSystem.instance.m_globalKeys.Contains(key)) return false;
      }
    }
    return true;
  }
}