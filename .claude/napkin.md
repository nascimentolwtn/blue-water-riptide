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
Each item tagged **[File]** (pure C# / ScriptableObject class defs / JSON / interfaces — completable in files, no Unity IDE needed, safe to do remotely) or **[Editor/Asset]** (scene/prefab composition, Inspector wiring, art/animation import, on-device testing — needs the Unity Editor open). Data that can be expressed as code/defaults is classified [File] even if it ends up serialized into a `.asset` — see the per-plan "Remote vs Editor-only work" section for the general rule.
1. **[2026-07-18] Build Core shared systems** — **[File]** — *partial*
   Do instead: implement `MatchController` state machine, `Session` model, `MatchRules` ScriptableObject *class* (fields/defaults) per `00-game-design-overview.md` §7 — pure C# logic and data-class definitions, no scenes/prefabs/art required. **Done so far**: M1-scoped versions of all three landed in `Assets/Scripts/Core/` (single round only, plain classes not ScriptableObjects yet). **Still missing**: `RoundCountdown`/`SuddenDeath` states, best-of-3 rounds, `ArenaCatalog`, `SaveService`, `ProgressionService`.
2. **[2026-07-18] Build Character & Ability framework** — **[File]** — *partial*
   Do instead: implement `SailorDefinition`/`AbilityDefinition` ScriptableObject *classes* and reusable ability behaviors (projectile, dash/leap, area, aura, wall) per `00` §2/§7 as C# code — no art dependency. **Done so far**: `IAbilityBehavior` seam + `ProjectileAbilityBehavior`, `MeleeArcAbilityBehavior`, `LeapSlamAuraAbilityBehavior` in `Assets/Scripts/Characters/` and `Assets/Scripts/Gameplay/Combat/` (plain C#, not ScriptableObjects). **Still missing**: wall-spawn behavior, standalone dash behavior (leap is currently fused into Anchor's Super rather than a reusable primitive). Creating per-Sailor asset instances and linking prefab refs is Editor-bound; tracked under item 3.
3. **[2026-07-18] Implement the 2 v1 Sailors** — **[Editor/Asset]** — *partial*
   Do instead: build Ensign Ace and Admiral Anchor *pawn prefabs* (animator, hitbox, HP bar hookup) and wire their `SailorDefinition`/`AbilityDefinition` asset instances through the framework per `00` §3 — needs the Unity Editor for prefab assembly, Inspector wiring, and eventual character art/animation. **Done so far**: both Sailors' full kits work end-to-end in a procedural-primitive prototype (`M1PrototypeBootstrap`) — Ace's Jump Serve, Anchor's Anchor Slam + Drop Anchor Super + Ballast trait. **Still missing**: real pawn prefabs/animator/hitbox/art (this item's actual [Editor/Asset] scope), and Ace has no Super yet.
4. **[2026-07-18] Build the Tideline Cove arena** — **[Editor/Asset]**
   Do instead: arena prefab + tile/spawn data asset per `00` §5, registered in `ArenaCatalog` — scene/prefab composition in the Editor; even greybox primitives require placing geometry in a scene.
5. **[2026-07-18] Implement Single Player mode (AI/session logic)** — **[File]** — *not started*
   Do instead: `AITeamBuilder`, `AIInputDriver`, `AIPerception`, utility-state AI, `AISquadBrain`, difficulty parameter sets, `SinglePlayerLauncher` per `01-single-player-mode.md` — pure C# simulation/decision code, no scenes required. A `DummyAIInputDriver` ("stands and shoots," no movement/utility-states) exists in `Assets/Scripts/AI/` as a placeholder for the M1 prototype only — don't mistake it for this item. TeamSelect/Results screen assembly is Editor-bound, tracked under item 7.
6. **[2026-07-18] Implement Local Network mode (networking logic)** — **[File]**
   Do instead: `NetworkInputDriver`, `LanSessionAdvertiser`/`Scanner`, `LobbyState`, `LanSessionLauncher`, rejoin/connection-approval logic per `02-local-network-mode.md` as C# code. Attaching `NetworkObject`/`NetworkTransform` to prefabs and configuring `NetworkManager` in-scene is Editor-bound — a small follow-up, not a blocker to writing the classes.
7. **[2026-07-18] Build menu navigation & lobby UI** — **[Editor/Asset]**
   Do instead: `UIStateController` nav-stack code is [File]-doable on its own, but the Home/Mode Select/Lobby/TeamSelect/Results *screens* (Canvas layout, prefab assembly, visual hierarchy) per `04-menu-navigation-lobby.md` need the Editor's UI tools.
8. **[2026-07-18] Implement progression: Trophies, Voyage Road, Doubloons, Fleet Rank** — **[File]**
   Do instead: local JSON save schema, `ProgressionService`, and `FleetRankDefinition` tier data per `00` §6 and `05-gamification-trophies-ranking.md` — pure C# + data, programmable without the Editor. Voyage Road/Locker screen visuals are Editor-bound, tracked under item 7.
9. **[2026-07-18] Lay online-readiness groundwork** — **[File]**
   Do instead: keep `Session`/ability framework transport-agnostic, define the `ISessionProvider` interface per the "do this now" checklist in `03-online-mode-future.md` — pure C# architecture, no assets involved.
