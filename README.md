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

`build-all` requires both game versions because the `c-api1` component scrapes each version at build time. Every other component builds with **no game directory**, so you can build them all (everything `build-all` does except `c-api1`) with:
```sh
dotnet run -- build-all-no-c-api1
```
or build a single component individually, e.g.:
```sh
dotnet run -- build-overlay1
```

### Releasing without the EGS version

`build-all` needs both game versions only because of the `c-api1` component: it launches each game version and scrapes its native API at build time. Every other component compiles from source with **no game directory**. So if your changes don't touch `c-api1`, you can cut a release with only the Steam version by rebuilding the components you changed and swapping them into the latest existing release bundle (which already contains an upstream-built `c-api1`).

The script `scripts/package-release.ps1` automates this (requires `gh`, authenticated):
```sh
./scripts/package-release.ps1 -Tag v0.5.1 -PublishRepo <owner>/<repo> -Publish
```
`-PublishRepo` is required whenever `-Publish` is used, so the release can only go to a repo you name explicitly. Omit `-Publish` to just build the bundle locally (`omp-release.zip`, a complete installable release) without publishing - e.g. to test it in a game first. Use `-OutZip <name>.zip` to name it.

The manual equivalent:

1. Rebuild every non-`c-api1` component with `dotnet run -- build-all-no-c-api1` (or just the component you changed, e.g. `dotnet run -- build-overlay1`). Output goes to `dist/<component>/`.
2. Download the latest release archive from the upstream [Releases page](https://github.com/open-modding-platform/omp-lswtss/releases/latest) and extract it.
3. In the extracted bundle, replace each changed component's file(s) under `mods/<component>/` (or the `omp-lswtss-runtime-engine-<version>/` folder for the runtime engine) with your rebuilt one(s).
4. Zip the bundle's contents (any archive tool works) and publish it as a release with the [GitHub CLI](https://cli.github.com/):
   ```sh
   gh release create <tag> <bundle>.zip --title "<title>" --notes "<notes>"
   ```
   Add `--repo <owner>/<repo>` if the repository has more than one remote (otherwise `gh` reports "no default remote repository has been set").

This reuses the upstream-built `c-api1` and any components you didn't change, so the EGS version is never needed.

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
2. Create `.vscode/tasks.local.json` with an `inputs` field. Replace each `PATH_TO_*` placeholder below with your game folder (the one with `LEGOSTARWARSSKYWALKERSAGA_DX11.exe`), listing only the version(s) you own. The four inputs:
   - `gameDirPath` - any version
   - `steamGameDirPath` / `egsGameDirPath` - that specific version
   - `devGameDirPath` - the install you develop against (`dev-*` tasks)

   Example:
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
    },
    {
      "id": "devGameDirPath",
      "description": "devGameDirPath",
      "type": "pickString",
      "options": [
        "PATH_TO_GAME_DIRECTORY_TO_DEVELOP_AGAINST",
      ]
    }
  ]
}
```
3. Use VSCode Tasks to perform various actions (building, installing etc.)

## Special Thanks

To all members of **TTGames LEGO Modding Discord Server** for unparallel help and guidance ❤️
