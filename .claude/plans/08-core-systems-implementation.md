# Core Systems Implementation Plan

Fable-model architectural plan for completing Backlog Item 1 (remaining Core shared systems): RoundCountdown, SuddenDeath, best-of-3 round loop, ArenaCatalog, SaveService.

**Status**: Step 1 (MatchRules.cs) complete. Step 2 (MatchController.cs) in progress.

---

## Overview

The M1 prototype currently has single-round-only logic (Setup → Combat → RoundEnd → MatchEnd). This plan extends it to support:
1. `RoundCountdown` state (timer → transition to `Combat`)
2. `SuddenDeath` phase under `Combat` (Rising Tide hazard, narrows playable area over time)
3. Best-of-3 round loop: `RoundEnd` checks score → if <2 wins, loop to next round; else `MatchEnd`
4. `ArenaCatalog` (loadable by arena id; start with just Tideline Cove v1)
5. `SaveService` (local JSON save schema for persistence — basics only for M1)

---

## Critical Findings

**Current `MatchController` gaps:**
- `_knockedOut` (HashSet<ParticipantId>) is **never cleared** — silently works today only because `MatchRules.M1Defaults.RoundsToWin = 1` means the match always ends before a second round could expose the bug.
- No timer of any kind — no round countdown, no 90s round timer, no sudden-death ring clock. `IMatchClock` exists but nothing calls it yet.

**Existing architecture:**
- `SailorPawn` already calls `_matchController.ReportKnockout(Id)` directly on death; confirms dependency direction is `Characters → Core`.
- Round-reset (HP/ammo/position/Super-retention/re-enabling render+collider) must follow same direction: MatchController fires event, pawns react.
- `M1PrototypeBootstrap` hardcodes arena half-extents and spawn positions inline — exact data `ArenaCatalog` should own instead.

**JSON serialization constraint:**
- No external JSON library in manifest (no Newtonsoft/System.Text.Json).
- `JsonUtility` doesn't serialize `Dictionary<TKey,TValue>` — the id-keyed `sailors` map in `05`'s schema needs a list-of-entries wrapper.

---

## State Transition Diagram

```
Setup
  │ StartMatch()
  ▼
RoundCountdown (round N, timer = MatchRules.RoundCountdownDuration)
  │  - clears _knockedOut
  │  - fires OnRoundCountdownStart(N) → host resets pawns to spawn pads,
  │    full HP, Super retained at 50% (round 1: 0%), spawn immunity
  │  - timer elapses
  ▼
Combat [phase = Normal]
  │  - round timer runs (MatchRules.RoundTimeLimit, 90s)
  │  - ReportKnockout(p) → elimination check
  │      ├─ one team fully KO'd ───────────────────────────┐
  │      └─ both teams still have survivors → stay Combat  │
  │  - round timer expires                                 │
  │      ├─ survivor counts unequal → winner = more-alive──┤
  │      └─ survivor counts tied → phase = SuddenDeath      │
  ▼                                                          │
Combat [phase = SuddenDeath]                                 │
  │  - RisingTideHazard ticks (ring shrinks every            │
  │    SuddenDeathRingInterval); DOT applied outside          │
  │    safe zone via host layer                               │
  │  - ReportKnockout(p) → elimination check                  │
  │      └─ one team fully KO'd ───────────────────────────────┤
  ▼                                                             ▼
RoundEnd (winner declared, dwell = MatchRules.RoundEndDuration)
  │  - OnRoundEnd(winner); _roundWins[winner]++
  │  - AdvanceRound():
  │      ├─ _roundWins[winner] >= RoundsToWin(2) ──► MatchEnd (terminal, OnMatchEnd)
  │      └─ else ──► loop back to RoundCountdown (round N+1)
  ▼
(loop to RoundCountdown, or MatchEnd)
```

---

## Critical Files & Classes

| File | Change | Reasoning |
|---|---|---|
| `Assets/Scripts/Core/MatchController.cs` | Modify | Add `MatchState.RoundCountdown`; add nested `CombatPhase { Normal, SuddenDeath }` (kept as a **sub-state under `Combat`**, not a new top-level `MatchState`, per `00` §7's explicit wording); add `Tick(IMatchClock clock)` driving countdown/round/sudden-death timers; fix the `_knockedOut.Clear()` bug; split `CheckRoundEnd` into elimination-check vs. `AdvanceRound()` (shared by both KO-triggered and timeout-triggered round ends); add `CurrentRound`, `OnRoundCountdownStart`, `OnCombatStart`, `OnSuddenDeathStart` events. |
| `Assets/Scripts/Core/MatchRules.cs` | Modify | ✅ **DONE** — Add `RoundCountdownDuration` (3f), `RoundEndDuration` (5f), `SuddenDeathRingInterval` (5f), `SuddenDeathDamagePerSecond`; add a `SinkOrSwimDefaults` factory (`RoundsToWin = 2`) alongside existing `M1Defaults` (kept as-is for single-round behavior). |
| `Assets/Scripts/Gameplay/Court/RisingTideHazard.cs` | New | Pure C# (no `SailorPawn`/`MonoBehaviour` reference) — given arena half-extents + ring interval/step, computes the current shrinking safe-zone rect from elapsed sudden-death time and exposes `IsPositionSafe(Vector3)`. Kept geometry-only so `Gameplay/Court` stays decoupled from `Characters`; damage application is wired at the host layer (bootstrap). |
| `Assets/Scripts/Core/ArenaCatalog.cs` | New | `ArenaDefinition` (Id, HalfExtents, spawn pad arrays per team, spawn immunity duration, Rising Tide params) + a static id-keyed catalog seeded with exactly one entry, `"arena.tideline_cove"`, using the values `M1PrototypeBootstrap` currently hardcodes. Exposes `ArenaCatalog.Get(id)` / `ArenaCatalog.Default`. |
| `Assets/Scripts/Core/SaveService.cs` + `Assets/Scripts/Core/ProfileData.cs` | New | `ProfileData` mirrors `05`'s schema (schemaVersion, profileId, displayName, createdAtUtc, doubloons, sailors) using `JsonUtility`-compatible shapes (`List<SailorSaveEntry>` instead of `Dictionary`). `SaveService.Load()`/`Save()`/`CreateDefault()`, atomic write (temp file + `File.Replace`) to `Application.persistentDataPath`. Include empty placeholder sections for future fields (voyageRoad, fleetRank, commendations, stats). |
| `Assets/Scripts/Characters/SailorPawn.cs` | Modify (small) | Add `ResetForRound(Vector3 spawnPosition, float superRetainFraction)`: restores HP/ammo, re-enables collider/renderer, clears `IsKnockedOut`, multiplies `SuperCharge01` by the retain fraction (0.5 per round-carry rule), grants spawn immunity. |
| `Assets/Scripts/Core/M1PrototypeBootstrap.cs` | Modify | Swap hardcoded arena numbers for `ArenaCatalog.Default`; use `MatchRules.SinkOrSwimDefaults`; add a small `MatchLoopHost : MonoBehaviour` (ticks `matchController.Tick(new UnityMatchClock())` every `Update()`) — thin glue matching the pattern of `M1Hud`/`TouchHud`; subscribe to round-lifecycle events to call `SailorPawn.ResetForRound`; apply Rising Tide DOT while `CombatPhase == SuddenDeath`; call `SaveService.Load()` once at boot (smoke check). |

---

## Implementation Order (7 Steps, Dependency-Ordered)

### Phase: Configuration & Core Logic (Steps 1–2)
1. **MatchRules.cs** — new fields + `SinkOrSwimDefaults`. Zero dependencies, unblocks everything else's numbers. **[DONE]**
2. **MatchController.cs** — state/phase enums, `Tick`, `AdvanceRound`, `_knockedOut.Clear()` fix, new events. Core of the task; everything downstream consumes its events. Can be validated with unit-style manual testing (pure C#, no Unity APIs) before touching any Unity-side files.

### Phase: Supporting Systems (Steps 3–4, parallel with Step 2)
3. **ArenaCatalog.cs** — independent of `MatchController`; can be built in parallel with step 2. Needed before bootstrap rewiring (step 6).
4. **Gameplay/Court/RisingTideHazard.cs** — depends only on `ArenaCatalog`'s data shape (half-extents/ring params), not on `MatchController` internals.

### Phase: Character & Bootstrap Integration (Steps 5–6, depend on 1–4)
5. **SailorPawn.ResetForRound** — small, isolated addition; depends on nothing new.
6. **M1PrototypeBootstrap.cs** — the integration point; depends on 1–5 all being done. This is where the state machine becomes observable/testable.

### Phase: Persistence (Step 7, fully parallel with 1–6)
7. **SaveService.cs + ProfileData.cs** — fully independent of the round-loop work; can be done any time. Lowest-risk, do-anytime piece.

---

## Complexity & Risk

| Piece | Complexity | Risk Flags |
|---|---|---|
| `MatchRules` additions | Low | None — pure data. ✅ **DONE** |
| `MatchController` (Tick, phases, best-of-3, `_knockedOut.Clear()` fix) | **Medium-High** | **Highest-risk piece.** Timing edge cases: a knockout landing in the same tick as the round-timer expiring; sudden-death entered exactly as the last survivor dies; round-end dwell racing a new `ReportKnockout` call if a stray pawn event fires after `RoundEnd` starts. Needs explicit guard clauses (state checks) at every entry point, matching the existing `if (State != MatchState.Combat) return;` style already in the file. |
| `ArenaCatalog` | Low | Straightforward id-keyed lookup; only risk is picking spawn-pad values that don't match what the eventual real Tideline Cove prefab (backlog item 4) uses — acceptable since that's explicitly a later reconciliation. |
| `RisingTideHazard` | Medium | Getting the shrink-rect math right (which edges recede, at what rate) against `00` §5's "floods inward from the south inlet and outer boundary" is somewhat interpretive for a greybox (no real geometry yet) — a simplified concentric/rect inset is the pragmatic M1 stand-in; flag this as a deliberate simplification, not a bug, until the real arena exists. |
| `SailorPawn.ResetForRound` | Low | Small, localized change to an existing class. |
| `M1PrototypeBootstrap` rewiring | Medium | Touches the one file everything currently runs through; needs care not to regress the already-working single-round M1 slice (Ace/Anchor kits, touch input) while switching it to best-of-3. |
| `SaveService`/`ProfileData` | Low-Medium | Mechanically simple, but `JsonUtility`'s no-`Dictionary` limitation and IL2CPP AOT quirks (already a recurring theme per napkin's Execution & Validation entries) mean the dictionary-via-list workaround needs to be gotten right the first time; also needs atomic-write handling to avoid a corrupted save if the app is killed mid-write on Android. |

---

## Success Criteria (Verifiable in M1PrototypeBootstrap)

1. **Round 2 visibly happens**: Losing a round no longer ends the match outright when `RoundsToWin = 2`. After one team is fully knocked out, pawns visibly reset to spawn positions at full HP, ammo, and roughly half their prior Super charge, and combat resumes for round 2 — proving `_knockedOut` correctly clears and `RoundCountdown` fires.

2. **Sudden Death triggers**: Letting the round timer run out with both Sailors alive transitions into a visibly narrowing playable area (test via `M1Hud` logging phase/safe-zone or a debug `Debug.Log` on phase change) rather than the match hanging.

3. **Best-of-3 loop works**: The match only ends (`OnMatchEnd`) after one team reaches 2 round wins, not 1.

4. **Restart is clean**: Restarting the scene (`R` key) works cleanly with the new tick-driven `MatchController`, with no leaked timers/events from the previous match instance.

5. **Arena data from catalog**: Arena half-extents and spawn positions are now read from `ArenaCatalog.Get("arena.tideline_cove")` instead of inline bootstrap constants — verifiable one-line change swap.

6. **SaveService smoke test**: A `Debug.Log` at boot shows a `SaveService.Load()` round-trip (default profile created on first run, same values reloaded on a second run).

7. **Performance maintained**: 60fps on-device, no regression from Tick loop.

---

## Notes & Assumptions

**Assumption flagged for confirmation**: The "5s reset" language in `00` §1 is modeled as `RoundEnd`'s own dwell time (pawns already reset to spawn/full HP during that window) followed by a fresh `RoundCountdown` (3s) for the next round — i.e. ~8s total dead time between rounds. This is a reasonable reading but not explicit in the docs; worth a one-line confirmation before final polish, non-blocking for implementation.
