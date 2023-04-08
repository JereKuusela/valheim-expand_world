- v1.28
  - Adds a new file `expand_rooms.yaml`.
  - Changes the blueprint search path also to include the mod profile folder.
  - Merges the PlanBuild and BuildShare search folders to the same folder.

- v1.27
  - Adds a new field `centerPiece` to the `expand_locations.yaml`.
  - Changes the field `offset` of the `expand_locations.yaml` to x,z,y.
  - Removes the field `center` from the `expand_locations.yaml` as obsolete.
  - Adds automatic reload for changed blueprints.

- v1.26
  - Adds blueprint support for vegetation.
  - Adds `expand_dungeons.yaml`.
  - Adds a new field `dungeon` to the `expand_locations.yaml`.

- v1.25
  - Adds a new setting Zone Spawners to the config file.
  - Changes the data files to be loaded in reverse order (so thaat the original files are loaded last).
  - Fixes map pins moving when panning the map with higher map sizes.

- v1.24
  - Adds scale, chance and zdo data to the field `objects` in the `expand_locations.yaml`.
  - Adds a new field `noBuildDungeon` to the `expand_locations.yaml` (default is false).
  - Adds new fields `paint`, `paintRadius` and `paintBorder` to the `expand_locations.yaml`.
  - Adds new fields `levelRadius` and `levelBorder` to the `expand_locations.yaml`.
  - Adds a new field `objects` to the `expand_spawns.yaml`.
  - Adds blueprint support to the field `objects` in the `expand_locations.yaml`.
  - Adds blueprint support to the blueprint files.
  - Adds a new command `ew_icons` to print available location icons.
  - Changes the format of field `paint` in the `expand_biomes.yaml` to match `expand_locations.yaml`.
  - Changes the field `noBuild` in the `expand_locations.yaml` to also accept numbers.
  - Changes the field `levelArea` in the `expand_locations.yaml` from number to true/false.
  - Changes the field `levelArea` in the `expand_locations.yaml` to also work for non-blueprint locations.
  - Changes the field `iconAlways` in the `expand_locations.yaml` to also accept text (to set the icon).
  - Changes the field `iconPlaced` in the `expand_locations.yaml` to also accept text (to set the icon).
  - Changes the default no build to not affect dungeons.
  - Removes the Custom Raids biome compatibility (Custom Raids added proper support).
  - Fixes error with the new update.

- v1.23
  - Adds a new field `noBuild` to the `expand_biomes.yaml` (default is false).
  - Fixes object swap not working for dungeon doors.
  - Fixes warning message from vanilla location data.
  - Fixes `expand_spawns.yaml` being affected by "Biome data" setting.
  - Fixes biome names being synced for every server synced mod.
  - Fixes no build not working for blueprints.
