# Prompt: Ensign Ace — In-Game 3D Pawn Model

Sequencing/ownership: `.claude/plans/09-graphics-and-alpha-build.md` §1a. This is the in-game low-poly model, distinct from the 2D portrait (`sailor_ace_portrait.md`) used in menus/HUD.

**Art style**: Stylized low-poly, game-ready, mobile-optimized. Same silhouette/palette logic as the portrait (navy/white/gold, Normal-build proportions — leaner/faster-reading than Anchor) but simplified for real-time rendering.

**Sourcing priority (per `project_asset_sourcing_strategy.md` and `07-asset-prompts.md` §11 — try these before generating)**:
1. Sketchfab (search "stylized low-poly naval character" / "sailor" / "beach sport character")
2. Poly Haven
3. itch.io game-assets
4. OpenGameArt.org
5. Quaternius
6. Kenney.nl
7. Mixamo (free tier, for a rigged base + retexture/redress)

Spend up to 30 minutes on the above before falling back to generation.

**Target service (fallback only)**: Tripo (free tier, ~16 generations/month) — treat as a gap-filler, not the primary path.

**Resolution/format**: Target 3,000–5,000 triangles (Android IL2CPP/ARM64 budget, S21 FE baseline device per napkin). Export `.fbx` or `.obj`. T-pose for rigging compatibility.

**Reference images**: Use the finished `sailor_ace_portrait.md` output (once generated) as a visual reference upload if the sourcing/generation service accepts image references — keeps the 3D model's outfit/palette consistent with the 2D portrait used elsewhere in the UI.

**Usage rights**: Marketplace models — verify the specific license (CC0 preferred; CC-BY requires attribution tracking; reject anything non-commercial-restricted since this is a game intended for eventual release). Tripo-generated output — confirm Tripo's free-tier output-ownership terms before committing to it as a shipped asset, not just a placeholder.

---

### Tripo fallback prompt (short spec-style, per Tripo's expected format)

```
Ensign Ace: young naval ensign character, stylized low-poly game-ready 3D model, T-pose, navy-blue and white beach-sailor uniform with gold trim, confident/cocky expression, slightly heroic-chibi proportions matching a Brawl Stars-style mobile arena game, lean/agile build (not bulky). Clean PBR textures, mobile-optimized topology, target ~3000-5000 triangles.
```

### Post-sourcing checklist (before this asset is considered alpha-ready)

- [ ] Import verified in Unity — no missing/corrupted geometry
- [ ] Materials re-mapped to `Universal Render Pipeline/Lit` (URP) — legacy Standard-shader materials render solid magenta under this project's active pipeline
- [ ] Poly count within budget; decimated in Blender if oversized
- [ ] Rig verified (clean bone hierarchy, no broken weights) before animating
- [ ] Retinted/redressed to navy/white/gold if sourced un-colored
