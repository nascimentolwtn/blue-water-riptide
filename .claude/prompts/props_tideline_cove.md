# Prompt: Tideline Cove — Props (Generation-Sourced Subset)

Sequencing/ownership: `.claude/plans/09-graphics-and-alpha-build.md` §4. Covers only the props best suited to AI generation (hero props, or gaps the marketplace-first sourcing pass doesn't fill) — driftwood/grass are expected to come from free marketplace packs (Kenney/Quaternius) per `09`'s prop table and don't need a generation prompt here.

**Art style**: Same stylized mobile-game register as `tideline_cove_arena.md` — bold shapes, saturated tropical colors, clean/simplified, not photorealistic. These are meant to visually match the overview concept art, not stand alone.

**Target service**: Gemini for 2D concept/reference art (to guide hand-modeling or a 3D-generation pass); Tripo for actual game-ready 3D models if no marketplace source is found (see `09` §4's per-prop sourcing table — try Sketchfab/itch.io first for the rowboat and net-wall specifically, since hero props are worth the extra sourcing time).

**Resolution/format**: Square, concept-art resolution (1024x1024+) for the Gemini reference prompts below; Tripo 3D output per its game-ready-model conventions (~3-5k tris, matching the Sailor pawn budget in `sailor_ace_pawn_model.md`).

**Reference images**: Use `tideline_cove_arena.md`'s finished overview art as a style reference once generated, so individual props match the established look rather than drifting stylistically.

**Usage rights**: Marketplace sourcing — verify CC0/CC-BY license per pack. Generated concept art / Tripo output — same commercial-use-terms caveat as the other prompt files in this set.

---

### Hero prop: volleyball net-wall chokepoint

```
A close-up concept illustration of a weathered volleyball net strung between two sturdy wooden posts, functioning as solid arena cover (blocks movement and straight-line attacks) in a stylized mobile-game art style. Sun-bleached rope netting, sand-worn wooden posts, taut cabling, set against a bright sandy beach backdrop. Clean, slightly stylized/cartoonish rendering — bold shapes and saturated colors, not photorealistic.
```

### Hero prop: beached rowboat

```
A close-up concept illustration of an old, weathered wooden rowboat half-beached in sand, tilted at a slight angle, used as solid arena cover in a top-down mobile arena game. Faded paint, visible wood grain, a bit of rope coiled inside, in a clean stylized mobile-game art style with bold shapes and saturated colors, not photorealistic.
```

### Hero prop: destructible coral rock cluster (before/after states — needed for the destructible-cover mechanic)

```
Two side-by-side concept illustrations of the same coral-covered rock formation used as destructible arena cover in a stylized mobile-game art style: on the left, an intact cluster of rounded beach rocks covered in colorful coral growth and small tidepool detail; on the right, the same cluster shattered into rubble with scattered coral fragments and a newly opened firing lane behind it. Bold shapes, saturated tropical colors, clean stylized (not photorealistic) rendering.
```

### Tripo fallback (short spec-style) — only if marketplace sourcing fails for the rowboat/net-wall

```
Beached wooden rowboat, stylized low-poly game-ready 3D model, weathered faded paint, tilted resting angle, mobile-optimized topology, target ~2000-4000 triangles, matches a Brawl Stars-style arena game prop aesthetic.
```

```
Volleyball net between two wooden posts, stylized low-poly game-ready 3D model, sun-bleached netting, sand-worn wood, mobile-optimized topology, target ~1500-3000 triangles, matches a Brawl Stars-style arena game prop aesthetic.
```

### Checklist before these are considered alpha-ready

- [ ] Every solid prop's collider matches its visual footprint (see napkin's IL2CPP `CreatePrimitive` Collider-stripping gotcha if any placeholder-primitive step is used before final geometry)
- [ ] Coral cluster has both intact and shattered prefab variants
- [ ] Poly budget checked at the whole-arena level, not just per-prop
