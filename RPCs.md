# Expand World Prefabs: RPCs

RPCs provide a way to call client code from the server.

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

## Simple RPCs

Some RPCs have simplified versions for ease of use.

```yaml
# Aggravates the creature (Dverger).
  rpc: aggravate
# Alerts the creature.
  rpc: alert
# Adds ammo to the turret.
  rpc: ammo, "item name"
# Decreases the ship speed.
  rpc: backward
# Broadcasts a message in the center.
  rpc: broadcast, "message"
# Calms the creature (Dverger).
  rpc: calm
# Damages the target.
  rpc: damage, "hit data"
# Toggles the door. Open direction is true by default.
  rpc: door, "open forward?"
# Increases the ship speed.
  rpc: forward
# Heals the target. Text is shown by default.
  rpc: heal, "amount", "show text?"
# Shows message at the top left.
  rpc: message, "message"
# Removes the structure.
  rpc: remove
# Repairs the structure.
  rpc: repair
# Sets the rudder angle.
  rpc: rudder, "rudder angle"
# Adds a status effect to the target. Timer is reset by default.
  rpc: status, "name", "reset timer?"
# Staggers the target. Direction is optional.
  rpc: stagger, "direction"
# Causes the player to lose or again stamina.
  rpc: stamina, "amount"
# Stops the ship.
  rpc: stop
# Teleports the player.
  rpc: teleport, x, z, y, rotY
```

## Object RPCs

This list all RPC calls related to some object.

Recommended RPC target is shown after the RPC name.

- Most calls check for ZDO ownership. Using `all` would just increase the network traffic without any effect.
- Calls with `all` can be used just with `owner` too, but this might not show all visual effects to the clients.
- Calls with `target` need a custom ZDO id to do anything meaningful.

### ArmorStand

```yaml
# Destroys item at specific slot.
  objectRpc:
  - RPC_DestroyAttachment, owner
  - int, "index of the item slot"
```

```yaml
# Drops item from specific slot.
  objectRpc:
  - RPC_DropItem, owner
  - int, "index of the item slot"
```

```yaml
# Drops item with specific name.
  objectRpc:
  - RPC_DropItemByName, owner
  - string, "name of the item"
```

```yaml
# Asks to become owner of the ZDO (not much use as the server).
  objectRpc:
  - RPC_RequestOwn, owner
```

```yaml
# Sets specific pose.
  objectRpc:
  - RPC_SetPose, all
  - int, "number of the pose"
```

```yaml
# Sets item to a specific slot.
  objectRpc:
  - RPC_SetVisualItem, all
  - int, "index of the item slot"
  - string, "name of the item"
  - int, "variant number of the item"
```

### BaseAI (AnimalAI + MonsterAI)

```yaml
# Alerts the creature.
  objectRpc:
  - Alert, owner
```

```yaml
# Alerts the creature and sets the attacker zdo as the target.
  objectRpc:
  - OnNearProjectileHit, owner
  - vec, "location of the projectile, unused"
  - float, "trigger range, unused"
  - zdo, "attacker zdo" 
```

```yaml
# Sets the aggravated state.
  objectRpc:
  - SetAggravated, owner
  - bool, "is aggravated"
  - enum_reason, Damage/Building/Theif # - int, 0/1/2
```

### Bed

```yaml
# Sets bed owner.
  objectRpc:
  - SetOwner, owner
  - long, "player id"
  - name, "owner name"
```

### Beehive

```yaml
# Alerts the creature.
  objectRpc:
  - RPC_Extract, owner
```

### Character (Humanoid + Player)

```yaml
# Sets noise level of the creature (which attracts enemies).
  objectRpc:
  - AddNoise, owner
  - float, "noise level"
```

```yaml
# Deals damage to the creature.
  objectRpc:
  - Damage, owner
  - hit, "hit data"
```

```yaml
# Freezes animations.
  objectRpc:
  - FreezeFrame, all
  - float, "duration of the freeze frame"
```

```yaml
# Heals the creature.
  objectRpc:
  - Heal, owner
  - float, "amount of health"
  - bool, "whether to show text"
```

```yaml
# Staggers the creature.
  objectRpc:
  - Stagger, owner
  - vec, "direction of the stagger"
```

```yaml
# Resets clothing.
  objectRpc:
  - ResetCloth, all
```

```yaml
# Sets the tame state.
  objectRpc:
  - SetTamed, owner
  - bool, "is tamed"
```

```yaml
# Teleports the creature. Only implemented for players.
  objectRpc:
  - RPC_TeleportTo, owner
  - vec, "location to teleport to"
  - quat, "rotation to teleport to"
  - bool, "whether is distant teleport"
```

### Container

```yaml
# Requests to open the container.
  objectRpc:
  - RequestOpen, owner
  - long, "player id"
```

```yaml
# Opens the container.
  objectRpc:
  - OpenRespons, target
  - bool, "can be opened?"
```

```yaml
# Requests to stack items.
  objectRpc:
  - RPC_RequestStack, owner
  - long, "player id"
```

```yaml
# Stacks items to the chest.
  objectRpc:
  - RPC_StackResponse, all
  - bool, "can be stacked?"
```

```yaml
# Requests to take items.
  objectRpc:
  - RequestTakeAll, all
  - long, "player id"
```

```yaml
# Takes items from the chest.
  objectRpc:
  - TakeAllRespons, all
  - bool, "can be taken?"
```

### CookingStation

```yaml
# Adds a single fuel.
  objectRpc:
  - AddFuel, owner
```

```yaml
# Adds a single item.
  objectRpc:
  - AddItem, owner
  - string, "name of the item"
```

```yaml
# Sets visual item of a slot.
  objectRpc:
  - SetSlotVisual, all
  - int, "index of the slot"
  - name, "name of the item"
```

```yaml
# Removes cooked items and spawns them at specific position.
  objectRpc:
  - RemoveDoneItem, owner
  - vec, "spawn position"
```

### Destructible

```yaml
# Creates visual fragments (if m_autoCreateFragments is true).
  objectRpc:
  - CreateFragments, all
```

```yaml
# Deals damage to the creature.
  objectRpc:
  - Damage, owner
  - hit, "hit data"
```

### Door

```yaml
# Destroys item at specific slot.
  objectRpc:
  - UseDoor, owner
  - bool, "forward or backward"
```

### FishingFloat

```yaml
# Nibbles???
  objectRpc:
  - RPC_Nibble, owner
  - zdo, "fish id"
  - bool, "is correct bait?"
```

### FootStep

```yaml
# Shows a footstep.
  objectRpc:
  - Step, all
  - int, "effect index"
  - vec, "position"
```

### Fermenter

```yaml
# Adds a single item.
  objectRpc:
  - AddItem, owner
  - string, "name of the item"
```

```yaml
# Drop the fermented item.
  objectRpc:
  - Tap, owner
```

### Fireplace

```yaml
# Adds a single fuel.
  objectRpc:
  - AddFuel, owner
```

### Fish

```yaml
# Part 2 of the pick up logic.
# Picks up the fish to the player inventory (doesn't do anything as the server).
  objectRpc:
  - Pickup, target
```

```yaml
# Part 1 of the pick up logic.
# Client sends this to the ZDO owner, that then sends back PickUp to cause the original client to pick up the fish.
  objectRpc:
  - RequestPickup, owner
```

### Incinerator

```yaml
# Sets the lever pulled.
  objectRpc:
  - RPC_AnimateLever, all
`

```yaml
# Sets the lever back.
  objectRpc:
  - RPC_AnimateLeverReturn, all
```

```yaml
# Attempts to incinerate the items.
  objectRpc:
  - RPC_RequestIncinerate, owner
```

```yaml
# Shows the incinerate message.
  objectRpc:
  - RPC_IncinerateRespons, owner
  - int, "result (0, 1, 2, 3)"
```

### ItemDrop

```yaml
# Asks to become owner of the ZDO (not much use as the server).
  objectRpc:
  - RPC_RequestOwn, owner
```

### ItemStand

```yaml
# Destroys the item.
  objectRpc:
  - DestroyAttachment, owner
```

```yaml
# Drops the item.
  objectRpc:
  - DropItem, owner
```

```yaml
# Sets the visual item.
  objectRpc:
  - SetVisualItem, all
  - string, "name of the item"
  - int, "variant number of the item"
  - int, "level of the item"
```

```yaml
# Asks to become owner of the ZDO (not much use as the server).
  objectRpc:
  - RPC_RequestOwn, owner
```

### MapTable

```yaml
# Sets data.
  objectRpc:
  - MapData, owner
  - zpkg, "unusable"
```

### MineRock

```yaml
# Deals damage to a part of the rock.
  objectRpc:
  - Hit, owner
  - hit, "hit data"
  - int, "part index"
```

```yaml
# Hides part of a rock as destroyed,
  objectRpc:
  - Hide, all
  - int, "part index"
```

### MineRock5

```yaml
# Deals damage to a part of the rock.
  objectRpc:
  - Hit, owner
  - hit, "hit data"
  - int, "part index"
```

```yaml
# Sets health of a part.
  objectRpc:
  - SetAreaHealth, all
  - int, "part index"
  - float, "health"
```

### MonsterAI

```yaml
# Awakens the creature.
  objectRpc:
  - RPC_Wakeup, owner
```

### MusicLocation

```yaml
# Sets the music as played.
  objectRpc:
  - SetPlayed, owner
```

### MusicVolume

```yaml
# Starts playing the music.
  objectRpc:
  - RPC_PlayMusic, all
```

### Pickable

```yaml
# Spawns the picked item.
  objectRpc:
  - Pick, owner
```

```yaml
# Sets the picked state.
  objectRpc:
  - SetPicked, all
  - bool, "is picked"
```

### PickableItem

```yaml
# Spawns the picked item.
  objectRpc:
  - Pick, owner
```

### Player

```yaml
# Shows message.
  objectRpc:
  - Message, owner
  - enum_message, TopLeft/Center  # - int, 1/2
  - string, "shown message"
  - int, "shown amount"
```

```yaml
# Removes visuals.
  objectRpc:
  - OnDeath, all
```

```yaml
# Triggers UI/music based on detection status.
  objectRpc:
  - OnTargeted, owner
  - bool, "is sensed"
  - bool, "is targeted"
```

```yaml
# Uses stamina.
  objectRpc:
  - UseStamina, owner
  - float, "amount of stamina"
```

### PrivateArea

```yaml
# Toggles the ward on or off. 
# Only works if the player id matches the creator.
  objectRpc:
  - ToggleEnabled, owner
  - long, "player id"
```

```yaml
# Toggles a permitted player on or off.
# Only works if the ward is not enabled.
  objectRpc:
  - TogglePermitted, owner
  - long, "player id"
  - string, "player name"
```

```yaml
# Flashes the ward.
  objectRpc:
  - FlashShield, all
```

### Projectile

```yaml
# Attaches the projectile to an object.
  objectRpc:
  - RPC_Attach, owner
  - zdo, "attached zdo"
```

```yaml
# Sets the project as it has hit something.
  objectRpc:
  - RPC_OnHit, owner
```

### ResourceRoot

```yaml
# Drains from the resource.
  objectRpc:
  - RPC_Drain, owner
  - float, "amount"
```

### Saddle

```yaml
# Controls the creature.
  objectRpc:
  - Controls, owner
  - vec, "direction"
  - int, "speed"
  - float, "ride skill"
```

```yaml
# Tries to get control of the creature.
  objectRpc:
  - RequestControl, owner
  - long, "player id"
```

```yaml
# Releases control of the creature.
  objectRpc:
  - ReleaseControl, owner
  - long, "player id"
```

```yaml
# Response from the control request.
  objectRpc:
  - RequestRespons, target
  - bool, "was request ok?"
```

```yaml
# Drops the saddle at specific position.
  objectRpc:
  - RemoveSaddle, owner
  - vec, "position"
```

### SapCollector

```yaml
# Drops the sap.
  objectRpc:
  - RPC_Extract, owner
  - zdo, "attached zdo"
```

```yaml
# Updates the visual based on the status.
  objectRpc:
  - RPC_UpdateEffects, all
```

### SEMan

```yaml
# Adds a status effect.
  objectRpc:
  - RPC_AddStatusEffect, owner
  - hash, "status effect"
  - bool, "reset time?",
  - int, "item level",
  - float, "skill level"
```

### Ship

```yaml
# Stops the ship.
  objectRpc:
  - Stop, owner
```

```yaml
# Increases the speed.
  objectRpc:
  - Forward, owner
```

```yaml
# Decreases the speed.
  objectRpc:
  - Backward, owner
```

```yaml
# Sets the rudder angle.
  objectRpc:
  - Rudder, owner
  - float, "rudder angle"
```

### ShipControl

```yaml
# Tries to get control of the ship.
  objectRpc:
  - RequestControl, owner
  - long, "player id"
```

```yaml
# Releases control of the ship.
  objectRpc:
  - ReleaseControl, owner
  - long, "player id"
```

```yaml
# Response from the control request.
  objectRpc:
  - RequestRespons, target
  - bool, "was request ok?"
```

### Smelter

```yaml
# Adds a single fuel.
  objectRpc:
  - AddFuel, owner
```

```yaml
# Adds a single item.
  objectRpc:
  - AddOre, owner
  - string, "name of the item"
```

```yaml
# Drops the smelted items
  objectRpc:
  - EmptyProcessed, owner
```

### Talker

```yaml
# Stops the ship.
  objectRpc:
  - Say, all
  - int, 0/1/2 # Whisper, normal, shout
  - userinfo, "unusable"
  - string, "message"
  - string, "sender network id"
```

### Tameable

```yaml
# Adds a saddle. Only works if the creature can be saddled.
  objectRpc:
  - AddSaddle, owner
```

```yaml
# Toggles the command state.
  objectRpc:
  - Command, owner
  - zdo, "commanding player"
  - bool, "whether to show the message"
```

```yaml
# Sets the name.
  objectRpc:
  - SetName, owner
  - string, "name of the creature"
  - string, "name of the author, for console parental controls"
```

```yaml
# Destroyes the creature and shows the unsummoning effect.
  objectRpc:
  - RPC_UnSummon, all
```

```yaml
# Sets the saddle visual.
  objectRpc:
  - SetSaddle, all
  - bool, "is saddled"
```

### TeleportWorld

```yaml
# Sets portal tag.
  objectRpc:
  - SetTag, owner
  - string, "tag"
  - string, "name of the author for console parental controls"
```

### TerrainComp

```yaml
# Performs a terrain operation.
  objectRpc:
  - ApplyOperation, owner
  - zpkg, "unusabke"
```

### Trap

```yaml
# Sets trap state.
  objectRpc:
  - RPC_RequestStateChange, owner
  - enum_trap, Armed/Disarmed/Triggered  # - int, 0/1/2
```

```yaml
# Shows trap state.
  objectRpc:
  - RPC_OnStateChanged, all
  - enum_trap, Armed/Disarmed/Triggered  # - int, 0/1/2
```

### TreeBase

```yaml
# Deals damage to the tree.
  objectRpc:
  - Damage, owner
  - hit, "hit data"
```

```yaml
# Grows the tree showing the growth effect.
  objectRpc:
  - Grow, all
```

```yaml
# Shows the shake effect.
  objectRpc:
  - Shake, all
```

### TreeLog

```yaml
# Deals damage to the tree.
  objectRpc:
  - Damage, owner
  - hit, "hit data"
```

### TriggerSpawner

```yaml
# Triggers the spawner.
  objectRpc:
  - Trigger, owner
```

### Turret

```yaml
# Adds a single ammo.
  objectRpc:
  - RPC_AddAmmo, owner
  - name, "name of the ammo"
```

```yaml
# Sets the target.
  objectRpc:
  - RPC_SetTarget, all
  - zdo, "target zdo"
```

### Vagon

```yaml
# Shows denied message.
  objectRpc:
  - RequestDenied, target
```

```yaml
# Asks to become owner of the ZDO (not much use as the server).
  objectRpc:
  - RequestOwn, owner
```

### WearNTear

```yaml
# Deals damage to the object.
  objectRpc:
  - WNTDamage, owner
  - hit, "hit data"
```

```yaml
# Sets health for visual style.
  objectRpc:
  - WNTHealthChanged, all
  - float, "amount of health"
```

```yaml
# Removes the object.
  objectRpc:
  - WNTRemove, owner
```

```yaml
# Repairs the object.
  objectRpc:
  - WNTRepair, owner
```

```yaml
# Shows visual fragments (if m_autoCreateFragments is true).
  objectRpc:
  - WNTCreateFragments, all
```

### ZSyncAnimation

```yaml
# Triggers animation state.
  objectRpc:
  - SetTrigger, all
  - name, "name of the trigger"
```

## Client rpcs

This list all RPC calls that are not related to any object.

If target is not give, the RPC is sent to all clients.

```yaml
# Teleports the client.
  clientRpc:
  - ChatMessage, target
  - vec, "text position"
  - int, type
  - userinfo, "unusable"
  - string, "message"
  - string, "sender network id"
```

```yaml
# Shows a damage text.
  clientRpc:
  - DamageText, target
  - zpkg, "unusable"
```

```yaml
# Destroys an object.
  clientRpc:
  - DestroyZDO, target
  - zpkg, "unusable"
```

```yaml
# Sends global keys. Only implemented for the client.
  clientRpc:
  - GlobalKeys, target
  - string list, "unusable"
```

```yaml
# Sends location icons. Only implemented for the client.
  clientRpc:
  - LocationIcons, target
  - zpkg, "unusable"
```

```yaml
# Calls pong RPC on the sender.
  clientRpc:
  - Ping, target
```

```yaml
# Prints network delay.
  clientRpc:
  - Pong, target
```

```yaml
# Requests ZDO from the server.
  clientRpc:
  - RequestZDO, target
  - zdo, "requested zdo"
```

```yaml
# Removes a global key. Only implemented for the server.
  clientRpc:
  - RemoveGlobalKey, target
  - string, "key"
```

```yaml
# Shows a message.
  clientRpc:
  - ShowMessage, target
  - enum_message, TopLeft/Center  # - int, 1/2
  - string, "message"
```

```yaml
# Sets the client event.
  clientRpc:
  - SetEvent, target
  - string, "name of event"
  - float, "event timer"
  - vec, "event position"
```

```yaml
# Adds a global key. Only implemented for the server.
  clientRpc:
  - SetGlobalKey, target
  - string, "key"
```

```yaml
# Starts sleeping.
  clientRpc:
  - SleepStart, target
```

```yaml
# Stops sleeping.
  clientRpc:
  - SleepStop, target
```

```yaml
# Sets the client event.
  clientRpc:
  - SpawnObject, target
  - vec, "spawn position"
  - quat, "spawn rotation"
  - hash, "prefab id"
```

```yaml
# Adds a location pin to the map.
  clientRpc:
  - RPC_DiscoverLocationResponse, target
  - string, "name of pin"
  - int, "pin type"
  - vec, "position"
  - bool, "open map"
```

```yaml
# Requests location discovery from the server.
  clientRpc:
  - RPC_DiscoverClosestLocation, target
  - string, "location name"
  - vec, "position"
  - string, "name of pin"
  - int, "pin type"
  - bool, "open map"
  - bool "discover all"
```

```yaml
# Teleports the client.
  clientRpc:
  - RPC_TeleportPlayer, target
  - vec, "location to teleport to"
  - quat, "rotation to teleport to"
  - bool, "is distant teleport"
```
