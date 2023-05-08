# Dungeon room blueprint examples

This example shows how to build a custom dungeon entirely from blueprints. It is important to do everything exactly as shown here. Any mistake can make the dungeon not work properly.

Used [blueprints](https://github.com/JereKuusela/valheim-expand_world/blob/main/room_blueprints.zip).

## General setup

1. Download [Infinity Hammer](https://valheim.thunderstore.io/package/JereKuusela/Infinity_Hammer/) mod (and dependencies).
2. Download [Upgrade World](https://valheim.thunderstore.io/package/JereKuusela/Upgrade_World/) mod.
3. Setup commands for easier usage:
 - `alias rs zones_reset zone start force` so that you can just use `rs` command to regenerate the current zone.
 - `alias door hammer dungeon_forestcrypt_door` to select the door more easily.
 - `alias center hammer GlowingMushroom scale=0.25` to select the center piece more easily.
 - `alias base hammer Ice_floor scale=$$,$$,0.25` to select the base more easily.
4. Use command `find Crypt2` to find the nearest crypt and go inside.


## Creating the entrance

1. Use `base 1,1` and place the ice floor. The floor is intended to give structural stability and a surface to build on.
  - Many other objects work for this as well.
  - If you are up in the air, hit Numpad 0 to freeze the placement. Your console probably gets filled with "Look rotation viewing vector is zero" but that is ok.
2. Build your room on top of it, for example 4 wood floors and some 1x1 walls. Leave 2 meter gap for the doors.
3. Use `door` to place connection markers (if snapping doesn't work at all, update Infinity Hammer mod).
  - Technically any object works but most objects don't have the origin point at the bottom.
  - Rotate the door so that the small red direction marker is pointing towards the room.
  - If rotated the correctly, the door handle should be on the right side when viewed from the room (and left side when viewed from the outside).
4. Use `center` to place the center marker.
  - Place it close to the middle of the room. The collision checks are done from this point.
  - Rotate the marker so that the small red direction marker aligns with the room shape.
5. Use Area Pipette to select your entire room and save it with `hammer_save Entrance GlowingMushroom`.
  - You can change the default center piece from the Infinity Hammer config so you don't have to put it manually every time.


## Creating the dungeon

1. Determine the size of your room.
  - Expand World tries to calculate the size automatically but it's not reliable.
  - Too small size means the rooms will overlap with each other.
  - Too big size means that the dungeon fails to generate because rooms take too much space.
2. Add to `expand_rooms.yaml`: 
```
- name: Entrance
  entrance: true
  theme: Crypt
  # 2 meters each: 2 floors, 2 floors, 1 wall
  size: 4,4,2
  connections:
   - position: dungeon_forestcrypt_door
     entrance: true
   - position: dungeon_forestcrypt_door
   - position: dungeon_forestcrypt_door
   - position: dungeon_forestcrypt_door
```

1. Edit `expand_dungeons.yaml` and find `DG_ForestCrypt`.
2. Changes `themes: ForestCrypt` to `themes: Crypt`.
3. Use `rs` to reset the zone. If everything is done correctly, you should see your room appear in front of the glowing entrance.


## Corridor

1. Use `base 1,0.5` to create a small corridor. For example with 2 wood floors and doors on both ends.
2. Use `door` to place connection markers.
  - Use Alt + Right Arrow to position the door at middle fo the floor.
3. Use `center` to place the center marker.
  - Check the marker direction. This is needed when determining the room size.
4. Area select and save with name Path.
5. Add to `expand_rooms.yaml`: 
```
- name: Path
  theme: Crypt
  # 1 floor perpendicular to the center piece direction
  # 2 floors towards the center piece direction
  # 1 wall
  size: 2,4,2
  connections:
   - position: dungeon_forestcrypt_door
   - position: dungeon_forestcrypt_door
```
6. Use `rs`, if everything is good your dungeon should now have straight corridors.


## End cap

1. Use `base 0,5.0,5`, `door` and `center` to create a room with a single door.
2. Add to `expand_rooms.yaml`: 
```
- name: End
  endCap: true
  theme: Crypt
  # Use zero size so that end caps always fit and can seal the dungeon.
  size: 0,0,2
  connections:
   - position: dungeon_forestcrypt_door
```
3. Use `rs`, if everything is good your dungeon should no longer have empty door ways.


## Corner

1. Use `base 0,5.0,5`, `door` and `center` to create a room with two doors, adjacent to each other.
2. Add to `expand_rooms.yaml`: 
```
- name: Corner
  theme: Crypt
  size: 2,2,2
  connections:
   - position: dungeon_forestcrypt_door
   - position: dungeon_forestcrypt_door
```
3. Use `rs`, if everything is good your dungeon should have more complicated geometry.
  - There might be rooms overlapping each other because rooms have very similar size compared to the end cap.
  - You can try making a bigger corner room to see if it fixes the problem.
  - Or an end cap that is just a single wall.


## Stairs

1. Use `base 1.0,5`, `door` and `center` to create a room with two stairs and two doors at tne ends.
2. Add to `expand_rooms.yaml`: 
```
- name: Stairs
  theme: Crypt
  size: 2,4,2
  connections:
   - position: dungeon_forestcrypt_door
   - position: dungeon_forestcrypt_door
```
3. Use `rs`, if everything is good your dungeon should have paths going up and down.

