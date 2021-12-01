# Expand world

This mod allows playing with the world generator settings to create bigger or radically different worlds.

Always back up your world before making any changes!

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim).
2. Download the latest zip.
3. Extract it in the \<GameDirectory\>\BepInEx\plugins\ folder.
4. Optionally also install the [Configuration manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/tag/v16.4).

# Features

If devcommands are enabled, coordiantes are shown on the minimap and ctrl + click will teleport the player there. This can be useful for exploring.

Configuration changes are automatically applied to the minimap when it's opened. This usually takes a while so be patient.

Biome changes are also automatically applied to the world (but only partially for loaded zones). This can lead to unexpected behavior.

Note: This mod will change the area outside the world edge to be Ocean instead of Ashlands.

# Configuration

The config can be found in the \<GameDirectory\>\BepInEx\config\ folder after the first start up.

## General:
- Active area: Determines how many zones are loaded around the player. This has major impact on performance and will affect the gameplay by expanding the active area (creatures move, etc.).
- Minimap pixel size multiplier: Increases the size of the minimap by reducing how detailed the map is. This doesn't affect performance. Recommended to use value 2 or 4 for bigger worlds (after that the map gets very chunky).
- Minimap size multiplier: Increases the size of the minimap without reducing the detail level. This has a major impact on the world load time. Recommended to use 2 or 4 for bigger worlds.
- World edge size: How big the empty buffer zone between the world and the edge is. Recommended to keep it as it is.
- World radius: Size of the world. If you increase this then you should also increase the map size. For example with 40000 radius, you should either increase pixel size multiplier to 4, size multiplier 4 or both to 2 (2 \* 2 = 4).

## Biomes

Determines how far each biome can appear in the world. The percentage values multiply the world radius parameter.

Ashlands and Deep North max values multiply the total size (world radius + world edge). This ensures they can extend up to the world edge.

If no valid biome is available, Black Forest is used (like in the base game).

## Seed

These settings allow locking world seed parameters.

- Biome seeds determine where each biome appears on the map. Base game randomly picks value from -10000 to 10000.
- Offset X and Y: Islands and mountains are determined by the base height map. These coordinates set where the world center is located on that height map. Base game randomly picks value from -10000 to 10000.
- Height variation: Determines the minor height differences applied over the base height map. Base game randomly picks value from -10000 to 10000.
- River and stream seeds: Determines where rivers and streams are located. Base game randomly picks any value.

# Changelog

- v1.0.0:
	- Initial release