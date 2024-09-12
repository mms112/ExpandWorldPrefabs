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

Most of the values can be parametrized. These are indicated by letter P in this document.

Following parameters are available to be used in the yaml file:

- `<prefab>`: Original prefab id.
- `<zdo>`: Object id.
- `<par>`: Triggered parameter.
- `<par0>`, ..., `<par9>`: Part of the parameter (split by spaces).
- `<x>`, `<y>` and `<z>`: Object center point.
- `<pos>`: Object center point as x,z,y.
- `<i>` and `<j>`: Object zone indices.
- `<a>`: Object rotation.
- `<rot>`: Object rotation as y,x,z.
- `<day>`: Days since the world start (int type).
- `<time>`: Seconds since the world start (float type).
  - Each day is 1800 seconds.
- `<ticks>`: Ticks since the world start (long type).
  - Each second is 10000000 ticks.
- `<key_*>`: Global key value.
- `<int_*>`: Integer value from the object data.
- `<float_*>`: Decimal value from the object data.
- `<long_*>`: Big integer value from the object data.
- `<string_*>`: Text value from the object data.
- `<bool_*>`: Integer value from the object converted to true or false.
- `<hash_*>`: Integer value from the object converted to prefab name.
- `<vec_*>`: Vector3 value from the object converted to x,z,y.
- `<quat_*>`: Quaternion value from the object converted to y,x,z.
- `<byte_*>`: Byte value from the object converted to base64 text.
- `<zdo_*>`: Object id value from the object.
- `<item_*>`: Amount of specific item in the container.
- `<pid>`: Steam/Playfab of the client that controls the object.
  - Note: The client always controls its player object.
- `<pname>`: Player name of the client that controls the object.
- `<pchar>`: Character id of the client that controls the object.

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
    - `globalkey`: When a global key is set or removed. Parameter is the key name.
      - Use field `remove` to trigger on key removal.
      - Note: There is no prefab or position for this type, so most fields won't work.
    - `event`: When an event starts or ends. Parameter is the event name.
      - Use field `end` to trigger on event end.
      - Note: There is no prefab for this type, so most fields won't work.
  - Objects spawned or removed by this mod won't trigger `create` or `destroy`.
- weight (default: `1`, P): Chance to be selected if multiple entries match.
  - All weights are summed and the probability is `weight / sum`.
  - If the sum is less than 1, the probability is `weight`, so there is a chance to not select any entry.
- fallback (default: `false`): If true, this entry can only be selected if no other entries match.

### Actions

- remove (default: `false`, P): If true, the original object is removed.
- removeDelay (P): Delay in seconds for remove.
- data (P): Changes data to the original object.
  - Name of the data entry (from `data.yaml`) or data code that is added to the object.
  - This is done by respawning the original object with the new data.
- injectData (default: `false`): If true, the object is not respawned when adding data.
  - Note: This doesn't work in most cases because clients don't load the new data.
  - Some possible cases are:
    - When adding data that is only used by this mod. In this case, clients wouldn't use the data anyway.
    - When changing data that changes during the normal game play. For example creature health.
- spawnDelay: Delay in seconds for spawns and swaps.
- spawn: Spawns another object.
  - Format is `id, posX,posZ,posY, rotY,rotX,rotZ, data, delay, triggerRules`.
  - Most parts are optional. For example following formats are valid:
    - `id, posX,posZ,posY, rotY,rotX,rotZ, delay`
    - `id, posX,posZ,posY, data, triggerRules`
    - `id, data, delay`
    - `id, triggerRules`
  - Id is required and supports parameters.
  - Position must be set before rotation.
  - PosY can be `snap` to snap to the ground.
- swap: Swaps the original object with another object.
  - Format and keywords are same as for `spawn`.
  - The initial data is copied from the original object.
  - Swap is done by removing the original object and spawning the swapped object.
  - If the swapped object is not valid, the original object is still removed.
  - Note: Swapping can break ZDO connection, so spawn points may respawn even when the creature is alive.
- command: Console command to run.
  - Parameters are supported.
  - Basic arithmetic is supported. For example `<x>+10` would add 10 meters to the x coordinate.
- triggerRules (default: `false`): If true, spawns or remove from this entry can trigger other entries.
- addItems: Data entry that is used to add items to the container object.
  - Data type "items" is used for this.
  - If the item exists, its stack amount is increased up to the max.
  - Remaining stack amount is added as new items.
  - For adding a single item, shorthand `itemid, amount` can be used.
- removeItems: Data entry that is used to removes items from the container object.
  - Data type "items" is used for this.
  - If the item doesn't exist then nothing happens.
  - For removing a single item, shorthand `itemid, amount` can be used.

## Filters

If a filter is not specified, it's not checked and is always considered valid.

- biomes: List of valid biomes.
- day (P): Valid during the day.
- night (P): Valid during the night.
- minDistance (P): Minimum distance from the world center.
- maxDistance (P): Maximum distance from the world center.
- minY (P): Minimum y coordinate.
- maxY (P): Maximum y coordinate.
- minAltitude (P): Minimum altitude (y coordinate + 30).
- maxAltitude (P): Maximum altitude (y coordinate + 30).
- environments: List of valid environments.
- bannedEnvironments: List of  invalid environments.
- globalKeys: List of global keys that must be set.
  - Parameters are supported.
  - The values are converted to lower case because the game always uses lower case.
- bannedGlobalKeys: List of global keys that must not be set.
  - Parameters are supported.
  - The values are converted to lower case because the game always uses lower case.
- locations: List of location ids. At least one must be nearby.
- locationDistance (default: `0` meters): Search distance for nearby locations.
  - If 0, uses the location exterior radius.
- events: List of event ids. At least one must be active nearby.
  - If set without `eventDistance`, the search distance is 100 meters.
- eventDistance: Search distance for nearby events.
  - If set without `events`, any nearby event is valid.
- filter: Data filter for the object. This can be a data entry or a single data value.
  - Format for a single data value is `type, key, value`. Supported types are bool, int, hash, float and string.
    - `filter: bool, boss, true` would apply only to boss creatures.
    - `filter: string, Humanoid.m_name, Piggy` would apply only to creatures with name "Piggy".
  - Ranges are supported for int and float.
    - `filter: int, level, 2;3` would apply to creatures with 1 or 2 stars
    - `filter: int, level, 0;1` is required for 1 star because 0 is the default value.
  - Wildcards are suppored for strings.
    - `filter: string, TamedName, *(S)*` would apply to pets with name containing "(S)".
  - For type `repair`, the filter is also checked for the player who did the repair.
    - Filter is valid if either the player or the object matches.
  - Data type "items" can be used to filter containers.
    - If item amount is not set, then the items must match exactly.
    - If item amount is set, then at least that many items must match.
    - Items are checked in the same order as they are defined in the "items" list.
    - If item amount is set but items are not, then only the item count is checked.
- bannedFilter: Data filter that must not be true.

### Object filters

- objectsLimit: How many of the filters must match (`min` or `min-max`).
  - If not set, then each filter must be matched at least once. One object can match multiple filters.
  - If set, that many filters must be matched. Each filter can be matched by multiple objects.
  - Note: When using max, all objects must be searched.
- objects: List of object information. Format is `- id, distance, data, weight`:
  - id: Object id. Keywords are supported ("all", "creature" and "<>").
  - distance: Distance to the object (`max` or `min-max`). Default is up to 100 meters.
    - Note: All objects are searched if the max distance is more than 10000 meters.
  - data: Optional. Entry in the `data.yaml` to be used as filter. All data entries must match.
  - weight: Optional. How much tis match counts towards the `objectsLimit`. Default is 1.
  - Note: If `objectsLimit` is set and multiple filters match, the first one is matched.
- bannedObjectsLimit: How many of the filters must not match (`min` or `min-max`).
- bannedObjects: List of object information.

See object filtering [examples](examples_object_filtering.md).

### Pokes

- poke (P): List of poke objects:
  - prefab: Target object id or value group.
  - parameter: Custom value used as the parameter for the `poke` type.
  - delay: Delay in seconds for poking. Default is 0 seconds.
  - limit: Maximum amount of poked objects. If not set, all matching objects are poked.
  - minDistance: Minimum distance from the poker. Default is 0 meters.
  - maxDistance: Maximum distance from the poker. Default is 100 meters.
  - data: Optional. Entry in the `data.yaml` to be used as filter. All data entries must match.

### Legacy pokes

Old way of poking.

- pokeDelay: Delay in seconds for poking.
- pokeParameter: Custom value used as the parameter for the `poke` type.
- pokeLimit: Maximum amount of poked objects.
  - If not set, all matching objects are poked.
- pokes: List of object information. Format is `- id, distance, data`:
  - id: Object id or value group.
  - distance: Distance to the object (`max` or `min-max`). Default is up to 100 meters.
  - data: Optional. Entry in the `data.yaml` to be used as filter. All data entries must match.

### RPCs

RPC calls are way to send data to clients. Usually these are used by clients but server can call them too.

Checks possible RPCs [here](RPCs.md).

- objectRpc: List of RPC calls. The RPC must be related to the triggering object.
- clientRpc: List of RPC calls. These calls are not related to any object.

RPC format:

- name: Name of the RPC call. Must be exact match.
  - See list of supported calls: (RPCs.md)
- target: Target of the RPC call. Default is `owner`.
  - `owner`: The RPC is sent to the owner of the object.
  - `all`: The is sent to all clients.
  - ZDO id: The RPC is sent to the owner of this ZDO.
    - Parameters are supported. For example `<zdo>` can be useful.
- delay: Delay in seconds for the RPC call.
- source: ZDO id. The RPC call is faked to be from owner of this ZDO.
  - Parameters are supported. For example `<zdo>` can be useful.
- packaged: If true, the parameters are sent as a package. Default is false.
  - This must be set to true for some RPC calls.
- 1: First parameter.
- 2: Second parameter.
- 3: Third parameter.
- ...: More parameters.

### Lists

To set multiple values, following fields can be used instead:

- types: List of types.
- swaps: Swaps the object with multiple objects.
- spawns: Spawns multiple objects.
- commands: List of console commands to run.
- filters: List of data value filters. All must match.
- bannedFilters: List of data value filters. None must match.

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
