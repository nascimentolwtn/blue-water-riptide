# Blue Water Riptide — Asset Generation Prompt Library

Ready-to-paste prompts for external AI generation services. Default image target is **Gemini** (natural-language prose, not tag-soup). 3D-model targets are marked **Service TBD** — no generation service has been chosen for this project yet (see note in §10).

No hand-authored art exists in the repo yet (`CLAUDE.md` project-status note; `06-splash-screen-and-icon.md` §4.5 confirms the bell mark would be the *first* finished art). Because no style guide exists, every character/prop prompt recommends a baseline style — **stylized 3D-render mobile-game look, slightly heroic/chibi proportions, clean cel-shading, comparable visual weight to Brawl Stars/Clash Royale character art** — as the closest analog to this game's described genre pattern (`00-game-design-overview.md`: "top-down 3v3 mobile arena battle game in the Brawl Stars pattern"). This is a recommendation pending real art-direction sign-off, not a locked decision — flagged the same way `06`'s open questions are flagged.

## Table of Contents
1. [Character Portraits — v1 Roster (Ensign Ace, Admiral Anchor)](#1-character-portraits--v1-roster)
2. [Character Portraits — Roadmap Roster (9 future Sailors)](#2-character-portraits--roadmap-roster)
3. [In-Game Character Pawns / 3D Models (Service TBD)](#3-in-game-character-pawns--3d-models-service-tbd)
4. [App Icon (Android Adaptive/Legacy/Round + Play Store)](#4-app-icon)
5. [Splash Screen Art](#5-splash-screen-art)
6. [Arena Environment — Tideline Cove](#6-arena-environment--tideline-cove)
7. [Projectiles &amp; Ability VFX](#7-projectiles--ability-vfx)
8. [Meta/UI Icons (Trophy, Doubloons, Fleet Rank, Voyage Road, Commendations)](#8-metaui-icons)
9. [HUD Elements](#9-hud-elements)
10. [Notes on Service-TBD Assets](#10-notes-on-service-tbd-assets)

### Source-doc map

| Category | Sourced from |
|---|---|
| Character portraits &amp; pawns | `00-game-design-overview.md` §3 (roster), `01-single-player-mode.md` §4 (prefab layout), `History.md` §2–3 (squadron identity, per-character hooks), `.claude/plans/sailors.md` (name brainstorm, tone only) |
| App icon | `06-splash-screen-and-icon.md` §2 |
| Splash art | `06-splash-screen-and-icon.md` §1, `History.md` §1 (Tideline Cup bell trophy) |
| Arena environment | `00-game-design-overview.md` §5 (Tideline Cove layout), `01-single-player-mode.md` §4 (`Assets/Prefabs/Environment/`) |
| Projectiles/VFX | `00-game-design-overview.md` §3 (ability descriptions), `01-single-player-mode.md` §4 (`Assets/Prefabs/Projectiles/`) |
| Meta/UI icons | `04-menu-navigation-lobby.md` §3 (Home/Locker/Voyage Road screens), `05-gamification-trophies-ranking.md` §2–4 (Fleet Rank tiers, Commendations) |
| HUD | `00-game-design-overview.md` §7.6, `04-menu-navigation-lobby.md` §3 (Match HUD) |

---

## 1. Character Portraits — v1 Roster

These are the two Sailors actually shipping in v1 (`00` §3) — highest priority. Needed for: Sailor Locker tiles, Sailor Detail screen, TeamSelect picker, Lobby pick rows, HUD teammate portraits, Results overlay (`04` §3).

### 1a. Ensign Ace

Grounding: Starter rarity, Blue Water Squad ("Do it right, or do it again" — disciplined, proper uniforms even on the beach), youngest officer and the squad's top scorer, `Serve` archetype (ranged volleyball-serve attacks), Normal speed/1300 HP (not a heavy build), "constantly a half-step ahead of his own confidence" (`History.md` §3). Recommended palette: deep navy `#0E2A47` (the splash/icon navy, already established in `06` as Blue Water's identity color) with white and gold trim, since no character color palette is specified elsewhere in the docs.

**Variant A — Action portrait, stylized 3D-render style (recommended default)**
```
A 3/4-view character portrait of a young, confident naval ensign on a tropical beach volleyball court, in a stylized 3D-render mobile-game art style with clean cel-shading and slightly heroic proportions (comparable to Brawl Stars character art). He wears a fitted navy-blue and white sailor's uniform adapted for beach sport — rolled-up sleeves, gold ensign rank insignia on the collar, dog tags, deck shoes — looking sharp and disciplined despite the sand underfoot. He's caught mid-motion winding up to serve a volleyball, cocky half-smile, eyes locked forward like he's a half-step ahead of his own confidence. Background is a soft blurred gradient of ocean blue and sandy beige so he reads clearly as a standalone character icon. Lighting is bright, warm, tropical daylight. Plain or transparent background preferred for UI cropping.
```

**Variant B — Flat vector illustration style (alternate direction)**
```
A flat vector illustration portrait of a young naval ensign, bust-up 3/4 view, in a bold graphic mobile-game icon style (thick clean outlines, flat color fills, minimal shading — think modern flat-design app icons rather than painterly art). Navy-blue and white beach-sailor uniform with gold trim and ensign insignia, confident smirk, mid-serve pose with a volleyball just leaving his hand. Solid deep-navy (#0E2A47) or transparent background so the character silhouette pops.
```

**Target**: Gemini | **Aspect/size**: square (1:1), min 1024x1024 for downscaling to Locker tile/portrait sizes | **Sources**: `00-game-design-overview.md` §3, `History.md` §2–3, `06-splash-screen-and-icon.md` (navy palette precedent)

### 1b. Admiral Anchor

Grounding: Rare rarity, Anchor Guard ("Nothing gets past us" — grew out of harbor-defense crews and lighthouse keepers), senior commander, "the closest thing the whole Cup has to an elder statesman," `Spike` archetype (close-range AOE ground pound), Slow speed/1900 HP tank, knockback-immune ("Ballast" trait), mentors younger Sailors across all three squadrons (`History.md` §3). No palette is specified for the Anchor Guard anywhere in the docs — I'm choosing a weathered steel-grey/harbor-teal with iron/rust accents specifically to contrast against Blue Water's navy blue, since the two squadrons need to read as visually distinct; flag for sign-off if a different squadron palette is preferred.

**Variant A — Action portrait, stylized 3D-render style (recommended default)**
```
A 3/4-view character portrait of a large, weathered, older naval officer in a stylized 3D-render mobile-game art style with clean cel-shading and heroic-tank proportions (broad shoulders, grounded stance — comparable to a "tank" character archetype in Brawl Stars-style games). He wears heavy-duty harbor-defense gear over a dark uniform — a weathered peacoat or foul-weather jacket left open at the beach, salt-stained, with faded admiral's rank insignia — steel-grey and teal color palette with rust-orange accents, suggesting decades of harbor and lighthouse duty rather than parade-ground polish. Grey beard, calm and immovable expression, one hand resting on a massive ship's anchor slung over his shoulder like a weapon. Background is a soft blurred gradient of stormy ocean grey-blue so he reads clearly as a standalone character icon. Lighting is overcast, moody, coastal. Plain or transparent background preferred for UI cropping.
```

**Variant B — Flat vector illustration style (alternate direction)**
```
A flat vector illustration portrait of a large, grey-bearded naval officer, bust-up 3/4 view, in a bold graphic mobile-game icon style (thick clean outlines, flat color fills, minimal shading). Steel-grey and teal weathered coat with rust-orange trim, calm immovable expression, a massive anchor resting over one shoulder. Solid deep-navy (#0E2A47) or transparent background so the character silhouette pops, with enough color contrast against Ensign Ace's palette that the two read as different squadrons at a glance.
```

**Target**: Gemini | **Aspect/size**: square (1:1), min 1024x1024 | **Sources**: `00-game-design-overview.md` §3, `History.md` §2–3

### Duplicate-Sailor tinting note (`01-single-player-mode.md` §2)
Duplicate Sailors on a team get "team-colored kerchiefs" for readability. This is a small in-engine tint/overlay applied to the base model, not a separate generated asset — no prompt needed unless the team later wants a hand-illustrated kerchief prop reference.

---

## 2. Character Portraits — Roadmap Roster

Not needed for v1 (`00` §3 confirms exactly 2 playable Sailors), but `History.md` §3 locks in personalities for the full expansion roster ahead of time, and `00` §4 names the build order. Included here so concept art can get ahead of implementation. One variant each, same recommended default style as Ace/Anchor Variant A for roster consistency — reuse each squadron's palette (Blue Water = navy/white/gold; Anchor Guard = steel-grey/teal/rust; Riptide Rush has no established palette in any doc, so I'm proposing a bright energetic teal/orange/yellow "fast and loud" palette to contrast the other two disciplined squadrons — flag for sign-off).

### The Blue Water Squad (navy/white/gold palette)

**Captain Cove** *(controller, wall-spawning Block Super)*
```
A 3/4-view stylized 3D-render mobile-game portrait of a calm, sharp-eyed naval tactician in a crisp navy-and-white beach-sailor uniform with gold trim, captain's insignia on the collar. Unsettlingly composed expression, arms crossed, standing beside a solid wall of stacked sandbags/nets she's just planted — she doesn't chase the fight, she decides where it happens. Clean cel-shading, blurred ocean-blue background, transparent-friendly crop.
```

**Sailor Smash** *(bruiser)*
```
A 3/4-view stylized 3D-render mobile-game portrait of a hulking former cargo-crew sailor built from years of manual labor, navy-and-white uniform straining at the shoulders, sleeves torn off entirely. No finesse in his stance — mid-swing, about to spike a volleyball with brute force, gritted-teeth grin. Clean cel-shading, blurred ocean-blue background, transparent-friendly crop.
```

**Deep Blue** *(first Legendary — delayed-strike Lob artillery)*
```
A 3/4-view stylized 3D-render mobile-game portrait of the most senior, unshakeable Sailor in the Blue Water Squad — weathered navy-and-white dress uniform with the most decorated collar of the squad, an unreadable, patient half-smile, standing perfectly still while a faint glowing arc telegraphs a distant strike about to land somewhere off-frame. A "served on something bigger than a patrol boat once" gravitas — subtly legendary, more gold trim and a sense of quiet authority than the rest of the squad. Clean cel-shading, blurred deep-ocean background, transparent-friendly crop.
```

### The Riptide Rush (proposed teal/orange/yellow palette — not specified in docs)

**Riptide Rookie** *(Common skirmisher)*
```
A 3/4-view stylized 3D-render mobile-game portrait of a young, grinning, high-energy small-boat sailor in a bright teal-and-orange beach-sport outfit, mid-sprint, hair windblown, thrilled expression like she just crashed a party she wasn't invited to. No polish, all momentum. Clean cel-shading, blurred bright-beach background, transparent-friendly crop.
```

**Surge Serve** *(marksman)*
```
A 3/4-view stylized 3D-render mobile-game portrait of a focused, precise sailor in a teal-and-orange Riptide Rush outfit, one eye narrowed in aim, mid-serve on a volleyball with tight controlled form (unlike his squadmates) — the one Rush Sailor who actually aims before firing. Clean cel-shading, blurred bright-beach background, transparent-friendly crop.
```

**Foam Flier** *(assassin)*
```
A 3/4-view stylized 3D-render mobile-game portrait of a lithe, showy sailor in a teal-and-yellow Riptide Rush outfit caught mid-air in an acrobatic flanking leap, playing to an imaginary crowd, confident smirk, motion-blur suggestion of speed. Clean cel-shading, blurred bright-beach background, transparent-friendly crop.
```

**Stormy Spike** *(ramping damage)*
```
A 3/4-view stylized 3D-render mobile-game portrait of a moody, intense sailor in a dark teal-and-orange Riptide Rush outfit, storm clouds gathering subtly behind him as if he brings his own weather, competitive glare, crackling energy building around his serving arm the longer he's watched a fight. Clean cel-shading, blurred stormy-beach background, transparent-friendly crop.
```

### The Anchor Guard (steel-grey/teal/rust palette)

**Tide Turner** *(Common support — first Set/healer)*
```
A 3/4-view stylized 3D-render mobile-game portrait of a warm, capable quartermaster-medic in steel-grey and teal Anchor Guard gear, holding a healing kit styled like a ship's supply satchel, encouraging half-smile like she's hyping up a teammate mid-match. Clean cel-shading, blurred grey-ocean background, transparent-friendly crop.
```

**Coral Claw** *(defensive specialist)*
```
A 3/4-view stylized 3D-render mobile-game portrait of a rugged salvage diver turned defensive specialist, steel-grey and teal Anchor Guard gear with sharp reef-hook attachments, weathered from reef and rock work, stubborn dig-in stance like he's claimed a piece of ground and dares anyone to take it. Clean cel-shading, blurred grey-ocean background, transparent-friendly crop.
```

**Target**: Gemini | **Aspect/size**: square (1:1), min 1024x1024 | **Sources**: `00-game-design-overview.md` §4, `History.md` §2–3

---

## 3. In-Game Character Pawns / 3D Models (Service TBD)

The plan docs describe "Sailor pawn prefabs" and a `SailorAvatar` scene-side component (animator, hitbox, HP bar hookup) — `01-single-player-mode.md` §4 — but **do not specify whether the in-game pawn is a 2D sprite or a 3D model**, and this project has not chosen a 3D-generation service (Meshy, Tripo, Luma, Kaedim, or sourcing from a marketplace like Sketchfab are the usual options — see §10). Do not assume one; confirm with the user before generating.

If/when a 3D-model service is chosen, the portrait prompts in §1/§2 above already establish outfit, palette, and proportions and can be adapted into that service's expected prompt format (typically shorter, spec-like: subject, pose, style tag, "T-pose," "game-ready," "low-poly," topology/polycount targets, PBR texture request). Example adaptation for Ensign Ace once a service is picked:

```
Ensign Ace: young naval ensign character, stylized low-poly game-ready 3D model, T-pose, navy-blue and white beach-sailor uniform with gold trim, confident/cocky expression, slightly heroic-chibi proportions matching a Brawl Stars-style mobile arena game. Clean PBR textures, mobile-optimized topology.
```

**Target**: Service TBD — ask the user which service before proceeding, or research current capabilities/pricing via WebSearch if asked to recommend | **Sources**: `01-single-player-mode.md` §4, `00-game-design-overview.md` §7.2

---

## 4. App Icon

Fully specified in `06-splash-screen-and-icon.md` §2 — the bell mark, no wave/net accent (too fine for small sizes), bronze/gold glyph on navy `#0E2A47`.

**Adaptive foreground layer (bell glyph, transparent)**
```
A single ship's bell icon glyph, viewed straight-on, rendered as a clean, solid, high-contrast bronze/gold silhouette with simple flat shading (no fine texture or barnacle detail — this must stay legible at very small sizes). The bell should be centered, simple, and bold enough to read clearly as a tiny app-icon-sized shape: a classic nautical hand bell with a small mounting bracket at top and a visible clapper hint at the bottom opening. Transparent background. No text, no additional elements — just the bell glyph, evenly padded so it comfortably fits within a circular or rounded-square safe crop area.
```

**Adaptive background layer**
```
A flat, solid deep-navy fill, color #0E2A47, completely even with no gradient, texture, or content — a plain background tile for an Android adaptive app icon.
```

**Legacy/Round composited icon (bell pre-composited on navy)**
```
A square Android app icon: the same bronze/gold ship's bell glyph (clean solid silhouette, no fine texture, simple flat shading, classic nautical hand bell with mounting bracket and clapper hint) centered on a flat solid deep-navy (#0E2A47) background, with the bell padded with clear margin so it survives being cropped into a circle for round-icon variants. No text, no additional elements.
```

**Play Store listing icon (512x512)**
```
A 512x512 app icon, 32-bit PNG with alpha, showing the same bronze/gold ship's bell glyph (clean solid silhouette, simple flat shading, no fine barnacle texture, classic nautical hand bell shape) centered on a flat solid deep-navy (#0E2A47) background, generously padded, crisp enough to read clearly at both full size and thumbnail size in a store listing. No text.
```

**Target**: Gemini | **Aspect/size**: square 1:1; export/downscale per the size table in `06-splash-screen-and-icon.md` §2 (432/324/216/162/108/81px adaptive; 192/144/96/72/48/36px round & legacy; 512x512 Play Store) | **Sources**: `06-splash-screen-and-icon.md` §2

---

## 5. Splash Screen Art

Fully specified in `06-splash-screen-and-icon.md` §1. Same bell mark as the icon, reused (not a separate concept), but *with* the wave/net accent and more texture since it renders at a larger safe size. Used both for Unity's native Splash Screen logo entry (§1a) and the `Boot` scene's own splash UI (§1b) — same file, `Splash_Bell_Logo.png`.

**Splash logo (bell mark with wave/net accent)**
```
A clean, flat, high-contrast logo mark: a barnacle-crusted bronze ship's bell, rendered with subtle weathered texture (light barnacle/verdigris detailing on the bell's surface, suggesting age and years at sea) and a small stylized wave crest curling beneath it, with a faint suggestion of netting rope looping around the base. The mark should be squadron-neutral — no team colors, no volleyball, no ball or net-as-sport-equipment imagery — just the bell as a trophy object with subtle nautical dressing. Centered composition, transparent background, bronze/gold bell tones against nothing (for placement over a solid deep-navy #0E2A47 background later). Style: clean vector-adjacent flat illustration with soft dimensional shading, no photorealism, reads clearly as a single unified mark at both large and small display sizes.
```

**Target**: Gemini | **Aspect/size**: transparent PNG, bell mark centered within a 1920x1080 (16:9) safe area (export at 2x / 3840x2160 recommended per doc), landscape-only (this project locks landscape orientation) | **Sources**: `06-splash-screen-and-icon.md` §1, `History.md` §1

---

## 6. Arena Environment — Tideline Cove

`00-game-design-overview.md` §5 fully specifies the single v1 arena's layout. These are concept-art prompts to guide either hand-modeling or a future 3D-asset pipeline (arena is built as Unity prefabs + tile data per `01-single-player-mode.md` §4, so final geometry work happens in-Editor, not from generated images — but concept art is a legitimate Gemini use here for establishing look/mood before that work starts).

**Arena overview concept art (top-down/isometric establishing shot)**
```
A top-down isometric concept illustration of a symmetric tropical beach volleyball-court-style arena for a mobile arena battle game, 34 tiles wide by 20 tiles deep with the long axis running left-to-right. At the center, two volleyball net-wall segments block the middle of the court with a gap between them — a chokepoint. A beached wooden rowboat sits mid-court, offset to one side, surrounded by scattered driftwood logs near each team's diagonal approach corners. Two clusters of coral-covered rocks sit near the rowboat. Along one long edge (the south edge), a shallow tidal inlet of pale turquoise water cuts a flanking lane; along the opposite edge, patches of tall dune grass offer concealment. Warm tropical daylight, bright sand, turquoise-to-deep-blue ocean gradient at the horizon. Clean stylized mobile-game environment art style (bold colors, simplified geometric forms, no photorealism) matching a Brawl Stars-style top-down arena game. No characters in frame — environment only.
```

**Hero prop: volleyball net-wall chokepoint**
```
A close-up concept illustration of a weathered volleyball net strung between two sturdy wooden posts, functioning as solid arena cover (blocks movement and straight-line attacks) in a stylized mobile-game art style. Sun-bleached rope netting, sand-worn wooden posts, taut cabling, set against a bright sandy beach backdrop. Clean, slightly stylized/cartoonish rendering — bold shapes and saturated colors, not photorealistic.
```

**Hero prop: beached rowboat**
```
A close-up concept illustration of an old, weathered wooden rowboat half-beached in sand, tilted at a slight angle, used as solid arena cover in a top-down mobile arena game. Faded paint, visible wood grain, a bit of rope coiled inside, in a clean stylized mobile-game art style with bold shapes and saturated colors, not photorealistic.
```

**Hero prop: destructible coral rock cluster (before/after states)**
```
Two side-by-side concept illustrations of the same coral-covered rock formation used as destructible arena cover in a stylized mobile-game art style: on the left, an intact cluster of rounded beach rocks covered in colorful coral growth and small tidepool detail; on the right, the same cluster shattered into rubble with scattered coral fragments and a newly opened firing lane behind it. Bold shapes, saturated tropical colors, clean stylized (not photorealistic) rendering.
```

**Rising Tide sudden-death hazard (flood visual)**
```
A concept illustration of a beach arena's outer edge being flooded by an incoming tide during a sudden-death phase — glowing, slightly ominous turquoise water creeping inward across the sand in a defined ring, with a subtle warning-color shimmer at the water's leading edge to communicate "this tile deals damage." Stylized mobile-game VFX-concept art style, bold and readable at a glance, not photorealistic.
```

**Target**: Gemini | **Aspect/size**: 16:9 landscape for overview shots, square for isolated prop concepts | **Sources**: `00-game-design-overview.md` §5, `01-single-player-mode.md` §4

---

## 7. Projectiles &amp; Ability VFX

Grounded in `00-game-design-overview.md` §3's ability descriptions. These are typically built as in-engine particle/shader effects rather than single generated images, but concept/texture-sprite prompts are useful for establishing look.

**Ensign Ace — Jump Serve / Ace Barrage projectile**
```
A single glowing volleyball-style projectile in flight, rendered as a clean stylized mobile-game VFX sprite: a bright, slightly luminous ball with a short motion-streak trail in navy-blue and white/gold tones (matching a disciplined "Blue Water Squad" identity), simple and readable at small size against a busy arena background. Transparent background, top-down game projectile sprite style, no photorealism.
```

**Admiral Anchor — Anchor Slam AOE telegraph**
```
A ground-impact telegraph effect for a heavy melee slam attack, rendered as a stylized mobile-game VFX sprite: a frontal arc-shaped warning marker on the ground (steel-grey/rust-orange tones matching the Anchor Guard identity) showing where an incoming AOE slam will land, with a faint cracked-sand/impact-crater visual cue. Top-down view, clean and readable, transparent background, no photorealism.
```

**Admiral Anchor — Drop Anchor Super (leap impact + damage-reduction aura)**
```
A stylized mobile-game VFX sprite sheet concept showing a heavy anchor-shaped impact burst (steel-grey and rust-orange, sand kicking up in a circular shockwave) with a soft teal protective aura ring lingering around the impact point, suggesting an ally-shielding buff zone. Top-down view, clean and readable at small size, transparent background, no photorealism.
```

**Target**: Gemini (as concept/texture reference — final VFX typically built in-engine via Shuriken/VFX Graph) | **Aspect/size**: square, transparent background | **Sources**: `00-game-design-overview.md` §3, `01-single-player-mode.md` §4

---

## 8. Meta/UI Icons

Sourced from `04-menu-navigation-lobby.md` §3 (Home/Locker/Voyage Road screens) and `05-gamification-trophies-ranking.md` §2–4 (Fleet Rank, Commendations, Voyage Road nodes).

**Trophy count icon (Home screen, next to Fleet Rank badge)**
```
A small, simple, monochrome-friendly icon of a nautical trophy: a stylized ship's bell paired with a small ribbon or laurel flourish beneath it, designed as a compact UI icon (reads clearly at ~32-48px). Bronze/gold coloring on a transparent background, flat clean vector style consistent with a mobile game's UI icon set, no fine texture detail (this is a small icon, not the splash-size bell mark).
```

**Doubloon currency icon**
```
A small, simple UI icon of a single gold coin (a "doubloon"), stamped with a subtle anchor or wave emblem, rendered in a clean flat vector style with a soft bevel highlight for a coin-like read at small sizes. Transparent background, bright gold coloring, no fine detail — must read clearly as a currency icon at ~32px.
```

**Fleet Rank badge set (7 tiers — ship-class medallions)**
```
A set of 7 circular UI rank badges for a mobile game, each showing a simplified ship-class silhouette centered inside a metallic medallion border, ranging from humble to grand: a tiny rowboat/dinghy silhouette (lowest tier), a small single-mast sailing sloop, a coast-guard-style cutter, a multi-mast frigate, a sleek modern cruiser, a heavy battleship, and finally an ornate, most-decorated flagship with a small pennant flourish (highest tier). Each badge uses a consistent circular medallion frame that upgrades in material richness with tier — bronze/simple rope-trim for the lowest tiers, escalating through silver to gold with more ornate trim for the highest tiers. Flat clean vector icon style, transparent background, each badge reads clearly at small UI size (~48px).
```
(Generate individually if the service handles multi-subject compositions poorly — substitute the relevant ship-class description per tier: Dinghy, Sloop, Cutter, Frigate, Cruiser, Battleship, Flagship, per `05-gamification-trophies-ranking.md` §2's tier table.)

**Voyage Road track + node icons**
```
A horizontal reward-track background illustration for a mobile game's progression screen: a coiled length of nautical rope or a dotted sailing route across a stylized ocean-chart backdrop, with small circular node markers spaced along it. Include a few sample node icon types at the end of the track: a small anchor-medal icon (character unlock node), a small treasure-chest/coin-pile icon (currency reward node), and a small pennant-flag icon (cosmetic unlock node). Clean flat vector mobile-game UI style, warm parchment/ocean-chart color palette, transparent background where possible for node icons.
```

**Commendation (achievement) emblem set — style guide**
```
A set of small circular achievement medal/ribbon icons for a mobile game's "Commendations" system, nautical-military themed (think naval service ribbons/medals rather than generic gold stars): each a simple circular medallion with a distinct nautical motif at center — crossed anchors, a laurel-wreathed bell, a storm-cloud-and-wave symbol, a compass rose, a signal-flag pair — using a consistent bronze/silver/gold medallion frame per rarity of achievement. Clean flat vector icon style, transparent background, reads clearly at small UI size (~40px), consistent visual family so the full set of 10-15 feels like one cohesive collection.
```

**Target**: Gemini | **Aspect/size**: square icons, transparent background, small-size-legible (design at 512px, will be downscaled to ~32-64px in UI) | **Sources**: `04-menu-navigation-lobby.md` §3, `05-gamification-trophies-ranking.md` §2–4

---

## 9. HUD Elements

Sourced from `00-game-design-overview.md` §7.6 and `04-menu-navigation-lobby.md` §3 (Match HUD: joystick, attack/Super buttons + aim indicators, round pips, timer, teammate status bars).

**HUD control icon set (joystick, attack button, Super button)**
```
A small set of mobile game touch-control UI icons in a clean flat vector style: (1) a circular virtual joystick base-and-knob pair, subtly nautical (a faint compass-rose etching on the base, otherwise minimal), semi-transparent so it doesn't obscure gameplay; (2) a round "attack" action button with a simple volleyball-serve icon glyph at its center; (3) a round "Super" action button with a bold lightning-bolt-through-a-wave icon glyph, designed to visually read as "charged/powered up" when lit and greyed/desaturated when on cooldown. Consistent icon family, transparent backgrounds, semi-transparent white/navy color scheme suited to overlaying on bright gameplay footage.
```

**Round pip / timer icon set**
```
A small set of clean flat vector UI icons for a mobile game's match HUD: a row of 3 small pip/dot indicators styled as tiny ship's-wheel or bell glyphs (for "rounds won" tracking, lit vs unlit states), and a simple stopwatch/hourglass icon with a subtle nautical rope-border frame for the round timer display. Transparent backgrounds, consistent with a navy-and-gold UI accent scheme, legible at small HUD size.
```

**Target**: Gemini | **Aspect/size**: small square icons, transparent background | **Sources**: `00-game-design-overview.md` §7.6, `04-menu-navigation-lobby.md` §3

---

## 10. Notes on Service-TBD Assets

The project has **not chosen a service for 3D models or other non-image asset types**. Every 3D asset flagged "Service TBD" above (in-game character pawns §3, and potentially the arena's final 3D geometry/props if the team decides to generate rather than hand-model them) needs one of:

1. **Ask the user directly** which service they intend to use — common current options include Meshy, Tripo, Luma (Genie), Kaedim (AI-assisted), or sourcing/adapting reference models from a marketplace like Sketchfab rather than pure generation.
2. **If asked to recommend one**, research current capabilities/pricing via WebSearch before committing to a prompt format, rather than relying on possibly-stale knowledge — each service has different expected prompt syntax (some want short spec-style prompts, some accept reference images, some need explicit polycount/rigging parameters for game-ready output).

Do not generate 3D-model prompts in a locked format until this is resolved — the prose descriptions in §3 (pose, palette, proportions, style) are reusable "ingredients" for whichever service is chosen, not a final prompt for any specific one.

---

## 11. Free Asset Sourcing Strategy (No-Budget Phase)

**Status**: No budget available during M1 prototype development. Prioritize free asset sourcing, then sparse AI generation if needed.

### Free Asset Marketplaces (Browse & Download)

Ordered by relevance to mobile game art style:

| Service | Best For | Notes |
|---------|----------|-------|
| **Sketchfab** (sketchfab.com) | Browsing existing models | Filter by CC-licensed ("Free Download" tab + license type: CC0, CC-BY). Massive catalog (100k+), quality varies; search "low poly character" or "stylized sailor" |
| **Poly Haven** (polyhaven.com) | Curated, game-ready models | 100% free, CC0 licensed, hand-curated. Fewer models than Sketchfab but consistently high quality. Stylized aesthetic often matches mobile games |
| **itch.io game assets** (itch.io/game-assets) | Game-specific free packs | Search "3D models" or "character models". Many indie devs share free stylized asset packs. Check license per pack |
| **OpenGameArt.org** | Free game assets with clear licensing | Community-curated, CC-licensed. Smaller catalog but vetted for game use |
| **Quaternius** (quaternius.com) | Low-poly voxel/stylized character models | Free low-poly 3D models in voxel or stylized style, perfect for mobile. Limited roster but good for rapid asset-grabbing |
| **Kenney.nl** (kenney.nl) | Mixed free + paid asset packs | Free packs are solid quality. Stylized game-dev aesthetic, consistent art family across packs |
| **Mixamo** (mixamo.adobe.com) | Rigged character models + animations | Free rigged characters (Adobe account required, free tier works). Animations included. Good for rapid iteration if rig quality is acceptable |

### Free AI Generation Tier

If no marketplace asset suits after browsing, use **Tripo** (~16 free generations/month, renewable). Serves as a gap-filler, not the primary source.

**Tripo free-tier workflow**:
1. Exhaustively search Sketchfab + itch.io first. If nothing matches, only then generate.
2. Adapt portrait prompts from §1–2 to Tripo's short spec format: "Ensign Ace: young naval ensign character, stylized low-poly game-ready 3D model, T-pose, navy-blue and white beach-sailor uniform with gold trim, confident expression, slightly heroic-chibi proportions, mobile-optimized topology (~5k tris)."
3. Generate at low poly count (request ~3k–5k tris for mobile). Tripo defaults to higher counts; explicitly constrain.
4. Export `.obj`/`.fbx` → import to Unity, verify rig/import pipeline before spending another free generation.

### Project Constraints for Asset Sourcing

- **Mobile-optimized**: Target ~3k–5k tris per character pawn (Android IL2CPP + ARM64 overhead). Marketplace models often come higher; may need decimation in Blender.
- **Stylized aesthetic match**: Brawl Stars / Clash Royale visual style (cel-shading, clean geometry, heroic proportions, no photorealism). Filter searches for "stylized" or "low-poly" to avoid photorealistic models.
- **Rigging requirement**: Character pawns need bone skeleton for animation (walk, attack, knockback anims). Marketplace models often come rigged; Tripo/Meshy generate un-rigged — rigging is manual post-work in Blender or via auto-rigging tools (Mixamo can auto-rig if import format compatible).
- **Palette flexibility**: §1–2 establish color palettes (Blue Water = navy/white/gold; Anchor Guard = steel-grey/teal/rust), but marketplace models may have different colors. Re-tinting in-engine (material override) is acceptable for M1; hand-painting textures is deferred.

### Success Criteria for Bootstrap Phase

- Acquire at least one browsable low-poly Sailor model (Ensign Ace or Admiral Anchor equivalent) from marketplace without paying.
- Import to Unity, verify basic import (no missing/corrupted geometry), test rig if rigged.
- If successful, model can be placeholder-deployed for playtest; hand-art refinement deferred to post-M1.
- If no marketplace model is acceptable after 30 min of browsing, then use 1 free Tripo generation as a targeted fallback.
