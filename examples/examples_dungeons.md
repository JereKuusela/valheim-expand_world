# Dungeon examples

It is important to understand which part of the dungeon should be changed.

Dungeons consist of three parts:
- The location that spawns the dungeon generator.
  - This includes the dungeon entrance, the white exit portal and the black environment cube.
  - Any changes to the entrances or the environment must be applied to the location.
- The actual dungeon generator that spawns the rooms.
  - While rooms are spawned separately, the static parts are saved as part of the generator.
  - Any changes to the static parts like vegvisirs must be applied to the dungeon generator.
- Other objects spawned by the location and by the rooms.
  - These can be manipulated with object swaps and object data fields.

## TODO: Dungeon examples

TODO

## Structure Tweaks mod

Required mods: 
- [Structure Tweaks](https://valheim.thunderstore.io/package/JereKuusela/Structure_Tweaks/) is required for all clients.
- [World Edit Commands](https://valheim.thunderstore.io/package/JereKuusela/World_Edit_Commands/) is required for the admin.
  - Check the documentation for the commands and data keys (at bottom of the file).

### Edit dungeon texts

1. Use `tweak_dungeon enter_text=Magical_Place` on the dungeon entrance.
2. Copy the data with `object copy=override_dungeon_enter_text`
  - Note: If you have done other changes, remember to include their data keys as well.
3. Apply the data to the location.
```
- prefab: Crypt2
  data: EAAAAAFw+521DU1hZ2ljYWwgUGxhY2U=
```

### Edit dungeon weather

1. Use `tweak_dungeon weather=Ashrain` on the dungeon entrance.
2. Copy the data with `object copy=override_dungeon_weather`
  - Note: If you have done other changes, remember to include their data keys as well.
3. Apply the data to the location.
```
- prefab: Crypt2
  data: TODO
```

### Edit dungeon vegvisirs

1. Find a vegvisir in the dungeon.
2. Use `tweak_runestone name=Find discover=AbandonedLogCabin02,Cabin!,Bed` on the vegvisir.
3. Copy the data with `object copy=override_discover`
  - Don't use just `object copy` because it copies all data.
  - For dungeons this would include the room layout.
  - Only a pre-existing runestone wouldn't have any extra data.
4. Apply the data to the dungeon generator object.
5. You can also change the "Register" text with `tweak_runestone name=Find discover=AbandonedLogCabin02,Cabin!,Bed`.
  - The display text would be "Find Cabin!".
  - Copy the data with `object copy=override_name,override_discover`
```
- prefab: Crypt2
  objectData:
    # This must id of the default dungeon object (even if you have changed the dungeon generator).
    - DG_ForestCrypt, EAAAAAJ4MbkQHUFiYW5kb25lZExvZ0NhYmluMDIsQ2FiaW4sQmVkCE8E2AdDYWJpbnMh
```
