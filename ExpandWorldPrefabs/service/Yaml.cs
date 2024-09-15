using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Service;

public class Yaml
{
  public static string BaseDirectory = Path.Combine(Paths.ConfigPath, "expand_world");
  public static string BackupDirectory = Path.Combine(Paths.ConfigPath, "expand_world_backups");


  public static List<T> Read<T>(string pattern, bool migrate)
  {
    if (!Directory.Exists(BaseDirectory))
      Directory.CreateDirectory(BaseDirectory);
    var files = Directory.GetFiles(BaseDirectory, pattern, SearchOption.AllDirectories).Reverse().ToList();
    return Read<T>(files, migrate);
  }

  public static List<T> Read<T>(List<string> files, bool migrate)
  {
    List<T> result = [];
    foreach (var file in files)
    {
      try
      {
        var lines = migrate ? Migrate(File.ReadAllLines(file)) : File.ReadAllText(file);
        result.AddRange(Deserialize<T>(lines, file));
      }
      catch (Exception ex)
      {
        Log.Error($"Error reading {Path.GetFileName(file)}: {ex.Message}");
      }
    }
    return result;
  }

  private static string Migrate(string[] lines)
  {
    List<string> result = [];
    foreach (var line in lines)
    {
      if (line.StartsWith("  spawn: "))
      {
        // Convert to spawns list.
        result.Add("  spawns:");
        result.Add("  - " + line.Substring(9));
      }
      else if (line.StartsWith("  swap: "))
      {
        // Convert to swaps list.
        result.Add("  swaps:");
        result.Add("  - " + line.Substring(8));
      }
      else result.Add(line);
    }
    return string.Join("\n", result);
  }
  public static Heightmap.Biome ToBiomes(string biomeStr)
  {
    Heightmap.Biome result = 0;
    if (biomeStr == "")
    {
      foreach (var biome in Enum.GetValues(typeof(Heightmap.Biome)))
        result |= (Heightmap.Biome)biome;
    }
    else
    {
      var biomes = Parse.Split(biomeStr);
      foreach (var biome in biomes)
      {
        if (Enum.TryParse<Heightmap.Biome>(biome, true, out var number))
          result |= number;
        else
        {
          if (int.TryParse(biome, out var value)) result += value;
          else throw new InvalidOperationException($"Invalid biome {biome}.");
        }
      }
    }
    return result;
  }
  public static void SetupWatcher(ConfigFile config)
  {
    FileSystemWatcher watcher = new(Path.GetDirectoryName(config.ConfigFilePath), Path.GetFileName(config.ConfigFilePath));
    watcher.Changed += (s, e) => ReadConfigValues(e.FullPath, config);
    watcher.Created += (s, e) => ReadConfigValues(e.FullPath, config);
    watcher.Renamed += (s, e) => ReadConfigValues(e.FullPath, config);
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }
  private static void ReadConfigValues(string path, ConfigFile config)
  {
    if (!File.Exists(path)) return;
    BackupFile(path);
    try
    {
      config.Reload();
    }
    catch
    {
      Log.Error($"There was an issue loading your {config.ConfigFilePath}");
      Log.Error("Please check your config entries for spelling and format!");
    }
  }
  public static void SetupWatcher(string pattern, Action<string> action) => SetupWatcher(Paths.ConfigPath, pattern, action);
  public static void SetupWatcher(string folder, string pattern, Action<string> action)
  {
    FileSystemWatcher watcher = new(folder, pattern);
    watcher.Created += (s, e) => action(e.FullPath);
    watcher.Changed += (s, e) => action(e.FullPath);
    watcher.Renamed += (s, e) => action(e.FullPath);
    watcher.Deleted += (s, e) => action(e.FullPath);
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }
  public static void SetupWatcher(string folder, string pattern, Action action)
  {
    FileSystemWatcher watcher = new(folder, pattern);
    watcher.Created += (s, e) => action();
    watcher.Changed += (s, e) => action();
    watcher.Renamed += (s, e) => action();
    watcher.Deleted += (s, e) => action();
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }
  public static void SetupWatcher(string pattern, Action action) => SetupWatcher(BaseDirectory, pattern, file =>
  {
    BackupFile(file);
    action();
  });
  private static void BackupFile(string path)
  {
    if (!File.Exists(path)) return;
    if (!Directory.Exists(BackupDirectory))
      Directory.CreateDirectory(BackupDirectory);
    var stamp = DateTime.Now.ToString("yyyy-MM-dd");
    var name = $"{Path.GetFileNameWithoutExtension(path)}_{stamp}{Path.GetExtension(path)}.bak";
    File.Copy(path, Path.Combine(BackupDirectory, name), true);
  }

  public static void Init()
  {
    if (!Directory.Exists(BaseDirectory))
      Directory.CreateDirectory(BaseDirectory);
  }

  private static IDeserializer Deserializer() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
  private static IDeserializer DeserializerUnSafe() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();

  private static List<T> Deserialize<T>(string raw, string file)
  {
    try
    {
      return Deserializer().Deserialize<List<T>>(raw) ?? [];
    }
    catch (Exception ex1)
    {
      Log.Error($"{Path.GetFileName(file)}: {ex1.Message}");
      try
      {
        return DeserializerUnSafe().Deserialize<List<T>>(raw) ?? [];
      }
      catch (Exception)
      {
        return [];
      }
    }
  }
}
