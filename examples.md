# Examples

## Locations

Open `expand_locations.yaml`.

### Testing

Locations are pregenerated during the world creation so changes won't update automatically.

Useful commands:
- `find [id] 200` pins locations to the map.
- `locations_add [id] start force` adds new locations (Upgrade World mod).
- `hammer_location [id]` manually places locations (Infinity Hammer mod), however blueprints or some modifications won't show up properly until placed.

### Location: Enable a new location

1. All available locations are generated so search `enabled: false` for disabled ones.
2. Remove `enabled: false` to enable the location and change other fields as needed.

### Location: Create a variant

1. Copy paste an existing location and add `:[name[` to the `prefab`.
2. These locations can be spawned by using the new name.

```
- prefab: Dolmen01:Ghost
# Replaces the skeleton with a ghost.
  objectSwap:
    - Spawner_Skeleton_night_noarcher, Spawner_Ghost
```

### Location: Custom data with Spawner Tweaks mod

1. Create a new variant if needed. For example `Eikthyrnir:Wolf`.
2. Use `tweak_altar amount=0 spawn=Wolf` on any existing altar.
3. The command can be used on any object but this will cause more data.
4. Use `data copy` and then paste (CTRL+V) to the `data` field.

```
- prefab: Eikthyrnir:Wolf
# Causes the altar to spawn a wolf instead and require no item.
  data: CAAAAAJrzPp8AAAAAHZc4rL6DUI8
```

### Location: Adding new objects

1. Create a new variant if needed.
2. It can be difficult to figure out the correct position for new objects.
3. So add a GlowingMushroom to the center and then use `hammer_location` command to place the location manually. 

```
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
    - Spawner_Skeleton, Spawner_Draugr:2, Spawner_Draugr_Elite
# Replaces the default spawner.
    - BonePileSpawner, Spawner_DraugrPile
```

### Location: Custom data for objects with Spawner Tweaks mod

1. Use the example above (`StoneTowerRuins05:Draugr`).
2. Use `tweak_spawner maxnear=20` on one of the spawners.
3. Add to the `objectData` field.
```
# Replaces the spawn limit with 20.
  objectData:
    - Spawner_DraugrPile, CAAAAAECXKVYFAAAAA==
```

### Location: Blueprint¨

1. Use PlanBuild mod or `hammer_save` to create a blueprint.
2. Add a new entry and use the blueprint filename as the `prefab`.


## Adding a vegetation variant with Spawner Tweaks mod

1. Install Server Devcommands, World Edit Commands and Spawner Tweaks mods.
2. Open `expand_vegetation.yaml`.
3. Copy paste `stubbe` entry.
4. Add `data:` to the entry.
5. Use command `tweak_chest force maxamount=2 item=Torch item=Coins,1,10,20` on any object.
6. Use command `data copy` and then paste (CTRL+V) to the `data` field.
7. Create a new world or use `zones_reset start` from Upgrade World mod.

## Bosses spawning enemies

1. Open `expand_events.yaml`.
2. Find boss_eikthyr or other boss event.
3. Copy `spawns` field from another entry like army_eikthyr.

## Adding a new biome

1. Open `expand_biomes.yaml`.
2. Copy-paste existing entry.
3. Change `biome`, add `name` and add `terrain`.
4. Add/modify other fields.
5. Open `expand_world.yaml`.
6. Look at it until the rules start to make sense.
7. Determine how you want the new biome to appear on the world.
8. Make changes until you succeed.
9. Edit other files to make spawns, locations and vegetation to work.

Example:

Copy-paste ashlands entry and change:

    - biome: desert
      name: Desert
      terrain: ashlands
      environments:
      - environment: Clear

Copy-paste plains entry and change the top one:

    - biome: desert
      amount: 0.5
