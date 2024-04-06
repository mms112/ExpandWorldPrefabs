# List of Valheim RPCs

This list all RPC calls registered in the vanilla game.

Recommended RPC target is shown after the RPC name.

- Most calls check for ZDO ownership. Using `all` would just increase the network traffic without any effect.
- Calls with `all` can be used just with `owner` too, but this might not show all visual effects to the clients.
- Calls with `target` need a custom ZDO id to do anything meaningful.

## Data types

Complex data types are defined as spacebar separated key-pair values.

Currently hit data is the only complex data type.

### Hit data

For example `- hit, fire=20 blunt=50`.

Options:

- damage: Raw damage that ignores resistances and armor.
- blunt: Amount of blunt damage (float)
- slash: Amount of slash damage (float)
- pierce: Amount of pierce damage (float)
- chop: Amount of chop damage (float)
- pickaxe: Amount of pickaxe damage (float)
- fire: Amount of fire damage (float)
- frost: Amount of frost damage (float)
- lightning: Amount of lightning damage (float)
- poison: Amount of poison damage (float)
- spirit: Amount of spirit damage (float)
- tier: Tool tier (int)
- force: Knockback force (float)
- backstab: Backstab multiplier (float)
- stagger: Stagger multiplier (float)
- dodge: Can be dodged? (bool)
. block: Can be blocked? (bool)
- dir: Direction of the hit (vec)
- ranged: Is ranged attack? (bool)
- attacker: Attacker ZDO id (zdo)
- pvp: Ignores PvP setting? (bool)
- pos: Position of the hit (vec)
- status: Status effect (string)
- skill: Skill level for status effect (float)
- level: Item level for status effect (int)
- world: World level tier (int)
- type: Hit type, used for player stats if dying to this hit (string)
  - Undefined, EnemyHit, PlayerHit, Fall, Drowning, Burning, Freezing, Poisoned
  - Water, Smoke, EdgeOfWorld, Impact, Cart, Tree, Self, Structural, Turret, Boat, Stalagtite
- spot: Index of Weak spot (int)

## ArmorStand

```yaml
# Destroys item at specific slot.
- customRpc:
  - RPC_DestroyAttachment, owner
  - int, "index of the item slot"
```

```yaml
# Drops item from specific slot.
- customRpc:
  - RPC_DropItem, owner
  - int, "index of the item slot"
```

```yaml
# Drops item with specific name.
- customRpc:
  - RPC_DropItemByName, owner
  - string, "name of the item"
```

```yaml
# Asks to become owner of the ZDO (not much use as the server).
- customRpc:
  - RPC_RequestOwn, owner
```

```yaml
# Sets specific pose.
- customRpc:
  - RPC_SetPose, all
  - int, "number of the pose"
```

```yaml
# Sets item to a specific slot.
- customRpc:
  - RPC_SetVisualItem, all
  - int, "index of the item slot"
  - string, "name of the item"
  - int, "variant number of the item"
```

## BaseAI (AnimalAI + MonsterAI)

```yaml
# Alerts the creature.
- customRpc:
  - Alert, owner
```

```yaml
# Alerts the creature and sets the attacker zdo as the target.
- customRpc:
  - OnNearProjectileHit, owner
  - vec, "location of the projectile, unused"
  - float, "trigger range, unused"
  - zdo, "attacker zdo" 
```

```yaml
# Sets the aggravated state.
- customRpc:
  - SetAggravated, owner
  - bool, "is aggravated"
  - enum_reason, Damage/Building/Theif # - int, 0/1/2
```

## Bed

```yaml
# Sets bed owner.
- customRpc:
  - SetOwner, owner
  - long, "player id"
  - name, "owner name"
```

## Beehive

```yaml
# Alerts the creature.
- customRpc:
  - RPC_Extract, owner
```

## Character (Humanoid + Player)

```yaml
# Sets noise level of the creature (which attracts enemies).
- customRpc:
  - AddNoise, owner
  - float, "noise level"
```

```yaml
# Deals damage to the creature.
- customRpc:
  - Damage, owner
  - hit, "hit data"
```

```yaml
# Freezes animations.
- customRpc:
  - FreezeFrame, all
  - float, "duration of the freeze frame"
```

```yaml
# Heals the creature.
- customRpc:
  - Heal, owner
  - float, "amount of health"
  - bool, "whether to show text"
```

```yaml
# Staggers the creature.
- customRpc:
  - Stagger, owner
  - vec, "direction of the stagger"
```

```yaml
# Resets clothing.
- customRpc:
  - ResetCloth, all
```

```yaml
# Sets the tame state.
- customRpc:
  - SetTamed, owner
  - bool, "is tamed"
```

```yaml
# Teleports the creature. Only implemented for players.
- customRpc:
  - RPC_TeleportTo, owner
  - vec, "location to teleport to"
  - quat, "rotation to teleport to"
  - bool, "whether is distant teleport"
```

## CookingStation

```yaml
# Adds a single fuel.
- customRpc:
  - AddFuel, owner
```

```yaml
# Adds a single item.
- customRpc:
  - AddItem, owner
  - string, "name of the item"
```

```yaml
# Sets visual item of a slot.
- customRpc:
  - SetSlotVisual, all
  - int, "index of the slot"
  - name, "name of the item"
```

```yaml
# Removes cooked items and spawns them at specific position.
- customRpc:
  - RemoveDoneItem, owner
  - vec, "spawn position"
```

## Destructible

```yaml
# Creates visual fragments (if m_autoCreateFragments is true).
- customRpc:
  - CreateFragments, all
```

```yaml
# Deals damage to the creature.
- customRpc:
  - Damage, owner
  - hit, "hit data"
```

## Door

```yaml
# Destroys item at specific slot.
- customRpc:
  - UseDoor, owner
  - bool, "forward or backward"
```

## FishingFloat

```yaml
# Nibbles???
- customRpc:
  - RPC_Nibble, owner
  - zdo, "fish id"
  - bool, "is correct bait?"
```

## FootStep

```yaml
# Shows a footstep.
- customRpc:
  - Step, all
  - int, "effect index"
  - vec, "position"
```

## Fermenter

```yaml
# Adds a single item.
- customRpc:
  - AddItem, owner
  - string, "name of the item"
```

```yaml
# Drop the fermented item.
- customRpc:
  - Tap, owner
```

## Fireplace

```yaml
# Adds a single fuel.
- customRpc:
  - AddFuel, owner
```

## Fish

```yaml
# Part 2 of the pick up logic.
# Picks up the fish to the player inventory (doesn't do anything as the server).
- customRpc:
  - Pickup, target
```

```yaml
# Part 1 of the pick up logic.
# Client sends this to the ZDO owner, that then sends back PickUp to cause the original client to pick up the fish.
- customRpc:
  - RequestPickup, owner
```

## Incinerator

```yaml
# Sets the lever pulled.
- customRpc:
  - RPC_AnimateLever, all
`

```yaml
# Sets the lever back.
- customRpc:
  - RPC_AnimateLeverReturn, all
```

```yaml
# Attempts to incinerate the items.
- customRpc:
  - RPC_RequestIncinerate, owner
```

```yaml
# Shows the incinerate message.
- customRpc:
  - RPC_IncinerateRespons, owner
  - int, "result (0, 1, 2, 3)"
```

## ItemDrop

```yaml
# Asks to become owner of the ZDO (not much use as the server).
- customRpc:
  - RPC_RequestOwn, owner
```

## ItemStand

```yaml
# Destroys the item.
- customRpc:
  - DestroyAttachment, owner
```

```yaml
# Drops the item.
- customRpc:
  - DropItem, owner
```

```yaml
# Sets the visual item.
- customRpc:
  - SetVisualItem, all
  - string, "name of the item"
  - int, "variant number of the item"
  - int, "level of the item"
```

```yaml
# Asks to become owner of the ZDO (not much use as the server).
- customRpc:
  - RPC_RequestOwn, owner
```

## MapTable

```yaml
# Sets data.
- customRpc:
  - MapData, owner
  - zpkg, "unusable"
```

## MineRock

```yaml
# Deals damage to a part of the rock.
- customRpc:
  - Hit, owner
  - hit, "hit data"
  - int, "part index"
```

```yaml
# Hides part of a rock as destroyed,
- customRpc:
  - Hide, all
  - int, "part index"
```

## MineRock5

```yaml
# Deals damage to a part of the rock.
- customRpc:
  - Hit, owner
  - hit, "hit data"
  - int, "part index"
```

```yaml
# Sets health of a part.
- customRpc:
  - SetAreaHealth, all
  - int, "part index"
  - float, "health"
```

## MonsterAI

```yaml
# Awakens the creature.
- customRpc:
  - RPC_Wakeup, owner
```

## MusicLocation

```yaml
# Sets the music as played.
- customRpc:
  - SetPlayed, owner
```

## MusicVolume

```yaml
# Starts playing the music.
- customRpc:
  - RPC_PlayMusic, all
```

## Pickable

```yaml
# Spawns the picked item.
- customRpc:
  - Pick, owner
```

```yaml
# Sets the picked state.
- customRpc:
  - SetPicked, all
  - bool, "is picked"
```

## PickableItem

```yaml
# Spawns the picked item.
- customRpc:
  - Pick, owner
```

## Player

```yaml
# Shows message.
- customRpc:
  - Message, owner
  - enum_message, TopLeft/Center  # - int, 1/2
  - string, "shown message"
  - int, "shown amount"
```

```yaml
# Removes visuals.
- customRpc:
  - OnDeath, all
```

```yaml
# Triggers UI/music based on detection status.
- customRpc:
  - OnTargeted, owner
  - bool, "is sensed"
  - bool, "is targeted"
```

```yaml
# Uses stamina.
- customRpc:
  - UseStamina, owner
  - float, "amount of stamina"
```

## PrivateArea

```yaml
# Toggles the ward on or off. 
# Only works if the player id matches the creator.
- customRpc:
  - ToggleEnabled, owner
  - long, "player id"
```

```yaml
# Toggles a permitted player on or off.
# Only works if the ward is not enabled.
- customRpc:
  - TogglePermitted, owner
  - long, "player id"
  - string, "player name"
```

```yaml
# Flashes the ward.
- customRpc:
  - FlashShield, all
```

## Projectile

```yaml
# Attaches the projectile to an object.
- customRpc:
  - RPC_Attach, owner
  - zdo, "attached zdo"
```

```yaml
# Sets the project as it has hit something.
- customRpc:
  - RPC_OnHit, owner
```

## ResourceRoot

```yaml
# Drains from the resource.
- customRpc:
  - RPC_Drain, owner
  - float, "amount"
```

## Saddle

```yaml
# Controls the creature.
- customRpc:
  - Controls, owner
  - vec, "direction"
  - int, "speed"
  - float, "ride skill"
```

```yaml
# Tries to get control of the creature.
- customRpc:
  - RequestControl, owner
  - long, "player id"
```

```yaml
# Releases control of the creature.
- customRpc:
  - ReleaseControl, owner
  - long, "player id"
```

```yaml
# Response from the control request.
- customRpc:
  - RequestRespons, target
  - bool, "was request ok?"
```

```yaml
# Drops the saddle at specific position.
- customRpc:
  - RemoveSaddle, owner
  - vec, "position"
```

## SapCollector

```yaml
# Drops the sap.
- customRpc:
  - RPC_Extract, owner
  - zdo, "attached zdo"
```

```yaml
# Updates the visual based on the status.
- customRpc:
  - RPC_UpdateEffects, all
```

## Ship

```yaml
# Stops the ship.
- customRpc:
  - Stop, owner
```

```yaml
# Increases the speed.
- customRpc:
  - Forward, owner
```

```yaml
# Decreases the speed.
- customRpc:
  - Backward, owner
```

```yaml
# Sets the rudder angle.
- customRpc:
  - Rudder, owner
  - float, "rudder angle"
```

## ShipControl

```yaml
# Tries to get control of the ship.
- customRpc:
  - RequestControl, owner
  - long, "player id"
```

```yaml
# Releases control of the ship.
- customRpc:
  - ReleaseControl, owner
  - long, "player id"
```

```yaml
# Response from the control request.
- customRpc:
  - RequestRespons, target
  - bool, "was request ok?"
```

## Smelter

```yaml
# Adds a single fuel.
- customRpc:
  - AddFuel, owner
```

```yaml
# Adds a single item.
- customRpc:
  - AddOre, owner
  - string, "name of the item"
```

```yaml
# Drops the smelted items
- customRpc:
  - EmptyProcessed, owner
```

## Tameable

```yaml
# Adds a saddle. Only works if the creature can be saddled.
- customRpc:
  - AddSaddle, owner
```

```yaml
# Toggles the command state.
- customRpc:
  - Command, owner
  - zdo, "commanding player"
  - bool, "whether to show the message"
```

```yaml
# Sets the name.
- customRpc:
  - SetName, owner
  - string, "name of the creature"
  - string, "name of the author, for console parental controls"
```

```yaml
# Destroyes the creature and shows the unsummoning effect.
- customRpc:
  - RPC_UnSummon, all
```

```yaml
# Sets the saddle visual.
- customRpc:
  - SetSaddle, all
  - bool, "is saddled"
```

## TeleportWorld

```yaml
# Sets portal tag.
- customRpc:
  - SetTag, owner
  - string, "tag"
  - string, "name of the author for console parental controls"
```

## TerrainComp

```yaml
# Performs a terrain operation.
- customRpc:
  - ApplyOperation, owner
  - zpkg, "unusabke"
```

## Trap

```yaml
# Sets trap state.
- customRpc:
  - RPC_RequestStateChange, owner
  - enum_trap, Armed/Disarmed/Triggered  # - int, 0/1/2
```

```yaml
# Shows trap state.
- customRpc:
  - RPC_OnStateChanged, all
  - enum_trap, Armed/Disarmed/Triggered  # - int, 0/1/2
```

## TreeBase

```yaml
# Deals damage to the tree.
- customRpc:
  - Damage, owner
  - hit, "hit data"
```

```yaml
# Grows the tree showing the growth effect.
- customRpc:
  - Grow, all
```

```yaml
# Shows the shake effect.
- customRpc:
  - Shake, all
```

## TreeLog

```yaml
# Deals damage to the tree.
- customRpc:
  - Damage, owner
  - hit, "hit data"
```

## TriggerSpawner

```yaml
# Triggers the spawner.
- customRpc:
  - Trigger, owner
```

## Turret

```yaml
# Adds a single ammo.
- customRpc:
  - RPC_AddAmmo, owner
  - name, "name of the ammo"
```

```yaml
# Sets the target.
- customRpc:
  - RPC_SetTarget, all
  - zdo, "target zdo"
```

## Vagon

```yaml
# Shows denied message.
- customRpc:
  - RequestDenied, target
```

```yaml
# Asks to become owner of the ZDO (not much use as the server).
- customRpc:
  - RequestOwn, owner
```

## WearNTear

```yaml
# Deals damage to the object.
- customRpc:
  - WNTDamage, owner
  - hit, "hit data"
```

```yaml
# Sets health for visual style.
- customRpc:
  - WNTHealthChanged, all
  - float, "amount of health"
```

```yaml
# Removes the object.
- customRpc:
  - WNTRemove, owner
```

```yaml
# Repairs the object.
- customRpc:
  - WNTRepair, owner
```

```yaml
# Shows visual fragments (if m_autoCreateFragments is true).
- customRpc:
  - WNTCreateFragments, all
```

## ZSyncAnimation

```yaml
# Triggers animation state.
- customRpc:
  - SetTrigger, all
  - name, "name of the trigger"
```
