# Napkin Runbook

## Curation Rules
- Re-prioritize on every read.
- Keep recurring, high-value notes only.
- Max 10 items per category.
- Each item includes date + "Do instead".

## Domain Behavior Guardrails (Highest Priority)
1. **[2026-07-17] This is a Brawl Stars–pattern arena brawler, NOT a volleyball sport sim**
   Do instead: marine/navy/volleyball is cosmetic theme only (names, art, flavor text). Core rules are basic-attack + charged Super + team-elimination, per `.claude/plans/00-game-design-overview.md`. Never reintroduce a ball, scoring zones, or sport rules into core mechanics without an explicit new user request.
2. **[2026-07-17] v1 content scope is locked: 2 Sailors, 1 arena**
   Do instead: Ensign Ace + Admiral Anchor only; Tideline Cove is the only arena. Don't expand roster/arena count in plans or code without the user explicitly asking — expansion paths are already documented as roadmap notes in `00-game-design-overview.md` §4.
3. **[2026-07-17] Terminology: "Sailor," never "brawler"**
   Do instead: match existing plan docs' vocabulary (Sailor, connection mode vs ruleset, Squadron) when writing new plans or code comments.
4. **[2026-07-17] Two independent axes: connection mode vs ruleset**
   Do instead: Single Player / Local Network / Online are *connection modes* (`01`-`03`); "Sink or Swim" knockout is the *ruleset* (`00` §1). Don't conflate them when adding new modes or rulesets later.

## Execution & Validation
1. **[2026-07-17] No Unity Editor run in this environment**
   Do instead: repo is scaffolded (folders, manifest.json, .gitignore) but `ProjectSettings/`/`Library/` aren't generated. Opening in Unity Hub is a step for the user, not something to fake or fabricate here.
2. **[2026-07-17] New plan docs need prior plans read first**
   Do instead: before spawning a subagent for a new `.claude/plans/*.md` file, tell it to read all existing numbered plans first so terminology/scope stay consistent (see guardrails above).

## User Directives
1. **[2026-07-17] Commit style enforced globally: `type: subject` only**
   Do instead: no body, no period, 1-2 words after colon, one commit per feature/fix grouping all affected files. Full rule in global memory (`feedback_commit_style_global.md`) — verify last commits match before adding new ones.
2. **[2026-07-17] Never commit without explicit ask**
   Do instead: stage and commit only when the user says so in that turn; otherwise leave changes staged/unstaged and say what's pending.
3. **[2026-07-17] Completed backlog items move to CHANGELOG.md**
   Do instead: when a Backlog item below is finished, add a dated entry to `CHANGELOG.md` and delete it from the Backlog category here — keep this list to pending work only.

## Backlog
1. **[2026-07-17] Bootstrap the Unity project**
   Do instead: open the repo folder in Unity Hub/Editor to generate `ProjectSettings/`/`Library/` and confirm `Packages/manifest.json` resolves cleanly.
2. **[2026-07-17] Build Core shared systems**
   Do instead: implement `MatchController` state machine, `Session` model, `MatchRules` ScriptableObject per `00-game-design-overview.md` §7.
3. **[2026-07-17] Build Character & Ability framework**
   Do instead: implement `SailorDefinition` + `AbilityDefinition` ScriptableObjects and reusable ability behaviors (projectile, dash/leap, area, aura, wall) per `00` §2/§7.
4. **[2026-07-17] Implement the 2 v1 Sailors**
   Do instead: build Ensign Ace (ranged) and Admiral Anchor (melee tank) through the framework above, not hard-coded, per `00` §3.
5. **[2026-07-17] Build the Tideline Cove arena**
   Do instead: arena prefab + tile/spawn data asset per `00` §5, registered in a single-entry `ArenaCatalog`.
6. **[2026-07-17] Implement Single Player mode**
   Do instead: `AITeamBuilder` sampling the 2-Sailor pool + per-archetype AI behaviors per `01-single-player-mode.md`.
7. **[2026-07-17] Implement Local Network mode**
   Do instead: Netcode for GameObjects host/client + UDP LAN discovery/advertisement per `02-local-network-mode.md`.
8. **[2026-07-17] Build menu navigation & lobby UI**
   Do instead: `UIStateController` nav stack, Home/Mode Select/Lobby screens per `04-menu-navigation-lobby.md`, thin over Core events.
9. **[2026-07-17] Implement progression: Trophies, Voyage Road, Doubloons, Fleet Rank**
   Do instead: local JSON save schema + Fleet Rank tiers per `00` §6 and `05-gamification-trophies-ranking.md`.
10. **[2026-07-17] Lay online-readiness groundwork**
    Do instead: keep `Session`/ability framework transport-agnostic per the "do this now" checklist in `03-online-mode-future.md`.
