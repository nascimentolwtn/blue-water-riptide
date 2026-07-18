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

## Platform & Device Constraints
1. **[2026-07-18] Target Android devices: S20 FE, S21 FE, S25, S25+, S25 FE**
   Do instead: optimize for S21 FE (lowest-end: 4GB RAM, Snapdragon 888) as baseline. Configured Player Settings: **Min API 30** (Android 11, covers S21 FE) / **Target API 34+** (Android 14, covers up through S25 FE). Package id: `com.libuyitservices.bluewaterriptide`. Day-to-day iteration uses the local `Pixel_9_API_35` emulator; test critical perf paths (combat, ability effects) on real S21 FE hardware before considering a feature done.

## Execution & Validation
1. **[2026-07-18] Unity 6000.5.4f1 LTS is installed and building successfully for Android**
   Do instead: confirmed working config — IL2CPP + ARM64, URP, Input System (New), **Optimized Frame Pacing disabled** (Player Settings → Android → Resolution and Presentation), custom base Gradle template forcing `kotlin-stdlib` 1.8.22. Don't re-suggest "Unity isn't installed" or Mono/Vulkan-default guidance.
2. **[2026-07-18] Swappy/Vulkan SIGABRT crash on emulator (silent close, no dialog)**
   Do instead: if an Android build installs but closes instantly with no visible error, check `adb logcat -d | grep -i fatal` first — Android often doesn't show a crash dialog for native (IL2CPP) crashes. This project's specific case: Swappy frame-pacing lib crashes in `SwappyVk_initAndGetRefreshCycleDuration` on emulator Vulkan; fixed by disabling Optimized Frame Pacing (see above). If it recurs on a different symptom, fall back to dropping Vulkan from Graphics APIs (GLES3-only) for emulator testing.
3. **[2026-07-17] New plan docs need prior plans read first**
   Do instead: before spawning a subagent for a new `.claude/plans/*.md` file, tell it to read all existing numbered plans first so terminology/scope stay consistent (see guardrails above).

## User Directives
1. **[2026-07-18] Commit style enforced globally: title + 1-2 lines of context**
   Do instead: short imperative title, then 1-2 lines of "why" — no bare title-only, no long prose body. One commit per feature/fix grouping all affected files. Full rule in global memory (`feedback_commit_style_global.md`) — verify last commits match before adding new ones.
2. **[2026-07-17] Never commit without explicit ask**
   Do instead: stage and commit only when the user says so in that turn; otherwise leave changes staged/unstaged and say what's pending.
3. **[2026-07-17] Completed backlog items move to CHANGELOG.md**
   Do instead: when a Backlog item below is finished, add a dated entry to `CHANGELOG.md` and delete it from the Backlog category here — keep this list to pending work only.

## Backlog
1. **[2026-07-17] Build Core shared systems**
   Do instead: implement `MatchController` state machine, `Session` model, `MatchRules` ScriptableObject per `00-game-design-overview.md` §7.
2. **[2026-07-17] Build Character & Ability framework**
   Do instead: implement `SailorDefinition` + `AbilityDefinition` ScriptableObjects and reusable ability behaviors (projectile, dash/leap, area, aura, wall) per `00` §2/§7.
3. **[2026-07-17] Implement the 2 v1 Sailors**
   Do instead: build Ensign Ace (ranged) and Admiral Anchor (melee tank) through the framework above, not hard-coded, per `00` §3.
4. **[2026-07-17] Build the Tideline Cove arena**
   Do instead: arena prefab + tile/spawn data asset per `00` §5, registered in a single-entry `ArenaCatalog`.
5. **[2026-07-17] Implement Single Player mode**
   Do instead: `AITeamBuilder` sampling the 2-Sailor pool + per-archetype AI behaviors per `01-single-player-mode.md`.
6. **[2026-07-17] Implement Local Network mode**
   Do instead: Netcode for GameObjects host/client + UDP LAN discovery/advertisement per `02-local-network-mode.md`.
7. **[2026-07-17] Build menu navigation & lobby UI**
   Do instead: `UIStateController` nav stack, Home/Mode Select/Lobby screens per `04-menu-navigation-lobby.md`, thin over Core events.
8. **[2026-07-17] Implement progression: Trophies, Voyage Road, Doubloons, Fleet Rank**
   Do instead: local JSON save schema + Fleet Rank tiers per `00` §6 and `05-gamification-trophies-ranking.md`.
9. **[2026-07-17] Lay online-readiness groundwork**
   Do instead: keep `Session`/ability framework transport-agnostic per the "do this now" checklist in `03-online-mode-future.md`.
