# Expand World Prefabs: RPCs

RPCs provide a way to call client code from the server.

## Data types

Complex data types are defined as spacebar separated key-pair values.

Currently hit data is the only complex data type.

### Hit data

For example `1: hit, fire=20 blunt=50`.

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

## Object RPCs

This list all RPC calls related to some object.

By default, the RPC is sent to the owner of the object.

- Most calls check for ZDO ownership. Using `all` would just increase the network traffic without any effect.
- Calls with `all` can be used just with `owner` too, but this might not show all visual effects to the clients.
- Calls with `all` can be replaced with `search` to use player search feature only send to nearby clients.
- Calls with `<zdo>` require custom handling to get proper ZDO id. The RPC is sent to owner of this ZDO.

### ArmorStand

```yaml
# Destroys item at specific slot.
  objectRpc:
  - name: RPC_DestroyAttachment
    1: int, "index of the item slot"
```

```yaml
# Drops item from specific slot.
  objectRpc:
  - name: RPC_DropItem
    1: int, "index of the item slot"
```

```yaml
# Drops item with specific name.
  objectRpc:
  - name: RPC_DropItemByName
    1: string, "name of the item"
```

```yaml
# Asks to become owner of the ZDO (not much use as the server).
  objectRpc:
  - name: RPC_RequestOwn
```

```yaml
# Sets specific pose.
  objectRpc:
  - name: RPC_SetPose
    target: all
    1: int, "number of the pose"
```

```yaml
# Sets item to a specific slot.
  objectRpc:
  - name: RPC_SetVisualItem
    target: all
    1: int, "index of the item slot"
    2: string, "name of the item"
    3: int, "variant number of the item"
```

### BaseAI (AnimalAI + MonsterAI)

```yaml
# Alerts the creature.
  objectRpc:
  - name: Alert
```

```yaml
# Alerts the creature and sets the attacker zdo as the target.
  objectRpc:
  - name: OnNearProjectileHit
    1: vec, "location of the projectile, unused"
    2: float, "trigger range, unused"
    3: zdo, "attacker zdo" 
```

```yaml
# Sets the aggravated state.
  objectRpc:
  - name: SetAggravated
    1: bool, "is aggravated"
    2: enum_reason, Damage/Building/Theif # - int, 0/1/2
```

### Bed

```yaml
# Sets bed owner.
  objectRpc:
  - name: SetOwner
    1: long, "player id"
    2: name, "owner name"
```

### Beehive

```yaml
# Alerts the creature.
  objectRpc:
  - name: RPC_Extract
```

### Character (Humanoid + Player)

```yaml
# Sets noise level of the creature (which attracts enemies).
  objectRpc:
  - name: AddNoise
    1: float, "noise level"
```

```yaml
# Deals damage to the creature.
  objectRpc:
  - name: Damage
    1: hit, "hit data"
```

```yaml
# Freezes animations.
  objectRpc:
  - name: FreezeFrame
    target: all
    1: float, "duration of the freeze frame"
```

```yaml
# Heals the creature.
  objectRpc:
  - name: Heal
    1: float, "amount of health"
    2: bool, "whether to show text"
```

```yaml
# Staggers the creature.
  objectRpc:
  - name: Stagger
    1: vec, "direction of the stagger"
```

```yaml
# Resets clothing.
  objectRpc:
  - name: ResetCloth
    target: all
```

```yaml
# Sets the tame state.
  objectRpc:
  - name: SetTamed
    1: bool, "is tamed"
```

```yaml
# Teleports the creature. Only implemented for players.
  objectRpc:
  - name: RPC_TeleportTo
    1: vec, "location to teleport to"
    2: quat, "rotation to teleport to"
    3: bool, "whether is distant teleport"
```

### Container

```yaml
# Requests to open the container.
  objectRpc:
  - name: RequestOpen
    source: <zdo>
    1: long, "player id"
```

```yaml
# Opens the container.
  objectRpc:
  - name: OpenRespons
    target: <zdo>
    1: bool, "can be opened?"
```

```yaml
# Requests to stack items.
  objectRpc:
  - name: RPC_RequestStack
    source: <zdo>
    1: long, "player id"
```

```yaml
# Stacks items to the chest.
  objectRpc:
  - name: RPC_StackResponse
    target: <zdo>
    1: bool, "can be stacked?"
```

```yaml
# Requests to take items.
  objectRpc:
  - name: RequestTakeAll
    source: <zdo>
    1: long, "player id"
```

```yaml
# Takes items from the chest.
  objectRpc:
  - name: TakeAllRespons
    target: <zdo>
    1: bool, "can be taken?"
```

### CookingStation

```yaml
# Adds a single fuel.
  objectRpc:
  - name: AddFuel
```

```yaml
# Adds a single item.
  objectRpc:
  - name: AddItem
    1: string, "name of the item"
```

```yaml
# Sets visual item of a slot.
  objectRpc:
  - name: SetSlotVisual
    target: all
    1: int, "index of the slot"
    2: name, "name of the item"
```

```yaml
# Removes cooked items and spawns them at specific position.
  objectRpc:
  - name: RemoveDoneItem
    1: vec, "spawn position"
```

### Destructible

```yaml
# Creates visual fragments (if m_autoCreateFragments is true).
  objectRpc:
  - name: CreateFragments
    target: all
```

```yaml
# Deals damage to the creature.
  objectRpc:
  - name: Damage
    1: hit, "hit data"
```

### Door

```yaml
# Destroys item at specific slot.
  objectRpc:
  - name: UseDoor
    1: bool, "forward or backward"
```

### FishingFloat

```yaml
# Nibbles???
  objectRpc:
  - name: RPC_Nibble
    1: zdo, "fish id"
    2: bool, "is correct bait?"
```

### FootStep

```yaml
# Shows a footstep.
  objectRpc:
  - name: Step
    target: all
    1: int, "effect index"
    2: vec, "position"
```

### Fermenter

```yaml
# Adds a single item.
  objectRpc:
  - name: AddItem
    1: string, "name of the item"
```

```yaml
# Drop the fermented item.
  objectRpc:
  - name: Tap
```

### Fireplace

```yaml
# Adds a single fuel.
  objectRpc:
  - name: AddFuel
```

### Fish

```yaml
# Part 2 of the pick up logic.
# Picks up the fish to the player inventory (doesn't do anything as the server).
  objectRpc:
  - name: Pickup
    target: <zdo>
```

```yaml
# Part 1 of the pick up logic.
# Client sends this to the ZDO owner, that then sends back PickUp to cause the original client to pick up the fish.
  objectRpc:
  - name: RequestPickup
    source: <zdo>
```

### Incinerator

```yaml
# Sets the lever pulled.
  objectRpc:
  - name: RPC_AnimateLever
    target: all
`

```yaml
# Sets the lever back.
  objectRpc:
  - name: RPC_AnimateLeverReturn
    target: all
```

```yaml
# Attempts to incinerate the items.
  objectRpc:
  - name: RPC_RequestIncinerate
```

```yaml
# Shows the incinerate message.
  objectRpc:
  - name: RPC_IncinerateRespons
    1: int, "result (0, 1, 2, 3)"
```

### ItemDrop

```yaml
# Asks to become owner of the ZDO (not much use as the server).
  objectRpc:
  - name: RPC_RequestOwn
    source: <zdo>
```

### ItemStand

```yaml
# Destroys the item.
  objectRpc:
  - name: DestroyAttachment
```

```yaml
# Drops the item.
  objectRpc:
  - name: DropItem
```

```yaml
# Sets the visual item.
  objectRpc:
  - name: SetVisualItem
    target: all
    1: string, "name of the item"
    2: int, "variant number of the item"
    3: int, "level of the item"
```

```yaml
# Asks to become owner of the ZDO (not much use as the server).
  objectRpc:
  - name: RPC_RequestOwn
    source: <zdo>
```

### MapTable

```yaml
# Sets data.
  objectRpc:
  - name: MapData
    1: zpkg, "unusable"
```

### MineRock

```yaml
# Deals damage to a part of the rock.
  objectRpc:
  - name: Hit
    1: hit, "hit data"
    2: int, "part index"
```

```yaml
# Hides part of a rock as destroyed,
  objectRpc:
  - name: Hide
    target: all
    1: int, "part index"
```

### MineRock5

```yaml
# Deals damage to a part of the rock.
  objectRpc:
  - name: Hit
    1: hit, "hit data"
    2: int, "part index"
```

```yaml
# Sets health of a part.
  objectRpc:
  - name: SetAreaHealth
    target: all
    1: int, "part index"
    2: float, "health"
```

### MonsterAI

```yaml
# Awakens the creature.
  objectRpc:
  - name: RPC_Wakeup
```

### MusicLocation

```yaml
# Sets the music as played.
  objectRpc:
  - name: SetPlayed
```

### MusicVolume

```yaml
# Starts playing the music.
  objectRpc:
  - name: RPC_PlayMusic
    target: all
```

### OfferingBowl

```yaml
# Shows the boss spawn message.
  objectRpc:
  - name: RPC_BossSpawnInitiated
    target: <zdo>
```

```yaml
# Removes items from the player inventory.
# Requires actually interacting with the bowl.
  objectRpc:
  - name: RPC_RemoveBossSpawnInventoryItems
    target: <zdo>
```

```yaml
# Attempts to spawn the boss.
  objectRpc:
  - name: RPC_SpawnBoss
    source: <zdo>
    1: vec, "spawn position"
    2: bool, "remove spawn items?"
```

### Pickable

```yaml
# Spawns the picked item.
  objectRpc:
  - name: Pick
```

```yaml
# Sets the picked state.
  objectRpc:
  - name: SetPicked
    target: all
    1: bool, "is picked"
```

### PickableItem

```yaml
# Spawns the picked item.
  objectRpc:
  - name: Pick
```

### Player

```yaml
# Shows message.
  objectRpc:
  - name: Message
    1: enum_message, TopLeft/Center  # - int, 1/2
    2: string, "shown message"
    3: int, "shown amount"
```

```yaml
# Removes visuals.
  objectRpc:
  - name: OnDeath
    target: all
```

```yaml
# Triggers UI/music based on detection status.
  objectRpc:
  - name: OnTargeted
    1: bool, "is sensed"
    2: bool, "is targeted"
```

```yaml
# Uses stamina.
  objectRpc:
  - name: UseStamina
    1: float, "amount of stamina"
```

### PrivateArea

```yaml
# Toggles the ward on or off. 
# Only works if the player id matches the creator.
  objectRpc:
  - name: ToggleEnabled
    1: long, "player id"
```

```yaml
# Toggles a permitted player on or off.
# Only works if the ward is not enabled.
  objectRpc:
  - name: TogglePermitted
    1: long, "player id"
    2: string, "player name"
```

```yaml
# Flashes the ward.
  objectRpc:
  - name: FlashShield
    target: all
```

### Projectile

```yaml
# Attaches the projectile to an object.
  objectRpc:
  - name: RPC_Attach
    1: zdo, "attached zdo"
```

```yaml
# Sets the project as it has hit something.
  objectRpc:
  - name: RPC_OnHit
```

### ResourceRoot

```yaml
# Drains from the resource.
  objectRpc:
  - name: RPC_Drain
    1: float, "amount"
```

### Saddle

```yaml
# Controls the creature.
  objectRpc:
  - name: Controls
    1: vec, "direction"
    2: int, "speed"
    3: float, "ride skill"
```

```yaml
# Tries to get control of the creature.
  objectRpc:
  - name: RequestControl
    source: <zdo>
    1: long, "player id"
```

```yaml
# Releases control of the creature.
  objectRpc:
  - name: ReleaseControl
    1: long, "player id"
```

```yaml
# Response from the control request.
  objectRpc:
  - name: RequestRespons
    target: <zdo>
    1: bool, "was request ok?"
```

```yaml
# Drops the saddle at specific position.
  objectRpc:
  - name: RemoveSaddle
    1: vec, "position"
```

### SapCollector

```yaml
# Drops the sap.
  objectRpc:
  - name: RPC_Extract
    1: zdo, "attached zdo"
```

```yaml
# Updates the visual based on the status.
  objectRpc:
  - name: RPC_UpdateEffects
    target: all
```

### SEMan

```yaml
# Adds a status effect.
  objectRpc:
  - name: RPC_AddStatusEffect
    1: hash, "status effect"
    2: bool, "reset time?",
    3: int, "item level",
    4: float, "skill level"
```

### Ship

```yaml
# Stops the ship.
  objectRpc:
  - name: Stop
    source: <zdo>
```

```yaml
# Increases the speed.
  objectRpc:
  - name: Forward
    source: <zdo>
```

```yaml
# Decreases the speed.
  objectRpc:
  - name: Backward
    source: <zdo>
```

```yaml
# Sets the rudder angle.
  objectRpc:
  - name: Rudder
    source: <zdo>
    1: float, "rudder angle"
```

### ShipControl

```yaml
# Tries to get control of the ship.
  objectRpc:
  - name: RequestControl
    source: <zdo>
    1: long, "player id"
```

```yaml
# Releases control of the ship.
  objectRpc:
  - name: ReleaseControl
    1: long, "player id"
```

```yaml
# Response from the control request.
  objectRpc:
  - name: RequestRespons
    target: <zdo>
    1: bool, "was request ok?"
```

### Smelter

```yaml
# Adds a single fuel.
  objectRpc:
  - name: AddFuel
```

```yaml
# Adds a single item.
  objectRpc:
  - name: AddOre
    1: string, "name of the item"
```

```yaml
# Drops the smelted items
  objectRpc:
  - name: EmptyProcessed
```

### Talker

```yaml
# Unusable.
  objectRpc:
  - name: Say
    target: all
    1: int, 0/1/2 # Whisper, normal, shout
    2: userinfo, "unusable"
    3: string, "message"
    4: string, "sender network id"
```

### Tameable

```yaml
# Adds a saddle. Only works if the creature can be saddled.
  objectRpc:
  - name: AddSaddle
```

```yaml
# Toggles the command state.
  objectRpc:
  - name: Command
    1: zdo, "commanding player"
    2: bool, "whether to show the message"
```

```yaml
# Sets the name.
  objectRpc:
  - name: SetName
    1: string, "name of the creature"
    2: string, "name of the author, for console parental controls"
```

```yaml
# Destroyes the creature and shows the unsummoning effect.
  objectRpc:
  - name: RPC_UnSummon
    target: all
```

```yaml
# Sets the saddle visual.
  objectRpc:
  - name: SetSaddle
    target: all
    1: bool, "is saddled"
```

### TeleportWorld

```yaml
# Sets portal tag.
  objectRpc:
  - name: SetTag
    1: string, "tag"
    2: string, "name of the author for console parental controls"
```

### TerrainComp

```yaml
# Performs a terrain operation.
  objectRpc:
  - name: ApplyOperation
    1: zpkg, "unusabke"
```

### Trap

```yaml
# Sets trap state.
  objectRpc:
  - name: RPC_RequestStateChange
    1: enum_trap, Armed/Disarmed/Triggered  # - int, 0/1/2
```

```yaml
# Shows trap state.
  objectRpc:
  - name: RPC_OnStateChanged
    target: all
    1: enum_trap, Armed/Disarmed/Triggered  # - int, 0/1/2
```

### TreeBase

```yaml
# Deals damage to the tree.
  objectRpc:
  - name: Damage
    1: hit, "hit data"
```

```yaml
# Grows the tree showing the growth effect.
  objectRpc:
  - name: Grow
    target: all
```

```yaml
# Shows the shake effect.
  objectRpc:
  - name: Shake
    target: all
```

### TreeLog

```yaml
# Deals damage to the tree.
  objectRpc:
  - name: Damage
    1: hit, "hit data"
```

### TriggerSpawner

```yaml
# Triggers the spawner.
  objectRpc:
  - name: Trigger
```

### Turret

```yaml
# Adds a single ammo.
  objectRpc:
  - name: RPC_AddAmmo
    1: name, "name of the ammo"
```

```yaml
# Sets the target.
  objectRpc:
  - name: RPC_SetTarget
    target: all
    1: zdo, "target zdo"
```

### Vagon

```yaml
# Shows denied message.
  objectRpc:
  - name: RequestDenied
    target: <zdo>
```

```yaml
# Asks to become owner of the ZDO (not much use as the server).
  objectRpc:
  - name: RequestOwn
    source: <zdo>
```

### WearNTear

```yaml
# Deals damage to the object.
  objectRpc:
  - name: WNTDamage
    1: hit, "hit data"
```

```yaml
# Sets health for visual style.
  objectRpc:
  - name: WNTHealthChanged
    target: all
    1: float, "amount of health"
```

```yaml
# Removes the object.
  objectRpc:
  - name: WNTRemove
```

```yaml
# Repairs the object.
  objectRpc:
  - name: WNTRepair
```

```yaml
# Shows visual fragments (if m_autoCreateFragments is true).
  objectRpc:
  - name: WNTCreateFragments
    target: all
```

### ZSyncAnimation

```yaml
# Triggers animation state.
  objectRpc:
  - name: SetTrigger
    target: all
    1: name, "name of the trigger"
```

## Client rpcs

This list all RPC calls that are not related to any object.

```yaml
# Teleports the client.
  clientRpc:
  - name: ChatMessage
    1: vec, "text position"
    2: int, type
    3: userinfo, "unusable"
    4: string, "message"
    5: string, "sender network id"
```

```yaml
# Shows a damage text.
  clientRpc:
  - name: DamageText
    1: zpkg, "unusable"
```

```yaml
# Destroys an object.
  clientRpc:
  - name: DestroyZDO
    1: zpkg, "unusable"
```

```yaml
# Sends global keys. Only implemented for the client.
  clientRpc:
  - name: GlobalKeys
    1: string list, "unusable"
```

```yaml
# Sends location icons. Only implemented for the client.
  clientRpc:
  - name: LocationIcons
    1: zpkg, "unusable"
```

```yaml
# Calls pong RPC on the sender.
  clientRpc:
  - name: Ping
```

```yaml
# Prints network delay.
  clientRpc:
  - name: Pong
```

```yaml
# Requests ZDO from the server.
  clientRpc:
  - name: RequestZDO
    1: zdo, "requested zdo"
```

```yaml
# Removes a global key. Only implemented for the server.
  clientRpc:
  - name: RemoveGlobalKey
    1: string, "key"
```

```yaml
# Shows a message.
  clientRpc:
  - name: ShowMessage
    1: enum_message, TopLeft/Center  # - int, 1/2
    2: string, "message"
```

```yaml
# Sets the client event.
  clientRpc:
  - name: SetEvent
    1: string, "name of event"
    2: float, "event timer"
    3: vec, "event position"
```

```yaml
# Adds a global key. Only implemented for the server.
  clientRpc:
  - name: SetGlobalKey
    1: string, "key"
```

```yaml
# Starts sleeping.
  clientRpc:
  - name: SleepStart
```

```yaml
# Stops sleeping.
  clientRpc:
  - name: SleepStop
```

```yaml
# Sets the client event.
  clientRpc:
  - name: SpawnObject
    1: vec, "spawn position"
    2: quat, "spawn rotation"
    3: hash, "prefab id"
```

```yaml
# Adds a location pin to the map.
  clientRpc:
  - name: RPC_DiscoverLocationResponse
    target: <zdo>
    1: string, "name of pin"
    2: int, "pin type"
    3: vec, "position"
    4: bool, "open map"
```

```yaml
# Requests location discovery from the server.
  clientRpc:
  - name: RPC_DiscoverClosestLocation
    source: <zdo>
    1: string, "location name"
    2: vec, "position"
    3: string, "name of pin"
    4: int, "pin type"
    5: bool, "open map"
    6: bool "discover all"
```

```yaml
# Teleports the client.
  clientRpc:
  - name: RPC_TeleportPlayer
    1: vec, "location to teleport to"
    2: quat, "rotation to teleport to"
    3: bool, "is distant teleport"
```
