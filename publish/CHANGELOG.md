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

- v1.34
  - Adds new parameters `save`, `load` and `clear` to save and load server side data (moved from Expand World Code mod).
  - Adds a new type `key` to trigger on save and clear events.
  - Adds new filters `keys` and `bannedKeys` to filter based on saved data.

- v1.33
  - Fixes global key resetting not triggering global key removed event.
  - Fixes custom paint values not working.
  - Fixes alpha color not being checked for paint.
