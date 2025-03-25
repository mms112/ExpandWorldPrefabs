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

- v1.37
  - Adds dependency to YamlDotNet.
  - Enables type `say`.

- v1.36
  - Disables type `say` until it is properly fixed.
  - Fixed for the new update.

- v1.35
  - Adds new filters `minTerrainHeight` and `maxTerrainHeight` to filter based on terrain height.
  - Adds new filters `minX`, `maxX`, `minZ` and `maxZ` to filter based on position.
  - Adds a new parameter `item_X_Y` to get the item name at specific inventory slot.
