# Prompt: Admiral Anchor — In-Game 3D Pawn Model

Sequencing/ownership: `.claude/plans/09-graphics-and-alpha-build.md` §1a. In-game low-poly model, distinct from the 2D portrait (`sailor_anchor_portrait.md`).

**Art style**: Stylized low-poly, game-ready, mobile-optimized. Heavier/broader silhouette than Ace (1900 HP tank, Slow speed) — proportions should visibly read "tank" at a glance in a 3v3 arena. Steel-grey/teal/rust palette (see `sailor_anchor_portrait.md`'s note that this is a proposed, not locked, Anchor Guard palette).

**Sourcing priority**: identical order to `sailor_ace_pawn_model.md` — Sketchfab → Poly Haven → itch.io → OpenGameArt → Quaternius → Kenney → Mixamo, 30 minutes each, then Tripo fallback. Given `Assets/Art/Characters/Vietnam Coast Guard/Venerated.fbx` is already imported/rigged (a broader-built military character model), **check it first as a possible Anchor body** before starting a fresh sourcing pass — see the open flag in `09` §0 about resolving its off-roster "Venerated" identity either way before committing.

**Target service (fallback only)**: Tripo free tier.

**Resolution/format**: 3,000–5,000 triangles, `.fbx`/`.obj`, T-pose.

**Reference images**: `sailor_anchor_portrait.md`'s finished output, once generated, as a style reference upload.

**Usage rights**: Same as `sailor_ace_pawn_model.md` — verify marketplace license terms (CC0 preferred) or Tripo's output-ownership terms.

---

### Tripo fallback prompt (short spec-style)

```
Admiral Anchor: large, weathered, older naval officer character, stylized low-poly game-ready 3D model, T-pose, steel-grey and teal harbor-defense coat with rust-orange trim, broad-shouldered heavy-tank build, calm grounded stance, grey beard, mobile arena-game proportions (heroic-chibi but visibly the "tank" of the roster — broader than a standard build). Clean PBR textures, mobile-optimized topology, target ~3000-5000 triangles.
```

### Post-sourcing checklist

- [ ] Import verified — no missing/corrupted geometry
- [ ] Materials re-mapped to URP `Universal Render Pipeline/Lit`
- [ ] Poly count within budget
- [ ] Rig verified before animating
- [ ] Retinted to steel-grey/teal/rust if sourced un-colored
- [ ] Silhouette distinctly broader/heavier than Ace's finished model when placed side by side (the actual acceptance test — a 3v3 arena needs "who's the tank" readable instantly)
