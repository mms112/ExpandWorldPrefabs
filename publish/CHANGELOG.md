- v1.43
  - Adds a field `bannedLocationDistance` so that different distance can be used for required and banned locations.

- v1.42
  - Adds a new parameter `<biome>` to get the biome of the object.
  - Adds a new field `filterLimit` to set how many filters must match.
  - Fixes comment or extra space at end of the fields `spawn` or `swap` breaking the file parsing.
  - Reworks the filtering system. Now multiple filters are checked separately, instead of being combined into a single check.

- v1.41
  - Changes the field `poke` and `objects` to use `filter` instead of `data` (old way still works).
  - Fixes multiple values not working for the field `data`.
  - Fixes the field `exec` not working with global triggers (like time).
  - Fixes GameObject and ItemDrop types not working for field parameters.

- v1.40
  - Adds support of vector and quaternion values to the type `change`.
  - Adds a new field `admin` to only trigger for admins or non-admins.
  - Adds wildcard support to the parameter `item` to match multiple items.
  - Adds shorthand format for data entries (automatically enables the field `injectData`).
  - Adds new parameter `<addlong>`, `<calclong>`, `<divlong>`, `<modlong>`, `<mullong>` and `<sublong>` for large integer values.
  - Adds new parameter `<amount>`, `<durability>` and `<quality>` to get item properties from a chest.
  - Changes the type `change` to return `<none>` instead of `none` for empty values (to match how data filtering works).
  - Changes the type `state` to return `<none>` instead of `none` for empty values (to match how data filtering works).
  - Fixes error when trying to spawn invalid prefab.
  - Fixes parameters with upper case letters not working for filters `keys`, `bannedKeys`, `globalKeys` and `bannedGlobalKeys`.

- v1.39
  - Adds a new trigger type `change` to allow reacting to most data changes.
  - Adds numeric range and multiple value support to the field `type` parameter.
  - Fixes the parameter `<item_X_Y>` only working for first few inventory slots.
  - Fixes the field `cancel` not cancelling chat messages.
  - Improves compatiblity with Discord Control mod.

- v1.38
  - Adds a new field `exec` for running parameters with side effects.
  - Adds new parameters `save++` and `save--` to increment and decrement values.
  - Fixes data storage keys being case sensitive (now properly lower cased).
  - Fixes weight not working on filters.
  - Fixes field default values not working on filters.
  - Fixes the field `spawn` copying persistency from the original object (affects whether the object is saved).
