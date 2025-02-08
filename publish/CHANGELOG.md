- v1.35
  - Adds new filters `minTerrainHeight` and `maxTerrainHeight` to filter based on terrain height.
  - Adds new filters `minX`, `maxX`, `minZ` and `maxZ` to filter based on position.

- v1.34
  - Adds new parameters `save`, `load` and `clear` to save and load server side data (moved from Expand World Code mod).
  - Adds a new type `key` to trigger on save and clear events.
  - Adds new filters `keys` and `bannedKeys` to filter based on saved data.

- v1.33
  - Fixes global key resetting not triggering global key removed event.
  - Fixes custom paint values not working.
  - Fixes alpha color not being checked for paint.

- v1.32
  - Adds a new parameter `safeprefab` to get prefab name with underscores replaced by dashes.
  - Reverts the underscore mess from v1.29-1.31.

- v1.31
  - Fixes data parameters with underscores like "max_health" not working.

- v1.30
  - Fixes prefab names with underscores not working properly as function parameters.

- v1.29
  - Fixes prefab names with underscores not working properly as function parameters.
  - Fixes data entries with duplicate keys not working (now gives a warning).
  - Fixes the field `drops` not working with the Ragdoll component.
