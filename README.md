# Blue Water Riptide

A top-down team arena brawler in the *Brawl Stars* pattern — team-based combat, basic attack + charged Super per character, knockout win condition. Marine/navy and beach-volleyball are **cosmetic theme only** (character names, attack names, art, arena dressing) — there is no ball, no scoring zones, no sport rules. Roster and lore seeds live in [`.claude/plans/sailors.md`](.claude/plans/sailors.md); see [`.claude/plans/00-game-design-overview.md`](.claude/plans/00-game-design-overview.md) for the authoritative design.

Every year the fleets gather at Tideline Cove for the **Tideline Cup** — a friendly, fiercely competitive tournament three squadrons (the disciplined Blue Water Squad, the reckless Riptide Rush, and the immovable Anchor Guard) show up to win. See [`History.md`](History.md) for the full world/squadron/character background.

## Engine & Target Platforms

- **Engine:** Unity (C#), Unity 2022 LTS or newer.
- **Primary platform:** Android.
- **Future platform:** iOS (build settings and input should stay platform-agnostic from day one to keep this port cheap later).

## Game Modes (initial scope)

1. **Single Player** — pick a team, play against a randomized AI-controlled opposing team.
2. **Local Network (LAN)** — same-team co-op or PvP with nearby devices over local Wi-Fi. Networking target: Unity Netcode for GameObjects + Unity Transport, with UDP broadcast for LAN host discovery (no internet required).
3. **Online (future)** — lobby system to join a friend's team or queue solo against other players over the internet.

## Repository Structure

```
Assets/
  Scripts/
    Core/         # game loop, match/session state, score & rules
    Gameplay/      # combat resolution (projectiles, melee/AOE, knockback), court/arena
    Characters/    # Sailor stats, abilities, controllers
    Networking/    # NGO setup, LAN discovery, session/lobby glue
    UI/            # menus, HUD, team select
    AI/            # single-player opponent AI
  Scenes/
  Prefabs/
    Characters/
    Projectiles/
    Environment/
  Art/
    Characters/
    Environment/
    UI/
  Audio/
    SFX/
    Music/
  Resources/
Packages/          # Unity Package Manager manifest
ProjectSettings/    # populated on first open in Unity Hub/Editor
docs/
.claude/
  plans/            # design & implementation plans (see below)
```

> Note: `ProjectSettings/` and `Packages/packages-lock.json` are normally generated/managed by the Unity Editor. Open this folder as a project in Unity Hub to finish initializing them.

## Design & Implementation Plans

See [`.claude/plans/`](.claude/plans/) for the initial version plans:

- `00-game-design-overview.md` — core concept, roster translation from Brawl Stars, match rules, meta systems.
- `01-single-player-mode.md`
- `02-local-network-mode.md`
- `03-online-mode-future.md`
- `04-menu-navigation-lobby.md`
- `05-gamification-trophies-ranking.md`
