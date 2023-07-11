- v1.52
  - Adds a new setting to disable automatic data migration.
  - Adds a new setting to disable automatic config reload (requires restart to take effect).
  - Fixes the default biome configuration being slightly off (most notably for Meadows).

- v1.51
  - Adds terrain snap support to the `objects` fields.
  - Adds a new field `clearRadius` to the `expand_vegetation.yaml` file.
  - Adds new fields `statusEffects`, `dayStatusEffects` and `nightStatusEffects` to the `expand_biomes.yaml` and `expand_environments.yaml` file.
  - Changes the fields `objectSwap` and `objectData` in the `expand_locations.yaml` to also affect dungeons (reverting previous change).
  - Changes the automatic data migration to keep comments and other changes.
  - Disables minimap generation if Better Continents is enabled (performance + compatiblity).
  - Fixes the scale not working on extra objects.
  - Fixes dungeon being blocked by CLLC dungeon reset.

- v1.50
  - Fixes `expand_biomes.yaml` file not being created.
  - Fixes custom biomes not loading correctly.

- v1.49
  - Fixes dungeons not generating correctly if the `bounds` field was missing.
  - Fixes wrong biomes near some biome borders.

- v1.48
  - Fixes another data loading issue.

- v1.47
  - Fixes crash.

- v1.46
  - Fixes custom data.

- v1.45
  - Adds new fields `requiredGlobalKey` and `forbiddenGlobalKey` to the `expand_vegetation.yaml` file.
  - Updated for the new game version.

- v1.44
  - Fixes dungeon environment boxes being scaled way too big (for real this time).

- v1.43
  - Fixes dungeon environment boxes being scaled way too big.

- v1.42
  - Fixes custom dungeons not working (for real this time).
  - Fixes custom environments not working.

- v1.41
  - Adds new fields `bounds`, `objectData` and `objectSwap` to the `expand_dungeons.yaml` file.
  - Adds a new field`objectSwap` to the `expand_rooms.yaml` file.
  - Adds support for custom room themes.
  - Changes the fields `objectSwap` and `objectData` in the `expand_locations.yaml` to not affect dungeons.
  - Changes the field `noBuildDungeon` in the `expand_locations.yaml` to affect the whole zone when set to true.
  - Fixes custom dungeons not working.
  - Fixes the default door spawn change being 1 instead of 0 (too many doors appeared for some dungeons).
  - Fixes the field `paint` in the `expand_biomes.yaml` file not working when set to `0,0,0,0`.
  - Fixes the health data not being ignored for locations with random damage.
  - Fixes blueprint object scale values not being read correctly.
  - Removes creator information automatically from blueprints and data values (so that things don't appear player built).
  - Improves the performance of the field `paint` in the `expand_biomes.yaml` file.
  - Improves the error message for failing blueprints.
  
- v1.40
  - Adds blueprint dungeons rooms.
  - Fixes dungeons not working when first starting a single player world and then joining a multiplayer world.
  - Fixes the blueprint live reload system not working.
  - Fixes dungeons being blocked sometimes.

- v1.39
  - Fixes corrupted or missing locations possibly causing an error during the gameplay.
  - Fixes dungeon room variants not working.
  - Fixes vegetation loading not working if mods overrode the vegetation prefab (now works with Krumpac Reforged mod).

- v1.38
  - Fixes invalid vegetation not being removed from the game system.
  - Fixes blueprint vegetation not working.

- v1.37
  - Further improves the location and vegetation loading logic to deal with edge cases and bad data.
  - Fixes blueprint terrain leveling creating holes at zone edges.

- v1.36
  - Adds missing vegetation entries automatically to the `expand_vegetation.yaml` file.
  - Changes the location distance meter threshold from 1.0 to 2.0 meters (now only values higher than 2.0 are considered as meters instead of relative to the world size).
  - Improves the location loading logic.
  - Fixes conflict with mods that cause client side dungeon regeneration.
  - Fixes the chance field not working on blueprint files.
  - Removes disabled entries from the default `expand_vegetation.yaml` file.

- v1.35
  - Improves suppport for massive locations (radius larger than 32 meters).
  - Fixes the water not rendering correctly past the normal world limits.

- v1.34
  - Adds a new field `groundOffset` to the `expand_locations.yaml`.

- v1.33
  - Adds a new command `ew_dungeons` to print their available rooms.
  - Adds a blueprint support to the field `objectSwap` in the `expand_locations.yaml`.
  - Adds a new field `centerPiece` to the `expand_vegetation.yaml`.
  - Adds a new field `objects` to the `expand_rooms.yaml`.
  - Changes dungeon room names to case insensitive in the `expand_dungeons.yaml` file.
  - Fixes the field `excludeRooms` in the `expand_dungeons.yaml` not working.
  - Fixes Jewelcrafting boss icons not working.
  - Removes the field `offset` from the `expand_locations.yaml`.
  - Removes network traffic when in "Server side only" mode.
