# Location examples

Open `expand_locations.yaml`.

Locations are pregenerated during the world creation so changes won't update automatically. 

Useful commands:
- `find [id] 200` pins locations to the map.
- `locations_add [id] start force` adds new locations (Upgrade World mod).
- `hammer_location [id]` manually places locations (Infinity Hammer mod), however blueprints or some modifications won't show up properly until placed.

## Location: Enable a new location

1. All available locations are generated so search `enabled: false` for disabled ones.
2. Remove `enabled: false` to enable the location and change other fields as needed.

## Location: Create a variant

1. Copy paste an existing location and add `:[name[` to the `prefab`.
2. These locations can be spawned by using the new name.
```
- prefab: Dolmen01:Ghost
# Replaces the skeleton with a ghost.
  objectSwap:
    - Spawner_Skeleton_night_noarcher, Spawner_Ghost
```

This also works for dungeons.
```
- prefab: Crypt2:Greydwarf
  objectSwap:
    - Spawner_Skeleton, Spawner_Greydwarf
    - Spawner_Ghost, Spawner_Greydwarf_Shaman
    - Spawner_Skeleton_poison, Spawner_Greydwarf_Elite
    - BonePileSpawner, Spawner_Greydwarf_Elite
```

## Location: Custom data with Spawner Tweaks mod

1. Create a new variant if needed. For example `Eikthyrnir:Wolf`.
2. Use `tweak_altar amount=0 spawn=Wolf` on any existing altar.
3. The command can be used on any object but this will cause more data.
4. Use `object copy` and then paste (CTRL+V) to the `data` field.
```
- prefab: Eikthyrnir:Wolf
# Causes the altar to spawn a wolf instead and require no item.
  data: CAAAAAJrzPp8AAAAAHZc4rL6DUI8
```

## Location: Adding new objects

1. Create a new variant if needed.
2. It can be difficult to figure out the correct position for new objects.
3. So add a GlowingMushroom to the center and then use `hammer_location` command to place the location manually. 
```
- prefab: StoneTowerRuins05:Draugr
  objects:
    - GlowingMushroom
```
4. Build new objects to the location. Then use `hammer_save` to create a blueprint.
5. Open the blueprint and calculate offsets for the new objects.
```
GlowingMushroom:-9.5,0,-1.5,...
Spawner_DraugrPile:-5,0,3... -> 4.5,0,4.5
Spawner_DraugrPile:-5,0,-6,... -> 4.5,0,-4.5
```
6. Add new objects to the location entry (and remove the mushroom).
```
- prefab: StoneTowerRuins05:Draugr
# Adds four new spawners.
  objects:
    - Spawner_DraugrPile, 4.5, -4.5
    - Spawner_DraugrPile, -4.5, -4.5
    - Spawner_DraugrPile, 4.5, 4.5
    - Spawner_DraugrPile, -4.5, 4.5
  objectSwap:
# Replaces skeletons with draugr (2:1 ratio for normal and elites).
# 2:1 ratio = 66% chance for normal and 33% chance for elite.
    - Spawner_Skeleton, Spawner_Draugr:2, Spawner_Draugr_Elite
# Replaces the default spawner.
    - BonePileSpawner, Spawner_DraugrPile
```

## Location: Custom data for objects with Spawner Tweaks mod

1. Spawner Tweaks mod is required for all clients.
2. Use the example above (`StoneTowerRuins05:Draugr`).
3. Use `tweak_spawner maxnear=20` on one of the spawners.
4. Copy the data with `object copy`.
5. Add to the `objectData` field.
```
# Replaces the spawn limit with 20.
  objectData:
    - Spawner_DraugrPile, CAAAAAECXKVYFAAAAA==
```

## Location: Blueprint

1. Use PlanBuild mod or `hammer_save` to create a blueprint.
2. Add a new entry and use the blueprint filename as the `prefab`.
3. Use `hammer_location` to test the location. Note: The blueprint only shows up after being placed. It's normal to see nothing while placing it.
4. By default, the blueprint is automatically centered. Use the `centerPiece` field.
5. By default, the terrain is smoothly leveled. Adjust the `levelArea`, `levelRadius` and `levelBorder` fields if needed.

Blueprint without leveling:
```
- prefab: Blueprint
  levelArea: false
```

Blueprint with 20 meters flat area:
```
- prefab: Blueprint
  levelRadius: 20
  levelBorder: 0
```

Blueprint with custom centering (add GlowingMushroom to your blueprint as the center marker):
```
- prefab: Blueprint
  centerPiece: GlowingMushroom
```

Blueprint with terrain painting:
```
- prefab: Blueprint
  paint: paved
```

Blueprint with 20 meters of paint without border:
```
- prefab: Blueprint
  # dirt,cultivated,paved,vegetation -> mix of cultivated and paved.
  paint: 0,0.5,0.5,0
  paintRadius: 20
  paintBorder: 0
```