# Prompt: Admiral Anchor — Character Portrait

Source: adapted from `.claude/plans/07-asset-prompts.md` §1b. Sequencing/ownership: `.claude/plans/09-graphics-and-alpha-build.md` §1b.

**Art style**: Stylized 3D-render mobile-game look, clean cel-shading, heroic-tank proportions (broad shoulders, grounded stance — Anchor is 1900 HP/Slow speed, the team's frontline tank). Palette: steel-grey and harbor-teal with iron/rust accents — **not specified anywhere in the design docs**, chosen specifically to contrast against Ace's navy-blue Blue Water Squad identity so the two Sailors read as visually distinct squadrons at a glance. Flag for art-direction sign-off if a different Anchor Guard palette is preferred.

**Target service**: Gemini (natural-language prose prompt).

**Resolution/format**: Square 1:1, minimum 1024x1024, transparent or plain-blurred background. PNG.

**Reference images**: None yet — see `sailor_ace_portrait.md`'s note; keep palette contrast with Ace's finished portrait once it exists (iterate Anchor second, referencing Ace's output for contrast-checking).

**Usage rights**: Same caveat as `sailor_ace_portrait.md` — confirm the generation service's commercial-use terms before shipping past internal alpha.

---

### Prompt — Variant A (recommended default: stylized 3D-render)

```
A 3/4-view character portrait of a large, weathered, older naval officer in a stylized 3D-render mobile-game art style with clean cel-shading and heroic-tank proportions (broad shoulders, grounded stance — comparable to a "tank" character archetype in Brawl Stars-style games). He wears heavy-duty harbor-defense gear over a dark uniform — a weathered peacoat or foul-weather jacket left open at the beach, salt-stained, with faded admiral's rank insignia — steel-grey and teal color palette with rust-orange accents, suggesting decades of harbor and lighthouse duty rather than parade-ground polish. Grey beard, calm and immovable expression, one hand resting on a massive ship's anchor slung over his shoulder like a weapon. Background is a soft blurred gradient of stormy ocean grey-blue so he reads clearly as a standalone character icon. Lighting is overcast, moody, coastal. Plain or transparent background preferred for UI cropping.
```

### Prompt — Variant B (alternate: flat vector)

```
A flat vector illustration portrait of a large, grey-bearded naval officer, bust-up 3/4 view, in a bold graphic mobile-game icon style (thick clean outlines, flat color fills, minimal shading). Steel-grey and teal weathered coat with rust-orange trim, calm immovable expression, a massive anchor resting over one shoulder. Solid deep-navy (#0E2A47) or transparent background so the character silhouette pops, with enough color contrast against Ensign Ace's palette that the two read as different squadrons at a glance.
```

**Must match whichever variant (A or B) was picked for Ace** — style consistency across the 2-Sailor roster matters more than either variant's individual merit.
