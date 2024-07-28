
using System.Collections.Generic;
using System.Linq;
using ExpandWorld.Prefab;
using Service;
using UnityEngine;

namespace Data;

public struct Pars(Dictionary<string, string> parameters, ZDO source)
{
  public Dictionary<string, string> Parameters = parameters;
  public ZDO source = source;
}

public class DataValue
{

  public static IZdoIdValue ZdoId(string values, HashSet<string> requiredParameters)
  {
    var split = SplitWithValues(values, requiredParameters);
    var zdo = Parse.ZdoId(split[0]);
    if (split.Length == 1 && zdo != ZDOID.None)
      return new SimpleZdoIdValue(zdo);
    return new ZdoIdValue(split);
  }
  // Different function name because string would be ambiguous.
  public static IIntValue Simple(int value) => new SimpleIntValue(value);
  public static IStringValue Simple(string value) => new SimpleStringValue(value);
  public static IFloatValue Simple(float value) => new SimpleFloatValue(value);
  public static ILongValue Simple(long value) => new SimpleLongValue(value);
  public static IVector3Value Simple(Vector3 value) => new SimpleVector3Value(value);
  public static IQuaternionValue Simple(Quaternion value) => new SimpleQuaternionValue(value);


  public static IIntValue Int(ZPackage pkg) => new SimpleIntValue(pkg.ReadInt());
  public static IIntValue Int(string values, HashSet<string> requiredParameters)
  {
    var split = SplitWithValues(values, requiredParameters);
    if (split.Length == 1 && int.TryParse(split[0], out var result))
      return new SimpleIntValue(result);
    return new IntValue(split);
  }

  public static IFloatValue Float(ZPackage pkg) => new SimpleFloatValue(pkg.ReadSingle());
  public static IFloatValue Float(string values, HashSet<string> requiredParameters)
  {
    var split = SplitWithValues(values, requiredParameters);
    if (split.Length == 1 && Parse.TryFloat(split[0], out var result))
      return new SimpleFloatValue(result);
    return new FloatValue(split);
  }

  public static ILongValue Long(ZPackage pkg) => new SimpleLongValue(pkg.ReadLong());
  public static ILongValue Long(string values, HashSet<string> requiredParameters)
  {
    var split = SplitWithValues(values, requiredParameters);
    if (split.Length == 1 && long.TryParse(split[0], out var result))
      return new SimpleLongValue(result);
    return new LongValue(split);
  }

  public static IStringValue String(ZPackage pkg) => new SimpleStringValue(pkg.ReadString());
  public static IStringValue String(string values, HashSet<string> requiredParameters)
  {
    var split = SplitWithValues(values, requiredParameters);
    if (split.Length == 1 && !HasParameters(split[0]))
      return new SimpleStringValue(split[0]);
    return new StringValue(split);
  }
  public static IBoolValue Bool(bool value) => new SimpleBoolValue(value);
  public static IBoolValue Bool(string values, HashSet<string> requiredParameters)
  {
    var split = SplitWithValues(values, requiredParameters);
    if (split.Length == 1 && bool.TryParse(split[0], out var result))
      return new SimpleBoolValue(result);
    return new BoolValue(split);
  }


  public static IHashValue Hash(string value) => new SimpleHashValue(value);
  public static IHashValue Hash(string values, HashSet<string> requiredParameters)
  {
    var split = SplitWithValues(values, requiredParameters);
    if (split.Length == 1 && !HasParameters(split[0]))
      return new SimpleHashValue(split[0]);
    return new HashValue(split);
  }
  public static IVector3Value Vector3(ZPackage pkg) => new SimpleVector3Value(pkg.ReadVector3());
  public static IVector3Value Vector3(string values, HashSet<string> requiredParameters)
  {
    var split = SplitWithValues(values, requiredParameters);
    var parsed = Parse.VectorXZYNull(split);
    if (parsed.HasValue)
      return new SimpleVector3Value(parsed.Value);
    return new Vector3Value(split);
  }
  public static IQuaternionValue Quaternion(ZPackage pkg) => new SimpleQuaternionValue(pkg.ReadQuaternion());
  public static IQuaternionValue Quaternion(string values, HashSet<string> requiredParameters)
  {
    var split = SplitWithValues(values, requiredParameters);
    var parsed = Parse.AngleYXZNull(split);
    if (parsed.HasValue)
      return new SimpleQuaternionValue(parsed.Value);
    return new QuaternionValue(split);
  }

  private static bool HasParameters(string value) => value.Contains("<") && value.Contains(">");

  private static string[] SplitWithValues(string str, HashSet<string> requiredParameters)
  {
    List<string> result = [];
    var split = Parse.SplitWithEmpty(str);
    foreach (var value in split)
    {
      if (!value.Contains("<") || !value.Contains(">"))
      {
        result.Add(value);
        continue;
      }
      var parSplit = value.Split('<', '>');
      List<string> parameters = [];
      List<int> hashes = [];
      for (var i = 1; i < parSplit.Length; i += 2)
      {
        var hash = parSplit[i].ToLowerInvariant().GetStableHashCode();
        // Value groups should work same as using the value directly.
        // So it makes sense to resolve them on load.
        // This way other code doesn't have to worry about resolving them and performance is better.
        // Originally they were resolved later because World Edit Commands allows overriding them.
        // But this is not needed for EW Prefabs.
        if (DataLoading.ValueGroups.ContainsKey(hash))
        {
          parameters.Add($"<{parSplit[i]}>");
          hashes.Add(hash);
        }
        else
          // Code would be cleaner if required parameters were parsed separately.
          // But the info is already here to take.
          requiredParameters.Add(parSplit[i]);
      }
      if (parameters.Count == 0)
        result.Add(value);
      else
        SubstitueValues(result, value, parameters, hashes, 0);

    }
    return [.. result];
  }

  // Recursion needed because there can be multiple value groups.
  private static void SubstitueValues(List<string> result, string format, List<string> parameters, List<int> hashes, int index)
  {
    var groups = DataLoading.ValueGroups[hashes[index]];
    foreach (var group in groups)
    {
      var newFormat = format.Replace(parameters[index], group);
      if (index == parameters.Count - 1)
        result.Add(newFormat);
      else
        SubstitueValues(result, newFormat, parameters, hashes, index + 1);
    }
  }
}


public class AnyValue(string[] values)
{
  protected readonly string[] Values = values;

  private string? RollValue()
  {
    if (Values.Length == 1)
      return Values[0];
    return Values[Random.Range(0, Values.Length)];
  }
  protected string? GetValue(Pars pars)
  {
    var value = RollValue();
    if (value == null || value == "<none>")
      return null;
    return ReplaceParameters(value, pars);
  }
  protected string? GetValue()
  {
    var value = RollValue();
    return value == null || value == "<none>" ? null : value;
  }
  protected string[] GetAllValues(Pars pars)
  {
    return Values.Select(v => ReplaceParameters(v, pars)).Where(v => v != null && v != "<none").ToArray();
  }

  protected string ReplaceParameters(string value, Pars pars) => Helper.ReplaceParameters(value, pars.Parameters, pars.source);
}
public class ItemValue(ItemData data, HashSet<string> requiredParameters)
{

  public static bool Match(Pars pars, List<ItemValue> data, Vector2i? size, ZDO zdo, IIntValue? amount)
  {
    var str = zdo.GetString(ZDOVars.s_items);
    Inventory inv = new("", null, size.HasValue ? size.Value.x : 4, size.HasValue ? size.Value.y : 2);
    if (str != "")
    {
      ZPackage pkg = new(str);
      inv.Load(pkg);
    }
    var matches = data.Where(item => item.Match(pars, inv)).Count();
    // If no amount is set, then must match exactly.
    if (amount == null)
      return matches == data.Count && inv.m_inventory.Count == 0;
    return amount.Match(pars, matches) == true;
  }
  public static bool Match(Pars pars, Vector2i? size, ZDO zdo, IIntValue amount)
  {
    var str = zdo.GetString(ZDOVars.s_items);
    Inventory inv = new("", null, size.HasValue ? size.Value.x : 4, size.HasValue ? size.Value.y : 2);
    if (str != "")
    {
      ZPackage pkg = new(str);
      inv.Load(pkg);
    }
    return amount.Match(pars, inv.m_inventory.Count) == true;
  }

  public static string LoadItems(Pars pars, List<ItemValue> items, Vector2i size, int amount)
  {
    ZPackage pkg = new();
    pkg.Write(106);
    items = Generate(pars, items, size, amount);
    pkg.Write(items.Count);
    foreach (var item in items)
      item.Write(pars, pkg);
    return pkg.GetBase64();
  }
  public static List<ItemValue> Generate(Pars pars, List<ItemValue> data, Vector2i size, int amount)
  {
    var fixedPos = data.Where(item => item.Position != "").ToList();
    var randomPos = data.Where(item => item.Position == "").ToList();
    Dictionary<Vector2i, ItemValue> inventory = [];
    foreach (var item in fixedPos)
    {
      if (!item.Roll(pars)) continue;
      inventory[item.RolledPosition] = item;
    }
    if (amount == 0)
      GenerateEach(pars, inventory, size, randomPos);
    else
      GenerateAmount(pars, inventory, size, randomPos, amount);
    return [.. inventory.Values];
  }
  private static void GenerateEach(Pars pars, Dictionary<Vector2i, ItemValue> inventory, Vector2i size, List<ItemValue> items)
  {
    foreach (var item in items)
    {
      if (!item.Roll(pars)) continue;
      var slot = FindNextFreeSlot(inventory, size);
      if (!slot.HasValue) break;
      item.RolledPosition = slot.Value;
      inventory[slot.Value] = item;
    }
  }
  private static void GenerateAmount(Pars pars, Dictionary<Vector2i, ItemValue> inventory, Vector2i size, List<ItemValue> items, int amount)
  {
    var maxWeight = items.Sum(item => item.Chance);
    for (var i = 0; i < amount && items.Count > 0; ++i)
    {
      var slot = FindNextFreeSlot(inventory, size);
      if (!slot.HasValue) break;
      var item = RollItem(items, maxWeight);
      item.RolledPosition = slot.Value;
      if (item.RollPrefab(pars))
        inventory[slot.Value] = item;
      maxWeight -= item.Chance;
      items.Remove(item);
    }
  }
  private static ItemValue RollItem(List<ItemValue> items, float maxWeight)
  {
    var roll = Random.Range(0f, maxWeight);
    foreach (var item in items)
    {
      if (roll < item.Chance)
        return item;
      roll -= item.Chance;
    }
    return items.Last();
  }
  private static Vector2i? FindNextFreeSlot(Dictionary<Vector2i, ItemValue> inventory, Vector2i size)
  {
    var maxW = size.x;
    var maxH = size.y;
    for (var y = 0; y < maxH; ++y)
      for (var x = 0; x < maxW; ++x)
      {
        var pos = new Vector2i(x, y);
        if (!inventory.ContainsKey(pos))
          return pos;
      }
    return null;
  }
  // Prefab is saved as string, so hash can't be used.
  public IStringValue Prefab = DataValue.String(data.prefab, requiredParameters);
  public float Chance = data.chance;
  public IIntValue? Stack = data.stack == null ? null : DataValue.Int(data.stack, requiredParameters);
  public IFloatValue? Durability = data.durability == null ? null : DataValue.Float(data.durability, requiredParameters);
  public string Position = data.pos;
  private Vector2i RolledPosition = Parse.Vector2Int(data.pos);
  public IBoolValue? Equipped = data.equipped == null ? null : DataValue.Bool(data.equipped, requiredParameters);
  public IIntValue? Quality = data.quality == null ? null : DataValue.Int(data.quality, requiredParameters);
  public IIntValue? Variant = data.variant == null ? null : DataValue.Int(data.variant, requiredParameters);
  public ILongValue? CrafterID = data.crafterID == null ? null : DataValue.Long(data.crafterID, requiredParameters);
  public IStringValue? CrafterName = data.crafterName == null ? null : DataValue.String(data.crafterName, requiredParameters);
  public Dictionary<string, IStringValue> CustomData = data.customData?.ToDictionary(kvp => kvp.Key, kvp => DataValue.String(kvp.Value, requiredParameters)) ?? [];
  public IIntValue? WorldLevel = data.worldLevel == null ? null : DataValue.Int(data.worldLevel, requiredParameters);
  public IBoolValue? PickedUp = data.pickedUp == null ? null : DataValue.Bool(data.pickedUp, requiredParameters);
  // Must know before writing is the prefab good, so it has to be rolled first.
  private string RolledPrefab = "";
  public bool RollPrefab(Pars pars)
  {
    RolledPrefab = Prefab.Get(pars) ?? "";
    return RolledPrefab != "";
  }
  public bool RollChance() => Chance >= 1f || Random.value <= Chance;
  public bool Roll(Pars pars) => RollChance() && RollPrefab(pars);
  public void Write(Pars pars, ZPackage pkg)
  {
    pkg.Write(RolledPrefab);
    pkg.Write(Stack?.Get(pars) ?? 1);
    pkg.Write(Durability?.Get(pars) ?? 100f);
    pkg.Write(RolledPosition);
    pkg.Write(Equipped?.GetBool(pars) ?? false);
    pkg.Write(Quality?.Get(pars) ?? 1);
    pkg.Write(Variant?.Get(pars) ?? 1);
    pkg.Write(CrafterID?.Get(pars) ?? 0);
    pkg.Write(CrafterName?.Get(pars) ?? "");
    pkg.Write(CustomData?.Count ?? 0);
    foreach (var kvp in CustomData ?? [])
    {
      pkg.Write(kvp.Key);
      pkg.Write(kvp.Value.Get(pars));
    }
    pkg.Write(WorldLevel?.Get(pars) ?? 0);
    pkg.Write(PickedUp?.GetBool(pars) ?? false);
  }

  public void AddTo(Pars pars, Inventory inv)
  {
    var stack = Stack?.Get(pars) ?? 1;
    stack = StackTo(pars, stack, inv);
    InsertTo(pars, stack, inv);
  }
  private int StackTo(Pars pars, int stack, Inventory inv)
  {
    foreach (var item in inv.m_inventory)
    {
      if (!MatchItem(pars, item)) continue;
      var amount = Mathf.Min(item.m_shared.m_maxStackSize - item.m_stack, stack);
      item.m_stack += amount;
      stack -= amount;
      if (stack <= 0) break;
    }
    return stack;
  }
  private int InsertTo(Pars pars, int stack, Inventory inv)
  {
    while (stack > 0)
    {
      var prefab = Prefab.Get(pars);
      var item = ObjectDB.instance.GetItemPrefab(prefab);
      if (item == null) return stack;
      var itemData = item.GetComponent<ItemDrop>().m_itemData.Clone();
      itemData.m_dropPrefab = item;
      itemData.m_quality = Quality?.Get(pars) ?? 1;
      itemData.m_variant = Variant?.Get(pars) ?? 0;
      itemData.m_crafterID = CrafterID?.Get(pars) ?? 0L;
      itemData.m_crafterName = CrafterName?.Get(pars) ?? "";
      itemData.m_worldLevel = WorldLevel?.Get(pars) ?? 0;
      itemData.m_durability = Durability?.Get(pars) ?? itemData.m_shared.m_maxDurability;
      itemData.m_equipped = Equipped?.GetBool(pars) ?? false;
      itemData.m_pickedUp = PickedUp?.GetBool(pars) ?? false;
      itemData.m_customData = CustomData.ToDictionary(x => x.Key, x => x.Value.Get(pars) ?? "");

      var amount = Mathf.Min(itemData.m_shared.m_maxStackSize, stack);
      stack -= amount;
      itemData.m_stack = amount;

      if (Position == "")
      {
        var slot = inv.FindEmptySlot(true);
        if (slot.x < 0) return stack;
        itemData.m_gridPos = slot;
        inv.m_inventory.Add(itemData);
      }
      else
      {
        itemData.m_gridPos = RolledPosition;
        inv.m_inventory.RemoveAll(x => x.m_gridPos == RolledPosition);
        inv.m_inventory.Add(itemData);
      }
    }
    return stack;
  }
  public void RemoveFrom(Pars pars, Inventory inv)
  {
    var stack = Stack?.Get(pars) ?? 1;
    for (var i = inv.m_inventory.Count - 1; i >= 0; --i)
    {
      var item = inv.m_inventory[i];
      if (!MatchItem(pars, item)) continue;
      var amount = Mathf.Min(item.m_stack, stack);
      item.m_stack -= amount;
      stack -= amount;
      if (stack <= 0) break;
    }
    inv.m_inventory.RemoveAll(x => x.m_stack <= 0);
  }

  public bool Match(Pars pars, Inventory inv)
  {
    var item = FindMatch(pars, inv);
    if (item == null) return false;
    inv.RemoveItem(item);
    return true;
  }
  private ItemDrop.ItemData? FindMatch(Pars pars, Inventory inv)
  {
    if (Position != "")
    {
      var item = inv.GetItemAt(RolledPosition.x, RolledPosition.y);
      if (item == null) return null;
      if (Stack?.Match(pars, item.m_stack) == false) return null;
      if (MatchItem(pars, item)) return item;
    }
    foreach (var item in inv.m_inventory)
    {
      if (Stack?.Match(pars, item.m_stack) == false) continue;
      if (MatchItem(pars, item)) return item;

    }
    return null;
  }
  private bool MatchItem(Pars pars, ItemDrop.ItemData item)
  {
    if (Prefab.Match(pars, item.m_dropPrefab?.name ?? item.m_shared.m_name) == false) return false;
    if (Durability?.Match(pars, item.m_durability) == false) return false;
    if (Equipped?.Match(pars, item.m_equipped) == false) return false;
    if (Quality?.Match(pars, item.m_quality) == false) return false;
    if (Variant?.Match(pars, item.m_variant) == false) return false;
    if (CrafterID?.Match(pars, item.m_crafterID) == false) return false;
    if (CrafterName?.Match(pars, item.m_crafterName) == false) return false;
    if (WorldLevel?.Match(pars, item.m_worldLevel) == false) return false;
    if (PickedUp?.Match(pars, item.m_pickedUp) == false) return false;
    foreach (var kvp in CustomData)
    {
      if (!item.m_customData.TryGetValue(kvp.Key, out var value)) return false;
      if (kvp.Value.Match(pars, value) == false) return false;
    }
    return true;
  }
}
