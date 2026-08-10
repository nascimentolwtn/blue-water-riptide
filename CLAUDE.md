# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project status: M1 prototype playable (code-only, procedural — no hand-authored scenes/prefabs/art yet)

The Unity project builds/runs on Android and a first vertical slice exists: `Assets/Scripts/{Core,Characters,Gameplay,AI}` implement the transport-agnostic Core seam plus a playable Ensign Ace vs. Admiral Anchor matchup, all spawned procedurally at runtime by `M1PrototypeBootstrap` (no scenes/prefabs/ScriptableObject assets hand-built in the Editor). See `CHANGELOG.md` for what's verified working, `.claude/napkin.md` for environment/debugging gotchas, and `.claude/plans/01-single-player-mode.md`'s "Implementation status" note for exactly what's still missing vs. the real M1/M2 milestones (touch input, on-device-verified, real arena, rounds/timer, real AI).

**Confirmed environment**: Unity 6000.5.4f1 LTS, Android Build Support, IL2CPP scripting backend, ARM64 target, URP (now actually assigned as the active pipeline — see `Assets/Rendering/`, napkin Execution & Validation #4), Input System (New) as the sole active handler. Min API 30 / Target API 34+ (see `.claude/napkin.md` Platform & Device Constraints for the target device list). A custom base Gradle template (`Assets/Plugins/Android/baseProjectTemplate.gradle`) forces `kotlin-stdlib` to 1.8.22 and excludes the old `kotlin-stdlib-jdk7`/`jdk8` artifacts to avoid a duplicate-class Gradle failure. **Optimized Frame Pacing is disabled** (Player Settings → Android → Resolution and Presentation) — leaving it on crashes with SIGABRT inside Swappy's Vulkan init path on emulator. `Assets/link.xml` preserves Collider types IL2CPP otherwise strips from `GameObject.CreatePrimitive` calls (napkin Execution & Validation #5).

**Build & run**: open the repo folder as a project in Unity Hub, then `File → Build And Run` (deploys to whatever's visible in `adb devices`) — or headlessly via `Assets/Editor/BuildTools/BuildScript.cs` (`-executeMethod BlueWaterRiptide.EditorTools.BuildScript.BuildAndroidDevelopment`, needs the Editor GUI closed first). No automated test suite yet — still a future backlog item.

## Game Design

**Core concept:** 3v3 top-down arena brawler in the *Brawl Stars* pattern. Each player controls one Sailor with a **basic attack** (ammo/charge-based, ranged or melee) and a **Super** that charges by dealing damage. Knock out the opposing team to win. Marine/navy and beach-volleyball are **cosmetic theme only** (character names, attack names, art, arena dressing) — **there is no ball, no scoring zones, no sport rules.** The actual game is a straightforward arena fight.

**Two independent axes:** (1) **Connection mode** — how players get into a match: Single Player vs AI, Local Network (LAN), Online (future). (2) **Ruleset** — the win condition. v1 ships one ruleset, used by all connection modes.

**v1 ruleset: "Sink or Swim"** (knockout pattern)
- **3v3, best of 3 rounds, no respawns within a round.**
- A round ends when one team is fully knocked out (that team loses), or when the **90-second round timer** expires — then the team with more surviving Sailors wins.
- Survivor tie at timeout → **Rising Tide sudden death**: water floods inward from edges (one tile ring every ~5s, dealing damage over time) until a knockout decides the round.
- First team to **2 round wins** takes the match (~3–5 minutes total). Between rounds: 5s reset, everyone back at spawn pads at full HP; **Super carry carries over at 50%**.
- Chosen for simplicity (no mid-round respawn logic, unambiguous end state) while producing tense, short mobile matches.

**Match flow:** `Round countdown (3s) → Combat → Round end → next round or match results`. Knocked-out players spectate until the round ends.

**Character framework**
- Every Sailor has: **Basic attack** (ammo-style: 3 charges regenerating over 1.5–2.5s), **Super** (charges by dealing damage; persists until used), **Trait** (small always-on passive).
- Attack archetypes (volleyball-themed labels): `Serve` (long-range straight projectile), `Lob` (arcing, clears walls), `Spike` (short-range AOE/melee), `Set` (support/utility), `Block` (defensive).

**v1 roster: 2 Sailors**
- **Ensign Ace** (Starter, All-rounder / ranged, `Serve` archetype): 1300 HP, Normal speed, 140 damage × 3 ammo (1.7s reload). **Jump Serve** 3-shot burst ranged attack. **Ace Barrage** Super: 5 fan-spread serves with knockback. **Fresh Legs** trait: +10% move speed for 3s after knockout.
- **Admiral Anchor** (Rare, Tank / frontline, `Spike` archetype): 1900 HP, Slow speed, 190 damage × 3 ammo (2.4s reload). **Anchor Slam** close-range frontal AOE with knockback. **Drop Anchor** Super: leap to aimed spot (clears walls) + 6s aura granting nearby allies +40% damage reduction. **Ballast** trait: immune to knockback.

**v1 arena: Tideline Cove**
- 34×20 tiles, symmetric beach court. Three distinct lanes: north (dune grass stealth), center (volleyball net chokepoint), south (shallow water flank).
- Terrain rules (theme-skinned standard mechanics): solid cover blocks movement and straight projectiles; arcing attacks clear it; destructible cover breaks under Supers; shallow water slows movement 30%; dune grass conceals (hidden unless enemy is adjacent).
- Layout (mirrored per half): volleyball nets (7-tile segments with 6-tile gap at midcourt), beached rowboat (3×2 solid), driftwood logs (2×1 solid, diagonal approaches), coral rock clusters (2×2 destructible), tidal inlet (3-tile shallow water, south edge), dune grass patches (2×3, north edge).
- Rising Tide sudden death: floods inward from south and boundaries, forcing final fight toward center net gap.

**Progression (lightweight for v1)**
- **Trophies** — per-Sailor score: +4 win / −2 loss (Single Player: +2/0, capped contribution). Total trophies drive the **Voyage Road**, a single linear reward track.
- **Voyage Road** unlocks (v1): Admiral Anchor at an early node (Ace is starter), then Doubloon caches and cosmetic flags; later nodes pre-labeled for future Sailors/arenas.
- **Doubloons** — only currency in v1. Earned from match results and Voyage Road nodes. Spent on **Sailor level-ups** (levels 1–5, +5% HP/damage per level; costs 100/200/400/800).
- **Out of scope v1** (design later; no hooks beyond data headroom): premium currency, battle pass, gadget/star-power equivalents, skins shop, clubs.
- Persistence: local JSON save (`Application.persistentDataPath`), versioned schema.

**Control scheme** (mobile touch, input-agnostic via Input System)
- **Left virtual joystick**: movement.
- **Right attack button**: tap = quick attack at nearest target (soft auto-aim); drag = aim indicator; release = fire; drag back = cancel.
- **Super button** (above attack): same tap/drag-aim; greyed until charged.
All gameplay actions resolve through `InputCommand` abstraction (move vector, aim vector, action id, timestamp) so touch, gamepad, AI, and network drivers are interchangeable.
**Game Modes (connection modes)**

1. **Single Player** — pick a Sailor, play against AI-controlled opposing team (3v3: you + 2 AI teammates vs 3 AI opponents). Same Sink or Swim ruleset, same Tideline Cove arena. AI uses utility-state behavior scoring (Engage, Retreat, Regroup, Flank, HoldChoke, SuddenDeathPush), per-archetype profiles (Serve/Spike presets), and lightweight squad coordination. Three difficulty tiers: Deckhand (easy, 400ms reaction), Bosun (normal, 250ms reaction), Skipper (hard, 150ms reaction with target leading).

2. **Local Network (LAN)** — co-op or PvP with nearby devices over local Wi-Fi. Host discovers clients via UDP broadcast; all gameplay runs host-authoritative (clients send input via RPC, host runs match simulation, mirrors state back via `NetworkMatchState`/`NetworkSailorState`). Transport: Unity Netcode for GameObjects + Unity Transport, direct IP:port connection (default 7777). Disconnect handling: disconnected players get AI takeover for 60s; host loss ends the match.

3. **Online (future)** — lobby system to join a friend's team or queue solo against other players over the internet. Deferred post-v1; architecture seam (`ISessionProvider`) is in place so modes 1-2 stay online-ready.

**UI & Navigation** (Plan 04)

Four scenes total:
- **Boot**: initializes services (save/profile, catalogs, audio), shows splash logo, auto-advances to Home.
- **MainMenu**: Home hub (PLAY, Locker, Voyage Road, Settings, Mode Select), Sailor Locker (browse, view stats, level-ups), Voyage Road screen, Settings, Mode Select (Single Player / LAN / Online-stub), LAN entry screens (Host-or-Join, Host Config, Lobby).
- **TeamSelect**: player picks Sailor before Single Player match (locked Sailors greyed with unlock hint).
- **Match**: loads arena prefab by id, spawns Sailors, runs MatchController, shows Results overlay at MatchEnd (trophies/Doubloons delta, rematch / menu buttons).

**Splash & Icon** (Plan 06)

App icon and Boot-scene splash logo: ship's bell mark from `docs/History.md`'s Tideline Cup trophy motif (navy `#0E2A47` on light background). Player Settings assignment and Unity Personal/Pro tier dependency (unconfirmed — ability to suppress Unity logo).

**Asset Sourcing** (Plan 07)

AI generation prompts for Sailor graphics, arena/props, audio, UI icons live in `.claude/prompts/` — ready-to-paste batches for Gemini, Tripo, Suno. Free marketplace browsing first (Sketchfab, Poly Haven, itch.io, Kenney); Tripo free tier as fallback.

## Architecture (from the plans — build to this shape)

- **Connection mode vs ruleset are independent axes.** Single Player / Local Network / Online (future) are *connection modes*; "Sink or Swim" is the (currently only) *ruleset*. Don't conflate them.
- **Shared simulation must stay transport-agnostic**, living in `Assets/Scripts/Core` and `Assets/Scripts/Gameplay`: pure C# driven by an `InputCommand` abstraction and ticked by an `IMatchClock`, no direct references to Netcode/UI/input devices. Single Player runs it locally, LAN runs it host-authoritative, Online reuses it unchanged.
  - `Core/MatchController` — state machine: `Setup → RoundCountdown → Combat → RoundEnd → (next round | MatchEnd) → Results`, with `SuddenDeath` under Combat.
  - `Core/Session` — describes a match-to-be (connection mode, ruleset id, arena id, participants + driver type human/AI/remote); the seam that keeps all three connection modes symmetric.
  - `Characters/`, `Gameplay/Abilities` — `SailorDefinition` + `AbilityDefinition` ScriptableObjects; adding a Sailor is data + prefab, no new mode code.
  - `Gameplay/Combat` — projectiles (straight vs arcing), melee/AOE, damage/knockback/HP, knockout, Super-charge accounting.
  - `Gameplay/Court` — arena geometry (walls, cover, hazards, spawn pads), loaded by id via a single-entry `ArenaCatalog`.
- **Script folder responsibilities** under `Assets/Scripts/`: `Core` (loop/match/session/rules), `Gameplay` (combat/court), `Characters` (Sailor stats/abilities/controllers), `Networking` (NGO setup, LAN discovery, session/lobby glue), `UI` (menus/HUD — thin layer over Core events only, no game logic), `AI` (single-player opponent behavior).
- **Terminology**: "Sailor," never "brawler."

## Other repo state

- `.claude/napkin.md` — curated cross-session guardrails and a capped Backlog list of pending implementation work; read it at the start of a session.
- `CHANGELOG.md` — completed backlog items get moved here (napkin Backlog stays pending-only).
- `docs/sailors.md` — original character-name/flavor brainstorm; tone reference only, not authoritative for mechanics.
- `docs/History.md` — world/squadron/character lore (the Tideline Cup premise, the three squadrons, per-Sailor backstory hooks); narrative/flavor only, same non-authoritative status as `docs/sailors.md`. Linked from `README.md`; not yet surfaced in-game (planned for a future menu/character-select screen).
- pt-BR translations exist for `README.md`, `docs/History.md`, and `.docs/sailors.md` as sibling `*.pt-BR.md` files, each with a language-switcher link at the top. Keep them in sync if the English originals change meaningfully — they're not auto-regenerated.
- Commit messages follow a strict global convention: short imperative title + 1-2 lines of context (the "why"). One commit per feature/fix, grouping all affected files.

## Useful command lines

**Emulator**
- `emulator -avd Pixel_9_API_35` — Start the Android emulator
- `Get-Process | Where-Object {$_.ProcessName -like "*qemu*"} | Stop-Process -Force -ErrorAction SilentlyContinue` — Kill all running emulator instances
- `emulator -avd Pixel_9_API_35 -wipe-data` — Start emulator and wipe data (fresh state)

**ADB & Installation**
- `adb devices` — List connected devices/emulator
- `adb install -r build/android/app-release.aab` — Install/reinstall the APK (after building)
- `adb uninstall com.BlueWaterRiptide.BlueWaterRiptide` — Uninstall the app
- `adb shell pm clear com.BlueWaterRiptide.BlueWaterRiptide` — Clear app data (cache/prefs)

**Logs & Debugging**
- `adb logcat` — Stream device logs (Ctrl+C to stop)
- `adb logcat -c` — Clear logcat buffer
- `adb logcat | grep -i "unity\|exception\|error"` — Filter for Unity/errors

**Build & Deploy** (from command line)
- `cd Assets/Editor/BuildTools && mono BuildScript.cs` — Headless build (if configured)
- `adb push <file> /sdcard/Download/` — Push file to device

**Device Interaction**
- `adb shell am start -n com.BlueWaterRiptide.BlueWaterRiptide/.MainActivity` — Launch app