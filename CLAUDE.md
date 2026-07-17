# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project status: pre-implementation

This repo currently contains **only Unity project scaffolding and design docs — no C# code exists yet** (`Assets/**` folders are empty placeholders, `Packages/manifest.json` lists intended dependencies, `ProjectSettings/` is not yet generated). There are no build, lint, or test commands to run until the folder has been opened as a project in Unity Hub (Unity 2022 LTS+) and code has been written.

The authoritative source for what to build is `.claude/plans/`, not this file — read the relevant plan doc(s) before writing any code in an area.

## Critical: theme vs mechanics

The game's marine/navy/volleyball theme is **cosmetic only** (character names, attack names, art, arena dressing). The actual game is a straightforward *Brawl Stars*–pattern top-down arena brawler: basic attack (ammo/charge-based) + charged Super per character, team-elimination win condition. **There is no ball, no volleyball scoring, no sport rules** — an early draft of `00-game-design-overview.md` got this wrong and was corrected; don't reintroduce ball/scoring-zone mechanics without an explicit new user request. `README.md`'s wording ("beach volleyball", "ball physics, serve/spike/block mechanics" in the folder comments) predates this correction and is stale — the plans in `.claude/plans/` are authoritative over the README for gameplay rules.

## Design docs (`.claude/plans/`)

Read in order; each assumes the ones before it:
- `00-game-design-overview.md` — core concept, connection-mode-vs-ruleset split, v1 ruleset ("Sink or Swim" knockout), character framework, v1 roster (exactly 2 Sailors: Ensign Ace, Admiral Anchor), v1 arena (Tideline Cove), progression (Trophies/Voyage Road/Doubloons), and the shared systems list below.
- `01-single-player-mode.md` — vs-AI mode; `AITeamBuilder` samples from the same 2-Sailor pool.
- `02-local-network-mode.md` — LAN co-op/PvP via Unity Netcode for GameObjects + Unity Transport, UDP broadcast for host discovery.
- `03-online-mode-future.md` — deferred internet lobby/matchmaking; lighter detail, includes a "do this now" checklist for keeping modes 1-2 online-ready.
- `04-menu-navigation-lobby.md` — full screen flow (Home → Mode Select → per-mode lobby → Match → Results), `UIStateController` nav stack.
- `05-gamification-trophies-ranking.md` — Fleet Rank (cosmetic ship-class tiers from account trophy total), leaderboard scoping, save schema.

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
- `sailors.md` / `.claude/plans/sailors.md` — original character-name/flavor brainstorm; tone reference only, not authoritative for mechanics.
- Commit messages follow a strict global convention: `type: subject` only — no body, no period. One commit per feature/fix, grouping all affected files.
