# Expand world

This mod allows creating bigger or radically different worlds.

Always back up your world before making any changes!

Must be installed on all clients and on the server.

Check any modding [guide](https://youtu.be/WfvA5a5tNHo) if you don't know how.

# Features

- Make the world bigger (or smaller).
- Change world height, globally or per biome.
- Change biome distribution.
- Customize the world seed.
- The minimap is generated in the background, lowering loading times.
- Config sync to ensure all clients use the same settings (when `Locked` setting is enabled).

For example you can create entirely flat worlds with only Meadows for building. Or group up colder biomes up north while more warmer biomes end up in the other side. Or just have a world with terrain shapes no one has ever seen before.

# Configuration

The mod supports live reloading when changing the configuration (either with [Configuration manager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/) or by saving the config file). This can lead to weird behavior so after playing with the settings it's recommended to make a fresh world.

Note: Pay extra attention when loading old worlds. Certain configurations can cause alter the terrain significantly and destroy your buildings.

# World size

The size can be increased by changing the `World radius` and `World edge size` settings. The total size is sum of these (default is 10000 + 500 = 10500 meters). Usually there is no need to change the edge size.

The world can be stretched with `Stretch world` setting. This can be used to keep the same world but islands and oceans are just bigger. This will also make biomes bigger which can be further tweaked with `Stretch biomes` setting (for example using 0.5 biome stretch with 2 world stretch).

The amount of locations (like boss altars) can be changed with `Locations` setting. This can significantly increase the initial world generation time (especially when the game fails to place most locations). If changing this on existing worlds, use `genloc` command to distribute unplaced locations.

Note: 2x world radius means 4x world area. So for 20000 radius you would need 4x locations and for 40000 radius you would need 16x locations.

Note: The location minimum and maximum distances are automatically scaled with world radius.

Note: If the game fails to place the spawn altar (for example if no Meadows), then it is forcefully placed at the middle of the map. With bad luck, this can be underwater.

## Minimap

The minimap size must be manually changed because there are two different settings. Both of the settings increase the minimap size but have a different drawback. `Minimap size` significantly increases the minimap generation time while `Minimap pixel size` makes the minimap less detailed.

Recommended settings:
- 20000 radius: 2 size, 1 pixel.
- 40000 radius: 2 size, 2 pixel.
- 80000 radius: 4 size, 2 pixel.
- 160000 radius: 4 size, 4 pixel.

Example generation times:
- Default: 15 seconds.
- 2 size: 1 minute.
- 4 size: 4 minutes.
- 8 size: 16 minutes.
- 16 size: 1 hour.

Note: The minimap is generated on the background. This is indicated by a small `Loading` text on the upper right corner.

Note: Changing `Minimap size` resets explored areas.

# Altitude

For the altitude, there are two types of settings: `* altitude delta` and `* altitude multiplier`. The multiplier multiplies the distance to the water level (by default at 30 meters). So increasing the multiplier will make water more deeper and other terrain higher. The delta directly affects the altitude. For example positive values will make the underwater terrain more shallow.

The formula is: `water level + (altitude - water level) * multiplier + delta`.

For the total altitude there are three layers:
- Base altitude affects biome distribution. For example increasing the altitude will cause more mountains.
- Biome altitudes only affect the altitude on that biome. This part of code also adds some elevation changes, even if the base altitude was made completely flat with 0 multiplier.
- Altitude is the last step and affets the whole world. This can be used to make whole world flat, or to increase elevation without having more mountains to appear.

## Water

Water level can be changed with `Water level` setting. This is currently experimental and probably causes some glitches.

Similarly wave size can be changed with `Wave multiplier` setting. With the `Wave only height` setting causing slightly different behavior. This is also experimental.

# Biomes

Most biomes have five settings which affects their distribution:

- Amount: The chance to appear on valid areas. For example 100 makes the biome fill all possible areas while 50 would it make only fill half of them.
- Begin percentage: The minimum distance from the world center (scaleds with the world radius).
- End percentage: The maximum distance from the world center (scaleds with the world radius).
- Sector begin percentage: The start of the sector.
- Sector end percentage: The end of the sector.

Note: The order of biomes is Ocean, Ashlands, Mountain (from altitude), Deep north, Mountain, Swamp, Mistlands, Plains, Black forest, Meadows and finally the biome defined by `Default biome` setting.

For sectors:
- South: 0
- West: 25
- North: 50
- East: 75

For example 25 sector begin and 75 sector end would make the biome only appear at the upper part of the world. Similarly 80 sector begin and 20 sector end would make the biome only appear at the bottom part.

Note: By default Mountains don't have any sector defined but only appear based on `Mountain minimum altitude` setting.

Ashlands and Deep north also have a curvature parameter which gives them the curved shape. This is bit hard to explain so better just experiment with different values.

## Biome borders

The default biome distribution causes the biome minimum distance to `wiggle`. This means that depending on the sector, the minimum distance is slightly increased or decreased (based on a sin wave). This is intended to make the world look more natural without artificial looking biome borders.

This is controlled by `Wiggle frequency` and `Wiggle width` settings. Again bit hard to explain but can be easily seen by increasing all biome amounts to 1 and setting base altitude multiplier to 0 and delta to 30.

For the biome sector parameters, there are `Distance wiggle length` and `Distance wiggle width` settings. These follow similar logic but are just applied to the sector begin and end percentages.

## Other

Rivers and streams can be disabled with `Rivers` and `Streams` settings.

Amount of forest can be changed with `Forest multiplier`. 

# Seeds

The default world generation derives sub seeds from the world seed. This mod allows fine tuning these sub seeds (and adds some new ones) to fine-tune the world. This is probably utterly pointless for most people.

The layout of the world is pre-determined, and each world is just a snapshot of it. The world can be manually moved in this layout with `Offset X` and `Offset Y` settings.

Each biome adds some height variation on top of the base altitude. This can be controlled with `Height variation seed` setting.

If biome amount is not 1, then the game must decide which areas to fill with the biome. This is controlled by `Black forest seed`, `Swamp seed`, `Plains seed` and `Mistlands seed` settings.

Similarly for rivers and streams there are `River seed` and `Stream seed` settings.

Since number 0 is a valid seed, each setting also a has setting whether to use the custom seed.

# Changelog

- v1.1
	- WARNING: Contains breaking changes for existing configs.
	- Adds a new setting `Stretch biomes`.
	- Adds an automatic fail-safe if some locations can't be placed (like Moder altar).
	- Adds data editing for biome weathers, events, locations, spawns, vegetation, weathers and world.
	- Removes most biome settings as obsolete.
	- Changes the name of setting `World strech` to `Stretch world` (remember to update existing configs).
	- Fixes wrong default values for settings `Black forest amount` (from 40 to 60), `Swamp amount` (from 60 to 40) and `Plains amount` (from 40 to 60).
	- Fixes the setting `Distance wiggle width` causing a minor anomaly.
	- Fixes the setting `Location` affecting the amount of start temples.
	- Fixes map data being some reset when joining dedicated servers.

- v1.0
	- Initial release.

Thanks for Azumatt for creating the mod icon!

Thanks for redseiko for the asynchronous minimap generating!