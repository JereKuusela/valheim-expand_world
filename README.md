# Expand world

This mod allows adding new biomes and changing most of the world generation.

Always back up your world before making any changes!

Install on all clients and on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

# Features

- Add new biomes.
- Make the world bigger or taller.
- Change biome distribution.
- Change data like events, spawners and weather.
- The minimap is generated in the background, lowering loading times.
- Config sync to ensure all clients use the same settings.

For example you can create entirely flat worlds with only Meadows for building. Or group up colder biomes up north while more warmer biomes end up in the other side. Or just have a world with terrain shapes no one has ever seen before.

# Configuration

The mod supports live reloading when changing the configuration (either with [Configuration manager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/) or by saving the config file). This can lead to weird behavior so after playing with the settings it's recommended to make a fresh world.

Note: Pay extra attention when loading old worlds. Certain configurations can cause alter the terrain significantly and destroy your buildings.

## Server side

This mod can be used only on the server. However only following files can be configured:

- `expand_events.yaml`: Fields enabled, duration, nearBaseOnly, biome, requiredGlobalKeys, notRequiredGlobalKeys, pauseIfNoPlayerInArea, random. 
- `expand_locations.yaml`: Only normally available or disabled [prefabs](https://valheim.fandom.com/wiki/Points_of_Interest_(POI)) can be used.
- `expand_vegetation.yaml`: All fields.
- `expand_world.cfg`: Only setting Location multiplier.

When doing this, enable `Server only` on the config to remove version check.

## Migration from version 1.5

- Verify that `expand_vegetation.yaml` uses `.` as the separate character for `scaleMin` and `scaleMax` instead of `,`. Fix manually or remove the file to regerenate it.

# World size

The size can be increased by changing the `World radius` and `World edge size` settings. The total size is sum of these (default is 10000 + 500 = 10500 meters). Usually there is no need to change the edge size.

The world can be stretched with `Stretch world` setting. This can be used to keep the same world but islands and oceans are just bigger. This will also make biomes bigger which can be further tweaked with `Stretch biomes` setting (for example using 0.5 biome stretch with 2 world stretch).

The amount of locations (like boss altars) can be changed with `Locations` setting. This can significantly increase the initial world generation time (especially when the game fails to place most locations). If changing this on existing worlds, use `genloc` command to distribute unplaced locations.

Note: 2x world radius means 4x world area. So for 20000 radius you would need 4x locations and for 40000 radius you would need 16x locations.

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

For the altitude, there are two settings: `Altitude delta` and `Altitude multiplier`. The multiplier multiplies the distance to the water level (by default at 30 meters). So increasing the multiplier will make water more deeper and other terrain higher. The delta directly affects the altitude. For example positive values will make the underwater terrain more shallow.

The formula is: `water level + (altitude - water level) * multiplier + delta`.

For the total altitude there are two layers:
- Altitude affects biome distribution. For example increasing the altitude will cause more mountains.
- Biome altitudes only affect the altitude on that biome. This part of code also adds some elevation changes, even if the altitude was made completely flat with 0 multiplier.

The final water depth can be multiplied with `Water depth multiplier`.

Amount of forest can be changed with `Forest multiplier`. 

# Seeds

The layout of the world is [pre-determined](https://www.reddit.com/r/valheim/comments/qere7a/the_world_map/), and each world is just a snapshot of it. The world can be manually moved in this layout with `Offset X` (to west) and `Offset Y` (to south) settings.

Each biome adds some height variation on top of the base altitude. This can be controlled with `Height variation seed` setting.

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
- terrain: Identifier of the base biome. Determines which terrain algorithm to use. Required for new biomes.
- nature: Identifier of the base biome. Determines which plants can grow here, whether bees are happy and foot steps. If not given, uses the terrain value.
- altitudeDelta: Flat increase/decrease to the terrain altitude. See Altitude section for more info.
- altitudeMultiplier: Multiplier to the terrain altitude (relative to the water level).
- forestMultiplier: Multiplier to the global forest multiplier. Using this requires an extra biome check which will lower the performance.
- environments: List of available environments (weathers) and their relative chances.
- maximumAltitude (default: `1000` meters): Maximum altitude.
- minimumAltitude (default: `-1000` meters): Minimum altitude.
- excessFactor (default: `0.5`): How strongly the altitude is reduced if over the maximum or minimum limit. For example 0.5 square roots the excess altitude.
- paint: Default terrain paint. r = dirt, g = cultivated, b = paved, a = does nothing. Values from 0.0 to 1.0. Using this will lower the performance.
- color: Terrain style. Not fully sure how this works but the color value somehow determines which default biome terrain style to use.
- mapColorMultiplier (default: `1.0`): Changes how quickly the terrain altitude affects the map color. Increasing the value can be useful for low altitude biomes to show the altitude differences better. Lowering the value can be useful for high altitude biomes to reduce amount of white color (from mountain altitudes). Negative value can be useful for underwater biomes to show the map color (normally all underwater areas get blueish color).
- mapColor: Color in the minimap (red, green, blue, alpha).
- musicMorning: Music override for the morning time.
- musicDay: Music override for the day time.
- musicEvening: Music override for the evening time.
- musicNight: Music override for the night time.

## World

The file `expand_world.yaml` sets the biome distribution.

Each entry in the file adds a new rule. When determing the biome, the rules are checked one by one from the top until a valid rule is found. This means the order of entries is especially important for this file.

- biome: Identifier of the biome if this rule is valid.
- maxAltitude (default: `1000` meters): Maximum terrain height relative to the water level.
- minAltitude (default: `0` meters if maxAltitude is positive, otherwise `-1000` meters): Minimum terrain height relative to the water level.
- maxDistance (default: `1.0` of world radius): Maximum distance from the world center.
- minDistance (default: `0.0` of world radius): Minimum distance from the world center.
- minSector (default: `0.0` of world angle): Start of the [circle sector](https://en.wikipedia.org/wiki/Circular_sector).
- maxSector (default: `1.0` of world angle): End of the [circle sector](https://en.wikipedia.org/wiki/Circular_sector).
- curveX (default: `0.0` of world radius): Moves the distance center point away from the world center.
- curveY (default: `0.0` of world radius): Moves the distance center point away from the world center.
- amount (default: `1.0` of total area): How much of the valid area is randomly filled with this biome. Uses normal distribution, see values below.
- stretch (default: `1.0`): Same as the `Stretch biomes` setting but applied just to a single entry. Multiplies the size of biome areas (average total area stays the same). 
- seed: Overrides the random outcome of `amount`. Numeric value fixes the outcome. Biome name uses a biome specific value derived from the world seed. No value uses biome from the `terrain` parameter.
- waterDepthMultiplier (default: `1.0`): Multiplies negative terrain altitude.
- wiggleDistance (default: `true`): Applies "wiggle" to the `minDistance`.
- wiggleSector (default: `true`): Applies "wiggle" to the `maxSector` and `minSector`.

Note: The world edge is always ocean. This is currently hardcoded.

### Amount

Technically the amount is not a percentage but something closer to a normal distribution.

Manual testing with `debug_biomes` command has given these rough values:
- 0.1: 0.4 %
- 0.2: 2.7 %
- 0.25: 5.3 %
- 0.3: 8.8 %
- 0.35: 14 %
- 0.4: 23 %
- 0.45: 32 %
- 0.5: 42 %
- 0.535: 50 %
- 0.55: 54 %
- 0.6: 64 %
- 0.65: 74 %
- 0.7: 83 %
- 0.75: 90 %
- 0.8: 94 %
- 0.85: 97 %
- 0.9: 99 %

For example if you want to replace 25% of Plains with a new biome you can calculate 0.6 -> 64 % -> 64 % / 4 = 16 % -> 0.35. So you would put 0.35 (or 0.36) to the amount of the new biome.

Note: The amount is of the total world size, not of the remaining area. If two biomes have the same seed then their areas will overlap which can lead to unexpected results.

For example if the new biome is a variant of Plains then there is no need to reduce the amount of Plains because the new biome only exists where they would have been Plains.

If the seeds are different, then Plains amount can be calculated with 0.6 -> 64 % -> 64 % * (1 - 0.25) / (1 - 0.16) = 57 % -> 0.56.

### Sectors

Sectors start at the south and increase towards clock-wise direction. So that:
- Bottom left part is between sectors 0 and 0.25.
- Top left part is between sectors 0.25 and 0.5.
- Top right part is between sectors 0.5 and 0.75.
- Top left part is between sectors 0.75 and 1.
- Left part is between sectors 0 and 0.5.
- Top part is between sectors 0.25 and 0.75.
- Right part is between sectors 0.5 and 1.
- Bottom part is between sectors -0.25 and 0.25 (or 0.75 and 1.25).

Note: Of course any number is valid for sectors. Like from 0.37 to 0.62.

### Wiggle

"Wiggle" adds a sin wave pattern to the distance/sector borders for less artifical biome transitions. The strength can be globally configured in the main .cfg file.

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

See the [wiki](https://valheim.fandom.com/wiki/Points_of_Interest_(POI)) for more info.

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
- data: ZDO data override. For example to change altars with Spawner Tweaks mod. To create a variant of an existing location, add `:text` to the prefab. For example "Eikthyrnir:Wolf".

## Vegetation

The file `expand_vegetations.yaml` sets the generated objects.

Changes only apply to unexplored areas. Upgrade World mod can be used to reset areas.

- prefab: Identifier of the object.
- enabled (default: `false`): Quick way to disable this entry.
- min (default: `1`): Minimum amount (of groups) to be placed.
- max (default: `1`): Maximum amount (of groups) to be placed. If less than 1, has only a chance to appear.
- forcePlacement (default: `false`): ???.
- scaleMin (default: `1`): Minimum scale. Number or x,z,y (with y being the height).  
- scaleMax (default: `1`): Maximum scale. Number or x,z,y (with y being the height).
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
- data: ZDO data override. For example to create hidden stashes with Spawner Tweaks mod.

## Spawns

The file `expand_spawns.yaml` sets the spawned creatures.

See the [wiki](https://valheim.fandom.com/wiki/Spawn_zones) for more info.

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
- spawnAtDay (default: `true`): Enabled during the day time.
- spawnAtNight (default: `true`): Enabled during the night time.
- requiredGlobalKey: Which [global keys](https://valheim.fandom.com/wiki/Global_Keys) must be set to enable this entry.
- requiredEnvironments: List of valid environments/weathers.
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
- requiredEnvironments: List of valid environments/weathers. Checked by the server so using `env` command in the client doesn't affect this.
- spawns: List of spawned objects. See spawns section for more info. Usually these have lower spawn times and less restrictions compared to normal spawns.
- pauseIfNoPlayerInArea (default: `true`): The event timer pauses if no player in the area.
- random (default: `true`): The event can happen randomly (unlike boss events which happen when near a boss).

# Water

Water settings are in the main `expand_world.cfg` file.

Water level can be changed with `Water level` setting. This is currently experimental and probably causes some glitches.

Similarly wave size can be changed with `Wave multiplier` setting. With the `Wave only height` setting causing slightly different behavior. This is also experimental.

## Lakes

Lakes are needed to generate rivers. The code searches for points with enough water and then merges them to lake objects. Use command `find_lakes` to show their positions on the map.

Note: Lake object is an abstract concept, not a real thing. So the settings only affect river generation.

Settings to find lakes:

- Lake search interval (default: `128` meters): How often a point is checked for lakes (meters). Increase to find more smaller lakes.
- Lake depth (default: `-20` meters): How deep the point must be to be considered a lake. Increase to find more shallow lakes.
- Lake merge radius (default: `800` meters): How big area is merged to a single lake. Decrease to get more lakes.

## Rivers

Rivers are generated between lakes. So generally increasing the amount of lakes also increases the amount of rivers.

However the lakes must have terrain higher than `Lake point depth` between them. So increase that value removes some of the rivers.

Settings to place rivers:

- River seed: Seed which determines the order of lakes (when selected by random). By default derived from the world seed.
- Lake max distance 1 (default: `2000` meters): Lakes within this distance get a river between them. Increase to place more and longer rivers.
- Lake max distance 2 (default: `5000` meters): Fallback. Lakes without a river do a longer search and place one river to a random lake. Increase to enable very long rivers without increasing the total amount that much. 
- River max altitude (default: `50` meters): The river is not valid if this terrain altitude is found between the lakes.
- River check interval (default: `128` meters): How often the river altitude is checked. Both `River max altitude` and `Lake point depth`.

Rivers have params:

- River seed: Seed which determines the random river widths. By default derived from the world seed.
- River maximum width (default: `100`): For each river, the maximum width is randomly selected between this and `River minimum width`.
- River minimum width (default: `60`): For each river, the minimum width is randomly selected between this and selected maximum width. So the average width is closer to the `River minimum width` than the `River maximum width`.
- River curve width (default: `15`): How wide the curves are.
- River curve wave length (default: `20`): How often the river changes direction.

## Streams

Streams are generated by trying to find random points within an altitude range. 

- Stream seed: Seed which determines the stream positions. By default derived from the world seed.
- Max streams (default: `3000`): How many times the code tries to place a stream. This is NOT scaled with the world radius.
- Stream search iterations (default: `100`): How many times the code tries to find a suitable start and end point.
- Stream start min altitude (default: `-4` meters): Minimum terrain height for stream starts.
- Stream start max altitude (default: `1` meter): Maximum terrain height for stream starts.
- Stream end min altitude (default: `6` meters): Minimum terrain height for stream ends.
- Stream end max altitude (default: `14` meters): Maximum terrain height for stream ends.

Streams have params:

- Stream seed: Seed which determines the random stream widths. By default derived from the world seed.
- Stream maximum width (default: `20`): For each stream, the maximum width is randomly selected between this and `Stream minimum width`.
- Stream minimum width (default: `20`): For each stream, the minimum width is randomly selected between this and selected maximum width. So the average width is closer to the `Stream minimum width` than the `Stream maximum width`.
- Stream min length (default: `80` meters): Minimum length for streams.
- Stream max length (default: `299` meters): Maximum length for streams.
- Stream curve width (default: `15`): How wide the curves are.
- Stream curve wave length (default: `20`): How often the stream changes direction.

# Examples

## Adding a new location

1. Open `expand_locations.yaml`.
2. Copy paste existing entry.
3. Change `prefab`, for example to XMasTree (check [wiki](https://valheim.fandom.com/wiki/Points_of_Interest_(POI)) for ids).
4. Change other fields.

## Adding a location variant with Spawner Tweaks mod

1. Install Server Devcommands, World Edit Commands and Spawner Tweaks mods.
2. Open `expand_locations.yaml`.
3. Copy paste `Eikthyrnir` entry and change prefab to `Eikthyrnir:Wolf`.
4. Add `data:` to the entry.
5. Find existing Eikthyr altar in the world and use command `tweak_altar amount=0 spawn=Wolf`.
6. Use command `data copy` and then paste (CTRL+V) to the `data` field.
7. Create a new world or use `locations_add Eikthyrnir:Wolf` from Upgrade World mod.

## Adding a vegetation variant with Spawner Tweaks mod

1. Install Server Devcommands, World Edit Commands and Spawner Tweaks mods.
2. Open `expand_vegetation.yaml`.
3. Copy paste `stubbe` entry.
4. Add `data:` to the entry.
5. Use command `tweak_chest force maxamount=2 item=Torch item=Coins,1,10,20` on any object.
6. Use command `data copy` and then paste (CTRL+V) to the `data` field.
7. Create a new world or use `zones_reset start` from Upgrade World mod.

## Bosses spawning enemies

1. Open `expand_events.yaml`.
2. Find boss_eikthyr or other boss event.
3. Copy `spawns` field from another entry like army_eikthyr.

## Adding a new biome

1. Open `expand_biomes.yaml`.
2. Copy-paste existing entry.
3. Change `biome`, add `name` and add `terrain`.
4. Add/modify other fields.
5. Open `expand_world.yaml`.
6. Look at it until the rules start to make sense.
7. Determine how you want the new biome to appear on the world.
8. Make changes until you succeed.
9. Edit other files to make spawns, locations and vegetation to work.

Example:

Copy-paste ashlands entry and change:
- biome: desert
  name: Desert
  terrain: ashlands
  environments:
  - environment: Clear

Copy-paste plains entry and change the top one:
- biome: desert
  amount: 0.5
