- v1.33
  - Fixes global key resetting not triggering global key removed event.

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

- v1.28
  - Improves data connection support.
