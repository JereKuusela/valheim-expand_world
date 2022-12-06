# Expand world

This mod allows adding new biomes and changing most of the world generation.

Always back up your world before making any changes!

Install on all clients and on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

# Usage

See [documentation](https://github.com/JereKuusela/valheim-expand_world/blob/main/README.md).

## Migration from version 1.15

- Regenerate `expand_clutter.yaml` by removing it to get values for the new fields `minVegetation`, `maxVegetation` and `minTilt`.
- Regenerate `expand_spawns.yaml` by removing it to get values for the new field `overrideLevelupChance`.
- Regenerate `expand_vegetation.yaml` by removing it to get values for the new fields `minVegetation`, `maxVegetation` and `snapToStaticSolid`.


## Migration from version 1.13

- Regenerate `expand_locations.yaml` by removing it to get values for the new fields `clearArea`, `exteriorRadius`, `noBuild` and `randomDamage`.

# Credits

Thanks for Azumatt for creating the mod icon!

Thanks for blaxxun for creating the server sync!

Thanks for redseiko for the asynchronous minimap generating!

Sources: [GitHub](https://github.com/JereKuusela/valheim-infinity_hammer)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)

# Changelog

- v1.16
  - Warning: Breaking changes!
  - Adds new fields `minVegetation`. `maxVegetation` and `minTilt` to the `expand_clutter.yaml`.
  - Adds a new field `overrideLevelupChance` to the `expand_spawns.yaml`.
  - Adds new fields `minVegetation`. `maxVegetation` and `snapToStaticSolid` to the `expand_vegetation.yaml`.

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
