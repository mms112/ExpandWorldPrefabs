using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using ServerSync;
using Service;
namespace ExpandWorld.Prefab;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("expand_world_data", "1.27")]
public class EWP : BaseUnityPlugin
{
  public const string GUID = "expand_world_prefabs";
  public const string NAME = "Expand World Prefabs";
  public const string VERSION = "1.8";
#nullable disable
  public static CustomSyncedValue<string> valuePrefabData;
  public static Harmony Harmony;
#nullable enable
  /* Disabled for now because not fully sure what should be handled on client.
  public static ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    ModRequired = true,
    IsLocked = true
  };*/
  public static Assembly? ExpandEvents;
  public void Awake()
  {
    Harmony = new(GUID);
    Harmony.PatchAll();
    //valuePrefabData = new CustomSyncedValue<string>(ConfigSync, "prefab_data");
    //valuePrefabData.ValueChanged += Prefab.Loading.FromSetting;
    try
    {
      if (ExpandWorldData.Configuration.DataReload)
      {
        Loading.SetupWatcher();
      }
    }
    catch (Exception e)
    {
      Log.Error(e.StackTrace);
    }
  }
  public void Start()
  {
    if (Chainloader.PluginInfos.TryGetValue("expand_world_events", out var plugin))
    {
      ExpandEvents = plugin.Instance.GetType().Assembly;
    }
  }
  public void LateUpdate()
  {
    if (ZNet.instance == null) return;
    HandleCreated.Execute();
    DelayedSpawn.Execute(Time.deltaTime);
    DelayedRemove.Execute(Time.deltaTime);
  }

  public static RandomEvent GetCurrentEvent(Vector3 pos)
  {
    if (ExpandEvents == null) return RandEventSystem.instance.GetCurrentRandomEvent();
    var method = ExpandEvents.GetType("ExpandWorld.EWE").GetMethod("GetCurrentRandomEvent", BindingFlags.Public | BindingFlags.Static);
    if (method == null) return RandEventSystem.instance.GetCurrentRandomEvent();
    return (RandomEvent)method.Invoke(null, [pos]);
  }
}
