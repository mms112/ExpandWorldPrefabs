# Expand World Prefabs

Allows creating rules to react to objects being spawned, destroyed and more.

Install on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

## Features

- Modify or swap spawned creatures.
- Modify or swap built structures.
- Modify or swap other objects.
- Swap destroyed creatures, structures and objects.
- And a lot more...

Note: When swapping creature spawns, the spawn limit still checks the amount of original creature. This can lead to very high amount of creatures.

## Configuration

The file `expand_world/expand_prefabs.yaml` is created when loading a world.

This mod uses the [data system](https://github.com/JereKuusela/valheim-world_edit_commands/blob/main/README_data.md) of World Edit Commands.

### Parameterrs

Following parameters are available to be used in the yaml file:

- `<prefab>`: Original prefab id.
- `<zdo>`: ZDO id.
- `<par>`: Triggered parameter.
- `<par0>`, ..., `<par4>`: Part of the parameter (split by spaces).
- `<x>`, `<y>` and `<z>`: Object center point.
- `<i>` and `<j>`: Object zone indices.
- `<a>`: Object rotation.

Following parameters are available for commands and playerSearch:

- `<pid>`: Player id.
- `<pname>`: Player name.
- `<px>`, `<py>` and `<pz>`: Player position.

### expand_prefabs.yaml

Most fields are put on a single line. List values are separated by `,`.

- prefab: List of affected object ids.
  - Wildcard `*` can be used for partial matches. For example `Trophy*` to match all trophies.
  - Value groups can be used ([data system](https://github.com/JereKuusela/valheim-world_edit_commands/blob/main/README_data.md#multiple-parameter-values)).
    - By default, each object component has its own value group. For example `Tameable` or `Piece`.
    - By default, keywords `creature` (Humanoid) and `structure` (WearNTear) have their own value group.
    - Note: Values from groups are cached, so the prefab yaml must be manually saved when changing an already used value group.
- type: Type of the trigger and parameter (`type, parameter`).
  - Parameter is optional and can be used to specify the trigger.
  - Supported types are:
    - `create`: When objects are created. No parameter.
    - `destroy`: When objects are destroyed. No parameter.
    - `repair`: When structures are repaired. No parameter.
    - `damage`: When structures or trees are damaged. No parameter.
    - `state`: When objects change state. Parameter is the state name.
    - `say`: When objects or players say something. Parameter is the text.
    - `command`: When admins say something. Parameter is the text.
    - `poke`: When `pokes` field is used.
  - Objects spawned or removed by this mod won't trigger `create` or `destroy`.
- weight (default: `1`): Chance to be selected if multiple entries match.
  - All weights are summed and the probability is `weight / sum`.
  - If the sum is less than 1, the probability is `weight`, so there is a chance to not select any entry.

### Actions

- remove (default: `false`): If true, the original object is removed.
- removeDelay: Delay in seconds for remove.
- data: Injects data to the original object.
  - Name of the data entry (from `data.yaml`) or data code.
  - Injection is done by respawning the original object with new data.
- spawnDelay: Delay in seconds for spawns and swaps.
- spawn: Spawns another object.
  - Format (each part is optional):
    - `id, posX,posZ,posY, rotY,rotX,rotZ, data, delay`
    - `id, posX,posZ,posY, rotY,rotX,rotZ, data`
    - `id, posX,posZ,posY, data`
    - `id, data`
  - Id supports parameters.
- swap: Swaps the original object with another object.
  - Format and keywords are same as for `spawn`.
  - The initial data is copied from the original object.
  - Swap is done by removing the original object and spawning the swapped object.
  - If the swapped object is not valid, the original object is still removed.
  - Note: Swapping can break ZDO connection, so spawn points may respawn even when the creature is alive.
- command: Console command to run.
  - Parameters are supported.
  - Basic arithmetic is supported. For example `<x>+10` would add 10 meters to the x coordinate.
- playerSearch: Searches for nearby players for `command`.
  - The command runs for each player. If no players are found, the command doesn't run.
  - Format is `mode, distance, heightDifference`:
    - Mode is `all` or `closest`.
    - Distance is the search distsance.
    - Height difference is optionnal, if given the player must be within that distance vertically.
- triggerRules (default: `false`): If true, spawns or remove from this entry can trigger other entries.

## Filters

- biomes: List of valid biomes.
- day (default: `true`): Valid during the day.
- night (default: `true`): Valid during the night.
- minDistance (default: `0` meters): Minimum distance from the world center.
- maxDistance (default: `100000` meters): Maximum distance from the world center.
- minAltitude (default: `-10000` meters): Minimum altitude (y coordinate - 30).
- maxAltitude (default: `10000` meters): Maximum altitude (y coordinate - 30).
- minY: Minimum y coordinate. Same as altitude but without the water level offset.
- maxY: Maximum y coordinate. Same as altitude but without the water level offset.
- environments: List of valid environments.
- bannedEnvironments: List of  invalid environments.
- globalKeys: List of global keys that must be set.
  - Parameters are supported.
- bannedGlobalKeys: List of  global keys that must not be set.
  - Parameters are supported.
- locations: List of location ids. At least one must be nearby.
- locationDistance (default: `0` meters): Search distance for nearby locations.
  - If 0, uses the location exterior radius.
- events: List of event ids. At least one must be active nearby.
  - If set without `eventDistance`, the search distance is 100 meters.
- eventDistance: Search distance for nearby events.
  - If set without `events`, any nearby event is valid.
- filter: Data filter for the object.
  - Format is `type, key, value`. Supported types are bool, int, hash, float and string.
    - `filter: bool, boss, true` would apply only to boss creatures.
    - `filter: string, Humanoid.m_name, Piggy` would apply only to creatures with name "Piggy".
  - Ranges are supported for int and float.
    - `filter: int, level, 2-3` would apply to creatures with 1 or 2 stars
    - `filter: int, level, 0-1` is required for 1 star because 0 is the default value.
  - For type `repair`, the filter is also checked for the player who did the repair.
    - Filter is valid if either the player or the object matches.
- bannedFilter: Data filter that must not be true.

### Object filters

- objectsLimit: How many of the filters must match (`min` or `min-max`).
  - If not set, then each filter must be matched at least once. One object can match multiple filters.
  - If set, that many filters must be matched. Each filter can be matched by multiple objects.
  - Note: When using max, all objects must be searched.
- objects: List of object information. Format is `- id, distance, data, weight`:
  - id: Object id. Keywords are supported ("all", "creature" and "<>").
  - distance: Distance to the object (`max` or `min-max`). Default is up to 100 meters.
  - data: Optional. Entry in the `data.yaml` to be used as filter. All data entries must match.
  - weight: Optional. How much tis match counts towards the `objectsLimit`. Default is 1.
  - Note: If `objectsLimit` is set and multiple filters match, the first one is matched.
- bannedObjectsLimit: How many of the filters must not match (`min` or `min-max`).
- bannedObjects: List of object information.

See object filtering [examples](examples_object_filtering.md).

### Pokes

- pokeParameter: Custom value used as the parameter for the `poke` type.
- pokeLimit: Maximum amount of poked objects.
  - If not set, all matching objects are poked.
- pokes: List of object information. Format is `- id, distance, data`:
  - id: Object id. Keywords are supported ("all", "creature" and "<>").
  - distance: Distance to the object (`max` or `min-max`). Default is up to 100 meters.
  - data: Optional. Entry in the `data.yaml` to be used as filter. All data entries must match.
- pokeDelay: Delay in seconds for poking.

### RPCs

RPC calls are way to send data to clients. Usually these are used by clients but server can call them too.

- rpc: Hardcoded RPC calls for ease of use.
  - See list of supported calls: (RPCs.md)
- rpcDelay: Delay in seconds for RPC calls.
- objectRpc: Advanced way for calling any object RPC. No handholding.
  - The first line is the RPC name and properties: `- name, target, delay`.
    - Target is optional. By default, the RPC is only sent to the ZDO owner. If target is `all`, it is sent to all clients.
    - Delay is optional. It can be used to override the delay for this RPC.
  - The next lines are the parameters: `- type, value`.
    - All parameters must be set with the correct data types.
  - For more than one RPC, add more lines starting with the first line format.
- clientRpc: Advanced way for calling any client RPC. No handholding.
  - The first line is the RPC name and properties: `- name, target, delay`.
    - Target is optional. By default, the RPC is sent to all clients.
    - Delay is optional. It can be used to override the delay for this RPC.
  - The next lines are the parameters: `- type, value`.
    - All parameters must be set with the correct data types.
  - For more than one RPC, add more lines starting with the first line format.

### Lists

To set multiple values, following fields can be used instead:

- types: List of types.
- swaps: Swaps the object with multiple objects.
- spawns: Spawns multiple objects.
- commands: List of console commands to run.
- filters: List of data filters. All must match.
- bannedFilters: List of data filters. None must match.
- rcps: List of hardcoded RPC calls.

### States

State works for following objects:

- Armor stand: Setting item triggers state with `itemid variant slot` or `none 0 slot`.
  - For specific item on any slot, use `itemid` or `itemid variant`.
  - For any item on specific slot, use `* * slot`.
- Ballista: Targeting triggers state with the target id.
- Cooking stations: Setting item triggers state with `itemid slot` or `none slot`.
  - For specific item on any slot, use `itemid`.
  - For any item on specific slot, use `* slot`.
- Creatures: Each animation such as attacks triggers state.
- Creatures: Being targeted by ballista triggers state `target`.
- Creatures: Setting saddle triggers state `saddle` or `unsaddle`.
- Creatures: Waking up from sleep triggers state `wakeup`.
- Item stands: Setting item triggers state with `itemid variant quality` or `none 0 0`.
  - For specific item of any variant or quality, use `itemid`.
  - For any item of specific quality, use `* * quality`.
- MusicVolume: Entering the volume triggers state without parameter.
- Obliterator: Using the lever triggers state `start` and `end`.
- Pickables: Picking triggers state `picked` or `unpicked`.
- Traps: Triggering the trap triggers state with the target id.
- Ward: Triggering the ward triggers state `flash`.

## Credits

Thanks for Azumatt for creating the mod icon!

Sources: [GitHub](https://github.com/JereKuusela/valheim-expand_world_prefabs)
Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)
