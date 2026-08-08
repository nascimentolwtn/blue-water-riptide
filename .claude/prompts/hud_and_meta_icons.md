# Prompt: HUD Controls & Meta/UI Icons (Batch Set)

Source: adapted/consolidated from `.claude/plans/07-asset-prompts.md` §8–9. Sequencing/ownership: `.claude/plans/09-graphics-and-alpha-build.md` §5. Consolidated into one file since these are a matched icon family meant to be visually consistent with each other — submit as one batch, don't generate piecemeal across sessions if avoidable.

**Art style**: Flat clean vector icon style, transparent backgrounds, navy/gold accent scheme (matches the established `#0E2A47` palette). Consistent line weight and corner treatment across the whole set — this is the single biggest risk with generating icons individually: style drift between batches. If regenerating any one icon later, reference the rest of the finished set for consistency.

**Target service**: Gemini.

**Resolution/format**: Square icons, transparent background, design at 512px and downscale to actual UI size (~32-64px) — generating small and upscaling produces soft/blurry results, always go the other direction.

**Reference images**: Once the first icon in this set is finished, use it as a style reference for every subsequent one in the same batch.

**Usage rights**: Same commercial-use-terms caveat as the other prompt files in this set.

---

### HUD control icon set (joystick, attack button, Super button)

```
A small set of mobile game touch-control UI icons in a clean flat vector style: (1) a circular virtual joystick base-and-knob pair, subtly nautical (a faint compass-rose etching on the base, otherwise minimal), semi-transparent so it doesn't obscure gameplay; (2) a round "attack" action button with a simple volleyball-serve icon glyph at its center; (3) a round "Super" action button with a bold lightning-bolt-through-a-wave icon glyph, designed to visually read as "charged/powered up" when lit and greyed/desaturated when on cooldown. Consistent icon family, transparent backgrounds, semi-transparent white/navy color scheme suited to overlaying on bright gameplay footage.
```

### Round pip / timer icon set

```
A small set of clean flat vector UI icons for a mobile game's match HUD: a row of 3 small pip/dot indicators styled as tiny ship's-wheel or bell glyphs (for "rounds won" tracking, lit vs unlit states), and a simple stopwatch/hourglass icon with a subtle nautical rope-border frame for the round timer display. Transparent backgrounds, consistent with a navy-and-gold UI accent scheme, legible at small HUD size.
```

### HP bar frame (new — not in `07`, needed for the Match HUD per `09` §5)

```
A clean flat vector UI element: a horizontal health-bar frame for a mobile game HUD, rope-trimmed border in a subtle nautical style, with a team-colorable fill area (design as a neutral grey-white fill placeholder that can be recolored per-team in-engine, not baked-in color). Include a matching smaller variant for teammate/enemy mini-bars. Transparent background, reads clearly at both large (self) and small (teammate/enemy) HUD sizes.
```

### Trophy count icon

```
A small, simple, monochrome-friendly icon of a nautical trophy: a stylized ship's bell paired with a small ribbon or laurel flourish beneath it, designed as a compact UI icon (reads clearly at ~32-48px). Bronze/gold coloring on a transparent background, flat clean vector style consistent with a mobile game's UI icon set, no fine texture detail.
```

### Doubloon currency icon

```
A small, simple UI icon of a single gold coin (a "doubloon"), stamped with a subtle anchor or wave emblem, rendered in a clean flat vector style with a soft bevel highlight for a coin-like read at small sizes. Transparent background, bright gold coloring, no fine detail — must read clearly as a currency icon at ~32px.
```

### Fleet Rank badge set (7 tiers)

```
A set of 7 circular UI rank badges for a mobile game, each showing a simplified ship-class silhouette centered inside a metallic medallion border, ranging from humble to grand: a tiny rowboat/dinghy silhouette (lowest tier), a small single-mast sailing sloop, a coast-guard-style cutter, a multi-mast frigate, a sleek modern cruiser, a heavy battleship, and finally an ornate, most-decorated flagship with a small pennant flourish (highest tier). Each badge uses a consistent circular medallion frame that upgrades in material richness with tier — bronze/simple rope-trim for the lowest tiers, escalating through silver to gold with more ornate trim for the highest tiers. Flat clean vector icon style, transparent background, each badge reads clearly at small UI size (~48px).
```
(Generate individually if the service handles multi-subject compositions poorly — substitute per tier name: Dinghy, Sloop, Cutter, Frigate, Cruiser, Battleship, Flagship, per `05-gamification-trophies-ranking.md` §2 / `FleetRankDefinition.cs`.)

### Voyage Road track + node icons

```
A horizontal reward-track background illustration for a mobile game's progression screen: a coiled length of nautical rope or a dotted sailing route across a stylized ocean-chart backdrop, with small circular node markers spaced along it. Include a few sample node icon types at the end of the track: a small anchor-medal icon (character unlock node), a small treasure-chest/coin-pile icon (currency reward node), and a small pennant-flag icon (cosmetic unlock node). Clean flat vector mobile-game UI style, warm parchment/ocean-chart color palette, transparent background where possible for node icons.
```

### Commendation (achievement) emblem set

```
A set of small circular achievement medal/ribbon icons for a mobile game's "Commendations" system, nautical-military themed (think naval service ribbons/medals rather than generic gold stars): each a simple circular medallion with a distinct nautical motif at center — crossed anchors, a laurel-wreathed bell, a storm-cloud-and-wave symbol, a compass rose, a signal-flag pair — using a consistent bronze/silver/gold medallion frame per rarity of achievement. Clean flat vector icon style, transparent background, reads clearly at small UI size (~40px), consistent visual family so the full set of 10 feels like one cohesive collection.
```
(10 entries needed to match `CommendationCatalog.cs`'s current roster: First Blood, Taking the Cup, Heavy Reinforcements, Down to the Wire, Ace's Wingmate, Anchor's Watch, Fleet Colors, Shipmates, Squadron Tactics, Friendly Fire.)

### File destinations

`Assets/Art/UI/HUD/` (HUD control set, pips/timer, HP bar frame), meta icons (trophy/doubloon/rank/road/commendations) → new `Assets/Art/UI/Meta/` subfolder.
