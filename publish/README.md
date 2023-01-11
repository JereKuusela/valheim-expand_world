# Expand world

This mod allows adding new biomes and changing most of the world generation.

Always back up your world before making any changes!

Install on all clients and on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

# Usage

See [documentation](https://github.com/JereKuusela/valheim-expand_world/blob/main/README.md).

See [location examples](https://github.com/JereKuusela/valheim-expand_world/blob/main/examples_locations.md).

See [other examples](https://github.com/JereKuusela/valheim-expand_world/blob/main/examples.md).

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

- v1.16
  - Warning: Breaking changes!
  - Adds new fields `minVegetation`, `maxVegetation` and `minTilt` to the `expand_clutter.yaml`.
  - Adds a new field `overrideLevelupChance` to the `expand_spawns.yaml`.
  - Adds new fields `minVegetation`, `maxVegetation` and `snapToStaticSolid` to the `expand_vegetation.yaml`.

- v1.15
  - Fixes error at start up if configs were missing.

- v1.14
  - Warning: Breaking changes!
  - Adds support for location blueprints (experimental / work in progress, use at your own risk).
  - Adds support for adding new objects to locations.
  - Changes `objectSwap` and `objectData` to also affect dungeon rooms.
  - Config files are now deleted automatically on startup if `expand_world.cfg` doesn't exist.
  - Possible fixes config loading conflict with CLLC.
  - Fixes config files not working on sub-directories.

- v1.13
  - Adds a new field `data` to the `expand_spawns.yaml`.
  - Adds new fields `objectSwap` and `objectData` to the `expand_locations.yaml`.
  - Fixes spawns not working.

- v1.12
  - Adds a new command `print_musics` to print available musics to the console.
  - Changes the format of `biome`, `biomeArea`, `requiredGlobalKeys`, `notRequiredGlobalKeys` and `requiredEnvironments` (old configs automatically converted).
  - Reworks the location system so that all available locations are loaded.
  - Removes location data syncing as it is no longer needed.
  - Fixes Jotunn locations not working.

- v1.11
  - Fixes the server only mode not working.

- v1.10
  - Fixes the black screen.
