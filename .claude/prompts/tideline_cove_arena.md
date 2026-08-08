# Prompt: Tideline Cove — Arena Concept Art

Source: adapted from `.claude/plans/07-asset-prompts.md` §6. Sequencing/ownership: `.claude/plans/09-graphics-and-alpha-build.md` §2. Full layout spec: `.claude/plans/00-game-design-overview.md` §5. This file covers **concept/establishing art only** — individual prop generation prompts (rowboat, net-wall, coral cluster) live in `props_tideline_cove.md`.

**Art style**: Clean stylized mobile-game environment art (bold colors, simplified geometric forms, no photorealism), matching a Brawl Stars-style top-down arena game.

**Target service**: Gemini.

**Resolution/format**: 16:9 landscape for the overview establishing shot; square for isolated prop/hazard concepts.

**Reference images**: None yet in-repo. Once `sailor_ace_portrait.md`/`sailor_anchor_portrait.md` are finished, cross-reference palette only (arena is a separate visual register — bright tropical daylight — from either Sailor's personal color scheme).

**Usage rights**: Confirm the generation service's commercial-use terms for the tier used, same caveat as the Sailor prompts.

---

### Arena overview (top-down/isometric establishing shot)

```
A top-down isometric concept illustration of a symmetric tropical beach volleyball-court-style arena for a mobile arena battle game, 34 tiles wide by 20 tiles deep with the long axis running left-to-right. At the center, two volleyball net-wall segments block the middle of the court with a gap between them — a chokepoint. A beached wooden rowboat sits mid-court, offset to one side, surrounded by scattered driftwood logs near each team's diagonal approach corners. Two clusters of coral-covered rocks sit near the rowboat. Along one long edge (the south edge), a shallow tidal inlet of pale turquoise water cuts a flanking lane; along the opposite edge, patches of tall dune grass offer concealment. Warm tropical daylight, bright sand, turquoise-to-deep-blue ocean gradient at the horizon. Clean stylized mobile-game environment art style (bold colors, simplified geometric forms, no photorealism) matching a Brawl Stars-style top-down arena game. No characters in frame — environment only.
```

### Rising Tide sudden-death hazard (flood visual)

```
A concept illustration of a beach arena's outer edge being flooded by an incoming tide during a sudden-death phase — glowing, slightly ominous turquoise water creeping inward across the sand in a defined ring, with a subtle warning-color shimmer at the water's leading edge to communicate "this tile deals damage." Stylized mobile-game VFX-concept art style, bold and readable at a glance, not photorealistic.
```

### Ground texture note (not a generation prompt — a direct dev fix)

`M1PrototypeBootstrap.cs`'s current placeholder ground material is a flat blue-tinted color (`SailorPawn.CreateUrpMaterial(new Color(0.2f, 0.45f, 0.65f))`, intended to stand in for sand but currently reads as water). Once a real sand texture exists from this concept pass, swap it in directly — cheap, immediate visual improvement independent of the rest of the arena-dressing timeline.
