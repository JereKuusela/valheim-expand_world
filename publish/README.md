# Expand world

This mod allows adding new biomes and changing most of the world generation.

Always back up your world before making any changes!

Install on all clients and on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

# Usage

See [documentation](https://github.com/JereKuusela/valheim-expand_world/blob/main/README.md).

## Migration from version 1.5

- Verify that `expand_vegetation.yaml` uses `.` as the separate character for `scaleMin` and `scaleMax` instead of `,`. Fix manually or remove the file to regerenate it.

# Credits

Thanks for Azumatt for creating the mod icon!

Thanks for blaxxun for creating the server sync!

Thanks for redseiko for the asynchronous minimap generating!

Sources: [GitHub](https://github.com/JereKuusela/valheim-infinity_hammer)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)

# Changelog

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

- v1.9
  - Fixes the altitude delta having the same effect as the altiude multiplier.

- v1.8
  - Adds a new field `data` to the `expand_locations.yaml`.
  - Adds a new field `data` to the `expand_vegetation.yaml`.
  - Fixes compatibility issue with Spawn That mod.

- v1.7
  - Adds a new field `waterDepthMultiplier` to the `expand_biomes.yaml`.
  - Adds a new field `requiredEnvironments` to the `expand_events.yaml`.
  - Adds new fields `maximumAltitude`, `minimumAltitude` and `excessFactor` to the `expand_biomes.yaml`.
  - Adds a failsafe for missing locations or vegetations (so that the world loads at least).
  - Changes the default minimum altitude of `expand_spawns.yaml` to depend on maximum altitude (-1000 or 0).
  - Improves compatibility with Spawn That mod.
  - Fixes possible error when flying.
  - Fixes incompatibility with Custom Raids mod (+ event data is disabled).

- v1.6
  - Fixes default vegetation scale being wrong in `expand_vegetation.yaml` (for some users).
