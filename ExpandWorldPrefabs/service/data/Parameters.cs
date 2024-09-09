using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Service;
using UnityEngine;

namespace Data;

// Parameters are technically just a key-value mapping.
// Proper class allows properly adding caching and other features.
// While also ensuring that all code is in one place.
public class Parameters(string prefab, string arg, Vector3 pos)
{

  protected string[]? args;
  private readonly double time = ZNet.instance.GetTimeSeconds();

  public string Replace(string str)
  {
    StringBuilder parts = new();
    int nesting = 0;
    var start = 0;
    for (int i = 0; i < str.Length; i++)
    {
      if (str[i] == '<')
      {
        if (nesting == 0)
        {
          parts.Append(str.Substring(start, i - start));
          start = i;
        }
        nesting++;

      }
      if (str[i] == '>')
      {
        if (nesting == 1)
        {
          var key = str.Substring(start, i - start + 1);
          parts.Append(ResolveParameters(key));
          start = i + 1;
        }
        if (nesting > 0)
          nesting--;
      }
    }
    if (start < str.Length)
      parts.Append(str.Substring(start));

    return parts.ToString();
  }
  private string ResolveParameters(string str)
  {
    for (int i = 0; i < str.Length; i++)
    {
      var end = str.IndexOf(">", i);
      if (end == -1) break;
      i = end;
      var start = str.LastIndexOf("<", end);
      if (start == -1) continue;
      var length = end - start + 1;
      if (TryReplaceParameter(str.Substring(start, length), out var resolved))
      {
        str = str.Remove(start, length);
        str = str.Insert(start, resolved);
        // Resolved could contain parameters, so need to recheck the same position.
        i = start - 1;
      }
      else
      {
        i = end;
      }
    }
    return str;
  }
  private bool TryReplaceParameter(string key, out string resolved)
  {
    resolved = GetParameter(key);
    if (resolved == "")
      resolved = ResolveValue(key);
    return resolved != key;
  }

  protected virtual string GetParameter(string key) =>
    key switch
    {
      "<prefab>" => prefab,
      "<par>" => arg,
      "<par0>" => GetArg(0),
      "<par1>" => GetArg(1),
      "<par2>" => GetArg(2),
      "<par3>" => GetArg(3),
      "<par4>" => GetArg(4),
      "<par5>" => GetArg(5),
      "<par6>" => GetArg(6),
      "<par7>" => GetArg(7),
      "<par8>" => GetArg(8),
      "<par9>" => GetArg(9),
      "<time>" => Format(time),
      "<day>" => EnvMan.instance.GetDay(time).ToString(),
      "<ticks>" => ((long)(time * 10000000.0)).ToString(),
      "<x>" => Format(pos.x),
      "<y>" => Format(pos.y),
      "<z>" => Format(pos.z),
      "<snap>" => Format(WorldGenerator.instance.GetHeight(pos.x, pos.z)),
      _ => "",
    };

  private string GetArg(int index)
  {
    args ??= arg.Split(' ');
    return args.Length <= index ? "" : args[index];
  }
  protected static string Format(float value) => value.ToString("0.#####", NumberFormatInfo.InvariantInfo);
  protected static string Format(double value) => value.ToString("0.#####", NumberFormatInfo.InvariantInfo);

  // Parameter value could be a value group, so that has to be resolved.
  private static string ResolveValue(string value)
  {
    if (!value.StartsWith("<", StringComparison.OrdinalIgnoreCase)) return value;
    if (!value.EndsWith(">", StringComparison.OrdinalIgnoreCase)) return value;
    var sub = value.Substring(1, value.Length - 2);
    if (TryGetValueFromGroup(sub, out var valueFromGroup))
      return valueFromGroup;
    return value;
  }

  private static bool TryGetValueFromGroup(string group, out string value)
  {
    var hash = group.ToLowerInvariant().GetStableHashCode();
    if (!DataLoading.ValueGroups.ContainsKey(hash))
    {
      value = group;
      return false;
    }
    var roll = UnityEngine.Random.Range(0, DataLoading.ValueGroups[hash].Count);
    // Value from group could be another group, so yet another resolve is needed.
    value = ResolveValue(DataLoading.ValueGroups[hash][roll]);
    return true;
  }
}
public class ObjectParameters(string prefab, string arg, ZDO zdo) : Parameters(prefab, arg, zdo.m_position)
{
  private Inventory? inventory;


  protected override string GetParameter(string key)
  {
    var value = base.GetParameter(key);
    if (value != "") return value;
    value = GetGeneralParameter(key);
    if (value != "") return value;
    var kvp = Parse.Kvp(key, '_');
    if (kvp.Value != "")
    {
      var zdoKey = kvp.Key.Substring(1);
      var zdoValue = kvp.Value.Substring(0, kvp.Value.Length - 1);
      return GetZdoValue(zdoKey, zdoValue);
    }
    return "";
  }

  private string GetGeneralParameter(string key) =>
    key switch
    {
      "<zdo>" => zdo.m_uid.ToString(),
      "<pos>" => $"{Format(zdo.m_position.x)},{Format(zdo.m_position.z)},{Format(zdo.m_position.y)}",
      "<i>" => ZoneSystem.instance.GetZone(zdo.m_position).x.ToString(),
      "<j>" => ZoneSystem.instance.GetZone(zdo.m_position).y.ToString(),
      "<a>" => Format(zdo.m_rotation.y),
      "<rot>" => $"{Format(zdo.m_rotation.y)},{Format(zdo.m_rotation.x)},{Format(zdo.m_rotation.z)}",
      "<pid>" => GetPid(zdo),
      "<pname>" => GetPname(zdo),
      "<pchar>" => GetPchar(zdo),
      _ => "",
    };

  private static string GetPid(ZDO zdo)
  {
    var peer = GetPeer(zdo);
    if (peer != null)
      return peer.m_rpc.GetSocket().GetHostName();
    else if (Player.m_localPlayer)
      return "Server";
    return "";
  }
  private static string GetPname(ZDO zdo)
  {
    var peer = GetPeer(zdo);
    if (peer != null)
      return peer.m_playerName;
    else if (Player.m_localPlayer)
      return Player.m_localPlayer.GetPlayerName();
    return "";
  }
  private static string GetPchar(ZDO zdo)
  {
    var peer = GetPeer(zdo);
    if (peer != null)
      return peer.m_characterID.ToString();
    else if (Player.m_localPlayer)
      return Player.m_localPlayer.GetPlayerID().ToString();
    return "";
  }
  private static ZNetPeer? GetPeer(ZDO zdo) => zdo.GetOwner() != 0 ? ZNet.instance.GetPeer(zdo.GetOwner()) : null;


  private string GetZdoValue(string key, string value) =>
   key switch
   {
     "key" => DataHelper.GetGlobalKey(value),
     "string" => zdo.GetString(value),
     "float" => zdo.GetFloat(value).ToString(CultureInfo.InvariantCulture),
     "int" => zdo.GetInt(value).ToString(CultureInfo.InvariantCulture),
     "long" => zdo.GetLong(value).ToString(CultureInfo.InvariantCulture),
     "bool" => zdo.GetBool(value) ? "true" : "false",
     "hash" => ZNetScene.instance.GetPrefab(zdo.GetInt(value))?.name ?? "",
     "vec" => DataEntry.PrintVectorXZY(zdo.GetVec3(value, Vector3.zero)),
     "quat" => DataEntry.PrintAngleYXZ(zdo.GetQuaternion(value, Quaternion.identity)),
     "byte" => Convert.ToBase64String(zdo.GetByteArray(value)),
     "zdo" => zdo.GetZDOID(value).ToString(),
     "item" => GetAmountOfItems(value).ToString(),
     _ => "",
   };

  private int GetAmountOfItems(string prefab)
  {
    LoadInventory();
    if (inventory == null) return 0;
    int count = 0;
    foreach (var item in inventory.m_inventory)
    {
      if ((item.m_dropPrefab?.name ?? item.m_shared.m_name) == prefab) count += item.m_stack;
    }
    return count;
  }

  private void LoadInventory()
  {
    if (inventory != null) return;
    var currentItems = zdo.GetString(ZDOVars.s_items);
    if (currentItems == "") return;
    inventory = new("", null, 4, 2);
    inventory.Load(new ZPackage(currentItems));
  }
}
