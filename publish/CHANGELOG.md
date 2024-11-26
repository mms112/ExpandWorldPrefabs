- v1.26
  - Adds new functions `calcf`, `calci`, `par` and `rest`.
  - Adds a new poke field `evaluate` to allow turning off automatic math evaluation.
  - Adds experimental support for custom C# functions.
  - Fixes empty string being hashed for data values with the hash type.
  - Improves automatic math evaluation to less likely trigger on non-math expressions.

- v1.25
  - Adds lots of text and number related functions as parameters.
  - Fixes animation related data not working as parameters.

- v1.24
  - Adds support for default values in the object data parameters.
  - Adds a new field `resetRadius` to the terrain operations.

- v1.23
  - Adds support for new state types (Feast eat and ItemDrop piece).
  - Adds new parameter `<pos_x,z,y>` to get the offset position from the object position.
  - Fixed for the new game version.
  - Fixes iten drops being spawned when stack size was explicitly set to 0.
  - Fixes `addItems` not working.
  - Updates the RPC.md file.

- v1.22
  - Adds a new field `owner` to set the object owner when using `injectData` field.
  - Changes the zdo field default data value to work recursively.
  - Fixes the default item variant being 1 which caused container data loading to fail.

- v1.21
  - Adds a new field `terrain` to more easily support the ApplyOperation RPC call.
  - Adds a new parameter `self` to poke actions.
  - Fixes arithmetic operations sometimes working incorrectly.
  - Fixes arithmetic operations not working on some parameters.
  - Fixes some rare edge cases with some parameters.
  - Fixes data with an unknown numeric key duplicating on the object.

- v1.20
  - Fixes wrong default value for the field `triggerRules`.
  - Fixes parameters with empty value not working.
