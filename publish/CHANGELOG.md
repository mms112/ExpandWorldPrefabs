- v1.16
  - Adds a new parameter "day" to get the current day of the world.
  - Adds a new parameter "ticks" to get ticks since the world start.
  - Adds a new parameter "pos" to get the object position as x,z,y.
  - Adds a new parameter "rot" to get the object rotation as y,x,z.
  - Adds new data fields `position` and `rotation`.
  - Adds a new field `packaged` to RPC calls to send parameters as a single package (some RPCs require this).
  - Adds support for the data field `connection` to use ZDO id instead of connection hash.
  - Changes the parameter "time" to be seconds instead of ticks.
  - Fixes value groups not working for data filters.
  - Fixes nested value groups not working.
  - Fixes parameters not working for data filters.
  - Updates the RPC.md file.

- v1.15
  - Adds a new type "globalkey" to trigger on global key changes.
  - Adds a new type "event" to trigger on event start or end.
  - Changes missing global keys to be evaluated as value 0.

- v1.14
  - Fixes some states not working.

- v1.13
  - Adds support for value "snap" to y-coordinate for `spawn` field.
  - Fixed for the new game version.

- v1.12
  - Adds support for parameter "<key_>" to get value of a global key.
  - Adds wild card matching to string filters.
  - Changes the fields `filter` and `bannedFilter` to use the new data system.
  - Fixes parameters not working on spawn or swap specific data.

- v1.11
  - Fixes arithmetics not working for int or long values.

- v1.10
  - Adds a new field `pokeDelay` to delay pokes.
  - Adds a new field `clientRpc` to trigger any client RPC call.
  - Adds a new field `objectRpc` to trigger any object RPC call.
  - Adds a new parameter `<zdo>` that contains the zdoid of the object.
  - Adds a new field `fallback` to only use the rule when no other rule is found.
  - Adds support for basic arithmetics.
  - Adds a new field `poke` for alternative way of doing pokes.
  - Adds support for using object data in parameters.
  - Changes the field `delay` to `spawnDelay` to make it more explicit.
  - Breaking change: Poke can now also poke the poker.
  - Fixes objects sometimes not found when out of the map bounds.
  - Fixes object filtering not working for far distances.
  - Fixes drop spawning not working on singleplayer.
  - Fixes possible error when matching float, int or long values.

- v1.9
  - Adds the new data system from World Edit Commands mod.
  - Adds a new field `triggerRules` to cause spawns from rules to trigger other rules.
  - Adds a new field `removeDelay` to delay object removing.
  - Adds value group support to the field `prefab`(from the new data system).
  - Adds parameter support to the fields `globalKeys` and `bannedGlobalKeys`.
  - Changes the fields `minDistance` and `maxDistance` to not scale with the world radius.
  - Fixes data filters not automatically updating when modifying the data entries.
  - Removes dependency of Expand World Data mod.

- v1.8
  - Adds a new field `drops` to spawn drops.
  - Adds the keyword "creature" to the field `objects` and `bannedObjects`.
  - Adds the keyword "all" to the field `prefab`.
  - Adds a new value "poke" to the field `type`.
  - Adds new fields `pokes`, `pokeLimit` and `pokeParameter`.

- v1.7
  - Adds support for checking data of the player who triggered armor stands, cooking stations, item stands or obliterators.
  - Adds a new field `delay` to delay spawns and swaps.
  - Fixes armor stand state not working.

- v1.6
  - Adds support for checking data of the player who caused the `repair` trigger.
  - Adds a new field `types` to set multiples types at once.
  - Changes the keyword format from `{}` to `<>`.
  - Fixes the mod not loading the yaml file automatically (changing the file was required).
