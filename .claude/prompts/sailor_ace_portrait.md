# Prompt: Ensign Ace — Character Portrait

Source: adapted from `.claude/plans/07-asset-prompts.md` §1a. Sequencing/ownership: `.claude/plans/09-graphics-and-alpha-build.md` §1b.

**Art style**: Stylized 3D-render mobile-game look, clean cel-shading, slightly heroic proportions (Normal build — Ace is not a tank; his stats are 1300 HP/Normal speed). Comparable visual weight to Brawl Stars/Clash Royena character art. Palette: deep navy `#0E2A47` (established splash/icon color, `06-splash-screen-and-icon.md`) with white and gold trim.

**Target service**: Gemini (natural-language prose prompt, not tag-soup).

**Resolution/format**: Square 1:1, minimum 1024x1024, transparent or plain-blurred background for easy UI cropping. PNG.

**Reference images**: None exist in-repo yet — this would be the first finished character art. If iterating, reference `Assets/Art/UI/Splash/Splash_Bell_Logo.png` (once produced per `06`) only for palette consistency, not composition.

**Usage rights**: Confirm the generation service's output-ownership/commercial-use terms for the tier used before shipping past internal alpha testing (Gemini's terms vary by API vs. consumer-app access path) — flag as unconfirmed, don't assume clearance for store release.

---

### Prompt — Variant A (recommended default: stylized 3D-render)

```
A 3/4-view character portrait of a young, confident naval ensign on a tropical beach volleyball court, in a stylized 3D-render mobile-game art style with clean cel-shading and slightly heroic proportions (comparable to Brawl Stars character art). He wears a fitted navy-blue and white sailor's uniform adapted for beach sport — rolled-up sleeves, gold ensign rank insignia on the collar, dog tags, deck shoes — looking sharp and disciplined despite the sand underfoot. He's caught mid-motion winding up to serve a volleyball, cocky half-smile, eyes locked forward like he's a half-step ahead of his own confidence. Background is a soft blurred gradient of ocean blue and sandy beige so he reads clearly as a standalone character icon. Lighting is bright, warm, tropical daylight. Plain or transparent background preferred for UI cropping.
```

### Prompt — Variant B (alternate: flat vector)

```
A flat vector illustration portrait of a young naval ensign, bust-up 3/4 view, in a bold graphic mobile-game icon style (thick clean outlines, flat color fills, minimal shading — think modern flat-design app icons rather than painterly art). Navy-blue and white beach-sailor uniform with gold trim and ensign insignia, confident smirk, mid-serve pose with a volleyball just leaving his hand. Solid deep-navy (#0E2A47) or transparent background so the character silhouette pops.
```

**Pick one variant and use it consistently across all Sailor portraits** — don't mix A and B between Ace and Anchor.
