using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Service;
namespace ExpandWorld.Prefab;

public class CodeLoading
{
  private static readonly string Pattern = "*.cs";

  public static void FromFile()
  {
    if (Helper.IsClient()) return;
    Load();
  }

  private static readonly Dictionary<string, MethodInfo> Functions = [];

  private static void Load()
  {
    if (Helper.IsClient()) return;
    if (MonoCSharpAssembly == null) return;
    if (Compiler == null)
      Compiler = InitCompiler();
    if (Compiler == null)
    {
      Log.Error("Error initializing code executor.");
      return;
    }

    if (!Directory.Exists(Yaml.BaseDirectory))
      Directory.CreateDirectory(Yaml.BaseDirectory);
    var files = Directory.GetFiles(Yaml.BaseDirectory, Pattern, SearchOption.AllDirectories).ToArray();
    if (files.Length == 0)
    {
      if (Functions.Count > 0)
      {
        Log.Info($"Reloading code functions (0 entries).");
        Functions.Clear();
      }
      return;
    }

    Functions.Clear();
    var evaluatorType = MonoCSharpAssembly.GetTypes().FirstOrDefault(t => t.Name == "Evaluator");
    foreach (var file in files)
    {
      var code = File.ReadAllText(file);
      try
      {
        var method = (Delegate?)evaluatorType.GetMethod("Evaluate", [typeof(string)])?.Invoke(Compiler, [code]);
        if (method == null)
        {
          Log.Error($"Error compiling code from file {file}.");
          continue;
        }
        Functions[Path.GetFileNameWithoutExtension(file)] = method.Method;
      }
      catch (InvalidCastException)
      {
        Log.Error($"Error compiling code from file {file}: The code must be a function");
      }
      catch (Exception e)
      {
        if (e.InnerException != null)
          Log.Error($"Error compiling code from file {file}: {e.InnerException.Message}");
        else
          Log.Error($"Error compiling code from file {file}: {e.Message}");
      }
    }
    if (Functions.Count == 0)
      return;
    Log.Info($"Reloading code functions ({Functions.Count} entries).");
  }
  private static Assembly? MonoCSharpAssembly;
  private static object? Compiler;
  private static object? InitCompiler()
  {
    if (MonoCSharpAssembly == null) return null;
    var types = MonoCSharpAssembly.GetTypes();
    var settingsType = types.FirstOrDefault(t => t.Name == "CompilerSettings");
    var contextType = types.FirstOrDefault(t => t.Name == "CompilerContext");
    var evaluatorType = types.FirstOrDefault(t => t.Name == "Evaluator");
    var printerType = types.FirstOrDefault(t => t.Name == "StreamReportPrinter");
    if (settingsType == null || contextType == null || evaluatorType == null || printerType == null) return null;
    var settings = Activator.CreateInstance(settingsType);
    settingsType.GetProperty("Version")?.SetValue(settings, 100);
    settingsType.GetProperty("GenerateDebugInfo")?.SetValue(settings, false);
    settingsType.GetProperty("StdLib")?.SetValue(settings, true);
    settingsType.GetProperty("Target")?.SetValue(settings, 0);
    settingsType.GetProperty("WarningLevel")?.SetValue(settings, 0);
    settingsType.GetProperty("EnhancedWarnings")?.SetValue(settings, false);
    settingsType.GetProperty("Optimize")?.SetValue(settings, true);
    settingsType.GetProperty("Unsafe")?.SetValue(settings, true);

    StringBuilder builder = new();
    StringWriter sw = new(builder);
    var printer = Activator.CreateInstance(printerType, sw);
    var context = Activator.CreateInstance(contextType, settings, printer);
    var codeExecutor = Activator.CreateInstance(evaluatorType, context);
    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
      if (asm.GetName().Name is not "mscorlib" and not "System.Core" and not "System" and not "System.Xml")
        evaluatorType.GetMethod("ReferenceAssembly")?.Invoke(codeExecutor, [asm]);
    AppDomain.CurrentDomain.AssemblyLoad += (_, args) =>
    {
      evaluatorType.GetMethod("ReferenceAssembly")?.Invoke(codeExecutor, [args.LoadedAssembly]);
    };
    string[] usings = ["using System;", "using System.Collections.Generic;", "using System.Globalization;", "using System.IO;", "using System.Linq;", "using System.Text;", "using System.Reflection;", "using UnityEngine;", "using UnityEngine.UI;", "using HarmonyLib;", "using ExpandWorld.Prefab;"];
    foreach (string u in usings)
      evaluatorType.GetMethod("Run")?.Invoke(codeExecutor, [u]);
    return codeExecutor;
  }
  public static string? Execute(string name) => Execute(name, []);
  public static string? Execute(string name, string arg) => Execute(name, arg.Split('_'));

  private static string? Execute(string name, string[] args)
  {
    if (!Functions.TryGetValue(name, out var method)) return null;
    var pars = method.GetParameters();
    var required = pars.Count(p => !p.IsOptional);
    if (required != args.Length)
    {
      Log.Error($"Method {name} expected requires {required} parameters, got {args.Length}.");
      return null;
    }
    var callArgs = args.Select((a, i) => ConvertType(a, pars[i].ParameterType)).ToArray();
    var result = method.Invoke(null, callArgs);
    return ConvertResult(result);
  }
  private static object ConvertType(string arg, Type type)
  {
    // TODO: Should check for unsuccessful conversions to return default value when inputs are not correct.
    if (type == typeof(string))
      return arg;
    if (type == typeof(int))
      return Parse.Int(arg);
    if (type == typeof(float))
      return Parse.Float(arg);
    if (type == typeof(bool))
      return bool.Parse(arg);
    return arg;
  }
  private static string ConvertResult(object result)
  {
    if (result is null)
      return "";
    if (result is string s)
      return s;
    if (result is int i)
      return i.ToString(CultureInfo.InvariantCulture);
    if (result is float f)
      return f.ToString(CultureInfo.InvariantCulture);
    if (result is bool b)
      return b.ToString();
    return result.ToString();
  }

  public static void SetupWatcher()
  {
    try
    {
      MonoCSharpAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Mono.CSharp");
      if (MonoCSharpAssembly == null)
        MonoCSharpAssembly = Assembly.Load("Mono.CSharp");
    }
    catch
    {
    }
    if (MonoCSharpAssembly == null) return;
    Log.Info("Code loading activated.");
    if (!Directory.Exists(Yaml.BaseDirectory))
      Directory.CreateDirectory(Yaml.BaseDirectory);
    Yaml.SetupWatcher(Pattern, FromFile);
    Yaml.SetupWatcher(Yaml.BaseDirectory, "ewp_data.yaml", LoadSavedData);
    LoadSavedData();
  }

  private static Dictionary<string, string> SavedData = [];

  public static void LoadSavedData()
  {
    if (UnsavedChanges) return;
    if (!Directory.Exists(Yaml.BaseDirectory))
      Directory.CreateDirectory(Yaml.BaseDirectory);
    if (!File.Exists(SavedDataFile)) return;
    var data = File.ReadAllText(SavedDataFile);
    SavedData = Yaml.DeserializeData(data);
    Log.Info($"Reloaded saved data ({SavedData.Count} entries).");
  }
  private static bool UnsavedChanges = false;
  private static long LastSave = 0;
  private static readonly string SavedDataFile = Path.Combine(Yaml.BaseDirectory, "ewp_data.yaml");
  public static void SaveSavedData()
  {
    if (!UnsavedChanges) return;
    // Save every 10 seconds at most.
    if (DateTime.Now.Ticks - LastSave < 10000000) return;
    LastSave = DateTime.Now.Ticks;
    if (!Directory.Exists(Yaml.BaseDirectory))
      Directory.CreateDirectory(Yaml.BaseDirectory);
    var yaml = Yaml.SerializeData(SavedData);
    File.WriteAllText(SavedDataFile, yaml);
    UnsavedChanges = false;
  }

  public static string GetString(string key, string defaultValue = "") => SavedData.TryGetValue(key, out var value) ? value : defaultValue;
  public static int GetInt(string key, int defaultValue = 0) => SavedData.TryGetValue(key, out var value) ? Parse.Int(value) : defaultValue;
  public static float GetFloat(string key, float defaultValue = 0f) => SavedData.TryGetValue(key, out var value) ? Parse.Float(value) : defaultValue;
  public static bool GetBool(string key, bool defaultValue = false) => SavedData.TryGetValue(key, out var value) ? bool.Parse(value) : defaultValue;
  public static long GetLong(string key, long defaultValue = 0) => SavedData.TryGetValue(key, out var value) ? Parse.Long(value) : defaultValue;
  public static void SetInt(string key, int value)
  {
    SavedData[key] = value.ToString(CultureInfo.InvariantCulture);
    UnsavedChanges = true;
  }
  public static void SetFloat(string key, float value)
  {
    SavedData[key] = value.ToString(CultureInfo.InvariantCulture);
    UnsavedChanges = true;
  }
  public static void SetString(string key, string value)
  {
    SavedData[key] = value;
    UnsavedChanges = true;
  }
  public static void SetBool(string key, bool value)
  {
    SavedData[key] = value.ToString();
    UnsavedChanges = true;
  }
  public static void SetLong(string key, long value)
  {
    SavedData[key] = value.ToString(CultureInfo.InvariantCulture);
    UnsavedChanges = true;
  }
}
