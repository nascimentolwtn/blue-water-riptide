# Blue Water Riptide — Game Design Overview

Top-down 3v3 mobile arena battle game in the Brawl Stars pattern, themed around navy sailors and beach volleyball. **The theme is cosmetic**: playable characters are Sailors, attacks are named after volleyball moves, arenas are beach courts with volleyball nets as scenery/obstacles — but the game rules are a straightforward Brawl-Stars-style arena fight (basic attack + charged Super, team-elimination objective), not a sport simulation. Unity 2022 LTS+, C#, Android-first (iOS later). The mode plans (`01`–`03`) all build on the systems defined here.

**v1 content scope: 2 playable Sailors and 1 arena.** The systems below are designed for the full game; the content is a deliberate vertical slice. See "Content Roadmap" (section 4).

## 1. Core Concept

### What the game is
- **3v3 top-down arena battles.** Each player controls one Sailor with a **basic attack** (ammo/charge-based, ranged or melee) and a **Super** that charges by dealing damage. Knock out the opposing team to win.
- **Marine/volleyball theming only.** An attack may be *called* "Jump Serve" and *look* like a served volleyball, but mechanically it is a projectile attack like any hero-shooter kit. No ball-carrying, no scoring zones, no sport rules.

### Two independent axes (don't conflate them)
1. **Connection mode** — how players get into a match: Single Player vs AI (`01`), Local Network/LAN (`02`), Online (`03`, future).
2. **Ruleset** — the win condition of a match. v1 ships **one** ruleset, used by all connection modes.

### v1 ruleset: "Sink or Swim" (knockout pattern)
- **3v3, best of 3 rounds, no respawns within a round.**
- A round ends when one team is fully knocked out (that team loses the round), or when the **90-second round timer** expires — then the team with more surviving Sailors wins the round.
- Survivor tie at timeout → **Rising Tide sudden death**: water floods the arena inward from the edges (one tile ring every ~5s; flooded tiles deal damage over time) until a decisive knockout ends the round.
- First team to 2 round wins takes the match (~3–5 minutes total). Between rounds: 5s reset, everyone back to spawn pads at full HP; Super charge carries over at 50%.
- Chosen because it is the simplest objective to implement (no mid-round respawn logic, unambiguous end state) while producing tense, short mobile matches.
- **Easy later variant** (not in v1, but the match state machine must not preclude it): "Deck Battle" — single 2:30 timed period, 4s respawns, most knockouts wins. Same systems with respawns enabled and a KO counter as the win condition.

### Match flow
`Round countdown (3s) → Combat → Round end (team KO / timeout) → next round or match results`.
Knocked-out players spectate teammates until the round ends.

### Controls (mobile touch, input-agnostic via Input System)
- **Left virtual joystick**: movement (floating origin on the left half of the screen).
- **Right attack button**: tap = quick attack at nearest target (soft auto-aim); touch-drag = aim indicator (line for shots, area marker for AOE/leaps); release = fire; drag back onto the button = cancel.
- **Super button** (above attack): same tap/drag-aim behavior; greyed until charged.
- All gameplay actions resolve through an `InputCommand` abstraction (move vector, aim vector, action id, timestamp) so touch, gamepad, AI, and network drivers are interchangeable.

## 2. Character framework

Every Sailor follows the same pattern:
- **Basic attack** — ammo-style: 3 charges, each regenerating over ~1.5–2.5s (per character). Ranged projectile, melee/AOE, or utility depending on the Sailor.
- **Super** — charges by dealing damage to enemies; once full it persists until used (carries between rounds at 50%).
- **Trait** — one small always-on passive reinforcing the character's identity (v1 implements traits; activated gadget-style items are post-v1).

Attack archetypes — volleyball-flavored labels for standard mechanical categories (used by stats, AI, and UI filtering):
- `Serve` — long-range straight projectile
- `Lob` — long-range arcing projectile (clears walls; artillery)
- `Spike` — short-range heavy hit / AOE slam
- `Set` — support/utility (heals, buffs)
- `Block` — defensive (shields, walls, zone denial)

## 3. Roster — v1: 2 Sailors

Two characters chosen to maximize contrast and prove the ability framework end-to-end: one ranged projectile attacker, one short-range AOE tank. Between them they exercise straight projectiles, melee AOE, knockback (dealing and immunity), a barrage Super, a leap+aura Super, and squishy-vs-tanky balance.

Stats are level-1 baselines. HP scale ~900 (fragile) to ~1900 (tank); damage is per basic-attack hit; speed tiers: Slow / Normal / Fast / Very Fast. Rarity tiers: **Starter, Common, Rare, Epic, Legendary** (rarity = unlock order and concept complexity, not raw power).

### Ensign Ace — The Blue Water Squad
- **Rarity**: Starter (every player begins with Ace)
- **Role**: All-rounder / ranged damage · **Archetype**: `Serve`
- **Stats**: 1300 HP · Normal speed · 140 damage per hit · 3 ammo, 1.7s reload each
- **Basic attack — Jump Serve**: mid-to-long-range straight projectile (a served volleyball), fired as a quick 3-round burst along the aim line. Blocked by walls and cover — rewards lane control and flanking.
- **Super — Ace Barrage**: five fast serves in a widening fan; each deals 140 and moderate knockback. Clears grouped enemies and shreds destructible cover.
- **Trait — Fresh Legs**: +10% move speed for 3s after landing a knockout.
- Playstyle: the "learn the game" character — positioning, aiming, poke damage, kiting.

### Admiral Anchor — The Anchor Guard
- **Rarity**: Rare (first Voyage Road unlock in v1)
- **Role**: Tank / frontline control · **Archetype**: `Spike`
- **Stats**: 1900 HP · Slow speed · 190 damage per slam · 3 ammo, 2.4s reload each
- **Basic attack — Anchor Slam**: close-range AOE ground pound in a frontal arc with strong knockback. Bullies chokepoints and punishes anyone who lets him close the distance.
- **Super — Drop Anchor**: leaps to an aimed spot (clears walls — his gap-closer) and anchors there: 200 impact damage plus a 6s aura granting nearby allies +40% damage reduction. Used to engage, to peel for a teammate, or to hold ground in sudden death.
- **Trait — Ballast**: immune to knockback.
- Playstyle: the "space owner" — absorb pressure, control chokepoints, win the shrinking-arena endgame.

Framework check: Ace vs Anchor covers ranged vs melee, burst vs single heavy hit, mobility Super vs zone Super, squishy-normal vs tanky-slow — enough variety to validate `SailorDefinition`/`AbilityDefinition` composition before scaling the roster.

## 4. Content Roadmap (post-v1 expansion path)

**Roster Roadmap.** Ace and Anchor are the vertical-slice roster. The rest of `docs/sailors.md`'s squadron structure is the planned expansion path, added in small batches with no framework changes (new Sailor = new `SailorDefinition` + prefab + ability data):
- **The Blue Water Squad** (navy discipline — balanced/tactical): next up **Captain Cove** (controller — wall-spawning `Block` Super) and **Sailor Smash** (bruiser); **Deep Blue** reserved as the first Legendary (delayed-strike `Lob` artillery).
- **The Riptide Rush** (fast, aggressive): next up **Riptide Rookie** (Common skirmisher — good early-unlock candidate) and **Surge Serve** (long-range marksman); later **Foam Flier** (assassin), **Stormy Spike** (ramping damage).
- **The Anchor Guard** (defensive): next up **Tide Turner** (Common support — first healer, exercises the `Set` archetype) and **Coral Claw** (defensive specialist).
- Priority: fill missing archetypes first — `Set` support (Tide Turner), `Lob` artillery, then assassin/controller kits.

**Arena Roadmap.** Tideline Cove (section 5) is the single vertical-slice arena. Future arenas vary along three axes: hazard mix (pier gaps, crab swarms, storm winds that push projectiles), size/layout (open courts vs corridor-heavy docks, asymmetric cover), and theme (harbor deck, coral lagoon, night storm). Arena loading is data-driven from the start (arena = prefab + tile/spawn data referenced by id) so a map-select step slots in later without touching match code.

## 5. The v1 arena: Tideline Cove

One hand-built symmetric beach court; no map pool and no map-select screen in v1 — matches load this arena directly. The volleyball net and beach scenery are **obstacles and dressing** under standard terrain rules.

- **Dimensions**: 34 x 20 tiles (1 tile = 1 Unity unit), long axis is the play axis. Team A spawns west, Team B east.
- **Spawn pads**: 3 per team on their back line; round starts place Sailors here with 2s of damage immunity.
- **Terrain rules** (generic, theme-skinned): solid cover blocks movement and straight projectiles (`Serve`); arcing attacks (`Lob`, leap Supers) clear it; destructible cover breaks under Supers; shallow water slows movement 30%; dune grass conceals (hidden unless an enemy is adjacent).
- **Layout (mirrored per half)**:
  - **Volleyball nets**: two 7-tile net-wall segments at midcourt (x = 17) with a 6-tile central gap — the arena's signature chokepoint. Mechanically ordinary solid cover; visually the beach-volleyball centerpiece.
  - A beached **rowboat** (3x2 solid) mid-court, offset north — the main flank-fight anchor.
  - Two **driftwood logs** (2x1 solid) guarding each team's diagonal approaches.
  - One **coral rock cluster** (2x2 destructible) south of each rowboat — a Super can open a new firing lane mid-round.
  - **Tidal inlet**: shallow water 3 tiles deep along the south edge — a coverless, slow flank route around the net's south segment.
  - **Dune grass**: two 2x3 patches per half along the north edge — the stealth approach.
- **Rising Tide sudden death**: floods inward from the south inlet and outer boundary, one tile ring every ~5s, forcing the final fight toward the central net gap.
- Design intent: three distinct lanes — north (stealth grass), center (net-gap fight), south (slow water flank) — so contrasting kits (Ace's range vs Anchor's chokepoint control) and future roster additions have meaningful route choices.

## 6. Progression & meta (deliberately lightweight for v1)

- **Trophies** — per-Sailor score: +4 win / −2 loss (Single Player: +2/0, capped contribution). Total trophies drive the **Voyage Road**, a single linear reward track.
- **Voyage Road** unlocks (v1): **Admiral Anchor** at an early trophy milestone (Ace is the starter), then Doubloon caches and cosmetic flags/sail emblems; later nodes are pre-labeled for future Sailors/arenas so the track visibly extends with content updates.
- **Doubloons** — the only currency in v1. Earned from match results and Voyage Road nodes. Spent on **Sailor level-ups** (levels 1–5, +5% HP and damage per level; costs 100/200/400/800).
- Explicitly **out of scope for v1** (design later; don't build hooks beyond data-model headroom): premium currency, battle pass, gadget/star-power equivalents ("Trick Plays"), skins shop, clubs.
- Persistence: local JSON save (`Application.persistentDataPath`), versioned schema. Keep per-Sailor records extensible (dictionary of typed progression entries) and the profile object id-keyed so a future cloud-save/account system can adopt it (see plan `03`).

## 7. Shared systems (all connection modes depend on these)

These live in `Assets/Scripts/Core` and `Assets/Scripts/Gameplay` and must stay **transport-agnostic**: pure C# simulation driven by `InputCommand`s and ticked by an `IMatchClock`, with no direct references to Netcode, UI, or input devices. Single Player runs them locally; LAN runs them host-authoritative; Online reuses them unchanged.

1. **Match state machine** (`Core/MatchController`):
   `Setup → RoundCountdown → Combat → RoundEnd → (next round | MatchEnd) → Results`, with a `SuddenDeath` sub-state under Combat. Emits events (`OnKnockout`, `OnRoundEnd`, `OnMatchEnd`) that UI/audio/network layers subscribe to. All rule numbers (round timer, rounds to win, respawn policy) live in a `MatchRules` ScriptableObject — the respawn-policy field is what later enables the "Deck Battle" variant without new states.
2. **Character & ability framework** (`Characters/`, `Gameplay/Abilities`): `SailorDefinition` ScriptableObject (stats, archetype, prefab refs, rarity, squadron) + `AbilityDefinition` assets composed from reusable behaviors (projectile spawn, dash/leap, area effect, aura, wall spawn). Adding a Sailor = data + prefab, no new mode code. Ace and Anchor must be built *through* this framework, not hard-coded.
3. **Combat resolution** (`Gameplay/Combat`): projectiles (straight vs arcing — arcing ignores walls), melee/AOE hits, damage, knockback, HP, knockout handling, Super-charge accounting.
4. **Court/arena** (`Gameplay/Court`): walls (net segments, boats, logs), destructible cover, water tiles, grass concealment, spawn pads, Rising Tide hazard. An arena is a prefab + tile/spawn data asset in `Assets/Prefabs/Environment`, loaded by id via an `ArenaCatalog` (single entry in v1: Tideline Cove).
5. **Session model** (`Core/Session`): describes a match-to-be (connection mode, ruleset id, arena id, participants with `ParticipantId`, chosen Sailor, team, driver type: human/AI/remote). Every connection mode constructs one and hands it to `MatchController` — this seam keeps SP/LAN/Online symmetric.
6. **Camera & HUD**: Cinemachine top-down follow with slight aim-lead; HUD (joystick, attack/super buttons, round pips, timer, teammate status) in `UI/`, driven only by Core events.

## 8. Remote vs Editor-only work

When breaking any plan (this one or `01`-`05`) into implementation tasks, tag each as **[File]** (pure C# scripts, ScriptableObject *class* definitions, JSON/data, interfaces, algorithms — writeable/reviewable without opening Unity) or **[Editor/Asset]** (scene composition, prefab assembly, Inspector wiring, importing art/audio/animation, Player Settings, on-device testing — needs the Unity Editor open). Prefer expressing tunable values as code-level defaults so more work qualifies as [File]; only the act of instantiating/wiring the resulting `.asset` or prefab in the Editor is [Editor/Asset]. The current classified split lives in `.claude/napkin.md` under Backlog — keep it in sync as items complete or new ones are added.
