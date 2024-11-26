# Code running

Expand World Prefab supports running custom C# functions.

This is currently experimental and can be activated by downloading [Mono.CSharp.dll](./Mono.CSharp.dll) and placing it manually to the plugins folder.

The compiler doesn't support latest C# features so you have write many things in a more verbose way.

You can also only access public functions from Valheim code so you may have to rewrite some functionality.

Expand World Prefabs automatically compiles all ".cs" files in the "/config/expand_world" folder.

The file name determines the function name that can be used in the EWP yaml files.

## Basic example

Each file must create a new function with the `new Func` syntax.

`sum.cs`

```csharp
new Func<int, int, int>((int a, int b) => a + b);
```

Then this function can be used in the yaml files by writing `<sum_1_2>` which will be replaced by `3`.

## Persistent data

Expand World Prefabs provides CodeLoading class to store data between game sessions, with following functions:

- setBool, setFloat, setInt, setLong, setString
- getBool, getFloat, getInt, getLong, getString

All data is saved as key-value pairs in the `config/expand_world/saved_data.yaml` file.

All data is saved as text with automatic conversion to the correct type.

`saveValue.cs`

```csharp
new Func<int, int>((int a) => {
 CodeLoading.SetInt("data", a);
 return a;
});
```

`loadValue.cs`

```csharp
new Func<string>(() => CodeLoading.GetString("data"));
```

## Advanced example

This function can be used to return the weather for a specific biome and day. For example `<forecast_Meadows_10>`.

`forecast.cs`

```csharp
var GetEnvironment = new Func<EnvMan, Heightmap.Biome, BiomeEnvSetup>((EnvMan env, Heightmap.Biome b) => {
    foreach (var setup in env.m_biomes)
    {
      if (setup.m_biome != b) continue;
      return setup;
    }
    return null;
});

var RollValue = new Func<BiomeEnvSetup, int, float>((BiomeEnvSetup setup, int period) => {
    var totalWeight = 0f;
    foreach (var e in setup.m_environments)
    {
      if (e.m_ashlandsOverride || e.m_deepnorthOverride) continue;
      totalWeight += e.m_weight;
    }

    var state = UnityEngine.Random.state;
    UnityEngine.Random.InitState((int)period);
    var randomValue = UnityEngine.Random.Range(0f, totalWeight);
    UnityEngine.Random.state = state;
    return randomValue;
});
var GetWeather = new Func<BiomeEnvSetup, float, string>((BiomeEnvSetup setup, float roll) => {
    var sum = 0f;
    foreach (var e in setup.m_environments)
    {
      if (e.m_ashlandsOverride || e.m_deepnorthOverride) continue;
      sum += e.m_weight;
      if (sum >= roll) return e.m_env.m_name;
    }
    return "No weather";
});
 
new Func<string, float, string>((string biome, float day) => {
    if (!Enum.TryParse<Heightmap.Biome>(biome, true, out var b))
      return "Invalid biome";
    var env = EnvMan.instance;
    var period = (int)(day * env.m_dayLengthSec / env.m_environmentDuration);
    var setup = GetEnvironment(env, b);
    if (setup == null)
      return "No weather";
    var roll = RollValue(setup, period);
    return GetWeather(setup, roll);
});
```
