# Examples

TODO

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
