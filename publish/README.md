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

## Server side

This mod can be used only on the server. However only following files can be configured:

- `expand_events.yaml`: Fields enabled, duration, nearBaseOnly, biome, requiredGlobalKeys, notRequiredGlobalKeys, pauseIfNoPlayerInArea, random. 
- `expand_locations.yaml`: Only normally available or disabled [prefabs](https://valheim.fandom.com/wiki/Points_of_Interest_(POI)) can be used.
- `expand_vegetation.yaml`: All fields.
- `expand_world.cfg`: Only setting Location multiplier.

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

# Data

This mod provides additional configuration files (.yaml) that can be used to change most world generation related data.

These files are generated automatically to the `config/expand_world` folder when loading a world (unless they already exist).

Each file can be disabled from the main .cfg file to improve compatibility and performance (less network usage and processing when joining the server).

All files with the same start will be loaded. For example both `expand_biomes.yaml` and `expand_biomes_custom.yaml` would get loaded to biomes.

## Biomes

The file `expand_biomes.yaml` sets available biomes and their configuration.

You can add up to 22 new biomes (on top of the 9 default ones).

- biome: Identifier for this biome. This is used in the other files.
- name: Display name. Required for new biomes.
- terrain: Identifier of a default biome. Determines which terrain algorithm to use. Required for new biomes.
- altitudeDelta: Flat increase/decrease to the terrain altitude. See Altitude section for more info.
- altitudeMultiplier: Multiplier to the terrain altitude (relative to the water level).
- environments: List of available environments (weathers) and their relative chances.
- color: Terrain style. Not fully sure how this works but the color value somehow determines which default biome terrain style to use.
- mapColor: Color in the minimap (red, green, blue, alpha).
- musicMorning: Music override for the morning time.
- musicDay: Music override for the day time.
- musicEvening: Music override for the evening time.
- musicNight: Music override for the night time.

Adding a new biome:

1. Copy-paste an existing entry.
2. Change `biome` field.
3. Add `name` and `terrain` fields.
4. Add/modify other fields.
5. Use the biome identifier in other files.

## World

The file `expand_world.yaml` sets the biome distribution.

Each entry in the file adds a new rule. When determing the biome, the rules are checked one by one from the top until a valid rule is found. This means the order of entries is especially important for this file.

- biome: Identifier of the biome if this rule is valid.
- maxAltitude (default: `1000` meters): Maximum terrain height relative to the water level.
- minAltitude (default: `-1000` meters): Minimum terrain height relative to the water level.
- maxDistance (default: `1.0` of world radius): Maximum distance from the world center.
- minDistance (default: `0.0` of world radius): Minimum distance from the world center.
- maxSector (default: `1.0` of world angle): Maximum angle.
- minSector (default: `0.0` of world angle): Minimum angle.
- curveX (default: `0.0` of world radius): Moves the distance center point away from the world center.
- curveY (default: `0.0` of world radius): Moves the distance center point away from the world center.
- amount (default: `1.0` of total area): How much of the valid area is randomly filled with this biome.
- seed (default: ` `): Overrides the random outcome of `amount`. By default derived from the world seed.
- wiggleDistance (default: `true`): Applies "wiggle" to the `minDistance`.
- wiggleSector (default: `true`): Applies "wiggle" to the `maxSector` and `minSector`.

"Wiggle" adds a sin wave pattern to the distance/sector borders for less artifical biome transitions. The strength can be globally configured in the main .cfg file.

Note: A rule with only the biome identifier is always valid.

Note: The world edge is always ocean. This is currently hardcoded.

Adding a new biome:

1. Look at the default world file until the rules start to make sense.
2. Determine how you want the new biome to appear on the world.
3. Make changes until you succeed.

## Environments

The file `expand_environments.yaml` sets the available weathers.

- name: Identifier to be used in other files.
- particles: Identifier of a default environment to set particles. Required for new environments.
- isDefault (default: `false`): The first default environment is loaded at the game start up. No need to set this true unless removing from the Clear environment.
- isWet (default: `false`): If true, is considered to be raining.
- isFreezing (default: `false`): If true, causes the freezing debuff.
- isFreezingAtNight (default: `false`): If true, causes the freezing at night.
- isCold (default: `false`): If true, causes the cold debuff.
- isColdAtNight (default: `false`): If true, causes the cold at night.
- alwaysDark (default: `false`): If true, causes constant darkness.
- windMin (default: `0.0`): The minimum wind strength.
- windMax (default: `1.0`): The maximum wind strength.
- rainCloudAlpha (default: `0.0`): ???.
- ambientVol (default: `0.3`): ???.
- ambientList: ???.
- musicMorning: Music override for the morning time. Higher priority than the biome value.
- musicDay: Music override for the day time. Higher priority than the biome value.
- musicEvening: Music override for the evening time. Higher priority than the biome value.
- musicNight: Music override for the night time. Higher priority than the biome value.
- ambColorDay, ambColorNight, sunColorMorning, sunColorDay, sunColorEvening, sunColorNight: Color values.
- fogColorMorning, fogColorDay, fogColorEvening, fogColorNight, fogColorSunMorning, fogColorSunDay, fogColorSunEvening, fogColorSunNight: Color values.
- fogDensityMorning, fogDensityDay, fogDensityEvening, fogDensityNight (default: `0.01`): ???.
- lightIntensityDay (default: `1.2`): ???.
- lightIntensityNight (default: `0`): ???.
- sunAngle (default: `60`): ???.

Note: As you can see, lots of values have unknown meaning. Probably better to look at the existing environments for inspiration.

## Clutter

The file `expand_clutter.yaml` sets the small visual objects.

- prefab: Name of the clutter object. 
- enabled (default: `true`): Quick way to disable this entry.
- amount (default: `80`): Amount of clutter.
- biome: List of possible biomes.
- instanced (default: `false`): Way of rendering or something. Might cause errors if changed.
- onUncleared (default: `true`): Only on uncleared terrain.
- onCleared (default: `false`): Only on cleared terrain.
- scaleMin (default: `1.0`): Minimum scale for instanced clutter.
- scaleMax (default: `1.0`): Maximum scale for instanced clutter.
- maxTilt (default: `10` degrees): Maximum terrain angle.
- minAltitude (default: `-1000` meters): Minimum terrain altitude.
- maxAltitude (default: `1000` meters): Maximum terrain altitude.
- snapToWater (default: `false`): Placed at water level instead of terrain.
- terrainTilt (default: `false`): Rotates with the terrain angle.
- randomOffset (default: `0` meters): Moves the clutter randomly up/down.
- minOceanDepth (default: `0` meters): Minimum water depth (if different from max).
- maxOceanDepth  (default: `0` meters): Maximum water depth (if different from min).
- inForest (default: `false`): Only in forests.
- forestTresholdMin (default: `0`): Minimum forest value (if only in forests).
- forestTresholdMax (default: `0`): Maximum forest value (if only in forests).
- fractalScale  (default: `0`): Scale when calculating the fractal value.
- fractalOffset  (default: `0`): Offset when calculating the fractal value.
- fractalThresholdMin  (default: `0`): Minimum fractal value.
- fractalThresholdMax  (default: `1`): Maximum fractal value.

## Locations

The file `expand_locations.yaml` sets the available locations and their placement.

Locations are pregenerated at world generation. You must use `genloc` command to redistribute them on unexplored areas after making any changes. For already explored aresa, you need to use Upgrade World mod.

- prefab: Identifier of the location object. Check wiki for available locations. Hidden ones work too.
- enabled (default: `true`): Quick way to disable this entry.
- biome: List of possible biomes.
- biomeArea: List of possible biome areas (edge = zones with multiple biomes, median = zones with only a single biome, 4 = unused, leftover from Valheim data).
- quantity: Maximum amount. Actual amount is determined if enough suitable positions are found. The base .cfg has a setting to multiply these.
- minDistance (default: `0.0` of world radius): Minimum distance from the world center. Values higher than 1.0 are considered meters and are automatically scaled with the world radius.
- maxDistance (default: `1.0` of world radius): Maximum distance from the world center. Values higher than 1.0 are considered meters and are automatically scaled with the world radius.
- minAltitude (default: `0`): Minimum altitude.
- maxAltitude (default: `1000`): Maximum altitude.
- prioritized (default: `false`): Generated first with more attempts.
- centerFirst (default: `false`): Generating is attempted at world center, with gradually moving towards the world edge.
- unique (default: `false`): When placed, all other unplaced locations are removed. Guaranteed maximum of one instance.
- group: Group name for `minDistanceFromSimilar`.
- minDistanceFromSimilar (default: `0` meters): Minimum distance between the same location, or locations in the `group` if given.
- iconAlways (default: `false`): Location icon is always shown.
- iconPlaced (default: `false`): Location icon is only shown when the location instance is actually placed.
- randomRotation (default: `false`): Randomly rotates the location (unaffected by world seed).
- slopeRotation (default: `false`): Rotates based on the terrain angle. For example for locations at mountain sides.
- snapToWater (default: `false`): Placed at the water level instead of the terrain.
- minTerrainDelta (default: `0` meters): Minimum nearby terrain height difference.
- maxTerrainDelta (default: `10` meters): Maximum nearby terrain height difference.
- inForest (default: `false`): Only in forests.
- forestTresholdMin (default: `0`): Minimum forest value (if only in forests).
- forestTresholdMax (default: `0`): Maximum forest value (if only in forests).

## Vegetation

The file `expand_vegetations.yaml` sets the generated objects.

Changes only apply to unexplored areas. Upgrade World mod can be used to reset areas.

- prefab: Identifier of the object.
- enabled (default: `false`): Quick way to disable this entry.
- min (default: `1`): Minimum amount (of groups) to be placed.
- max (default: `1`): Maximum amount (of groups) to be placed. If less than 1, has only a chance to appear.
- forcePlacement (default: `false`): ???.
- scaleMin (default: `1`): Minimum scale.
- scaleMax (default: `1`): Maximum scale.
- randTilt (default: `0` degrees): Random rotation.
- chanceToUseGroundTilt (default: `0.0`): Chance to set rotation based on terrain angle.
- biome: List of possible biomes.
- biomeArea: List of possible biome areas (edge = zones with multiple biomes, median = zones with only a single biome, 4 = unused, leftover from Valheim data).
- blockCheck (default: `true`): Requires clear ground.
- minAltitude (default: `0` meters): Minimum terrain altitude.
- maxAltitude (default: `1000` meters): Maximum terrain altitude.
- minOceanDepth (default: `0` meters): Minimum ocean depth.
- maxOceanDepth (default: `0` meters): Maximum ocean depth.
- minTilt (default: `0` degrees): Minimum terrain angle.
- maxTilt (default: `90` degrees): Maximum terrain angle.
- terrainDeltaRadius (default: `0` meters): Radius for terrain delta limits.
- minTerrainDelta (default: `0` meters): Minimum terrain height change.
- maxTerrainDelta (default: `10` meters): Maximum terrain height change.
- snapToWater (default: `false`): Placed at the water level instead of the terrain.
- groundOffset (default: `0` meters): Placed above the ground.
- groupSizeMin (default: `1`): Minimum amount to be placed at the same time.
- groupSizeMax (default: `1`): Maximum amount to be placed at the same time.
- inForest (default: `false`): Only in forests.
- forestTresholdMin (default: `0`): Minimum forest value (if only in forests).
- forestTresholdMax (default: `0`): Maximum forest value (if only in forests).

## Spawns

The file `expand_spawns.yaml` sets the spawned creatures.

- prefab: Identifier of the object. Any [object](https://valheim.fandom.com/wiki/Item_IDs) is valid, not just creatures.
- enabled (default: `false`): Quick way to disable this entry.
- biome: List of possible biomes.
- biomeArea: List of possible biome areas (edge = zones with multiple biomes, median = zones with only a single biome, 4 = unused, leftover from Valheim data).
- spawnChance (default: `100` %): Chance to spawn when attempted.
- maxSpawned: Limit for this entry. Also how many spawn attempts are stacked over time.
- spawnInterval: How often the spawning is attempted.
- minLevel (default: `1`): Minimum creature level.
- maxLevel (default: `1`): Maximum creature level.
- minAltitude (default: `-1000` meters): Minimum terrain altitude.
- maxAltitude (default: `1000` meters): Maximum terrain altitude.
- requiredGlobalKey: Which [global keys](https://valheim.fandom.com/wiki/Global_Keys) must be set to enable this entry.
- requiredEnvironments: List of possible environments/weathers.
- spawnDistance (default: `10` meters): Distance to suppress similar spawns.
- spawnRadiusMin (default: `40` meters): Minimum distance from every player.
- spawnRadiusMax (default: `80` meters): Maximum distance from any player.
- groupSizeMin (default: `1`): Minimum amount spawned at the same time.
- groupSizeMax (default: `1`): Maximum amount spawned at the same time.
- groupRadius (default: `3` meters): Radius when spawning multiple objects.
- minTilt (default: `0` degrees): Minimum terrain angle.
- maxTilt (default: `35` degrees): Maximum terrain angle.
- inForest (default: `true`): Enabled in forests.
- outsideForest (default: `true`): Enabled outside forests.
- minOceanDepth (default: `0` meters): Minimum ocean depth.
- maxOceanDepth (default: `0` meters): Maximum ocean depth.
- huntPlayer (default: `false`): Spawned creatures are more aggressive.
- groundOffset (default: `0.5` meters): Spawns above the ground.
- levelUpMinCenterDistance (default: `0` meters): Distance from the world center to enable higher creature levels. This is not scaled with the world size.

## Events

The file `expand_events.yaml` sets the boss and random events.

See the [wiki](https://valheim.fandom.com/wiki/Events) for more info.

- name: Identifier.
- enabled (default: `false`): Quick way to disable this entry.
- duration (default: `60` seconds): How long the event lasts.
- nearBaseOnly (default: `true`): Only triggers when near 3 player base items.
- biome: List of possible biomes.
- requiredGlobalKeys: Which [global keys](https://valheim.fandom.com/wiki/Global_Keys) must be set for this event.
- notRequiredGlobalKeys: Which [global keys](https://valheim.fandom.com/wiki/Global_Keys) must not be set for this event.
- startMessage: Message shown on the screen during the event.
- endMessage: The end message.
- forceMusic: Event music.
- forceEnvironment: Event environment/weather.
- spawns: List of spawned objects. See spawns section for more info. Usually these have lower spawn times and less restrictions compared to normal spawns.
- pauseIfNoPlayerInArea (default: `true`): The event timer pauses if no player in the area.
- random (default: `true`): The event can happen randomly (unlike boss events which happen when near a boss).

# Rivers and streams

Lakes are needed to generate rivers. The code searches for points with enough water and then merges them to lake objects. Use command `find_lakes` to show their positions on the map.

Note: Lake object is an abstract concept, not a real thing. So the settings only affect river generation.

Settings to find lakes:

- Lake search interval (default: 128 meters): How often a point is checked for lakes (meters). Increase to find more smaller lakes.
- Lake point depth (default: -20 meters): How deep the point must be to be considered a lake. Increase to find more shallow lakes.
- Lake merge radius (default: 800 meters): How big area is merged to a single lake. Decrease to get more lakes.

Rivers are generated between lakes. So generally increasing the amount of lakes also increases the amount of rivers.

However the lakes must have terrain higher than `Lake point depth` between them. So increase that value removes some of the rivers.

Settings to place rivers:

- River seed: Seed which determines the order of lakes (when selected by random).
- Max distance 1 (default: 2000 meters): Lakes within this distance get a river between them. Increase to place more and longer rivers.
- Max distance 2 (default: 5000 meters): Fallback. Lakes without a river do a longer search and place one river to a random lake. Increase to enable very long rivers without increasing the total amount that much. 
- River max altitude (default: 50 meters): The river is not valid if this terrain altitude is found between the lakes.
- River check interval (default: 128 meters): How often the river altitude is checked. Both `River max altitude` and `Lake point depth`.

Rivers have params:

- River seed: Seed which determines the random river widths.
- Maximum width (default: 100): For each river, the maximum width is randomly selected between this and `Minimum width`.
- Minimum width (default: 60): For each river, the minimum width is randomly selected between this and selected maximum width. So the average width is closer to the `Minimum width` than the `Maximum width`.
- Curve width (default: 1/15): 1/15
- Curve wave length (default: 1/20): How

Streams are gemerated by trying to find random points within an altitude range. 

- Stream seed:  Seed for randomness.
- Max streams: 3000
- Start min height: 26
- Start max height: 31
- Start iterations: 100, how many times tries to find a random point within height range.
- End min height: 36
- End max height: 44
- End iterations: 100, how many times tries to find a random point within height range.

Streams have params:

- River seed: Seed for randomness.
- Minimum width: 20
- Maximum width: 20
- Curve width: 1/15
- Curve wave length: 1/20



# Changelog

- v1.1
	- WARNING: Contains breaking changes for existing configs.
	- Adds a new setting `Stretch biomes`.
	- Adds an automatic fail-safe if some locations can't be placed (like Moder altar).
	- Adds data editing for biome weathers, events, locations, spawns, vegetation, weathers and world.
	- Adds lots of new settings for rivers.
	- Removes most biome settings as obsolete.
	- Removes the `Locked` setting as obsolete (you never want this mod unsynced).
	- Changes the name of setting `World strech` to `Stretch world` (remember to update existing configs).
	- Fixes wrong default values for settings `Black forest amount` (from 40 to 60), `Swamp amount` (from 60 to 40) and `Plains amount` (from 40 to 60).
	- Fixes the setting `Distance wiggle width` causing a minor anomaly.
	- Fixes the setting `Location` affecting the amount of start temples.
	- Fixes map data being some reset when joining dedicated servers.
	- Fixes map dragging sometimes causing map icons to appear.
	- Fixes possible terrain desync from rivers.

- v1.0
	- Initial release.

Thanks for Azumatt for creating the mod icon!

Thanks for redseiko for the asynchronous minimap generating!