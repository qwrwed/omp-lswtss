# GE-Proton8-21 Notes

## DXVK swap chain layout

The offset `+0x78` for `m_window` (HWND) derived from the DXVK v2.3 source tag is **wrong** for the GE-Proton8-21 binary (`v2.3-11-ge00db245`). Reading `Marshal.ReadIntPtr(swapChainNativeHandle, 0x78)` returns ASCII garbage, not an HWND. Do not read HWND from the swap chain object.

## Game window HWND

Get it from `PeekMessageA` keyboard messages - `lpMsg->hwnd` is always the focused window (the game window). Set `_currentWindowNativeHandle = lpMsg->hwnd` (when non-zero) in the keyboard handling block of the PeekMessageA hook.

## Keyboard message hwnd filter

Keyboard messages must bypass the `lpMsg->hwnd == _currentWindowNativeHandle` check. On Wine/Proton, the hwnd on keyboard messages does not reliably match whatever is stored in the swap chain object. Handle all `WM_KEYFIRST..WM_KEYLAST` messages regardless of hwnd.

## SharpDX COM reference counting

`new SharpDX.DXGI.SwapChain(nativePointer)` does **not** call `AddRef`. Its finalizer calls `Release`. Call `Marshal.AddRef(nativePointer)` before constructing any SharpDX wrapper that will be stored beyond the current scope, otherwise the wrapper's eventual `Release` will drop the game's only COM reference and destroy the swap chain on the next frame.

## White screen root cause

The `+0x10` null check in `GetOrCreateCurrentRenderTargetView` was checking `DxgiObject::m_privateData._M_start` - the first pointer of an empty `std::vector`, which is always null in DXVK v2.3 because no game calls `SetPrivateData` on its swap chains. The check was intended to detect "internal presenter nulled during fullscreen transition" but the offset and comment were wrong for DXVK v2.3. Removed the check.

## Working state (2026-06-19)

- Game launches without crashing
- F1 opens the Galaxy Unleashed overlay menu
- Mouse is captured by the overlay in menu mode (camera does not move)
- Touchpad controls the overlay cursor in menu mode
- Esc closes the overlay and returns to play mode

---

## libcef.dll binary mismatch

The `libcef.dll` shipped by the `cef.redist.x64` NuGet package (pulled in via `CefSharp.OffScreen.NetCore 131.3.10`) has a **different binary** than the one in the upstream release bundle. Both files are 226 455 552 bytes but have different hashes:

- Upstream bundle (works under Wine/Proton): `90f4fa80cad751723a0c8fe77cc264d6`
- NuGet restore (crashes under Wine/Proton): `56f653a9e4ea992782b918341d12efa8`

## libcef.dll crash symptom

Game process exits ~4 seconds after launch; no window ever appears. Steam console log shows all mod assemblies loading cleanly through `CefSharp.Core.dll`, then the process dies when the runtime requests `CefSharp.Core.Runtime` (which P/Invokes into `libcef.dll` natively).

## libcef.dll root cause in BuildOverlay1.cs

The old two-stage build deleted `dist/overlay1/` before copying from the `--arch x64` output. This destroyed the upstream-seeded `libcef.dll` and replaced it with the NuGet-restored one.

## libcef.dll fix (2026-06-20)

`BuildOverlay1.cs` no longer deletes the dist directory before building. It overlays new managed files on top of whatever is already there. Native CEF files (`libcef.dll`, `chrome_elf.dll`, `libEGL.dll`, `libGLESv2.dll`, `d3dcompiler_47.dll`, `dxcompiler.dll`, `dxil.dll`, `icudtl.dat`, `.pak` files, `Ijwhost.dll`) are copied from the NuGet build **only when absent** - i.e. as a fallback for local dev builds that have no upstream seed. When running via `package-release.ps1` the seed is present and those files are preserved.

## Current upstream bundle also has the bad libcef.dll

As of 2026-06-20, the upstream release bundle (seeded by `package-release.ps1`) also ships the bad `libcef.dll`. Neither the upstream bundle nor the NuGet restore is safe to use.

The good `libcef.dll` lives in `dist/overlay1/libcef.dll` on the dev SD card (hash `90f4fa80`). It must be kept there and never overwritten.

`package-release.ps1` was updated to skip native CEF files already present in `dist/overlay1/` during the seeding step, so repeated release builds preserve the good version automatically.

**If building on a new machine**: copy the good `libcef.dll` (hash `90f4fa80`, 226 455 552 bytes) into `dist/overlay1/libcef.dll` before running the release script. The backup on the Deck is at `~/mod-backup/mods/overlay1/libcef.dll`.

## libcef.dll rule of thumb

Never let `dotnet build` or the upstream bundle seeding overwrite `dist/overlay1/libcef.dll`. Both produce a version that crashes under Wine/Proton.
