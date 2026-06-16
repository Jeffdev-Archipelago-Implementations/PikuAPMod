# Pikuniku Archipelago Mod (PikunikuAPMod)

Archipelago.gg client mod for Pikuniku, built with BepInEx 5 + Harmony +
[Archipelago.MultiClient.Net](https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net).
Same structure as UnfairFlipsAPMod / SkulAPMod.

## Project layout

| File | Purpose |
| --- | --- |
| `PikunikuAPMod.cs` | BepInEx plugin entry point - wires up all handlers |
| `ArchipelagoHandler.cs` | AP session: connect/login, location check queue, item receive, message log, DeathLink |
| `ConnectionUI.cs` | F1 IMGUI connect window (host/port/slot/password, remembers last connection) |
| `LoginHandler.cs` | Creates/destroys the connection UI |
| `ItemHandler.cs` | Receives items, caches them until the game is ready, applies effects (TODO) |
| `GameHandler.cs` | Game-side logic: Harmony patches, location checks, DeathLink kill (TODO) |
| `SlotData.cs` | Parses slot_data options from the apworld (TODO: add your options) |
| `SaveDataHandler.cs` | Per seed+slot JSON save in `./ArchipelagoSaves` (tracks ItemIndex etc.) |
| `FileWriter.cs` | Persists last connection info |
| `ArchipelagoConstants.cs` | Game name, base ID, item/location ID constants (TODO) |
| `Log.cs` | BepInEx logger wrapper |
| `Package/` | Thunderstore package (manifest.json, README, icon.png - add a 256x256 icon) |

## Setup

Pikuniku runs Unity 2017.4.5f1 on the legacy Mono runtime (CLR 2.0.50727), so this
project targets **net35**. Unity references come straight from the game's own
`Pikuniku_Data/Managed` folder, and the net35 build of Archipelago.MultiClient.Net
bundles `Newtonsoft.Json.dll` + `websocket-sharp.dll`, which ship with the plugin.

1. Install BepInEx 5 (x64) into the Pikuniku game folder and run the game once (already done if `BepInEx/LogOutput.log` exists).
2. If Steam isn't at `C:\Program Files (x86)\Steam`, copy `.env.example` to `.env` and set `STEAM_PATH`.
3. `dotnet build` - the post-build step packages everything into `Package/` and copies the
   plugin straight into `BepInEx/plugins/PikunikuAPMod` for quick testing, plus zips
   Thunderstore (`output/Pikuniku_Archipelago_TS.zip`) and plain (`output/PikunikuAPMod.zip`) packages.

### net35 notes

- No `ConcurrentQueue`/value tuples - the location queue uses a locked `Queue<long>`,
  the item cache uses `KeyValuePair`.
- No `ConnectAsync`/`LoginAsync` - `ArchipelagoHandler.Connect()` uses the synchronous
  `TryConnectAndLogin`, and scouting (`ScoutLocation`) is callback-based.

## Hot reload (dev only)

[UnityHotReload](https://github.com/xiaoxiao921/UnityHotReload) (MIT) is vendored in
`HotReload/`, retargeted to net35. In **Debug** builds only:

1. Launch the game (the post-build step puts `UnityHotReload.dll` in `BepInEx/plugins/UnityHotReload`).
2. Change code, rebuild (`dotnet build` or IDE build) while the game runs.
3. Press **F2** in-game - the freshly built `PikunikuAPMod.dll` is loaded in place.

Limitations (from upstream): method bodies, new methods, and new classes are fine, but
don't add/remove/change **fields** on existing types - runtime state stays bound to the
original type definitions. Release builds contain none of this.

## Where to start

1. Decompile `Pikuniku_Data/Managed/Assembly-CSharp.dll` with dnSpy/ILSpy to find the
   game's classes (player, level/progression, UI message systems).
2. Fill in `ArchipelagoConstants.cs` with your base ID + location IDs (match the apworld).
3. Define items in `ItemHandler.cs` (`PikunikuItem` enum + `ProcessItem` switch).
4. Add Harmony patches in `GameHandler.cs` that call
   `PikunikuAPMod.ArchipelagoHandler.CheckLocation(...)` when the player does things.
5. Add your apworld's options to `SlotData.cs`.
6. Hook `GameHandler.ShowMessage` into the game's UI so AP chat/log shows in-game.
7. Call `PikunikuAPMod.ArchipelagoHandler.Release()` when the goal is completed.
