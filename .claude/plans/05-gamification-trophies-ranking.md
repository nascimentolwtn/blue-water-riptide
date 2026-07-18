# Plan 05 — Gamification: Trophies, Ranking & Rewards

Expands `00` §6's deliberately lightweight meta (per-Sailor Trophies → Voyage Road → Doubloons, local JSON save) into a full gamification plan. Nothing here contradicts `00`; every addition layers on top of that spine. Guiding constraint: **v1 has no backend** (Plan 03 defers online/accounts), so everything v1 ships must be computable from the local profile alone — and the save schema (§6 below) is designed so the future cloud/account system adopts it without migration pain.

## 1. Trophies — earning and losing

Per-Sailor score, as in `00` §6. Rates by **connection mode** (trust-tiered — untrusted paths earn capped, loss-free rates so a local-only save can't be griefed or trivially farmed into meaninglessness):

| Connection mode | Win | Loss | Rationale |
|---|---|---|---|
| Single Player | +2 | 0 | `00` §6's cap, carried forward. Local AI results are farmable; capped and loss-free keeps SP a low-stakes practice/progression path. |
| LAN — co-op vs AI | +2 | 0 | Plan 02 §5: LAN grants SP-tier rewards; each device applies its own progression from replicated results. |
| LAN — PvP | +2 | 0 | Same SP tier (Plan 02 §5). No trophy loss on LAN: results aren't trusted and losing trophies to a sibling on the couch is bad feel. |
| Online — solo queue (future) | +4 | −2 | The full rate from `00` §6 activates only when Plan 03's backend can validate results. Trophy-banded matchmaking makes −2 fair. |
| Online — friend lobby (future) | +2 | 0 | Private matches stay SP-tier (self-selected opponents; farmable). |

Rules:
- Trophies are earned by the Sailor played; a match's delta applies once, at Results, via `ProgressionService` (Plan 01 §4).
- Trophies never go below 0 per Sailor.
- **No trophy decay and no per-Sailor rank gates in v1.** Revisit only if online ranked ships.
- Disconnect/host-loss = no result = no delta (Plan 02 §4's "neither win nor loss").

**Account-level Trophy total** = sum of all per-Sailor trophies (already how `00` §6 drives the Voyage Road). It is derived — never stored as its own mutable field — so it can't drift from the per-Sailor records.

## 2. Fleet Rank — the v1 league-tier system

**Decision: v1 ships trophy-derived cosmetic tiers ("Fleet Rank"), not a seasonal ranked mode.** A seasonal ranked queue needs matchmaking, trusted results, and resets — all Plan 03 backend territory. Trophy-derived tiers need zero new systems: rank is a pure function of the account trophy total.

Tiers are ship classes (avoids colliding with Sailor names like *Ensign* Ace):

| Tier | Trophy total | Notes |
|---|---|---|
| Dinghy | 0 | start |
| Sloop | 40 | ~first evening of play |
| Cutter | 100 | past the Anchor unlock milestone |
| Frigate | 200 | |
| Cruiser | 350 | |
| Battleship | 550 | long-tail for v1's content |
| Flagship | 800 | aspirational cap for a 2-Sailor slice; extend thresholds as roster grows |

- Surfaced as a badge next to the trophy total on Home (Plan 04 §3) and in the LAN lobby player rows. Rank-up moment: full-screen flourish + a small Doubloon grant (one-time per tier, recorded in the save).
- Thresholds live in a `FleetRankDefinition` ScriptableObject (id-keyed tiers) so tuning and extension are data changes.
- **Future — "Flagship League" seasonal ranked (requires Plan 03):** separate ranked queue with its own seasonal points (Brawl-Stars-Ranked pattern: point thresholds → ranks, seasonal reset, end-of-season cosmetic rewards). Explicitly out of scope until online solo-queue exists and result trust is solved (Plan 03 §2's caveats: client-hosted ranked is marginal; may need dedicated servers). Fleet Rank stays as the permanent, non-resetting track alongside it.

## 3. Leaderboards

Scoped by what exists without a backend:

| Leaderboard | v1? | Backing |
|---|---|---|
| **Personal bests** — per-Sailor trophies, highest-ever trophies, win streaks, total KOs | v1 | Local save only. Shown on Sailor Detail + a "Records" panel in the profile. |
| **LAN session scoreboard** — trophy total / Fleet Rank of everyone in the current lobby | v1 | Ephemeral: each client already shares a small profile summary at lobby join (extend Plan 02's `LobbyState` with `{trophyTotal, fleetRankId}`). Bragging rights on the couch; nothing persisted about other players. |
| **Friends leaderboard** | Future | Requires accounts + friend list (Plan 03 §1 "later layers"). |
| **Global / regional leaderboards** | Future | Requires Plan 03's backend (authenticated ids, server-validated trophy submissions). Do not fake with local data. |
| **Per-Sailor global top lists** | Future | Same backend; note that per-Sailor trophies are already id-keyed in the save, so submission is a serialization job, not a schema change. |

## 4. Other gamification — prioritized list

Kept deliberately small for a 2-Sailor/1-arena slice. Everything v1 must be checkable from local state; nothing introduces a new currency or screen beyond Plan 04's map.

| # | Feature | Ship | Notes |
|---|---|---|---|
| 1 | **First win of the day** — first win each day (any mode) grants a Doubloon bonus (e.g. +20) | v1 | One field in the save (`lastFirstWinUtc`); shown as a badge on the PLAY button. Local clock is fine at zero stakes; re-validate server-side when cloud sync exists. |
| 2 | **Achievements ("Commendations")** — ~10–15 one-shot goals: First Knockout, Win a Match, Unlock Admiral Anchor, Win a Sudden Death round, Win 10 with Ace, Reach Frigate, Play a LAN match, Win a 3-human co-op match… each grants Doubloons or a cosmetic emblem | v1 | Id-keyed definitions in a `CommendationCatalog` SO; progress counters already tracked in stats (§6). Surface as a panel inside the profile/records screen (Plan 04 §5 — no new top-level screen). |
| 3 | **Voyage Road milestone moments** — already in `00` §6; add celebration UX (claim animation, "next unlock" preview on Home) rather than new mechanics | v1 | Pure presentation over the existing track. |
| 4 | **Match-end bonus objectives** — small Doubloon bumps at Results for in-match feats (3 KOs, no deaths, survived sudden death) | v1.x | Cheap (reads existing match stats), but cut first if the slice is tight. |
| 5 | **Daily quests ("Sailing Orders")** — 2–3 rotating tasks (win 2 matches, get 5 KOs with a `Spike` Sailor…) for Doubloons | Later | Wants a quest board surface and enough content variety to rotate against; thin with 2 Sailors/1 ruleset. Design when roster ≥ 4. |
| 6 | **Login streaks** | Later | Streak pressure is hostile for a casual local slice; revisit with live-service ops. |
| 7 | **Seasonal resets / season pass** | Later | Requires seasons, which require the Plan 03 backend; `00` §6 already excludes battle pass from v1. |
| 8 | **Flagship League seasonal ranked** | Later | §2 above. |

## 5. Doubloon economy sanity check

New Doubloon sources (first-win bonus, Commendations, rank-up grants, bonus objectives) all feed the single existing sink: Sailor level-ups (100/200/400/800 per `00` §6). Tune so a regular player fully levels both v1 Sailors in roughly 2–3 weeks of casual play — sources at that pace: ~40–60/day active play. Keep one spreadsheet (`.claude/plans/` sibling or design sheet) of all source/sink rates; every new source added here must be entered there. No premium currency, no purchasable Doubloons in v1 (`00` §6 exclusion stands).

## 6. Data model — local JSON save schema

Extends `00` §6's save (versioned schema, `Application.persistentDataPath`, id-keyed profile). Concrete field list — the contract is: **string content ids everywhere, UTC timestamps, monotonic counters, derived values never stored** — so Plan 03's cloud adoption is "upload + merge," not migration.

```json
{
  "schemaVersion": 2,
  "profileId": "guid-generated-locally",        // Plan 03 §3.6: maps to cloud id later
  "displayName": "Player",
  "createdAtUtc": "2026-07-17T00:00:00Z",
  "doubloons": 240,
  "sailors": {                                   // id-keyed, per 00 §6 "dictionary of typed progression entries"
    "sailor.ace": {
      "trophies": 84,
      "highestTrophies": 90,                     // needed NOW so future seasonal resets have a basis
      "level": 3,
      "wins": 31, "losses": 12,
      "knockouts": 118
    },
    "sailor.anchor": { "trophies": 40, "highestTrophies": 40, "level": 1, "wins": 9, "losses": 6, "knockouts": 25 }
  },
  "voyageRoad": { "claimedNodeIds": ["voyage.anchor-unlock", "voyage.doubloons-1"] },  // ids, never indices
  "fleetRank": { "claimedTierIds": ["rank.dinghy", "rank.sloop"] },                     // one-time rank-up grants
  "commendations": { "unlocked": { "comm.first-ko": "2026-07-16T19:02:11Z" } },
  "stats": {                                     // monotonic counters feeding Commendations/records
    "matchesByMode": { "singlePlayer": 30, "lanCoop": 8, "lanPvp": 5 },
    "suddenDeathWins": 4,
    "winStreakCurrent": 2, "winStreakBest": 6
  },
  "daily": { "lastFirstWinUtc": "2026-07-17T08:30:00Z" },
  "settings": { "aiDifficulty": "bosun", "joystickSide": "left", "musicVolume": 0.8, "sfxVolume": 1.0 }
}
```

Rules that prevent migration pain later:
- **Derived, not stored**: account trophy total and current Fleet Rank tier are computed from `sailors` / thresholds every read. Only *claims* (one-time grants) are stored.
- **Ids, not indices**: Voyage Road nodes, ranks, Commendations, Sailors all referenced by string id (Plan 03 §3.4) — content reordering never corrupts a save.
- **Counters only go up**: `stats` fields are monotonic, which makes the eventual cloud merge rule trivial (`max(local, cloud)` per counter; union of claimed/unlocked id sets; `max` of `highestTrophies`; current `trophies` takes the higher-timestamped write).
- **UTC timestamps** on anything time-gated (`lastFirstWinUtc`, unlock times) so server-side re-validation can replace client-clock trust without a schema change.
- **`schemaVersion` bump + explicit migrator** for any shape change from day one; never mutate meaning of an existing field.
- Multi-profile stays out of scope; one profile per device, keyed by `profileId` for the future account link (Plan 03 §3.6).

## 7. Sequencing

1. **With Plan 01 M3** (save/trophies/Voyage Road milestone): trophy rates table, derived account total, schema above, Fleet Rank (derivation + badge + rank-up grants), first-win-of-the-day.
2. **Plan 01 M4 / Plan 02 M4**: Commendations, personal records panel, LAN lobby scoreboard fields, celebration UX polish.
3. **Post-v1 (content-gated)**: match-end bonus objectives, Sailing Orders daily quests.
4. **Plan 03-gated**: global/friends leaderboards, Flagship League seasonal ranked, server-validated rewards.

## 8. Remote vs Editor-only work

`ProgressionService`, the JSON save schema (§6), and `FleetRankDefinition`/`CommendationCatalog` tier/id data are **[File]** work — pure C# + data, programmable without Unity open. The badges, claim animations, and panels that surface this data (Home widgets, Voyage Road screen, records panel — Plan 04's screens) are **[Editor/Asset]**. See the general rule in `00` §8 and the classified backlog in `.claude/napkin.md`.
