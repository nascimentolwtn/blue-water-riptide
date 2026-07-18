# Unity & Android Environment Setup

One-time machine setup required before any code in this repo can build or run. This is a prerequisite for the "Bootstrap the Unity project" backlog item in `.claude/napkin.md` — implementation work (subagent-driven or otherwise) should not start until this is done and verified.

## Current machine state (as of 2026-07-17)

- **Unity Hub and Unity Editor: not installed.** Nothing can compile, build, or test-on-device until this exists.
- **Android tooling already present**: Android SDK at `E:\android_sdk` (on PATH: `platform-tools`/`adb`, `emulator`, `cmdline-tools`), an emulator AVD named `Pixel_9_API_35`, and Java 17 (`java -version` → 17.0.12 LTS). `adb devices` currently shows nothing attached/running.

## Steps

1. **Install Unity Hub** — download from unity.com/download, run the installer, sign in with (or create) a free Unity Personal account.

2. **Install the Unity Editor via Hub**
   - Hub → **Installs** → **Install Editor** → **Unity 2022 LTS** (matches `Packages/manifest.json`).
   - On the module screen, check **Android Build Support**, including its **OpenJDK** and **Android SDK & NDK Tools** sub-modules.
   - An Android SDK/emulator/JDK already exist on this machine (see above) — letting Unity install its own bundled copies is simplest and avoids version-mismatch issues; pointing Unity at the existing `E:\android_sdk` instead (via **Preferences → External Tools**) is a later optional optimization, not required.

3. **Open this repo as the Unity project**
   - Hub → **Projects** → **Open → Add project from disk** → select `E:\dev\blue-water-riptide`.
   - First open populates `Library/` and `ProjectSettings/` and resolves `manifest.json`'s packages (Netcode for GameObjects, Unity Transport, Input System, Cinemachine, TextMeshPro, URP, Addressables). Takes several minutes; needs internet access.

4. **Switch build target to Android**
   `File → Build Settings → Android → Switch Platform`.

5. **Set Player Settings for Android**
   `Edit → Project Settings → Player`, Android tab:
   - Package name: reverse-domain id, e.g. `com.nascimentolwtn.bluewaterriptide`
   - Scripting backend: **IL2CPP** (Mono doesn't support ARM64 on Android at all, and Google Play has required 64-bit since 2021 — not deferrable to "later")
   - Target architecture: ARM64 (drop ARMv7 unless targeting old 32-bit-only devices)
   - **Minimum API Level:** 30 (Android 11; covers Galaxy S21 FE, the lowest-end target device)
   - **Target API Level:** 34+ (Android 14+; supports S20 FE through S25 FE per `.claude/napkin.md` Platform & Device Constraints)

6. **Confirm the test target works**
   - Start the existing `Pixel_9_API_35` AVD (`emulator -avd Pixel_9_API_35`, or via Android Studio's Device Manager) and confirm `adb devices` sees it — or use a physical device with USB debugging enabled.
   - `File → Build And Run` in Unity should deploy straight to it.

7. **Device testing strategy**
   - The emulator is a high-end baseline for day-to-day iteration. Later, test critical performance paths (combat, ability effects) on a **Galaxy S21 FE** or equivalent low-end device (4GB RAM baseline) to catch optimization regressions early — S21 FE is the lowest-end target in the `.claude/napkin.md` Platform & Device Constraints list.

## Definition of done

Project opens in the Editor with zero compile errors, all packages resolved, and a test build deploys to the emulator/device via Build And Run. Once that's true, implementation work on plan `00`/`01` can start.
