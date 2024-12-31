- v1.29
  - Fixes prefab names with underscores not working properly as function parameters.
  - Fixes data entries with duplicate keys not working (now gives a warning).
  - Fixes the field `drops` not working with the Ragdoll component.

- v1.28
  - Improves data connection support.

- v1.27
  - Adds parameter support to the RPC field `delay`.
  - Fixes types "globalkey" and "event" not triggering without the field `remove` being set to false.
  - Removes support for custom C# functions (split to Expand World Code mod).

- v1.26
  - Adds new functions `calcf`, `calci`, `par` and `rest`.
  - Adds a new poke field `evaluate` to allow turning off automatic math evaluation.
  - Adds experimental support for custom C# functions.
  - Fixes empty string being hashed for data values with the hash type.
  - Improves automatic math evaluation to less likely trigger on non-math expressions.

- v1.25
  - Adds lots of text and number related functions as parameters.
  - Fixes animation related data not working as parameters.
