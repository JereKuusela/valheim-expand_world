# Expand world

This mod allows adding new biomes and changing most of the world generation.

Always back up your world before making any changes!

Install on all clients and on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

# Usage

See [documentation](https://github.com/JereKuusela/valheim-expand_world/blob/main/README.md).

See [location examples](https://github.com/JereKuusela/valheim-expand_world/blob/main/examples_locations.md).

See [other examples](https://github.com/JereKuusela/valheim-expand_world/blob/main/examples.md).

# Tutorials

- How to make custom biomes: https://youtu.be/TgFhW0MtYyw (33 minutes, created by StonedProphet)

## Migration from version 1.19

- If you have modded creatures spawning in wrong places, regenerate `expand_spawns.yaml` by removing it or manually set `biome: None` when missing.

## Migration for Mistlands update

- Regenerate all yaml files by removing them.
- Regenerate at least environments to remove constant raining.

## Migration from version 1.15

- Regenerate `expand_clutter.yaml` by removing it to get values for the new fields `minVegetation`, `maxVegetation` and `minTilt`.
- Regenerate `expand_spawns.yaml` by removing it to get values for the new field `overrideLevelupChance`.
- Regenerate `expand_vegetation.yaml` by removing it to get values for the new fields `minVegetation`, `maxVegetation` and `snapToStaticSolid`.

# Credits

Thanks for Azumatt for creating the mod icon!

Thanks for blaxxun for creating the server sync!

Thanks for redseiko for the asynchronous minimap generating!

Sources: [GitHub](https://github.com/JereKuusela/valheim-infinity_hammer)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)

# Changelog

- v1.24
  - Adds scale, chance and zdo data to the field `objects` in the `expand_locations.yaml`.
  - Adds a new field `noBuildDungeon` to the `expand_locations.yaml` (default is false).
  - Adds a new field `paint` to the `expand_locations.yaml`.
  - Adds a new field `objects` to the `expand_spawns.yaml`.
  - Adds blueprint support to the field `objects` in the `expand_locations.yaml`.
  - Adds blueprint support to the blueprint files.
  - Adds a new command `ew_icons` to print available location icons.
  - Changes the format of field `paint` in the `expand_biomes.yaml` to match `expand_locations.yaml`.
  - Changes the field `noBuild` in the `expand_locations.yaml` to also accept numbers.
  - Changes the field `iconAlways` in the `expand_locations.yaml` to also accept text (to set the icon).
  - Changes the field `iconPlaced` in the `expand_locations.yaml` to also accept text (to set the icon).
  - Changes the default no build to not affect dungeons.
  - Removes the Custom Raids biome compatibility (no longer works with the latest version of Custom Raids).

- v1.23
  - Adds a new field `noBuild` to the `expand_biomes.yaml` (default is false).
  - Fixes object swap not working for dungeon doors.
  - Fixes warning message from vanilla location data.
  - Fixes `expand_spawns.yaml` being affected by "Biome data" setting.
  - Fixes biome names being synced for every server synced mod.
  - Fixes no build not working for blueprints.

- v1.22
  - Fixes error message caused by removed locations.
  - Fixes location blueprints causing terrain issues when overflowing to adjacent zones.

- v1.21
  - Adds a new field `scaleUniform` to the `expand_vegetations.yaml` (default is true).
  - Improves mod compatibility.
  - Fixes the vegetation scaling being separate for each axis.

- v1.20
  - Fixes None biome not being saved to the `expand_spawns.yaml` which caused spawning in every biome (some mods have the default biome as None).

- v1.19
  - Fixes default config generation sometimes failing.

- v1.18
  - Adds data sync for location `prefab`, `exteriorRadius` and `noBuild` fields.
  - Fixes automatic migration for missing `exteriorRadius` overwriting blueprint and location variant names.
  - Fixes minimap loading error with Marketplace territories.
  - Improves compatiblity with mods adding new vegetation.

- v1.17
  - Adds a new command `ew_seeds` to output seed information.
  - Adds a new setting `seed` to set the world seed.
  - Adds preloading for biome names (improves mod compatibility).
  - Adds automatic migration for missing `exteriorRadius` in `expand_locations.yaml`.	
  - Adds new fields `offset` and `center` (for blueprints) to the `expand_locations.yaml`.
  - Changes console commands to start with `ew_`.
  - Changes the format of `objects`, `objectData` and `objectSwap` in `expand_locations_yaml` (automatically migrated).
  - Changes the `levelArea` field in `expand_locations_yaml` to be a numeric (for smoothness).
  - Changes the location blueprints to snap to the ground.
  - Increases terrain leveling performance (for blueprints).
  - Increases the default altitude cap from 1000 meters to 10000 meters.
  - Increases the network timeout duration to 10x of the normal.
  - Fixes objects sometimes being duplicated.
  - Fixes purple thunder sky if a custom environment is not set properly.
