# Expand world

This mod allows adding new biomes and changing most of the world generation.

Always back up your world before making any changes!

Install on all clients and on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Some features are available also as server only mode.

# Usage

See [documentation](https://github.com/JereKuusela/valheim-expand_world/blob/main/README.md).

See [location examples](https://github.com/JereKuusela/valheim-expand_world/blob/main/examples_locations.md).

See [other examples](https://github.com/JereKuusela/valheim-expand_world/blob/main/examples.md).

# Tutorials

- How to make custom biomes: https://youtu.be/TgFhW0MtYyw (33 minutes, created by StonedProphet)
- How to use blueprints as locations with custom spawners: https://youtu.be/DXtm-WLF6KE (30 minutes, created by StonedProphet)

## Migration from version 1.23

- If you have used paint in expand_biomes.yaml, then update the values to match the new format.

## Migration from version 1.19

- If you have modded creatures spawning in wrong places, regenerate `expand_spawns.yaml` by removing it or manually set `biome: None` when missing.

# Credits

Thanks for Azumatt for creating the mod icon!

Thanks for blaxxun for creating the server sync!

Thanks for redseiko for the asynchronous minimap generating!

Sources: [GitHub](https://github.com/JereKuusela/valheim-infinity_hammer)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)

# Changelog

- v1.26
  - Adds blueprint support for vegetation.

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

- v1.22
  - Fixes error message caused by removed locations.
  - Fixes location blueprints causing terrain issues when overflowing to adjacent zones.

- v1.21
  - Adds a new field `scaleUniform` to the `expand_vegetations.yaml` (default is true).
  - Improves mod compatibility.
  - Fixes the vegetation scaling being separate for each axis.
