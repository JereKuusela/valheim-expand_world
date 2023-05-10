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

# Tutorials

- How to make custom biomes: https://youtu.be/TgFhW0MtYyw (33 minutes, created by StonedProphet)
- How to use blueprints as locations with custom spawners: https://youtu.be/DXtm-WLF6KE (30 minutes, created by StonedProphet)

# Configuration

The mod supports live reloading when changing the configuration (either with [Configuration manager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/) or by saving the config file). This can lead to weird behavior so after playing with the settings it's recommended to make a fresh world.

Note: Pay extra attention when loading old worlds. Certain configurations can cause alter the terrain significantly and destroy your buildings.

## Server side

This mod can be used server only, without requiring clients to have it. However only following files can be configured:

- `expand_dungeons.yaml`: All fields.
- `expand_events.yaml`: Fields enabled, duration, nearBaseOnly, biome, requiredGlobalKeys, notRequiredGlobalKeys, pauseIfNoPlayerInArea, random. 
- `expand_locations.yaml`: All fields, but some disabled locations won't work.
- `expand_rooms.yaml`: All fields.
- `expand_vegetation.yaml`: All fields.
- `expand_world.cfg`: Only settings Location multiplier, Random event chance, Random event interval and Zone spawners.

When doing this, enable `Server only` on the config to remove version check.

# World size

The size can be increased by changing the `World radius` and `World edge size` settings. The total size is sum of these (default is 10000 + 500 = 10500 meters). Usually there is no need to change the edge size.

The world can be stretched with `Stretch world` setting. This can be used to keep the same world but islands and oceans are just bigger. This will also make biomes bigger which can be further tweaked with `Stretch biomes` setting (for example using 0.5 biome stretch with 2 world stretch).

The amount of locations (like boss altars) can be changed with `Locations` setting. This can significantly increase the initial world generation time (especially when the game fails to place most locations). If changing this on existing worlds, use `genloc` command to distribute unplaced locations.

Note: 2x world radius means 4x world area. So for 20000 radius you would need 4x locations and for 40000 radius you would need 16x locations.

Note: If the game fails to place the spawn altar (for example if no Meadows), then it is forcefully placed at the middle of the map. With bad luck, this can be underwater.

Note: File `expand_world.yaml` must be enabled to scale biomes properly.

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

Note: Altitude based snow is hard coded and can't be changed.

# Seeds

The layout of the world is [pre-determined](https://www.reddit.com/r/valheim/comments/qere7a/the_world_map/), and each world is just a snapshot of it.

The world can be manually moved in this layout with `Offset X` (to west) and `Offset Y` (to south) settings.

For example x = 0 and y = 0 would move the world center to the center of the big map. Similarly x = -20000 and y = 0 would move it to the edge of the big map.

Command `ew_seeds` prints the default offset and other seeds of the world.

Each biome adds some height variation on top of the base altitude. This can be controlled with `Height variation seed` setting.

The whole seed can be replaced with `Seed` setting. This gets permanently saved to the save file.

# Data

This mod provides additional configuration files (.yaml) that can be used to change most world generation related data.

These files are generated automatically to the `config/expand_world` folder when loading a world (unless they already exist).

Each file can be disabled from the main .cfg file to improve compatibility and performance (less network usage and processing when joining the server).

Data can be split to multiple files. The files are loaded alphabetically in reverse order. For example `expand_biomes_custom.yaml` would be loaded first and then `expand_biomes.yaml`.

Note: Some files are server side only. Use single player for testing so that your client has access to all of the information. Mods that cause clients to regenerate dungeons or locations may not work correctly.

### Blueprints

Dungeons, locations and vegetation support using blueprints to spawn multiple objects. Both PlanBuild .blueprint and BuildShare .vbuild files are supported (however PlanBuild files are preferred).

The file format is slightly modified from the usual:

- Two new fields are added to both .blueprint and .vbuild files.
  - zdoData initializes the object with a specific data. Infinity Hammer automatically saves this when creating .blueprint files.
  - chance is a number between 0 and 1. These must be added manually to the file.
  - .blueprint format: name;posX;posY;posZ;rotX;rotT;rotZ;rotW;info;scaleX;scaleY;scaleZ;zdoData;chance
  - .vbuild format: name;rotX;rotT;rotZ;rotW;posX;posY;posZ;zdoData;chance
- Blueprints can contain other blueprints as objects. These must added manually to the file.
- Center piece (bottom center of the blueprint) can be set to a certain object. This object is never spawned to the world.
  - Infinity Hammer saves this information to .blueprint files.
  - Center piece can also be set in the .yaml files.
  - If a blueprint is used multiple times, it should always use the same center piece.

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
- paint: Default terrain paint. Using this will lower the performance. Format is `dirt,cultivated,paved,vegetation` (from 0.0 to 1.0) or a pre-defined color (cultivated, dirt, grass, grass_dark, patches, paved, paved_dark, paved_dirt, paved_moss)
- color: Terrain style. Not fully sure how this works but the color value somehow determines which default biome terrain style to use.
- mapColorMultiplier (default: `1.0`): Changes how quickly the terrain altitude affects the map color. Increasing the value can be useful for low altitude biomes to show the altitude differences better. Lowering the value can be useful for high altitude biomes to reduce amount of white color (from mountain altitudes). Negative value can be useful for underwater biomes to show the map color (normally all underwater areas get blueish color).
- mapColor: Color in the minimap (red, green, blue, alpha).
- musicMorning: Music override for the morning time.
- musicDay: Music override for the day time.
- musicEvening: Music override for the evening time.
- musicNight: Music override for the night time.
- noBuild (default: `false`): If true, players can't build in this biome.

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
- centerX (default: `0.0` of world radius): Moves the center point away from the world center.
- centerY (default: `0.0` of world radius): Moves the center point away from the world center.
- amount (default: `1.0` of total area): How much of the valid area is randomly filled with this biome. Uses normal distribution, see values below.
- stretch (default: `1.0`): Same as the `Stretch biomes` setting but applied just to a single entry. Multiplies the size of biome areas (average total area stays the same). 
- seed: Overrides the random outcome of `amount`. Numeric value fixes the outcome. Biome name uses a biome specific value derived from the world seed. No value uses biome from the `terrain` parameter.
- waterDepthMultiplier (default: `1.0`): Multiplies negative terrain altitude.
- wiggleDistance (default: `true`): Applies "wiggle" to the `minDistance`.
- wiggleSector (default: `true`): Applies "wiggle" to the `maxSector` and `minSector`.

Note: The world edge is always ocean. This is currently hardcoded.

### Amount

Technically the amount is not a percentage but something closer to a normal distribution.

Manual testing with `ew_biomes` command has given these rough values:
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

The file `expand_environments.yaml` sets the available weathers. Command `ew_musics` can be used to print available musics.

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
- minTilt (default: `0` degrees): Minimum terrain angle.
- maxTilt (default: `10` degrees): Maximum terrain angle.
- minAltitude (default: `-1000` meters): Minimum terrain altitude.
- maxAltitude (default: `1000` meters): Maximum terrain altitude.
- minVegetation (default: `0`): Minimum vegetation mask.
- maxVegetation (default: `0`): Maximum vegetation mask.
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

The file `expand_locations.yaml` sets the available locations and their placement. This is a server side feature, clients don't have access to this data.

Note: Missing locations are automatically added to the file. To disable, set `enabled` to `false` instead of removing anything.

Note: Each zone (64m x 64m) can only have one size.

See the [wiki](https://valheim.fandom.com/wiki/Points_of_Interest_(POI)) for more info.

See [examples](https://github.com/JereKuusela/valheim-expand_world/blob/main/examples_locations.md).

Locations are pregenerated at world generation. You must use `genloc` command to redistribute them on unexplored areas after making any changes. For already explored areas, you need to use Upgrade World mod.

- prefab: Identifier of the location object or name of blueprint file. Check wiki for available locations. Hidden ones work too. To create a clone of an existing location, add `:text` to the prefab. For example "Dolmen01:Ghost".
- enabled (default: `true`): Quick way to disable this entry.
- biome: List of possible biomes.
- biomeArea: List of possible biome areas (edge = zones with multiple biomes, median = zones with only a single biome).
- dungeon: Overrides the default dungeon generator with a custom one from `expand_dungeons.yaml`.
- quantity: Maximum amount. Actual amount is determined if enough suitable positions are found. The base .cfg has a setting to multiply these.
- minDistance (default: `0.0` of world radius): Minimum distance from the world center. Values over 2.0 are considered as meters.
- maxDistance (default: `1.0` of world radius): Maximum distance from the world center. Values over 2.0 are considered as meters.
- minAltitude (default: `0`): Minimum altitude.
- maxAltitude (default: `1000`): Maximum altitude.
- prioritized (default: `false`): Generated first with more attempts.
- centerFirst (default: `false`): Generating is attempted at world center, with gradually moving towards the world edge.
- unique (default: `false`): When placed, all other unplaced locations are removed. Guaranteed maximum of one instance.
- group: Group name for `minDistanceFromSimilar`.
- minDistanceFromSimilar (default: `0` meters): Minimum distance between the same location, or locations in the `group` if given.
- iconAlways: Location icon that is always shown. Use `ew_icons` to see what is available.
- iconPlaced: Location icon to show when the location is generated. Use `ew_icons` to see what is available.
- randomRotation (default: `false`): Randomly rotates the location (unaffected by world seed).
- slopeRotation (default: `false`): Rotates based on the terrain angle. For example for locations at mountain sides.
- snapToWater (default: `false`): Placed at the water level instead of the terrain.
- minTerrainDelta (default: `0` meters): Minimum nearby terrain height difference.
- maxTerrainDelta (default: `10` meters): Maximum nearby terrain height difference.
- inForest (default: `false`): Only in forests.
- forestTresholdMin (default: `0`): Minimum forest value (if only in forests).
- forestTresholdMax (default: `0`): Maximum forest value (if only in forests).
- groundOffset (default: `0` meters): Placed above the ground.
- data: ZDO data override. For example to change altars with Spawner Tweaks mod (`object copy` from World Edit Commands).
- objectSwap: Changes location objects to other objects, doesn't include dungeons.
    - Use format `id, swapid` for a direct swap.
    - Use format `id, swapid1, swapid2, ...` for multiple possible outcomes.
    - Use format `id, swapid1:weight1, swapid2:weight2, ...` to control the chance of each outcome.
    - Empty id can be used to spawn nothing.
    - Blueprints are supported.
    - Use command `ew_locations` to print location contents.
    - [Example](https://github.com/JereKuusela/valheim-expand_world/blob/main/examples_locations.md#location-adding-new-objects)
- objectData: List to set child object data. Format is `id,data`.
  - id: Prefab name.
  - data: ZDO data override.
- objects: Extra objects or blueprints. Format is `id,posX,posZ,posY,rotY,rotX,rotZ,scaleX,scaleZ,scaleY,chance,data`.
  - id: Prefab name.
  - posX, posZ, posY: Offset from the location position. Defalt is 0.
  - rotY, rotX, rotZ: Rotation. Default is 0.
  - scaleX, scaleZ, scaleY: Scale. Default is 1.
  - chance: Chance to spawn (from 0 to 1). Default is 1.
  - data: ZDO data override.
- applyRandomDamage (default: `false`): If true, pieces are randomly damaged.
- exteriorRadius: How many meters are cleared, leveled or no build. If not given for blueprints, this is the radius of the blueprint (+ 2 meters).
  - Note: Maximum suggested value is 32 meters. Higher values go past the zone border and can cause issues.
- clearArea (default: `false`): If true, vegetation is not placed within `exteriorRadius`.
- noBuild (default: `false`): If true, players can't build within `exteriorRadius`. If number, player can't build within the given radius.
- noBuildDungeon (default: `false`): If true, players can't build inside dungeons within the whole zone. If number, player can't build inside dungeons within the given radius.
- levelArea (default: `true` for blueprints): Flattens the area.
- levelRadius (default: half of `exteriorRadius`): Size of the leveled area.
- levelBorder (default: half of `exteriorRadius`): Adds a smooth transition around the `levelRadius`.
- paint: Paints the terrain. Format is `dirt,cultivated,paved,vegetation` (from 0.0 to 1.0) or a pre-defined color (cultivated, dirt, grass, grass_dark, patches, paved, paved_dark, paved_dirt, paved_moss).
- paintRadius (default: `exteriorRadius`): Size of the painted area.
- paintBorder (default: `5`): Adds a smooth transition around the `paintRadius`.
- centerPiece (default: `piece_bpcenterpoint`): Which object determines the blueprint bottom and center point. If the object is not found, the blueprint is centered automatically and placed 0.05 meters towards the ground.

## Dungeons

The file `expand_dungeons.yaml` sets dungeon generators. This is a server side feature, clients don't have access to this data.

Command `ew_dungeons` can be used to list available rooms for each dungeon.

- name: Name of the dungeon generator.
- algorithm: Type of the dungeon. Possible values are `Dungeon`, `CampGrid` or `CampRadial`.
- bounds (default: `64`): Maximum size of the dungeon in meters. Format is `x,z,y` or a single number for all directions.
  - Reasonable maximum is 3 zones which is 192 meters.
  - Note: Zone size is 64m x 64m. So values above that causes overflow to the adjacent zones.
  - Note: Dungeons have an environment cube that has 64 meter size. This is automatically scaled, unless running in the server side only mode.
- themes: List of available room sets. Possible values are `Crypt`, `SunkenCrypt`, `Cave`, `ForestCrypt`, `GoblinCamp`, `MeadowsVillage`, `MeadowsFarm`, `DvergerTown`, `DvergerBoss`. For example `MeadowsVillage,MeadowsFarm` would use both sets.
- maxRooms (default: `1`): Maximum amount of rooms. Only for Dungeon and CampRadial.
- minRooms (default: `1`): Minimum amount of rooms. Only for Dungeon and CampRadial.
- minRequiredRooms (default: `1`): Minimum amount of rooms in the required list. Only for Dungeon and CampRadial.
- requiredRooms: List of required rooms. Generator stops after required rooms and minimum amount of rooms are placed. Use command `ew_rooms` to print list of rooms.
- excludedRooms: List of rooms removed from the available rooms.
- roomWeights (default: `false`): Changes how rooms are randomly selected. Only for Dungeon.
  - If false, every room has the same chance to be selected (`weight` field is ignored).
  - If false, end cap is selected based on the highest `endCapPriority` field (`weight` field is not used).
- doorChance (default: `0`): Chance for a door to be placed. Only for Dungeon.
- doorTypes: List of possible doors. Each door has the same chance of being selected.
  - prefab: Identifier of the door object.
  - connectionType: Type of the door connection.
  - chance: Chance to be spawned if this door is selected. 
- maxTilt (default: `90` degrees): Maximum terrain angle. Only for CampGrid and CampRadial.
- perimeterSections (default: `0`): Amount of perimeter walls to spawn. Only for CampRadial.
- perimeterBuffer (default: `0` meters): Size of the perimeter area around the camp. Only for CampRadial.
- campRadiusMin (default: `0` meters): Minimum radius of the camp. Only for CampRadial.
- campRadiusMax (default: `0` meters): Maximum radius of the camp. Only for CampRadial.
- minAltitude (default: `0` meters): Minimum altitude for the room. Only for CampRadial.
- gridSize (default: `0`): Size of the grid. Only for CampGrid.
- tileWidth (default: `0` meters): Size of a single tile. Only for CampGrid.
- spawnChance (default: `1`): Chance for each tile to spawn. Only for CampGrid.
- interiorTransform (default: `false`): Some locations may require this being true. If you notice weird warnings, try setting this to true.
- objectData: List to set dungeon object data. Format is `id,data`.
  - id: Prefab name.
  - data: ZDO data override.
- objectSwap: Changes dungeon objects to other objects.
    - Use format `id, swapid` for a direct swap.
    - Use format `id, swapid1, swapid2, ...` for multiple possible outcomes.
    - Use format `id, swapid1:weight1, swapid2:weight2, ...` to control the chance of each outcome.
    - Empty id can be used to spawn nothing.
    - Blueprints are supported.
    - Use command `ew_rooms` to print room contents.

## Rooms

The file `expand_rooms.yaml` sets available dungeon rooms. This is a server side feature, clients don't have access to this data.

See [examples](https://github.com/JereKuusela/valheim-expand_world/blob/main/examples_room_blueprints.md).

New rooms can be created from blueprints or cloning an existing room by adding `:suffix` to the name.

- name: Name of the room prefab.
- theme: Determines in which dungeons this room can appear. See dungeons for available values.
- enabled (default `true`): Quick way to disable this room.
- entrance (default `false`): If true, this room is used only as the first room.
  - At least one entrance room is required. If multiple exist, one is randomly selected (`weight` field is never used).
  - One of the connections must be set as `entrance`. Even if multiple exist, the first one is used.
- endCap (default `false`): If true, this room is used to seal open ends at end of the generation.
  - These rooms should only have one connection, so that no open ends are left.
  - Each connection type should have a corresponding end cap, so that each connection can be sealed.
- divider (default `false`): If true, this room is used to seal mismatching connections.
  - The generator can create cycles so that two open connections are in the same position.
    - If the connection types are the same, nothing is done.
    - However if the types are different, a divider room is used to seal the connection.
  - These rooms should only have one connection, so that no open ends are left. The connection type doesn't matter.
  - These rooms should be very small (probably just a single wall).
- endCapPriority (default `0`): Rooms with a higher priority are attempted first, unless `roomWeights` is enabled.
- minPlaceOrder (default `0`): Minimum amount of rooms between this room and the entrance.
- weight (default: `1`): Chance of this room being selected (relative to other weights), unless `roomWeights` is disabled.
- faceCenter (default: `false`): If true, the room is always rotated towards the camp center. If false, the room is randomly rotated.
- perimeter (default: `false`): If true, this room is placed on the camp perimeter (edge).
- size: Format `x,z,y`. Size of this room in meters. Only integers.
  - Probably no reason to change this for existing rooms.
  - For blueprints, this is automatically calculated but recommended to be set manually.
- connections: List of doorways.
  - position: Format `posX,posZ,posY,rotY,rotX,rotZ` or `id` for blueprints. Position relative to the room.
    - If missing, the base room position is used.
    - For blueprints, this must be set. The easiest way is to mark the position with an object and use the `id`.
  - type: Type of the connection. Only connections with the same type are connected.
  - entrance (default: `false`): If true, used for the entrance.
  - door (default: `true`): If true, allows placing door. If `other`, allows placing door if the other connection also allows placing a door.
- objects: Extra objects or blueprints. Format is `id,posX,posZ,posY,rotY,rotX,rotZ,scaleX,scaleZ,scaleY,chance,data`.
  - id: Prefab name.
  - posX, posZ, posY: Offset from the location position. Defalt is 0.
  - rotY, rotX, rotZ: Rotation. Default is 0.
  - scaleX, scaleZ, scaleY: Scale. Default is 1.
  - chance: Chance to spawn (from 0 to 1). Default is 1.
  - data: ZDO data override.
- objectSwap: Changes room objects to other objects.
    - Use format `id, swapid` for a direct swap.
    - Use format `id, swapid1, swapid2, ...` for multiple possible outcomes.
    - Use format `id, swapid1:weight1, swapid2:weight2, ...` to control the chance of each outcome.
    - Empty id can be used to spawn nothing.
    - Blueprints are supported.
    - Use command `ew_rooms` to print room contents.
    - Note: If dungeon has object swaps, those are applied first.

## Vegetation

The file `expand_vegetations.yaml` sets the generated objects. This is a server side feature, clients don't have access to this data.

Changes only apply to unexplored areas. Upgrade World mod can be used to reset areas.

Note: Missing vegetation are automatically added to the file. To disable, set `enabled` to `false` instead of removing anything.

- prefab: Identifier of the object or name of blueprint file.
- enabled (default: `false`): Quick way to disable this entry.
- min (default: `1`): Minimum amount (of groups) to be placed per zone (64m x 64m).
- max (default: `1`): Maximum amount (of groups) to be placed per zone (64m x 64m). If less than 1, has only a chance to appear.
- forcePlacement (default: `false`): By default, only one attempt is made to find a suitable position for each vegetation. If enabled, 50 attempts are done for each vegetation.
- scaleMin (default: `1`): Minimum scale. Number or x,z,y (with y being the height).  
- scaleMax (default: `1`): Maximum scale. Number or x,z,y (with y being the height).
- scaleUniform (default: `true`): If disabled, each axis is scaled independently.
- randTilt (default: `0` degrees): Random rotation within the degrees.
- chanceToUseGroundTilt (default: `0.0`): Chance to set rotation based on terrain angle (from 0.0 to 1.0).
- biome: List of possible biomes.
- biomeArea: List of possible biome areas (edge = zones with multiple biomes, median = zones with only a single biome, 4 = unused from Valheim data).
- blockCheck (default: `true`): If enabled, clear ground is required.
- minAltitude (default: `0` meters): Minimum terrain altitude.
- maxAltitude (default: `1000` meters): Maximum terrain altitude.
- minOceanDepth (default: `0` meters): Minimum ocean depth (interpolated from zone corners so slightly different from `minAltitude`).
- maxOceanDepth (default: `0` meters): Maximum ocean depth (interpolated from zone corners so slightly different from `maxAltitude`).
- minVegetation (default: `0`): Minimum vegetation mask (random value from 0.0 to 1.0, only used in Mistlands biome).
- maxVegetation (default: `0`): Maximum vegetation mask (random value from 0.0 to 1.0, only used in Mistlands biome).
- minTilt (default: `0` degrees): Minimum terrain angle.
- maxTilt (default: `90` degrees): Maximum terrain angle.
- terrainDeltaRadius (default: `0` meters): Radius for terrain delta limits.
  - 10 random points are selected within this radius.
  - The altitude difference between the lowest and highest point must be within the limits.
- minTerrainDelta (default: `0` meters): Minimum terrain height change.
  - Higher values cause the vegetation to be based on slopes.
- maxTerrainDelta (default: `10` meters): Maximum terrain height change.
  - Lower values cause the vegetation to be based on flatter areas.
- snapToWater (default: `false`): If enabled, placed at the water level instead of the terrain level.
- snapToStaticSolid (default: `false`): If enabled, placed at the top of solid objects instead of terrain.
- groundOffset (default: `0` meters): Placed above the ground.
- groupSizeMin (default: `1`): Minimum amount to be placed per group.
- groupSizeMax (default: `1`): Maximum amount to be placed per group.
- groupRadius (default: `0` meters): Radius for group placement. This should be less than 32 meters to avoid overflowing to adjacent zones.
- inForest (default: `false`): If enabled, forest thresholds are checked.
  - This creates clusters of vegetation, instead of them being placed evenly.
  - Thresholds between 0 and 1.15 are shown as forest in the minimap.
  - Smaller values would be more closer to the forest center.
  - Larger values would be more away from the forest center.
- forestTresholdMin (default: `0`): Minimum forest value.
- forestTresholdMax (default: `0`): Maximum forest value.
- data: ZDO data override. For example to create hidden stashes with Spawner Tweaks mod (`object copy` from World Edit Commands).
- centerPiece (default: `piece_bpcenterpoint`): Which object determines the blueprint bottom and center point. If the object is not found, the blueprint is centered automatically and placed 0.05 meters towards the ground.

## Spawns

The file `expand_spawns.yaml` sets the spawned creatures.

See the [wiki](https://valheim.fandom.com/wiki/Spawn_zones) for more info.

If setting Zone Spawners is disabled, all spawns stop working from unexplored areas.

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
- overrideLevelupChance (default: `-1` percent): Chance per level up (from the default 10%).
- data: ZDO data override. For example to change faction with Spawner Tweaks mod (`object copy` from World Edit Commands).
- objects: Extra objects to spawn. Spawned on top of any obstacles. The spawning is skipped if 10 meters above the original position. Format is `id,posX,posZ,posY,chance,data`.
  - id: Prefab name.
  - posX, posZ, posY: Offset from the location position. Defalt is 0.
  - chance: Chance to spawn (from 0 to 1). Default is 1.
  - data: ZDO data override.

## Events

The file `expand_events.yaml` sets the boss and random events.

See the [wiki](https://valheim.fandom.com/wiki/Events) for more info.

If setting Zone Spawners is disabled, all spawns stop working from unexplored areas.

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

Lakes are needed to generate rivers. The code searches for points with enough water and then merges them to lake objects. Use command `ew_lakes` to show their positions on the map.

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
