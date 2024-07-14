# Examples for farming

This mod can be used to enhance farming by increasing yield or adding new rare drops.

## Increase carrot yield on Plains

`expand_prefabs.yaml`: 100% chance to inject plains_carrot data to carrots in Plains.

```yaml
- prefab: Pickable_Carrot
  type: create
  data: plains_carrot
  weight: 1
  biomes: Plains
```

`expand_data.yaml`: Changes display name and the doubles the drops.

```yaml
- name: plains_carrot
  ints:
  - Pickable.m_amount, 2
  strings:
  - Pickable.m_overrideName, Big Carrot
```

## Random chance for different drop

`expand_prefabs.yaml`: 1% chance to inject rotten data to carrots.

```yaml
- prefab: Pickable_Carrot
  type: create
  data: rotten
  weight: 0.01
```

`expand_data.yaml`: Changes display name and the dropped item.

```yaml
- name: rotten_carrot
  strings:
  - Pickable.m_overrideName, Rotten Carrot
  - Pickable.m_itemPrefab, Guck
```

## Better yield near windmills

`expand_prefabs.yaml`: 50% chance to inject windmill_crops data to all crops when within 50 meters of a windmill.

```yaml
- prefab: Pickable_Barley, Pickable_Carrot, Pickable_Flax, Pickable_Turnip
  type: create
  weight: 0.5
  data: windmill_crops
  objects: windmill
  objectDistance: 50
```

`expand_data.yaml`: Doubles the drops without changing the display name.

```yaml
- name: windmill_crops
  ints:
  - Pickable.m_amount, 2
```
