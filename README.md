# Open Modding Platform for LEGO Star Wars: The Skywalker Saga

## Support

At this moment, discussion about all project's related stuff is held on [**TTGames LEGO Modding Discord Server**](https://discord.gg/9gYXPka). Feel free to join and find the discussion in `modding_forum/Open Modding Platform - LSW:TSS (Development Updates)` thread.

## Installation

OMP runs on a **clean installation** of the game. The game files must **not** be extracted and the game must **not** be patched - a fresh install from Steam or EGS works as-is.

1. Download the latest release archive from the [Releases page](https://github.com/qwrwed/omp-lswtss/releases/latest).
2. Extract its **entire contents** into the game's root directory - the folder that contains `LEGOSTARWARSSKYWALKERSAGA_DX11.exe`. After extracting, that folder should contain (alongside the game's own files):
   - `dinput8.dll`
   - `omp-lswtss-driver.dll`
   - `omp-lswtss-driver-config.json`
   - `omp-lswtss-runtime-engine-<version>/`
   - `mods/` - one subfolder per mod, each containing a `mod.json`
3. Launch the game normally from Steam or EGS. OMP loads automatically via `dinput8.dll`; no patching or launcher is required.

To uninstall, delete the files and folders listed in step 2 from the game directory.

### Multi-GPU (laptop) note

On systems with two GPUs (e.g. a laptop with integrated + discrete graphics), mods that draw an on-screen overlay (such as Galaxy Unleashed) need the game **and** Chromium's helper process to run on the **same** GPU - otherwise the overlay's GPU texture can't be shared and the overlay won't appear.

If a mod's overlay doesn't show up, set both of these executables to the **same** GPU in Windows → Settings → System → Display → Graphics (add each app, choose "High performance"):

- `LEGOSTARWARSSKYWALKERSAGA_DX11.exe` (in the game's root directory)
- `CefSharp.BrowserSubprocess.exe` (under `mods/overlay1/`)

## Contributing

### Requirements

- [Rust](https://www.rust-lang.org/tools/install)
- [.NET SDK](https://dotnet.microsoft.com/en-us/download)
- [NodeJS](https://nodejs.org/en/download/package-manager)
- [Yarn Classic](https://classic.yarnpkg.com/lang/en/docs/install)
- [LEGO Star Wars: The Skywalker Saga (Steam Version)](https://store.steampowered.com/app/920210/LEGO_Star_Wars_The_Skywalker_Saga/)
- [LEGO Star Wars: The Skywalker Saga (EGS Version)](https://store.epicgames.com/en-US/p/lego-star-wars-the-skywalker-saga)

### Building

Navigate to `workspaces/dotnet/dev-tools` and execute:
```sh
dotnet run -- build-all [PATH_TO_STEAM_GAME_DIRECTORY] [PATH_TO_EGS_GAME_DIRECTORY]
```

### Testing

Navigate to `workspaces/dotnet/dev-tools` and execute:
```sh
dotnet run -- install-all [PATH_TO_GAME_DIRECTORY]
```

Now, start the game from Steam/EGS.

For Steam version, you can use this command to inspect process stdout:
```sh
dotnet run -- run-steam-game [PATH_TO_STEAM_GAME_DIRECTORY]
```

### VSCode Integration

1. Install recommended VSCode Extensions
2. Create `.vscode/tasks.local.json` and add entries to `inputs` field, example:
```json
{
  "version": "2.0.0",
  "inputs": [
    {
      "id": "gameDirPath",
      "description": "gameDirPath",
      "type": "pickString",
      "options": [
        "PATH_TO_STEAM_GAME_DIRECTORY",
        "PATH_TO_EGS_GAME_DIRECTORY",
      ]
    },
    {
      "id": "steamGameDirPath",
      "description": "steamGameDirPath",
      "type": "pickString",
      "options": [
        "PATH_TO_STEAM_GAME_DIRECTORY",
      ]
    },
    {
      "id": "egsGameDirPath",
      "description": "egsGameDirPath",
      "type": "pickString",
      "options": [
        "PATH_TO_EGS_GAME_DIRECTORY",
      ]
    }
  ]
}
```
3. Use VSCode Tasks to perform various actions (building, installing etc.)

## Special Thanks

To all members of **TTGames LEGO Modding Discord Server** for unparallel help and guidance ❤️
