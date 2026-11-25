# Necrosmith 2 — Memory Leak Fix

Necrosmith 2 has a well-documented memory leak: the game slowly accumulates RAM over a session until the system runs out of memory and either crashes or kills the process. The developer (Alawar) has not addressed this despite community reports going back to 2024.

This BepInEx plugin forces the IL2CPP garbage collector to run every 60 seconds, preventing the unbounded memory growth.

---

## Requirements

- [BepInEx 6 (IL2CPP build)](https://github.com/BepInEx/BepInEx/releases) — **not** BepInEx 5

---

## Installation — Windows

1. Download and extract BepInEx 6 (Windows x64 IL2CPP) into the game folder:
   `Steam\steamapps\common\Necrosmith 2\`
2. Run the game once — BepInEx will generate interop files (takes ~2 min, normal)
3. Copy `Necrosmith2MemoryFix.dll` into:
   `Steam\steamapps\common\Necrosmith 2\BepInEx\plugins\`
4. Launch the game — confirm the fix is running by checking:
   `Steam\steamapps\common\Necrosmith 2\BepInEx\LogOutput.log`
   You should see `[MemoryFix] Loaded` on startup and `[MemoryFix] GC complete` every 60 seconds.

---

## Installation — Linux / Steam Deck (Proton)

1. Download and extract BepInEx 6 (Windows x64 IL2CPP) into the game folder:
   `~/.steam/steam/steamapps/common/Necrosmith 2/`
2. In Steam → Necrosmith 2 → Properties → Launch Options, add:
   ```
   WINEDLLOVERRIDES="winhttp=n,b" %command%
   ```
3. Run the game once to let BepInEx generate interop files (~2 min, normal)
4. Copy `Necrosmith2MemoryFix.dll` into:
   `~/.steam/steam/steamapps/common/Necrosmith 2/BepInEx/plugins/`
5. Launch and confirm in `BepInEx/LogOutput.log`

---

## How it works

Necrosmith 2 is a Unity IL2CPP game. The leak is in the IL2CPP managed heap — objects are allocated for minions and enemies but never freed. This plugin calls `il2cpp_gc_collect(-1)` (a full IL2CPP GC cycle) plus `System.GC.Collect()` every 60 seconds, releasing the accumulated memory before it can destabilise the system.

Memory usage is logged to `BepInEx/LogOutput.log` so you can confirm it's working.

---

## Building from source

```bash
cd src
dotnet build -c Release
```

Requires .NET SDK 6+ and the BepInEx NuGet feed (configured in `NuGet.Config`).

---

## Credits

Discovered and fixed by [FloopDaPig](https://github.com/FloopDaPig).
