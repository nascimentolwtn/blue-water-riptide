# Prompt: App Icon & Splash Screen (Ship's Bell Mark)

Fully specified in `.claude/plans/06-splash-screen-and-icon.md` §1–2; prompt text sourced from `.claude/plans/07-asset-prompts.md` §4–5. This file consolidates both into one batch-submittable prompt set since they reuse the same bell mark. Sequencing/ownership: `06` remains the authoritative spec for exact sizes/Player Settings wiring — this file is the prompt text only.

**Art style**: Clean, flat, high-contrast bell silhouette (icon) / bell mark with subtle weathered texture and wave/net accent (splash) — squadron-neutral (all three squadrons in the lore claim the bell per `docs/History.md`), deliberately avoids volleyball/ball/net-as-sport-equipment imagery per `CLAUDE.md`'s theme-vs-mechanics guardrail.

**Target service**: Gemini.

**Resolution/format**: Icon — per-density exports at 432/324/216/162/108/81px (adaptive), 192/144/96/72/48/36px (round/legacy), 512x512 (Play Store listing) — see `06` §2's full table; author at high resolution and downscale. Splash — transparent PNG, bell centered in a 1920x1080 (16:9) safe area, export at 2x (3840x2160 recommended), landscape-only (this project locks landscape orientation).

**Reference images**: None yet — this is confirmed to be the **first finished art asset** in the repo (`06` §4.5). No prior art to reference.

**Usage rights**: Confirm the generation service's commercial-use terms before this ships in a Play Store listing (the 512x512 Play Store icon specifically is public-facing store material, a higher bar than in-app-only assets).

---

### Icon — Adaptive foreground layer (bell glyph, transparent)

```
A single ship's bell icon glyph, viewed straight-on, rendered as a clean, solid, high-contrast bronze/gold silhouette with simple flat shading (no fine texture or barnacle detail — this must stay legible at very small sizes). The bell should be centered, simple, and bold enough to read clearly as a tiny app-icon-sized shape: a classic nautical hand bell with a small mounting bracket at top and a visible clapper hint at the bottom opening. Transparent background. No text, no additional elements — just the bell glyph, evenly padded so it comfortably fits within a circular or rounded-square safe crop area.
```

### Icon — Adaptive background layer

```
A flat, solid deep-navy fill, color #0E2A47, completely even with no gradient, texture, or content — a plain background tile for an Android adaptive app icon.
```

### Icon — Legacy/Round composited (bell pre-composited on navy)

```
A square Android app icon: the same bronze/gold ship's bell glyph (clean solid silhouette, no fine texture, simple flat shading, classic nautical hand bell with mounting bracket and clapper hint) centered on a flat solid deep-navy (#0E2A47) background, with the bell padded with clear margin so it survives being cropped into a circle for round-icon variants. No text, no additional elements.
```

### Icon — Play Store listing (512x512)

```
A 512x512 app icon, 32-bit PNG with alpha, showing the same bronze/gold ship's bell glyph (clean solid silhouette, simple flat shading, no fine barnacle texture, classic nautical hand bell shape) centered on a flat solid deep-navy (#0E2A47) background, generously padded, crisp enough to read clearly at both full size and thumbnail size in a store listing. No text.
```

### Splash — bell mark with wave/net accent (also reused for the `Boot` scene's own splash UI)

```
A clean, flat, high-contrast logo mark: a barnacle-crusted bronze ship's bell, rendered with subtle weathered texture (light barnacle/verdigris detailing on the bell's surface, suggesting age and years at sea) and a small stylized wave crest curling beneath it, with a faint suggestion of netting rope looping around the base. The mark should be squadron-neutral — no team colors, no volleyball, no ball or net-as-sport-equipment imagery — just the bell as a trophy object with subtle nautical dressing. Centered composition, transparent background, bronze/gold bell tones against nothing (for placement over a solid deep-navy #0E2A47 background later). Style: clean vector-adjacent flat illustration with soft dimensional shading, no photorealism, reads clearly as a single unified mark at both large and small display sizes.
```

### File destinations (per `06`)

- Icon layers → `Assets/Art/UI/Icon/`: `Icon_Foreground.png`, `Icon_Background.png`, `Icon_Legacy_Composited.png`, `Icon_PlayStore_512.png`
- Splash → `Assets/Art/UI/Splash/Splash_Bell_Logo.png` (single file, reused for both Unity's native Splash Screen and the `Boot` scene's own UI)
