using System;
using System.Globalization;
using System.Linq;
using Data;
using Service;
using UnityEngine;

namespace ExpandWorld.Prefab;

public class Manager
{
  public static void HandleGlobal(ActionType type, string args, Vector3 pos, bool remove)
  {
    if (!ZNet.instance.IsServer()) return;
    Parameters parameters = new("", args, pos);
    var info = InfoSelector.SelectGlobal(type, args, parameters, pos, remove);
    if (info == null) return;
    if (info.Commands.Length > 0)
      Commands.Run(info, parameters);
    if (info.ClientRpcs != null)
      GlobalClientRpc(info.ClientRpcs, parameters);
    PokeGlobal(info, parameters, pos);
  }
  public static bool Handle(ActionType type, string args, ZDO zdo, ZDO? source = null)
  {
    // Already destroyed before.
    if (ZDOMan.instance.m_deadZDOs.ContainsKey(zdo.m_uid)) return false;
    if (!ZNet.instance.IsServer()) return false;
    var name = ZNetScene.instance.GetPrefab(zdo.m_prefab)?.name ?? "";
    ObjectParameters parameters = new(name, args, zdo);
    var info = InfoSelector.Select(type, zdo, args, parameters, source);
    if (info == null) return false;

    if (info.Commands.Length > 0)
      Commands.Run(info, parameters);

    if (info.ObjectRpcs != null)
      ObjectRpc(info.ObjectRpcs, zdo, parameters);
    if (info.ClientRpcs != null)
      ClientRpc(info.ClientRpcs, zdo, parameters);

    var remove = info.Remove?.GetBool(parameters) == true;
    var regenerate = info.Regenerate;
    var dataStr = info.Data?.Get(parameters) ?? "";
    HandleSpawns(info, zdo, parameters, remove, regenerate, dataStr);
    Poke(info, zdo, parameters);
    Terrain(info, zdo, parameters);
    if (info.Drops?.GetBool(parameters) == true)
      SpawnDrops(zdo);
    // Original object was regenerated to apply data.
    if (remove || regenerate)
      DelayedRemove.Add(info.RemoveDelay?.Get(parameters) ?? 0f, zdo, remove && info.TriggerRules);
    else if (info.InjectData)
    {
      var data = DataHelper.Get(dataStr);
      var removeItems = info.RemoveItems;
      var addItems = info.AddItems;
      if (data != null)
      {
        ZdoEntry entry = new(zdo);
        entry.Load(data, parameters);
        entry.Write(zdo);
      }
      removeItems?.RemoveItems(parameters, zdo);
      addItems?.AddItems(parameters, zdo);
      var owner = info.Owner?.Get(parameters);
      if (owner.HasValue)
        zdo.SetOwner(owner.Value);
      if (data != null || removeItems != null || addItems != null || owner.HasValue)
      {
        zdo.DataRevision += 100;
        ZDOMan.instance.ForceSendZDO(zdo.m_uid);
      }
    }
    var cancel = info.Cancel?.GetBool(parameters) == true;
    return cancel;
  }
  private static void HandleSpawns(Info info, ZDO zdo, Parameters pars, bool remove, bool regenerate, string dataStr)
  {
    // Original object must be regenerated to apply data.
    var regenerateOriginal = !remove && regenerate;
    if (info.Spawns == null && info.Swaps == null && !regenerateOriginal) return;

    var customData = DataHelper.Get(dataStr);
    if (info.Spawns != null)
      foreach (var p in info.Spawns)
        CreateObject(p, zdo, customData, pars);

    if (info.Swaps == null && !regenerateOriginal) return;
    var data = DataHelper.Merge(new DataEntry(zdo), customData);
    if (info.Swaps != null)
      foreach (var p in info.Swaps)
        CreateObject(p, zdo, data, pars);
    if (regenerateOriginal)
    {
      var removeItems = info.RemoveItems;
      var addItems = info.AddItems;
      ZdoEntry entry = new(zdo);
      if (data != null)
        entry.Load(data, pars);
      var newZdo = CreateObject(entry, false);
      if (newZdo != null)
      {
        removeItems?.RemoveItems(pars, newZdo);
        addItems?.AddItems(pars, newZdo);
        PrefabConnector.AddSwap(zdo.m_uid, newZdo.m_uid);
      }
    }
  }
  public static void RemoveZDO(ZDO zdo, bool triggerRules)
  {
    if (!triggerRules)
      ZDOMan.instance.m_deadZDOs[zdo.m_uid] = ZNet.instance.GetTime().Ticks;
    zdo.SetOwner(ZDOMan.instance.m_sessionID);
    ZDOMan.instance.DestroyZDO(zdo);
  }
  public static void CreateObject(Spawn spawn, ZDO originalZdo, DataEntry? data, Parameters parameters)
  {
    var pos = originalZdo.m_position;
    var rotQuat = originalZdo.GetRotation();
    pos += rotQuat * (spawn.Pos?.Get(parameters) ?? Vector3.zero);
    rotQuat *= spawn.Rot?.Get(parameters) ?? Quaternion.identity;
    var rot = rotQuat.eulerAngles;
    if (spawn.Snap?.GetBool(parameters) == true)
      pos.y = WorldGenerator.instance.GetHeight(pos.x, pos.z);
    data = DataHelper.Merge(data, DataHelper.Get(spawn.Data?.Get(parameters) ?? ""));
    var prefab = spawn.GetPrefab(parameters);
    ZdoEntry zdoEntry = new(prefab, pos, rot, originalZdo);
    if (data != null)
      zdoEntry.Load(data, parameters);
    DelayedSpawn.Add(spawn.Delay?.Get(parameters) ?? 0f, zdoEntry, spawn.TriggerRules?.GetBool(parameters) ?? false);
  }

  public static ZDO? CreateObject(ZdoEntry entry, bool triggerRules)
  {
    HandleCreated.Skip = !triggerRules;
    var zdo = entry.Create();
    HandleCreated.Skip = false;
    if (zdo == null) return null;
    FixOwner(zdo);
    return zdo;
  }

  private static void FixOwner(ZDO zdo)
  {
    // Some client should always be the owner so that creatures are initialized correctly (for example max health from stars).
    // Things work slightly better when the server doesn't have ownership (for example max health from stars).

    // During ghost init, objects are meant to be unloaded so setting owner could cause issues.
    if (ZNetView.m_ghostInit) return;
    // When single player, the owner is always the client.
    // When self-hosted, things might not work but can be fixed later if needed.
    if (!ZNet.instance.IsDedicated()) return;
    // For client spawns, the original owner can be just used.
    if (zdo.GetOwner() != ZDOMan.instance.m_sessionID) return;

    var closestClient = ZDOMan.instance.m_peers.OrderBy(p => Utils.DistanceXZ(p.m_peer.m_refPos, zdo.m_position)).FirstOrDefault(p => p.m_peer.m_uid != zdo.GetOwner());
    zdo.SetOwnerInternal(closestClient?.m_peer.m_uid ?? 0);
  }

  public static void SpawnDrops(ZDO zdo)
  {
    if (ZNetScene.instance.m_instances.ContainsKey(zdo))
    {
      SpawnDrops(ZNetScene.instance.m_instances[zdo].gameObject);
    }
    else
    {
      var obj = ZNetScene.instance.CreateObject(zdo);
      obj.GetComponent<ZNetView>().m_ghost = true;
      ZNetScene.instance.m_instances.Remove(zdo);
      SpawnDrops(obj);
      UnityEngine.Object.Destroy(obj);
    }
  }
  private static void SpawnDrops(GameObject obj)
  {
    HandleCreated.Skip = true;
    if (obj.TryGetComponent<DropOnDestroyed>(out var drop))
      drop.OnDestroyed();
    if (obj.TryGetComponent<CharacterDrop>(out var characterDrop))
    {
      characterDrop.m_character = obj.GetComponent<Character>();
      if (characterDrop.m_character)
        characterDrop.OnDeath();
    }
    if (obj.TryGetComponent<Piece>(out var piece))
      piece.DropResources();
    if (obj.TryGetComponent<TreeBase>(out var tree))
    {
      var items = tree.m_dropWhenDestroyed.GetDropList();
      foreach (var item in items)
        UnityEngine.Object.Instantiate(item, obj.transform.position, Quaternion.identity);
    }
    if (obj.TryGetComponent<TreeLog>(out var log))
    {
      var items = log.m_dropWhenDestroyed.GetDropList();
      foreach (var item in items)
        UnityEngine.Object.Instantiate(item, obj.transform.position, Quaternion.identity);
    }
    HandleCreated.Skip = false;
  }

  public static void Poke(Info info, ZDO zdo, Parameters pars)
  {
    var pos = zdo.m_position;
    var rot = zdo.GetRotation();
    if (info.LegacyPokes != null)
    {
      var zdos = ObjectsFiltering.GetNearby(info.PokeLimit, info.LegacyPokes, pos, rot, pars);
      var pokeParameter = Evaluate(pars.Replace(info.PokeParameter));
      DelayedPoke.Add(info.PokeDelay, zdos, pokeParameter);
    }
    if (info.Pokes == null) return;
    foreach (var poke in info.Pokes)
    {
      var pokeParameter = Evaluate(pars.Replace(poke.Parameter ?? ""));
      var delay = poke.Delay?.Get(pars) ?? 0f;
      if (poke.Self?.GetBool(pars) == true)
        DelayedPoke.Add(delay, zdo, pokeParameter);
      else
      {
        var zdos = ObjectsFiltering.GetNearby(poke.Limit?.Get(pars) ?? 0, poke.Filter, pos, rot, pars);
        DelayedPoke.Add(delay, zdos, pokeParameter);
      }

    }
  }
  public static void PokeGlobal(Info info, Parameters pars, Vector3 pos)
  {
    if (info.LegacyPokes != null)
    {
      var zdos = ObjectsFiltering.GetNearby(info.PokeLimit, info.LegacyPokes, pos, Quaternion.identity, pars);
      var pokeParameter = Evaluate(pars.Replace(info.PokeParameter));
      DelayedPoke.Add(info.PokeDelay, zdos, pokeParameter);
    }
    if (info.Pokes == null) return;
    foreach (var poke in info.Pokes)
    {
      var pokeParameter = Evaluate(pars.Replace(poke.Parameter ?? ""));
      var zdos = ObjectsFiltering.GetNearby(poke.Limit?.Get(pars) ?? 0, poke.Filter, pos, Quaternion.identity, pars);
      DelayedPoke.Add(poke.Delay?.Get(pars) ?? 0f, zdos, pokeParameter);

    }
  }

  private static string Evaluate(string str)
  {
    var expressions = str.Split(' ').ToArray();
    bool changed = false;
    for (var i = 0; i < expressions.Length; ++i)
    {
      var expression = expressions[i];
      if (expression.Length == 0) continue;
      // Single negative number would get handled as expression.
      var sub = expression.Substring(1);
      if (!sub.Contains('*') && !sub.Contains('/') && !sub.Contains('+') && !sub.Contains('-')) continue;
      changed = true;
      var value = Calculator.EvaluateFloat(expression) ?? 0f;
      expressions[i] = value.ToString("0.#####", NumberFormatInfo.InvariantInfo);
    }
    return changed ? string.Join(" ", expressions) : str;
  }
  public static void Poke(ZDO[] zdos, string parameter)
  {
    foreach (var z in zdos)
      Handle(ActionType.Poke, parameter, z);
  }
  public static void Poke(ZDO zdo, string parameter)
  {
    Handle(ActionType.Poke, parameter, zdo);
  }
  public static void Terrain(Info info, ZDO zdo, Parameters pars)
  {
    if (info.Terrains == null) return;
    var pos = zdo.m_position;
    var rot = Quaternion.Euler(zdo.m_rotation);
    var source = zdo.GetOwner();
    foreach (var terrain in info.Terrains)
    {
      var delay = terrain.Delay?.Get(pars) ?? 0f;
      terrain.Get(pars, pos, rot, out var p, out var s, out var resetRadius, out var pkg);
      DelayedTerrain.Add(delay, source, p, s, pkg, resetRadius);
    }
  }

  public static void ObjectRpc(ObjectRpcInfo[] info, ZDO zdo, Parameters parameters)
  {
    foreach (var i in info)
      i.Invoke(zdo, parameters);
  }
  public static void ClientRpc(ClientRpcInfo[] info, ZDO zdo, Parameters parameters)
  {
    foreach (var i in info)
      i.Invoke(zdo, parameters);
  }
  public static void GlobalClientRpc(ClientRpcInfo[] info, Parameters parameters)
  {
    foreach (var i in info)
      i.InvokeGlobal(parameters);
  }
  public static void ModifyTerrain(long source, Vector3 pos, float radius, ZPackage pkg, float resetRadius)
  {
    // Terrain may have to be modified in multiple zones.
    var corner1 = pos + new Vector3(radius, 0, radius);
    var corner2 = pos + new Vector3(-radius, 0, -radius);
    var corner3 = pos + new Vector3(-radius, 0, radius);
    var corner4 = pos + new Vector3(radius, 0, -radius);
    var zone1 = ZoneSystem.GetZone(corner1);
    var zone2 = ZoneSystem.GetZone(corner2);
    var zone3 = ZoneSystem.GetZone(corner3);
    var zone4 = ZoneSystem.GetZone(corner4);
    var startI = Mathf.Min(zone1.x, zone2.x, zone3.x, zone4.x);
    var endI = Mathf.Max(zone1.x, zone2.x, zone3.x, zone4.x);
    var startJ = Mathf.Min(zone1.y, zone2.y, zone3.y, zone4.y);
    var endJ = Mathf.Max(zone1.y, zone2.y, zone3.y, zone4.y);

    for (var i = startI; i <= endI; i++)
    {
      for (var j = startJ; j <= endJ; j++)
      {
        var zone = new Vector2i(i, j);
        if (!ZoneSystem.instance.IsZoneGenerated(zone)) continue;
        ModifyZoneTerrain(source, pos, zone, pkg, resetRadius);
      }
    }
  }
  private static readonly int TerrainActionHash = "ApplyOperation".GetStableHashCode();
  private static void ModifyZoneTerrain(long source, Vector3 pos, Vector2i zone, ZPackage pkg, float resetRadius)
  {
    var compiler = FindTerrainCompiler(zone);
    if (compiler != null && compiler.HasOwner())
    {
      if (resetRadius > 0f)
        ResetTerrainInZdo(pos, resetRadius, zone, compiler);
      else
        Rpc(source, compiler.GetOwner(), compiler.m_uid, TerrainActionHash, [pkg]);
    }
    // Compiler should be already there.
  }

  public static bool GenerateTerrainCompilers(long source, Vector3 pos, float radius)
  {
    // Terrain may have to be modified in multiple zones.
    var corner1 = pos + new Vector3(radius, 0, radius);
    var corner2 = pos + new Vector3(-radius, 0, -radius);
    var corner3 = pos + new Vector3(-radius, 0, radius);
    var corner4 = pos + new Vector3(radius, 0, -radius);
    var zone1 = ZoneSystem.GetZone(corner1);
    var zone2 = ZoneSystem.GetZone(corner2);
    var zone3 = ZoneSystem.GetZone(corner3);
    var zone4 = ZoneSystem.GetZone(corner4);
    var startI = Mathf.Min(zone1.x, zone2.x, zone3.x, zone4.x);
    var endI = Mathf.Max(zone1.x, zone2.x, zone3.x, zone4.x);
    var startJ = Mathf.Min(zone1.y, zone2.y, zone3.y, zone4.y);
    var endJ = Mathf.Max(zone1.y, zone2.y, zone3.y, zone4.y);

    var created = false;
    for (var i = startI; i <= endI; i++)
    {
      for (var j = startJ; j <= endJ; j++)
      {
        var zone = new Vector2i(i, j);
        if (!ZoneSystem.instance.IsZoneGenerated(zone)) continue;
        created |= GenerateZoneTerrainCompiler(source, zone);
      }
    }
    return created;
  }
  private static void ResetTerrainInZdo(Vector3 pos, float radius, Vector2i zone, ZDO zdo)
  {
    var byteArray = zdo.GetByteArray(ZDOVars.s_TCData);
    if (byteArray == null) return;
    var center = ZoneSystem.GetZonePos(zone);
    var change = false;
    var from = new ZPackage(Utils.Decompress(byteArray));
    var to = new ZPackage();
    to.Write(from.ReadInt());
    to.Write(from.ReadInt() + 1);
    from.ReadVector3();
    to.Write(center);
    from.ReadSingle();
    to.Write(radius);
    var size = from.ReadInt();
    to.Write(size);
    var width = (int)Math.Sqrt(size);
    for (int index = 0; index < size; index++)
    {
      var wasModified = from.ReadBool();
      var modified = wasModified;
      var j = index / width;
      var i = index % width;
      if (j >= 0 && j <= width - 1 && i >= 0 && i <= width - 1)
      {
        var worldPos = VertexToWorld(center, j, i);
        if (Utils.DistanceXZ(worldPos, pos) < radius)
          modified = false;
      }
      to.Write(modified);
      if (modified)
      {
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
      }
      if (wasModified && !modified)
      {
        change = true;
        from.ReadSingle();
        from.ReadSingle();
      }
    }
    size = from.ReadInt();
    to.Write(size);
    for (int index = 0; index < size; index++)
    {
      var wasModified = from.ReadBool();
      var modified = wasModified;
      var j = index / width;
      var i = index % width;
      var worldPos = VertexToWorld(center, j, i);
      if (Utils.DistanceXZ(worldPos, pos) < radius)
        modified = false;
      to.Write(modified);
      if (modified)
      {
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
        to.Write(from.ReadSingle());
      }
      if (wasModified && !modified)
      {
        change = true;
        from.ReadSingle();
        from.ReadSingle();
        from.ReadSingle();
        from.ReadSingle();
      }
    }
    if (!change) return;
    var bytes = Utils.Compress(to.GetArray());
    zdo.DataRevision += 100;
    zdo.Set(ZDOVars.s_TCData, bytes);
  }
  private static Vector3 VertexToWorld(Vector3 pos, int j, int i)
  {
    pos.x += i - 32.5f;
    pos.z += j - 32.5f;
    return pos;
  }
  private static readonly int TerrainCompilerHash = "_TerrainCompiler".GetStableHashCode();
  private static bool GenerateZoneTerrainCompiler(long source, Vector2i zone)
  {
    var compiler = FindTerrainCompiler(zone);
    if (compiler != null && compiler.HasOwner())
      return false;
    if (compiler == null)
    {
      var zdo = ZDOMan.instance.CreateNewZDO(ZoneSystem.GetZonePos(zone), TerrainCompilerHash);
      var view = ZNetScene.instance.GetPrefab(TerrainCompilerHash).GetComponent<ZNetView>();
      zdo.m_prefab = TerrainCompilerHash;
      zdo.Persistent = view.m_persistent;
      zdo.Type = view.m_type;
      zdo.Distant = view.m_distant;
      zdo.SetOwnerInternal(source);
    }
    return true;
  }
  // Terrain operations requires a terrain compiler in the zone.
  // These are only created when needed, so it might have to be added.
  private static ZDO? FindTerrainCompiler(Vector2i zone)
  {
    var index = ZDOMan.instance.SectorToIndex(zone);
    var zdos = index < 0 || index >= ZDOMan.instance.m_objectsBySector.Length
      ? ZDOMan.instance.m_objectsByOutsideSector.TryGetValue(zone, out var list) ? list : null
      : ZDOMan.instance.m_objectsBySector[index];
    return zdos?.FirstOrDefault(z => z.m_prefab == TerrainCompilerHash);
  }

  public static void Rpc(long source, long target, ZDOID id, int hash, object[] parameters)
  {
    var router = ZRoutedRpc.instance;
    ZRoutedRpc.RoutedRPCData routedRPCData = new()
    {
      m_msgID = router.m_id + router.m_rpcMsgID++,
      m_senderPeerID = source,
      m_targetPeerID = target,
      m_targetZDO = id,
      m_methodHash = hash
    };
    ZRpc.Serialize(parameters, ref routedRPCData.m_parameters);
    routedRPCData.m_parameters.SetPos(0);
    if (target == router.m_id || target == ZRoutedRpc.Everybody)
      router.HandleRoutedRPC(routedRPCData);
    if (target != router.m_id)
      router.RouteRPC(routedRPCData);
  }
}
