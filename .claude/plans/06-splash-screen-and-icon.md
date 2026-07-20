# Plan 06 — Splash Screen & App Icon

Covers the two pieces of first-impression branding the project currently has none of: the `Boot` scene's splash moment (Plan 01 §1, Plan 04 §3) and the Android app icon. Nothing here changes gameplay, scope, or scene flow — it fills in a gap Plan 01/04 left as a placeholder ("static logo while services init") and specs the icon Player Settings has never had assets for. Read `00-game-design-overview.md`, `01-single-player-mode.md` §1, `04-menu-navigation-lobby.md`, and `docs/History.md` before touching this doc — the creative direction below is derived from the Tideline Cup lore, not invented fresh.

**Confirmed from the repo (2026-07-19):**
- `ProjectSettings/ProjectSettings.asset`: `defaultScreenOrientation: 4` (AutoRotation) with `allowedAutorotateToPortrait: 0`, `allowedAutorotateToPortraitUpsideDown: 0`, `allowedAutorotateToLandscapeLeft: 1`, `allowedAutorotateToLandscapeRight: 1` — the project is landscape-only (both landscape rotations allowed, both portrait rotations disabled), matching commit `7c82d5c chore: lock orientation to landscape only`. All splash/icon art below is speced for landscape.
- `m_ShowUnitySplashScreen: 1`, `m_ShowUnitySplashLogo: 1`, `m_SplashScreenLogos: []` — Unity's native Splash Screen system is on and currently empty (no custom logo added yet).
- `m_SplashScreenBackgroundColor: {r: 0.1216, g: 0.1216, b: 0.1255, a: 1}` (~`#1F1F20`) — Unity's neutral default, not yet themed.
- Android icon slots exist in `m_BuildTargetPlatformIcons` (`m_BuildTarget: Android`) at the correct sizes for Adaptive/Round/Legacy (see §2), but every entry has `m_Textures: []` — no icon art has ever been assigned.
- `Assets/Scenes/Boot.unity` already exists (from the bootstrap commit) but has no splash UI in it yet.
- `Assets/Art/UI/` already exists (sibling to `Assets/Art/Characters/`, `Assets/Art/Environment/`) as the established home for UI art — splash and icon source art belong under it.

## 1. Splash screen

Two independent layers show during app launch, in order. Don't conflate them — they're configured differently and one is a licensing-gated constraint, not a design choice.

### 1a. Unity's native Splash Screen (Player Settings → Splash Screen)

This plays automatically before the `Boot` scene's own content is visible. It is config only — no code, no scene objects.

**Known constraint — flag, don't assume:** on Unity Personal (free tier), "Show Splash Screen" is forced on and the checkbox is greyed out — the Unity logo always shows, either as its own step or alongside a custom logo, and cannot be suppressed. Only Unity Pro/Enterprise can disable it entirely. `m_ShowUnitySplashScreen: 1` in `ProjectSettings.asset` is consistent with either case (Personal forces it true; a Pro project could also just leave it true) — **the current file does not prove which tier this project is on.** Confirm the tier in the Editor (Player Settings → Splash Screen: is the checkbox interactable?) or via the Unity Hub license badge before assuming a Unity-logo-free launch is possible. Until confirmed, plan for the Unity logo being present.

**Custom logo content**: a single logo entry — the barnacle-crusted ship's bell from `docs/History.md` §1 (the Tideline Cup trophy), rendered as a clean, flat, high-contrast bell silhouette with a small wave/net accent beneath it. This is the strongest available visual anchor for the game's identity: it's the one object every squadron in the lore recognizes and fights over, it reads clearly at any size, and it avoids leaning on a single squadron's colors (the bell is squadron-neutral — three squadrons claim it). Do not use a volleyball-net or ball-centric mark here — per `CLAUDE.md`'s theme-vs-mechanics guardrail, sport iconography risks implying scoring/ball mechanics that don't exist; the bell is thematic without being mechanically misleading. A wordmark ("BLUE WATER RIPTIDE") beneath the bell mark is optional — see Open Questions §4.

**Technical specs**:
- Canvas is landscape-only (confirmed above) — do not produce a portrait variant; leave `m_SplashScreenBackgroundPortraitUvs`/`splashScreenBackgroundSourcePortrait` untouched.
- Target devices are 19.5:9–20:9 landscape displays (napkin Platform & Device Constraints: S20 FE / S21 FE / S25 / S25+ / S25 FE), i.e. roughly 2400×1080 physical landscape resolution.
- Use a **solid background color**, not a background image — avoids per-aspect-ratio cropping across that device spread, and Unity's splash background color fills the full screen regardless of aspect ratio while the logo image sits centered. Leave `splashScreenBackgroundSourceLandscape` unset (`{fileID: 0}`, already the case).
- Recommended background color: a deep navy rather than Unity's neutral default grey, matching the Blue Water Squad's "disciplined navy blue" identity (`docs/History.md` §2) — e.g. `#0E2A47` → `{r: 0.0549, g: 0.1647, b: 0.2784, a: 1}`. (The project's existing UWP tile color, `metroSplashScreenBackgroundColor: {r: 0.1294, g: 0.1725, b: 0.2157}`, is already in this same dark-navy family — this recommendation is consistent with that precedent, not a new direction.) Treat the exact hex as a creative recommendation pending art sign-off, not a locked value — see Open Questions §3.
- Logo image asset: transparent-background PNG, bell mark centered in a **1920×1080 (16:9) safe area** even though the canvas may render wider (up to 20:9) — the extra width on wider devices is just more background color, not cropped logo. Export at 2x for crispness (e.g. author at 3840×2160 and let Unity downscale, or ship a single high-res PNG) — this is a static image, not a sprite atlas, so oversized-but-simple is fine.
- Unity's Splash Screen system natively supports (Player Settings → Splash Screen, cross-platform config in Unity 6): an ordered list of logos, each with its own **Duration**, **Fade-in time**, and **Fade-out time**; a **Draw Mode** choosing whether the Unity logo plays as its own sequential step before custom logos, or below custom logos simultaneously; and an overlay opacity. It is a fixed fade/hold sequence — there is no particle/shader animation support, so don't plan anything beyond fade + hold.
- Recommended sequence: single custom logo entry, ~0.3s fade-in, ~1.2s hold, ~0.3s fade-out (~1.8s total) — Draw Mode "Unity Logo Below" if the Personal-tier Unity logo can't be suppressed, so both appear in one screen rather than as two sequential blocking steps and the perceived launch delay stays short.
- **Source asset location**: `Assets/Art/UI/Splash/` (new subfolder, sibling to how `Assets/Art/UI/` is already the established UI-art home). File: `Splash_Bell_Logo.png`.

### 1b. Boot scene's own splash UI (the "static logo while services init" content)

This is separate from 1a — it's the actual content of the `Boot` scene itself, per Plan 01 §1 ("Splash — static logo while services init: save load, catalogs, audio. No interaction. Auto-advances to Home") and Plan 04 §3 ("logo, version string, indeterminate progress. No interaction. Auto-advances to Home"). It appears *after* Unity's native splash sequence (§1a) finishes, once the `Boot` scene has actually loaded and is running its own init code.

- Content: the same bell-mark logo image (reuse `Splash_Bell_Logo.png`, don't make a second asset), a version string (`Application.version`, bottom corner), and an indeterminate progress indicator (spinner or looping bar — not a determinate percentage, since save/catalog/audio init isn't meaningfully progress-trackable). No buttons, no touch handling.
- Duration is driven by real init work finishing, not a fixed timer — the screen holds until save/profile load, catalogs, and audio are ready, then auto-advances to Home (Plan 04 nav map: `Boot (splash) → Home`). Don't hardcode a minimum display time; if init is ever fast enough to feel like a flash, that's a later polish concern (M4 tutorial/polish pass in Plan 01 §5), not something to fake here.
- On corrupted save: Plan 04 §3 already specifies a recover/reset dialog surfaces here, before anything reads the profile — this doc doesn't change that, just noting the splash UI must have that dialog as a documented exception to "no interaction."
- This is a Canvas/UI prefab living in the `Boot` scene, built with the same UI conventions as everything else in `Assets/Scripts/UI/` (thin layer over Core events/init state, no game logic).

## 2. App icon

Android adaptive icons (API 26+, which the whole target device list clears — Min API 30 per napkin) are two layers composited by the launcher, plus legacy/round fallbacks for launchers that don't support adaptive icons, plus a separate Play Store listing icon. `ProjectSettings.asset`'s `m_BuildTargetPlatformIcons` for `Android` already has the correctly-sized empty slots for all of these (`m_Kind: 2` = Adaptive layers, `m_Kind: 1` = Round, `m_Kind: 0` = Legacy) — this section specs what art fills them in.

### Icon concept

The ship's bell again — same mark as the splash logo, for a consistent identity across launch touchpoints — but as a **foreground-only silhouette**, no wave/net accent (too fine-detailed to survive down to a 36×36px legacy icon). A bell shape is a strong, simple, recognizable silhouette that stays legible at launcher size, is distinct from the generic-anchor icon look most navy-themed mobile games default to, and is squadron-neutral (per `docs/History.md`, all three squadrons claim the bell). Render it as a solid bronze/gold bell glyph, no barnacle texture at this size (texture only reads on the splash-size version).

- **Foreground layer**: the bell glyph only, centered, sized to comfortably fit the ~66dp safe zone within the 108×108dp canvas (Android adaptive icons crop up to 18dp per edge depending on launcher mask shape — content outside the safe zone may be clipped on some launchers). Transparent background.
- **Background layer**: flat deep navy fill (reuse the splash background color, `#0E2A47`, for cross-touchpoint consistency) — adaptive icon backgrounds should carry no important content since launcher masks may crop them aggressively.
- **Legacy icon** (non-adaptive-icon launchers): bell glyph pre-composited onto the same navy background as a single flat square/squircle image — this is the fallback, not a separate concept.
- **Round icon**: same composited bell-on-navy art, cropped to a circle.

### File/resolution list (Player Settings → Icon, Android)

Matches the empty slots already declared in `ProjectSettings.asset`:

| Kind | Sizes (px) | Density mapping | Content |
|---|---|---|---|
| Adaptive (foreground) | 432 / 324 / 216 / 162 / 108 / 81 | xxxhdpi / xxhdpi / xhdpi / hdpi / mdpi / ldpi (108dp base × density scale) | Bell glyph, transparent bg, centered in safe zone |
| Adaptive (background) | 432 / 324 / 216 / 162 / 108 / 81 | same | Flat navy fill, no content |
| Round | 192 / 144 / 96 / 72 / 48 / 36 | xxxhdpi → ldpi | Bell-on-navy, circle-cropped |
| Legacy | 192 / 144 / 96 / 72 / 48 / 36 | xxxhdpi → ldpi | Bell-on-navy, square/squircle |

Plus, **separately from Player Settings** — the Play Store listing icon: **512×512 PNG, 32-bit with alpha**, uploaded directly in Google Play Console (Store Listing → App icon), not through Unity's Icon panel at all. Same bell-on-navy composited art as the legacy/round icon, just at listing resolution.

**Source asset location**: `Assets/Art/UI/Icon/` (new subfolder alongside `Assets/Art/UI/Splash/`). Suggested filenames: `Icon_Foreground.png`, `Icon_Background.png`, `Icon_Legacy_Composited.png`, `Icon_PlayStore_512.png` — export the per-density sizes from these masters at build-assignment time rather than hand-authoring six separate foreground files; Unity's Icon panel accepts one texture per slot and the sizes above are what each slot expects.

## 3. Remote vs Editor-only work

Following the convention in `00` §8 / `01` §6 / `04` §6: prefer expressing tunable values as plain specs/data so more of this qualifies as **[File]**; only the act of creating art or wiring it into the Editor's Inspector panels is **[Editor/Asset]**.

- **[File]** — this plan doc itself (concept, resolution tables, hex color, filenames, sequence timings); folder scaffolding for `Assets/Art/UI/Splash/` and `Assets/Art/UI/Icon/` (empty dirs + `.gitkeep`, matching the existing `Assets/Art/*` convention — no Unity needed to create a folder); the `Boot` scene's init-gating logic (waiting on save/catalog/audio load before advancing) since that's plain C#, not a UI-layout concern.
- **[Editor/Asset]** — actually illustrating the bell mark (splash + icon variants); importing the PNGs and setting Texture Type/sprite import settings; adding the logo entry to Player Settings → Splash Screen (list, duration, fade in/out, draw mode) and setting the background color there; assigning the six Android icon slots (Adaptive fg/bg, Round, Legacy) in Player Settings → Icon; building the `Boot` scene's actual Canvas/logo/spinner/version-string UI prefab; confirming the Unity license tier (Personal vs Pro/Enterprise) by opening the Editor or Unity Hub; uploading the 512×512 icon to Play Console (not Unity at all, but still not a repo-file task); on-device verification that the icon renders correctly across launcher shapes and the splash timing feels right on real hardware (S21 FE per napkin's baseline device).

## 4. Open questions / dependencies

1. **Unity license tier is unconfirmed.** `m_ShowUnitySplashScreen: 1` in `ProjectSettings.asset` doesn't distinguish "Personal, forced on" from "Pro, left on by choice." Confirm in-Editor (is the checkbox greyed out?) or via Unity Hub's account/license badge before assuming the Unity logo can ever be removed from the launch sequence.
2. **Orientation lock is confirmed** (see top of this doc) — landscape-only via `AutoRotation` with both portrait flags off. Not open, just called out because getting this wrong would have meant designing portrait splash assets nobody needs.
3. **Splash/icon background color (`#0E2A47`) is a creative recommendation, not a locked brand color.** No brand/style guide exists elsewhere in the repo; this value was chosen for consistency with the existing (already-navy) `metroSplashScreenBackgroundColor` and the Blue Water Squad's lore color identity. Needs sign-off once actual art direction happens, not a blocker to scaffolding.
4. **Wordmark question**: should the splash screen carry a "BLUE WATER RIPTIDE" text wordmark alongside the bell mark, or bell-only (matching the icon)? This doc defaults to "wordmark optional on splash, bell-only on icon" but that's a guess, not a confirmed decision — flag before an artist starts on the logo.
5. **No character/environment art exists yet either** (per `CLAUDE.md` project-status note) — the bell mark will be the first piece of finished game art in the repo. Worth confirming whether it should anticipate a specific visual style (flat vector vs painterly) that the rest of the game's art will later match, so this doesn't get redone.
