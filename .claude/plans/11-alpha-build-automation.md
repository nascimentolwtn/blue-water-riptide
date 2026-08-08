# Plan 11 — Alpha Build Automation

Scripts and conventions for a repeatable build → install → launch → verify loop for the LAN alpha, plus where generated/sourced art and audio (from `09-graphics-and-alpha-build.md`) actually land in the asset pipeline. Builds on the headless build path that already exists (`Assets/Editor/BuildTools/BuildScript.cs`, `RenderPipelineSetup.cs`, napkin Execution & Validation #6) rather than inventing a new one — this plan extends that loop, it doesn't replace it.

## 0. What already exists (don't rebuild it)

- `Assets/Editor/BuildTools/BuildScript.cs` — `BuildAndroidDevelopment()`, invokable via `-executeMethod BlueWaterRiptide.EditorTools.BuildScript.BuildAndroidDevelopment` or the `Tools > Blue Water Riptide > Build Android (Development)` menu. Runs `RenderPipelineSetup.EnsureUrpPipelineAsset()` first, then builds `Assets/Scenes/Boot.unity` → `Builds/Android/blue-water-riptide.apk` with `BuildOptions.Development`. Needs the Editor GUI fully closed first (project-lock rule, same as batch-mode compiling).
- `Assets/Editor/BuildTools/RenderPipelineSetup.cs` — idempotent URP pipeline-asset setup, safe to re-run.
- The napkin's existing manual loop (Execution & Validation #6): build → `adb install -r` → `adb shell am start -n com.libuyitservices.bluewaterriptide/com.unity3d.player.UnityPlayerGameActivity` → `adb logcat` → `adb exec-out screencap -p` (readable directly as an image). This plan's job is to **script that sequence**, not redesign it.
- **No CI/CD exists yet** (`.github/workflows/` is empty) — see §5 for why that stays true for now.

## 1. Compile verification (blocker — do this first, before any build)

**Status update 2026-08-07**: The codebase landed a large batch of [File] work (AI, LAN, Progression, ability behaviors) on 2026-08-07 that was reviewed for internal consistency but **never compiled** (no Unity Editor available that session). Per napkin item 7 (Execution & Validation):
- **First action**: Open the repo in Unity Editor and check the Console for compile errors in:
  - `Assets/Scripts/AI/` (AIInputDriver, AITeamBuilder, AISquadBrain, SinglePlayerLauncher, AIPerception, AIBehaviorProfile, etc.)
  - `Assets/Scripts/Networking/` (LanSessionAdvertiser, LanSessionScanner, LobbyState, NetworkInputDriver, NetworkBootstrap, etc.)
  - `Assets/Scripts/Core/ProgressionService.cs` and `Core/Modes/` (progression, SinglePlayerLauncher dependency)
  - `Assets/Scripts/Gameplay/Combat/` (WallSpawnAbilityBehavior, DashAbilityBehavior — new ability primitives)

- Fix any compile errors (likely missing `using` statements, type mismatches against actual package APIs like NGO/UTP if the source was reviewed offline, or obsolete references).
- Once Console is clean, none of the code below is blocked — proceed to §2.

## 2. End-to-end automation script

Goal: one command that builds, installs, launches, and captures proof-of-life (a screenshot + a logcat tail) with zero manual Editor/device steps, matching napkin's existing loop but as a single script instead of a remembered command sequence. **Do this after verifying compile succeeds.**

**`scripts/build_and_verify.ps1`** (new, PowerShell — matches the project's Windows dev environment):

```powershell
param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.5.4f1\Editor\Unity.exe",
    [string]$ProjectPath = (Resolve-Path "$PSScriptRoot\.."),
    [string]$Avd = "Pixel_9_API_35",
    [switch]$SkipEmulatorStart
)

# 1. Ensure emulator is up (napkin: check `adb devices`, start if not present)
$devices = adb devices
if (-not ($devices -match "device$") -and -not $SkipEmulatorStart) {
    Start-Process emulator -ArgumentList "-avd $Avd" -NoNewWindow
    # Poll (Monitor-style until-loop) until `adb devices` shows a booted device, timeout ~120s
}

# 2. Headless build — Editor GUI must be closed; this script assumes CI/local discipline enforces that
& $UnityPath -batchmode -quit -projectPath $ProjectPath `
    -executeMethod BlueWaterRiptide.EditorTools.BuildScript.BuildAndroidDevelopment `
    -logFile "$ProjectPath\Builds\Android\build.log"
if ($LASTEXITCODE -ne 0) { Write-Error "Build failed — see build.log"; exit 1 }

# 3. Install
adb install -r "$ProjectPath\Builds\Android\blue-water-riptide.apk"

# 4. Launch
adb shell am start -n com.libuyitservices.bluewaterriptide/com.unity3d.player.UnityPlayerGameActivity

# 5. Give the app a moment to reach the M1 bootstrap / Boot scene, then capture proof-of-life
Start-Sleep -Seconds 8
adb exec-out screencap -p > "$ProjectPath\Builds\Android\verify_screenshot.png"
adb logcat -d | Select-String -Pattern "BWR_|Exception|FATAL" | Out-File "$ProjectPath\Builds\Android\verify_logcat.txt"

Write-Host "Build, install, launch complete. Screenshot: Builds/Android/verify_screenshot.png"
```

- **File destination**: `scripts/build_and_verify.ps1` (new top-level `scripts/` folder — the repo doesn't have one yet; matches where `.claude`/tooling scripts conventionally live outside `Assets/`).
- **Notes on the `BWR_` log tags**: the codebase already tags its own diagnostic logs this way (`BWR_SAVE`, `BWR_BOOT_FAILED`, `BWR_BUILD_OK`/`BWR_BUILD_FAILED`, `BWR_SETUP_OK` — confirmed in `M1PrototypeBootstrap.cs`, `BuildScript.cs`, `RenderPipelineSetup.cs`). The verify script's log filter reuses this existing convention rather than inventing a new one — **extend this pattern**: any new automation-relevant log line (LAN connection success/failure, once `NetworkBootstrap` compiles successfully per §1) should also use a `BWR_` prefix so this same filter catches it for free.
- **Two-instance LAN smoke test** (once LAN code compiles successfully per §1): extend this script with a `-Mode Host|Client` parameter and a second invocation pointed at a second emulator instance or a Standalone headless build on loopback (`10` §3 already flags this as the right automatable slice — host+client connecting over `127.0.0.1` with different ports, verifiable via logcat/stdout without a human). Don't attempt to automate real Wi-Fi discovery or two-physical-device soak testing — `10` §3 is explicit that those need a human.

## 3. Screenshot capture as a build gate (optional but cheap)

Since `adb exec-out screencap -p` is already a known-working step (napkin item 6), wire it into the script above as a lightweight regression signal: diff the new screenshot's file size/dimensions against the previous run's (a crash-to-black-screen or a stuck splash typically produces a suspiciously uniform image) — not a full visual-diff pipeline, just a cheap smoke check that *something* rendered. Skip building actual image-diff tooling unless a real regression slips through this simple check first; don't over-engineer ahead of a demonstrated need.

## 4. Asset pipeline automation — where generated/sourced assets land

Ties directly to `09-graphics-and-alpha-build.md`'s file-destination notes; this section is the "how it gets from a downloaded/generated file into the build" mechanics, plus naming conventions so automation (and the next person) can find things predictably.

### Naming convention

`<Category>_<SailorOrArenaOrSystem>_<Variant>.<ext>`, matching the pattern `06-splash-screen-and-icon.md` already established (`Splash_Bell_Logo.png`, `Icon_Foreground.png`) and `09` §3b's SFX table (`SFX_JumpServe_Fire.wav`). Examples already decided in `09`: `SFX_AnchorSlam_Impact.wav`, `Menu_Loop.ogg`, `Combat_Loop.ogg`. Apply the same pattern going forward — no per-artist naming freelancing, since inconsistent names are exactly what breaks "find the asset for X" automation later (e.g. a future Addressables pass, per `01`'s M4 milestone note).

### Where each category is injected

| Asset category | Source (per `09`) | Destination folder | Import/wiring step |
|---|---|---|---|
| Sailor 3D models | Marketplace / Tripo | `Assets/Art/Characters/<SailorName>/` → assembled prefab in `Assets/Prefabs/Characters/` | [Editor/Asset]: import settings (Model tab: mobile-appropriate mesh compression, rig type Humanoid if Mixamo-sourced), material URP remap, prefab assembly with `SailorPawn`/eventual `NetworkSailorState` components attached |
| Sailor portraits | Gemini | `Assets/Art/UI/Portraits/` | [Editor/Asset]: Texture Type = Sprite (2D and UI), appropriate compression (ASTC for Android per napkin's platform notes) |
| Arena geometry/props | Marketplace / Tripo / hand-modeled | `Assets/Prefabs/Environment/` (+ `Props/` subfolder) | [Editor/Asset]: collider verification (napkin's `CreatePrimitive`/IL2CPP Collider-stripping gotcha — check `Assets/link.xml` if any prop uses a primitive placeholder), prefab assembly |
| App icon / splash | Gemini | `Assets/Art/UI/Icon/`, `Assets/Art/UI/Splash/` (already scaffolded per `06`) | [Editor/Asset]: Player Settings → Icon/Splash Screen panel assignment, per `06` §2/§3 |
| HUD/meta icons | Gemini | `Assets/Art/UI/HUD/` | [Editor/Asset]: Sprite import, then referenced by the eventual HUD prefab (`UI/Match/`, not built yet per `04`'s backlog) |
| Music | Suno | `Assets/Audio/Music/` (already scaffolded) | [Editor/Asset]: Audio Import Settings — Ogg Vorbis compression, "Loop" flag enabled, "Preload Audio Data" per Android memory budget; wired to an `AudioSource` on a persistent menu/match-scene object |
| SFX | Kenney packs / generated | `Assets/Audio/SFX/` (already scaffolded) | [Editor/Asset]: WAV/compressed-in-memory for short one-shots (lower latency than streaming), triggered via thin `AudioSource.PlayOneShot` calls subscribed to the event hooks `09` §3b already enumerates (`MatchController` events, `SailorPawn.ApplyDamage`, ability `Execute` calls) — the event hooks are [File]-stable already; only the subscription glue is new |
| Voice lines | TTS | `Assets/Audio/SFX/VO/` (new subfolder) | Same as SFX; lowest priority to wire (per `09` §3c, cut first under time pressure) |

### A concrete gap this surfaces (flag for dev, not art)

`09` §2 already flags that `Projectile.OnTriggerEnter` (`Assets/Scripts/Gameplay/Combat/Projectile.cs`) only recognizes `SailorPawn` and `TemporaryObstacle` components today — static arena geometry (once real Tideline Cove props exist) has no collision-response path yet. This automation plan restates it here because it's exactly the kind of thing that *should* surface in the build-verify loop (§1) once real geometry lands — a projectile visibly passing through a rock that should block it is a fast, screenshot-catchable regression, not something that needs a human staring at gameplay for minutes. Worth a dedicated smoke-test note once §1's script exists and real arena props are in.

## 5. CI/CD — deliberately deferred, with a concrete "do this now" note instead

No `.github/workflows/` exists, and this plan does **not** propose adding one yet, for a specific reason: Android builds need a licensed Unity install + Android SDK/NDK on the runner, which means either (a) a self-hosted runner (defeats most of CI's convenience — you're still managing a machine) or (b) a hosted CI Unity license + GameCI-style Docker images (real option, but adds a paid/managed-license dependency this project hasn't taken on yet — no budget confirmed per the asset-sourcing memory). Given the project is currently solo/small-team and already has a working local headless loop (§0), **the pragmatic "CI" for the alpha phase is `scripts/build_and_verify.ps1` run manually before any "this is ready to test" claim** — not a hosted pipeline.

**If/when CI is greenlit later**, the seams are already in the right shape to adopt it cheaply:
- `BuildScript.BuildAndroidDevelopment` is already a clean `-executeMethod` entry point — a GitHub Actions job using `game-ci/unity-builder` would call the exact same method, no build-script rewrite needed.
- The `BWR_` log-tag convention (§1) is already CI-log-parseable.
- Keep this section's absence of a workflow file intentional, not an oversight — don't add a half-working CI config speculatively; wire it when there's an actual runner/license story confirmed by the user.

## 6. Remote vs Editor-only summary

- **[File]**: this plan doc; compile verification (§1); `scripts/build_and_verify.ps1` and any future two-instance LAN smoke-test extension of it (§2); the naming-convention table (§4); the event-hook subscription *code* for SFX triggers (the hooks themselves already exist as stable C# per `09` §6's note).
- **[Editor/Asset]**: every actual import step in §4's table; Player Settings wiring (icon/splash, per `06`); on-device screenshot verification's *interpretation* (a human confirming the captured screenshot looks right, vs. the script's cheap automated "did something render" check in §3); any eventual CI runner setup (§5, deferred).
