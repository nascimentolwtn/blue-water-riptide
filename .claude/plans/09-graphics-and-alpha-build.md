# Plan 09 — Graphics, Audio & Alpha Build

Turns the code-only M1 prototype (`CLAUDE.md` project-status note: "no hand-authored scenes/prefabs/art yet") into a presentable local-LAN alpha: real Sailor models, a real Tideline Cove, audio, and UI graphics for the screens `04-menu-navigation-lobby.md` specifies. Read `00-game-design-overview.md` (roster/arena spec), `04-menu-navigation-lobby.md` (screens needing art), `06-splash-screen-and-icon.md` (established navy palette + bell mark, the only finished-art precedent in the repo), and `.claude/plans/07-asset-prompts.md` (the existing master prompt library — this plan does not replace it) before touching this doc.

**Relationship to `07-asset-prompts.md`**: `07` is the full prompt library (roadmap roster included, 11 sections). This plan is the *execution plan* for the alpha-critical subset of it (2 v1 Sailors, 1 arena, UI, plus audio/props `07` doesn't cover) — sequencing, ownership, checklists, and file destinations. The per-asset prompt files this plan creates under `.claude/prompts/` are adapted/split from `07`'s inline prompts (reused, not rewritten) plus new audio/props prompts `07` never covered. Don't duplicate prompt text between the two docs when editing later — `07` is the source of truth for prompt wording; this plan is the source of truth for sequencing/ownership.

## 0. Overview

### 3D vs 2D pipeline decision

**Decision: stay 3D, low-poly stylized.** Reasons, grounded in what already exists:
- The prototype is already 3D end-to-end: `SailorPawn` capsules, a perspective top-down-angled camera (`M1PrototypeBootstrap.BuildCamera`, position `(0,20,-14)` looking at origin — not an orthographic 2D top-down), URP `Universal Render Pipeline/Lit` materials (`SailorPawn.CreateUrpMaterial`). Switching to 2D now discards this, not extends it.
- `Assets/Art/Characters/` already has sourced 3D packs (`vietnam-coast-guard.zip`, `vietnam-people-navy.zip`, `us-navy-mechanic.zip`, plus two volleyball packs) and one is already extracted and rigged/imported (`Vietnam Coast Guard/Venerated.fbx`, wired into `M1PrototypeBootstrap` as a placeholder pawn — see §1's flag below on why this needs a decision, not silent reuse).
- The user's own asset-sourcing memory (`project_asset_sourcing_strategy.md`) is written entirely in 3D-model terms (Sketchfab/Poly Haven/itch.io/Quaternius/Kenney/Mixamo, Tripo fallback) — this is the established, already-followed playbook.
- Packages installed (`Packages/manifest.json`) include `com.unity.2d.sprite`/`com.unity.2d.tilemap` (Unity default template inclusions, unused so far) alongside `com.unity.cinemachine` and full 3D/physics modules — nothing indicates 2D was ever the direction.
- **Fallback contingency, not the plan**: if the free-tier sourcing playbook (07 §11) repeatedly fails to find an acceptable 3D marketplace character after its documented 30-minute rule, 2D top-down sprites (using the already-installed 2D packages) become the documented fallback — re-open this decision explicitly with the user at that point rather than silently switching mid-project.

### Timeline estimate (parallel tracks, no-budget/free-tier sourcing)

| Track | Owner | Est. duration | Blocks |
|---|---|---|---|
| Sailor graphics (Ace + Anchor: source/generate, rig-verify, retint, animate) | Artist (+ agent for prompt drafting) | 1.5–2 weeks, both in parallel | Sailor prefab assembly (backlog item 3) |
| Arena graphics (Tideline Cove greybox → dressed geometry) | Artist/Dev | 1 week (layout is fully specified in `00` §5 — this is sourcing+placement, not design) | Backlog item 4 |
| Props (net-walls, rowboat, driftwood, coral, water, grass) | Artist (+ agent) | 3–5 days, parallel with arena | Arena dressing pass |
| Audio (music loop, SFX set, voice barks) | Artist/Sound (+ agent) | ~1 week, fully parallel with all art | Polish pass, not a blocker to playable alpha |
| UI graphics (buttons, HP bars, lobby screens, icons) | Artist/Dev | ~1 week, parallel once `04`'s screens are being built | Backlog item 7 |
| LAN networking code | Dev | See `10-lan-mode-implementation.md` — fully parallel, no art dependency | — |

**Critical path to a "looks decent" LAN alpha**: Sailor graphics + Arena graphics (the two Editor-heavy, longest tracks) run in parallel, ~1.5–2 weeks, gated behind them is prefab/scene assembly (Editor-only work, backlog items 3/4/7). Audio and UI icons can complete earlier and simply wait to be wired in — they never block the critical path. LAN networking code (`10`) has zero art dependency and should start immediately, in parallel with art sourcing.

### Style baseline (applies to every category below)

Per `07-asset-prompts.md`'s established baseline (no other style guide exists in the repo): **stylized 3D-render mobile-game look, slightly heroic/chibi proportions, clean cel-shading, comparable visual weight to Brawl Stars/Clash Royale.** UI icons use a flatter vector variant of the same palette (per `06`'s bell-mark icon spec). Palette anchors already established and not to be re-litigated per-asset: navy `#0E2A47` (Blue Water Squad / splash / icon), proposed steel-grey/teal/rust for Anchor Guard (`07` §1b, flagged pending sign-off).

### Open flag carried from asset discovery (resolve before Sailor prefab work starts)

`M1PrototypeBootstrap.cs` currently spawns a Sailor named **"Venerated"** (`SailorDefinitionData.Venerated`, using the imported `Vietnam Coast Guard/Venerated.fbx` model) as the *player* pawn, with the real roster's Ace only as the AI-controlled opponent. `Venerated` is not on the locked v1 roster (napkin guardrail #2: "Ensign Ace + Admiral Anchor only... don't expand roster... without the user explicitly asking"). Treat this as a **rig/import pipeline smoke test that never got cleaned up**, not a third roster member. Before real Sailor prefab work (backlog item 3) starts, decide explicitly with the user:
1. Repurpose the already-imported, already-rigged `Venerated.fbx` as a placeholder body for Ace or Anchor (fastest path — skip a sourcing cycle), retinted/re-dressed to match, with the `Venerated` identity fully removed from code; or
2. Treat it as import-pipeline validation only and source dedicated models per-Sailor from scratch.
Either is fine — just don't let a 3rd, non-canon "Sailor" persist silently into the alpha build.

---

## 1. Sailor graphics — Ensign Ace, Admiral Anchor

Needed for: in-game pawn (`SailorPawn`/`Characters/SailorAvatar`, `01` §4), Sailor Locker tiles, Sailor Detail portraits, TeamSelect/Lobby pick rows, HUD teammate portraits, Results overlay (`04` §3).

### 1a. In-game 3D pawn model

- **Grounding**: `SailorDefinitionData.EnsignAce`/`AdmiralAnchor` (`Assets/Scripts/Characters/SailorDefinitionData.cs`) already fix each Sailor's silhouette-relevant stats — Ace: Normal speed, 1300 HP, ranged `Serve` archetype; Anchor: Slow speed, 1900 HP tank, melee `Spike` archetype, knockback-immune. Model proportions should read these differences at a glance (Ace leaner/faster-looking, Anchor broader/heavier) — this is exactly what `07-asset-prompts.md` §1's portrait prompts already establish (heroic-tank proportions for Anchor vs. Normal-speed proportions for Ace); carry the same silhouette logic into the 3D model.
- **Sourcing path** (per `project_asset_sourcing_strategy.md`, no-budget phase): Sketchfab → Poly Haven → itch.io → OpenGameArt → Quaternius → Kenney → Mixamo, in that order, 30 minutes each before falling back. Target ~3k–5k tris (Android IL2CPP/ARM64 budget, S21 FE baseline per napkin). Fallback: Tripo free tier (~16 gens/month) using the prompt in `.claude/prompts/sailor_ace_pawn_model.md` / `sailor_anchor_pawn_model.md`.
- **Rigging**: needs a bone skeleton for animation (walk, basic-attack windup/release, Super, knockback, KO). Marketplace-sourced models are often pre-rigged; Tripo output is not — plan a Mixamo auto-rig pass if generation is used.
- **Animation set** (minimum for alpha, per ability behaviors actually implemented — `Assets/Scripts/Gameplay/Combat/`): Idle, Move (walk/run cycle), Basic Attack (Ace: `ProjectileAbilityBehavior` 3-shot burst windup+release; Anchor: `MeleeArcAbilityBehavior` frontal swing), Super (Anchor only has one implemented — `LeapSlamAuraAbilityBehavior`, needs a leap+slam animation; Ace's Super/`Ace Barrage` isn't implemented in code yet per `SailorDefinitionData.EnsignAce`'s `Super = null` — don't animate a Super for Ace until that lands), Knockback reaction, Knockout/ragdoll-or-collapse. Mixamo's free animation library covers generic humanoid walk/idle/hit-react adequately as a starting retarget target if the sourced model is Mixamo-rig-compatible.
- **File destination**: `Assets/Art/Characters/EnsignAce/`, `Assets/Art/Characters/AdmiralAnchor/` (sibling to the existing `Vietnam Coast Guard/` folder), final assembled prefabs in `Assets/Prefabs/Characters/` (already scaffolded, currently empty except `.gitkeep`).
- **Manual polish checkpoints** (artist, [Editor/Asset]):
  - [ ] Import verified: no missing/corrupted geometry, materials render correctly under URP (watch for the same "legacy Standard shader renders magenta" issue `SailorPawn.CreateUrpMaterial`'s comment already documents for primitives — imported FBX materials can hit this too if not re-mapped to a URP shader).
  - [ ] Poly count within mobile budget (~3–5k tris); decimate in Blender if a marketplace source overshoots.
  - [ ] Rig verified (bone hierarchy sane, no broken weights) before any animation work.
  - [ ] Palette matches the established squadron colors (navy/white/gold for Ace, steel-grey/teal/rust for Anchor) via material override/retint — hand-painting deferred past alpha, per the sourcing memory.
  - [ ] Silhouette readable at HUD/minimap distance and distinct between Ace and Anchor (the core "can I tell who's who at a glance" test for a 3v3 arena brawler).
  - [ ] Duplicate-Sailor kerchief tint hook (`01-single-player-mode.md` §2) has an obvious attach point (a material slot or child object) on the finished prefab.

### 1b. Character portraits (2D, for menu/HUD screens)

Fully specified already in `07-asset-prompts.md` §1 — reuse those prompts verbatim, split into per-Sailor files for batch submission: `.claude/prompts/sailor_ace_portrait.md`, `.claude/prompts/sailor_anchor_portrait.md`. Target Gemini, square 1:1, min 1024x1024.
- **File destination**: `Assets/Art/UI/Portraits/` (new subfolder, sibling to `Assets/Art/UI/Splash/` and `Assets/Art/UI/Icon/` from `06`).
- **Manual polish checkpoints**: transparent/croppable background confirmed; readable at Locker-tile size (~128px) and HUD teammate-bar size (~48px); Variant A (3D-render) vs Variant B (flat vector) style choice locked once picked — don't ship a mix of both styles across the two Sailors.

---

## 2. Arena graphics — Tideline Cove

Backlog item 4 (`.claude/napkin.md`): arena prefab + tile/spawn data, registered in `ArenaCatalog` (`Assets/Scripts/Core/ArenaCatalog.cs` — already has the data shape and exactly one entry, `arena.tideline_cove`, with placeholder half-extents/spawn points that currently drive the flat-plane greybox). Full layout spec: `00-game-design-overview.md` §5.

- **Geometry list** (from `00` §5, already itemized in `07-asset-prompts.md` §6's concept-art prompts): two 7-tile volleyball net-wall segments at midcourt (x=17, 6-tile gap), one beached rowboat (3x2 solid), four driftwood logs (2x1 solid, two per team's diagonal approach), two destructible coral rock clusters (2x2, one per half), a 3-tile-deep tidal inlet (shallow water, south edge), two 2x3 dune grass patches per half (north edge, concealment).
- **Lighting**: current bootstrap uses a single directional light (`M1PrototypeBootstrap.BuildLighting`, intensity 1.2, `Euler(50,-30,0)`) — fine as a starting point for a bright tropical-daylight look; alpha polish should add a soft ambient/skybox matching the turquoise-to-deep-blue horizon gradient `07` §6's overview prompt describes, plus a subtle rim/fill light so Sailors don't read flat against bright sand.
- **Decals/ground texture**: current ground is a flat single-color URP material (`SailorPawn.CreateUrpMaterial(new Color(0.2f,0.45f,0.65f))` — this is actually a water-blue tint standing in for sand, worth flagging as an easy first fix even before real texture art lands: swap to a warm sand tone). Real pass needs a tiled sand texture, the tidal-inlet water as a separate shallow-water shader/material (slowing movement 30% per `00` §5 is a gameplay rule, not a visual one — visual only needs to read as "shallow water" at a glance), and dune-grass texture/mesh clumps.
- **AI generation prompts**: `.claude/prompts/tideline_cove_arena.md` (overview concept art) and `.claude/prompts/props_tideline_cove.md` (per-prop generation/sourcing, see §4 below — arena geometry and props are split into two files since props are individually sourced/placed while the arena file is concept-art-only).
- **File destination**: arena prefab + tile/spawn data asset → `Assets/Prefabs/Environment/` (scaffolded, empty). Update `ArenaCatalog.TidelineCoveId`'s entry to reference real spawn pad transforms once the prefab exists — the catalog's shape (`ArenaDefinition`) already supports this without a schema change.
- **Manual polish checkpoints** ([Editor/Asset], per `00` §8 — even greybox primitives require placing geometry in a scene):
  - [ ] All terrain rules from `00` §5 hold structurally: solid cover blocks `Serve`-type projectiles (verify against `Projectile.cs`'s `TemporaryObstacle`-style collision, which currently only recognizes ability-spawned walls, not static level geometry — **flag for dev**: static arena colliders need the same trigger-based hit detection `Projectile.OnTriggerEnter` already does for spawned walls, or projectiles will pass through real level geometry once it exists).
  - [ ] Destructible coral cluster has both intact and shattered states/prefab variants (`07` §6 already specs a before/after concept prompt for this).
  - [ ] Spawn pads placed and immunity radius/visual telegraph reads clearly (`ArenaDefinition.SpawnImmunityDuration`, currently 1.5f).
  - [ ] Rising Tide flood visual (`RisingTideHazard.cs`'s shrinking safe-zone rect) has a matching ground-level VFX so the mechanic is legible, not just a math boundary — `07` §6 already has a flood-visual concept prompt.
  - [ ] Three lanes (north stealth grass, center net-gap, south water flank per `00` §5's design intent) are visually distinguishable from a top-down camera angle, not just data-distinguishable.

---

## 3. Audio

Not covered in `07-asset-prompts.md` (image-only library) or any existing plan doc — new ground. `Assets/Audio/Music/` and `Assets/Audio/SFX/` already exist as empty scaffolded folders.

### Style reference

Matches the game's tone: arcade sports-brawler energy over the navy/volleyball theme, not military-realistic and not somber. Percussive, upbeat, tropical-tinged — think steel-drum/marimba melodic elements over light electronic/EDM percussion, the same "confident, a little cocky" energy `docs/History.md`/`07`'s Ace portrait prompt describes for the Blue Water Squad, balanced against a slower, weightier motif for Anchor Guard material (per `07`'s steel-grey/teal palette — a lower, brassier register rather than a full mood shift). **Cartoonish, not realistic**, in the SFX specifically: attacks are volleyball-serve/anchor-slam themed, not gunfire — a "Jump Serve" should sound like a stylized sports-whoosh-and-splash, not a projectile weapon.

### 3a. Music

- **Menu loop**: calmer, ~80–100 BPM, establishes the tropical/nautical motif without combat urgency. Loops seamlessly (no audible seam at the loop point) — a hard requirement for any menu-screen background track.
- **Combat loop**: faster, ~120–140 BPM, percussive, matches a ~90s round length (`MatchRules.RoundTimeLimit`) without needing to be exactly that duration — it should loop cleanly under a variable-length round.
- **Sudden Death sting/loop variant**: a tension layer or filter-shift on the combat loop once `CombatPhase.SuddenDeath` triggers (`MatchController.OnSuddenDeathStart` event already exists to hook this) — doesn't need to be a fully separate track for alpha, a mix/intensity change is enough.
- **Generation**: Suno (per user direction) for the composed loops — see `.claude/prompts/audio_music.md` for ready-to-paste prompts and the **usage-rights caveat**: verify Suno's specific plan tier's commercial-use terms before shipping past internal alpha testing (free/basic tiers have historically carried usage restrictions that differ from paid tiers) — flag as unconfirmed, don't assume clearance.
- **File destination**: `Assets/Audio/Music/Menu_Loop.ogg`, `Assets/Audio/Music/Combat_Loop.ogg` (Ogg Vorbis — Unity's standard compressed format for looping music on Android).

### 3b. SFX

Grounded directly in what's implemented in code today (don't script SFX for abilities that don't exist yet, e.g. Ace's Super):

| Event | Trigger (code hook) | Sound direction |
|---|---|---|
| Jump Serve fire | `SailorPawn.Update()` → `Definition.BasicAttack.Execute(this)` (Ace) | Sports-whoosh + light "thwack" |
| Jump Serve impact | `Projectile.OnTriggerEnter` → `CombatResolver.ApplyDamage` | Soft splash/impact, not a gun hit |
| Anchor Slam swing | `MeleeArcAbilityBehavior.Execute` | Heavy whoosh + ground thud |
| Anchor Slam impact/knockback | Same, on a hit | Deep impact + a "sand kicking up" texture |
| Drop Anchor Super (leap+slam+aura) | `LeapSlamAuraAbilityBehavior.Execute` | Heavy metallic anchor-drop clang + splash, followed by a soft sustained aura hum for the 6s buff window |
| Knockout | `SailorPawn.ApplyDamage` → `IsKnockedOut = true` branch | Short "out" stinger, distinct per team so it's parseable in a 3v3 mix |
| Round start / countdown | `MatchController.OnRoundCountdownStart` | Ship's-bell ding (reuses the bell motif from `06`'s splash mark — good thematic consistency) |
| Round end / match end | `MatchController.OnRoundEnd`/`OnMatchEnd` | Short fanfare (win) / lower stinger (loss) |
| Sudden Death start | `MatchController.OnSuddenDeathStart` | Alarm/tide-rising audio cue, matches the visual flood cue from §2 |
| UI button tap | Any menu screen (`04`'s screens, not built yet — SFX just needs to exist and get wired when they are) | Short, clean nautical-bell-click, consistent across all buttons |
| Doubloon/reward gain | Voyage Road/Results progression moments | Coin-chime, bright and short |

- **Generation/sourcing**: Kenney's free UI/SFX audio packs (kenney.nl) as a first pass for generic UI clicks (matches the existing free-asset-sourcing playbook's philosophy — check free packs before generating); Gemini's audio-adjacent tools or a dedicated SFX generator (e.g. ElevenLabs' sound-effects endpoint, if available on a free tier) for the bespoke combat hits described above. Prompts: `.claude/prompts/audio_sfx.md`.
- **File destination**: `Assets/Audio/SFX/`, one clip per row above, named to match the event (e.g. `SFX_JumpServe_Fire.wav`, `SFX_AnchorSlam_Impact.wav`, `SFX_Knockout.wav`).

### 3c. Voice lines (Sailor callouts)

Short barks only for alpha — not full VO. Per-Sailor personality already established in `docs/History.md`/`07-asset-prompts.md` §1: Ace is young, cocky, "a half-step ahead of his own confidence"; Anchor is calm, weathered, an elder-statesman mentor tone.

| Moment | Ace direction | Anchor direction |
|---|---|---|
| Round start / spawn | Confident, quick — "Let's move!" energy | Steady, low-key — "Anchor's down." energy |
| Basic attack (occasional, not every hit — avoid VO spam) | Short competitive callout | Grunt/effort sound, minimal words |
| Super use | A confident one-liner (once Ace's Super exists — for now, Anchor only) | A weighty "Drop Anchor!"-style callout |
| Knockout scored | Quick taunt-lite (kept friendly, not toxic — this is a couch-LAN alpha) | A calm, respectful line — matches his mentor role, not a gloat |
| Own knockout | Short "aw, come on" | Unbothered, dry |

- **Generation**: TTS (user-suggested) — see `.claude/prompts/audio_voice_lines.md` for per-line scripts and voice-direction notes. **Usage-rights caveat**: use only a TTS service's stock/licensed synthetic voices, never a voice-cloning feature targeting a real person (celebrity or otherwise) — this is a hard constraint, not a style preference, to avoid rights/likeness issues in a shipped game.
- **File destination**: `Assets/Audio/SFX/VO/` (voice lines are short one-shots like SFX, not loopable music — group under SFX with a VO subfolder rather than inventing a third top-level Audio category).
- **Alpha scope note**: voice lines are the first thing to cut if the timeline is tight (per the coordinator's parallelism note below) — SFX and music carry more of the "feels finished" weight per minute of effort than barks do.

---

## 4. Props / environmental detail objects

New ground (not in `07`). These are Tideline Cove's individual placed objects, as opposed to §2's overall arena geometry/lighting pass — split out because each prop is sourced/generated and placed independently, on its own checklist.

| Prop | Count | Solid/destructible | Sourcing approach |
|---|---|---|---|
| Volleyball net-wall segment | 2 (midcourt, mirrored) | Solid (blocks movement + `Serve`) | Sketchfab/Kenney first (generic net+post prop is common); Tripo fallback |
| Beached rowboat | 1 (mid-court, offset) | Solid | Sketchfab/itch.io first (hero prop, worth spending full sourcing budget); Tripo fallback using `07` §6's rowboat concept prompt as the generation spec |
| Driftwood log | 4 (2 per team, diagonal approaches) | Solid | Generic — Kenney/Quaternius nature packs are a strong fit, likely free and fast |
| Coral rock cluster | 2 (one per half), each needs intact + shattered states | Destructible | Sketchfab (coral/rock assets are common); shattered-state may need a simple in-Editor fracture (Unity's built-in mesh tools or a quick fractured-variant model) rather than a second full sourcing pass |
| Tidal inlet (shallow water) | 1 strip, south edge | Terrain/shader, not an object | Not a sourced model — a water shader/material (URP has built-in simple water options) + a slow-zone trigger volume (gameplay logic, not art) |
| Dune grass patch | 4 (2 per half, north edge) | Concealment terrain | Kenney/Quaternius grass-clump packs; needs enough density to plausibly conceal (per `00` §5's "hidden unless an enemy is adjacent" rule) without tanking mobile performance — instancing/GPU-friendly grass, not individually placed high-poly meshes |

- **AI generation prompts**: `.claude/prompts/props_tideline_cove.md` (covers the props that do need generation — rowboat, net-wall, coral cluster; the others are marketplace-sourced per the table and don't need a generation prompt).
- **File destination**: `Assets/Prefabs/Environment/Props/` (new subfolder under the existing scaffolded `Assets/Prefabs/Environment/`).
- **Manual polish checkpoints**:
  - [ ] Every solid prop has a working collider sized to its visual footprint (napkin's IL2CPP `CreatePrimitive` Collider-stripping gotcha, Execution & Validation #5, applies here too if any prop uses `GameObject.CreatePrimitive` as a placeholder before real geometry lands — check `Assets/link.xml` covers whatever Collider type ends up used).
  - [ ] Coral cluster's shattered-state swap is wired to whatever destroys it (destructible-cover mechanic isn't implemented in code yet per the backlog — this checkpoint is art-readiness, not a claim the mechanic exists).
  - [ ] Poly budget sanity check across the *whole* dressed arena, not just per-prop — mobile perf is a scene-total concern (napkin: target 60fps on S21 FE baseline).
  - [ ] Grass patches don't visually break the "hidden unless adjacent" concealment rule by being too sparse/see-through.

---

## 5. UI graphics

Needed for the screens `04-menu-navigation-lobby.md` specifies (Home, Locker, Voyage Road, Mode Select, Local Play stack, Match HUD, Results). Icon-level prompts already largely exist in `07-asset-prompts.md` §4/§8/§9 (icon, trophy/doubloon/rank/commendation icons, HUD control icons) — this section is about assembling them into actual screen graphics, which `07`'s prompt library correctly stops short of (it's image-asset prompts, not screen layout).

- **HP bars**: not covered anywhere yet — new. Simple, readable at a glance for both self and 2 teammates + up to 3 visible enemies in a 3v3 HUD. Flat-fill bar, team-colored border (ties into the duplicate-Sailor kerchief tint convention from `01` §2), red/low-HP state distinct enough to read peripherally during combat.
- **Buttons**: attack/Super action buttons already speced (`07` §9's HUD control icon set); menu buttons (PLAY, Locker, Settings, etc. from `04`'s Home screen) need a consistent button-frame treatment — reuse the navy/gold palette, rounded-rect or circular per `04`'s described layout, consistent corner radius/border treatment across every screen so the UI reads as one system.
- **Lobby screens**: Host-or-Join, Host Config, Lobby player rows (`04` §3, Local Play section) — player row template (name, Sailor pick thumbnail using the portraits from §1b, ready-flag toggle state, team-color grouping).
- **File destination**: `Assets/Art/UI/HUD/`, `Assets/Art/UI/Buttons/`, `Assets/Art/UI/Lobby/` (new subfolders alongside the existing `Splash/`/`Icon/`).
- **AI generation prompts**: `.claude/prompts/hud_and_meta_icons.md` consolidates the HUD control set, round-pip/timer icons, and meta icons (trophy/doubloon/Fleet Rank/Voyage Road/Commendations) from `07` §4/§8/§9 into one batch-submittable file — split further only if a specific icon needs individual iteration.
- **Manual polish checkpoints**: legible at actual render size (test at target device DPI, not just at generation resolution — `07`'s icon prompts already call out "design at 512px, downscale to 32-64px" as the right workflow); consistent icon family (same line weight/corner style across the whole set, not a mismatched grab-bag); HP bar and Super-charge indicator states (empty/partial/full/greyed-cooldown) all mocked before implementation, not just the "full" state.

---

## 6. Remote vs Editor-only work

Following the convention in `00` §8 / `06` §3: prompt-writing, this plan doc, and the `.claude/prompts/*.md` files themselves are **[File]** work. Everything downstream of "art exists as a generated/sourced file" is **[Editor/Asset]** — importing, rigging, retexturing, prefab assembly, scene placement, Player Settings wiring, on-device visual verification. Audio import/compression settings and AudioSource wiring onto the ability-behavior/event hooks listed in §3b are also **[Editor/Asset]** (the event hooks themselves — `MatchController`'s events, `SailorPawn.ApplyDamage`, etc. — already exist as [File]-built C# and don't need to change; only *subscribing* an AudioSource trigger to them is new work, and that wiring happens in-Editor or in a thin MonoBehaviour glue script).
