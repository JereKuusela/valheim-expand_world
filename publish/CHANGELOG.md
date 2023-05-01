- v1.36
  - Adds missing vegetation entries automatically to the `expand_vegetation.yaml` file.
  - Changes the location distance meter threshold from 1.0 to 2.0 meters (now only values higher than 2.0 are considered as meters instead of relative to the world size).
  - Fixes conflict with mods that cause client side dungeon regeneration.
  - Fixes the chance field not working on blueprint files.

- v1.35
  - Improves suppport for massive locations (radius larger than 32 meters).
  - Fixes the water not rendering correctly past the normal world limits.

- v1.34
  - Adds a new field `groundOffset` to the `expand_locations.yaml`.

- v1.33
  - Adds a new command `ew_dungeons` to print their available rooms.
  - Adds a blueprint support to the field `objectSwap` in the `expand_locations.yaml`.
  - Adds a new field `centerPiece` to the `expand_vegetation.yaml`.
  - Adds a new field `objects` to the `expand_rooms.yaml`.
  - Changes dungeon room names to case insensitive in the `expand_dungeons.yaml` file.
  - Fixes the field `excludeRooms` in the `expand_dungeons.yaml` not working.
  - Fixes Jewelcrafting boss icons not working.
  - Removes the field `offset` from the `expand_locations.yaml`.
  - Removes network traffic when in "Server side only" mode.

- v1.32
  - Adds missing location entries automatically to the `expand_locations.yaml` file.
  - Fixes configs not loading correctly on the first run.
