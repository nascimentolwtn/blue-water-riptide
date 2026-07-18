# Plan 01 — Single Player Mode (vs AI)

Single Player is the first connection mode built, and it doubles as the proving ground for every shared system in `00-game-design-overview.md`. One human (plus 2 AI teammates) vs 3 AI opponents, playing the "Sink or Swim" ruleset on Tideline Cove. No networking code is involved, but everything runs through the same `Session`/`InputCommand` seams the LAN mode will use.

## 1. Scene & flow

Four scenes in `Assets/Scenes/` (no map-select screen — v1 has one arena, loaded directly):

1. **`Boot`** — initializes services (save/profile load, catalogs, audio), then loads MainMenu. Keep it dumb; it exists so every other scene can assume services are up.
2. **`MainMenu`** — play button (→ TeamSelect), Voyage Road/progression screen, settings. Mode picker appears here once LAN ships; for the SP-only milestone the play button goes straight to TeamSelect.
3. **`TeamSelect`** — player picks their Sailor (Ace or Anchor in v1; locked Sailors shown greyed with unlock hint). Confirm → build the `Session` → load Match. With only one arena, arena choice is hard-wired here as `ArenaCatalog.Default`; when a map-select step is added later it becomes a field the TeamSelect (or a new screen) writes into the Session — match code never knows the difference.
4. **`Match`** — loads the arena prefab by id from the Session, spawns Sailors on spawn pads, runs `MatchController`, shows Results overlay at MatchEnd (trophies/Doubloons delta, rematch / back-to-menu buttons). Results is an overlay in the Match scene, not a fifth scene — cheaper to rematch.

Flow: `Boot → MainMenu → TeamSelect → Match (→ Results overlay) → rematch or MainMenu`.

## 2. Session construction for Single Player

`SinglePlayerLauncher` (in `Core/Modes/`) builds the Session:
- Participant 0: human, chosen Sailor, Team A, driver = `TouchInputDriver`.
- Participants 1–2 (teammates) and 3–5 (opponents): driver = `AIInputDriver`.
- **AI team selection with a 2-Sailor pool**: v1's "randomized opposing team" necessarily draws from {Ace, Anchor} with duplicates allowed (e.g. Ace/Ace/Anchor). Implement selection as a proper roster-sampling function now — `AITeamBuilder.Build(availablePool, constraints)` with constraints like "max 2 of one Sailor per team" and "at most 1 tank" — so it degrades gracefully today (constraints mostly no-op with a pool of 2) and scales automatically when the roster grows. Do not hard-code Ace/Anchor anywhere in AI code.
- Duplicate Sailors get visual tinting (team-colored kerchiefs) so mirrors are readable.

## 3. AI design

### Architecture
- **`AIInputDriver`** implements the same input-driver interface as touch: each AI tick it emits `InputCommand`s (move vector, aim, fire/super). AI never calls gameplay APIs directly — this guarantees AI, human, and (later) remote players are indistinguishable to the simulation, and it means AI can later serve as a drop-in for disconnected LAN players.
- Behavior = **utility-scored behavior states** (simpler than a full behavior-tree asset system, adequate for v1): each state (Engage, Retreat, Regroup, Flank, HoldChoke, SuddenDeathPush) computes a score from the world snapshot; highest score wins; minimum state dwell time of ~0.7s to prevent flip-flopping.
- World snapshot comes from a read-only `AIPerception` view over the simulation (positions, HP, ammo, grass concealment — AI must respect concealment: enemies in grass are invisible beyond adjacency, no cheating).

### Per-archetype behavior profiles
Behavior parameters live in a `AIBehaviorProfile` ScriptableObject referenced by `SailorDefinition`, keyed to attack archetype so future Sailors inherit sensible defaults:
- **`Serve` (Ace)**: keep preferred range band (~70% of max attack range), strafe perpendicular to threats, fire when line-of-sight and ammo ≥ 2, retreat when HP < 40% or ammo empty, use Super on ≥2 clustered enemies or to break destructible cover protecting a target.
- **`Spike` (Anchor)**: advance via cover toward nearest enemy, commit when within 1.5x slam range, body-block the net gap when defending a survivor lead, use Super to close on a fleeing low-HP target or to drop the ally-shield aura when ≥1 teammate is under 50% HP nearby.
- Profiles for `Lob`/`Set`/`Block` are defined when those Sailors ship; the profile system is the scaling point — new archetype = new profile asset, no AI code changes.

### Team-level coordination (lightweight)
A per-team `AISquadBrain` assigns simple shared intents: focus-fire target suggestion (lowest-HP visible enemy), lane assignments at round start (one AI north, one center/south), and "play safe" flag when holding a survivor advantage under 20s remaining (win-by-timeout awareness — critical for Sink or Swim).

### Difficulty
Three tiers (v1 default: Deckhand for first matches, then Bosun; expose in settings):
- **Deckhand (easy)**: 400ms reaction delay, ±12° aim error, never leads moving targets, uses Super late, ignores timeout tactics.
- **Bosun (normal)**: 250ms reaction, ±6° aim error, leads targets at 60% accuracy, decent Super timing.
- **Skipper (hard)**: 150ms reaction, ±2° aim error, full target leading, coordinated focus fire, plays the round timer deliberately.
Difficulty is a parameter set applied on top of behavior profiles — never separate behavior code per difficulty.

## 4. Script layout

| Path | Contents |
|---|---|
| `Assets/Scripts/Core/` | `MatchController`, `MatchRules` (SO), `Session`, `ParticipantId`, `IMatchClock`, `ArenaCatalog`, `SaveService`, `ProgressionService` (trophies/Doubloons/Voyage Road) |
| `Assets/Scripts/Core/Modes/` | `SinglePlayerLauncher`, `AITeamBuilder` |
| `Assets/Scripts/Gameplay/` | `ProjectileSystem` (straight/arcing), `CombatResolver` (damage/knockback/KO), `SuperChargeSystem`, `Court/` (walls, water, grass, spawn pads, `RisingTideHazard`), `Abilities/` (ability behavior components) |
| `Assets/Scripts/Characters/` | `SailorDefinition` (SO), `AbilityDefinition` (SO), `SailorAvatar` (scene-side pawn: animator, hitbox, HP bar hookup), `IInputDriver`, `TouchInputDriver` |
| `Assets/Scripts/AI/` | `AIInputDriver`, `AIPerception`, `AIBehaviorProfile` (SO), utility states, `AISquadBrain`, difficulty parameter sets |
| `Assets/Scripts/UI/` | virtual joystick, attack/super buttons + aim indicators, HUD (round pips, timer, teammate bars), TeamSelect screen, Results overlay, MainMenu screens |
| `Assets/Prefabs/Characters/` | Ace and Anchor pawn prefabs |
| `Assets/Prefabs/Projectiles/` | serve ball, barrage ball, slam AOE telegraphs |
| `Assets/Prefabs/Environment/` | Tideline Cove arena prefab + tile/spawn data asset, net/boat/log/coral props |

Keep `Assets/Scripts/Networking/` empty in this phase — nothing SP-specific may leak into it, and nothing network-specific may leak out of it later.

## 5. Milestones

**Implementation status (2026-07-18):** an out-of-order, code-only slice of M1+M2 exists in `Assets/Scripts/{Core,Characters,Gameplay,AI}` via `M1PrototypeBootstrap` (procedurally spawns everything at runtime — no scenes/prefabs authored). Ace *and* Anchor are both playable already (M2 pulled forward to prove the ability framework early, per its own stated reason below), but several M1 exit criteria are still unmet: input is keyboard/mouse, not touch; verified in the Unity Editor only, not on-device Android; the arena is a flat plane, not Tideline Cove's real geometry; rounds/timer/sudden death aren't implemented; the opponent is a stationary `DummyAIInputDriver`, not real utility AI. Treat this as a step ahead of the milestone checklist below, not a completed M1.

**M1 — Greybox vertical slice (the "is it fun" gate)**
- Tideline Cove greybox (primitive shapes), Ace only, human + 1 dummy AI opponent (stands and shoots).
- Movement, touch controls (joystick + tap/drag attack), straight projectiles, damage, KO, single-round Sink or Swim (no rounds/timer yet).
- Exit criteria: a full fight loop on-device (Android build) at 60fps.

**M2 — Real ruleset + real AI**
- Full Sink or Swim: best-of-3 rounds, 90s timer, survivor-count timeout, Rising Tide sudden death.
- Anchor implemented through the ability framework (melee AOE, leap Super, aura, knockback immunity) — this milestone deliberately forces the framework to prove it handles a non-projectile kit.
- Utility AI with Serve/Spike profiles, `AITeamBuilder`, 3v3 full matches, Bosun difficulty.
- TeamSelect scene, Results overlay, session flow end-to-end.

**M3 — Progression + difficulty spread**
- Save/profile, trophies, Voyage Road (Anchor unlock node), Doubloons, Sailor level-ups.
- Deckhand/Skipper difficulty tiers, `AISquadBrain` coordination, timeout-aware play.
- Grass concealment honesty check (AI perception audit), duplicate-Sailor tinting.

**M4 — Polish gate for SP release**
- Final art pass on both Sailors + arena, SFX/music hooks (`Assets/Audio/`), haptics, tutorial toast strip on first match, settings (difficulty, audio, joystick side).
- Performance pass on low-end Android (target: 60fps on a ~2019 mid-range device); Addressables for arena/Sailor assets.
- Balance pass: Ace vs Anchor matchup data from AI-vs-AI batch simulations (run headless in editor — a side benefit of the transport-agnostic sim).

Milestone M2's exit is the gate for starting Plan 02 work in parallel — the Session/InputCommand seams must be stable by then.

## 6. Remote vs Editor-only work

Tag tasks from the script layout/milestones above as **[File]** (AI/session C# logic — `AITeamBuilder`, `AIInputDriver`, `AIPerception`, utility states, `AISquadBrain`, difficulty sets, `SinglePlayerLauncher` — writeable without Unity open) or **[Editor/Asset]** (scene setup, Sailor pawn prefabs, TeamSelect/Results screen assembly, on-device 60fps verification). See the general rule in `00` §8 and the classified backlog in `.claude/napkin.md`.
