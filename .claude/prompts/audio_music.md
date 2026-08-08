# Prompt: Music (Menu + Combat Loops)

New ground — not covered in `.claude/plans/07-asset-prompts.md` (image-only library). Sequencing/ownership: `.claude/plans/09-graphics-and-alpha-build.md` §3a.

**Art/audio style**: Percussive, tropical-tinged, upbeat arcade-sports energy — steel-drum/marimba melodic elements over light electronic/EDM percussion. Confident and a little cocky (matches the Blue Water Squad's established personality, `docs/History.md`/`07` §1a), not somber or militaristic despite the navy theme. Cartoonish sports-brawler tone, not a war/combat score.

**Target service**: Suno (per project direction).

**Format**: Ogg Vorbis (Unity's standard compressed loop format for Android), seamless loop point (no audible click/gap at the loop boundary — verify by ear before committing an asset).

**Reference images**: N/A (audio). Reference points instead: general tone comparison to Brawl Stars/Clash Royale's menu and combat music (upbeat, short-loop, percussive, mobile-game register) — don't reference or attempt to reproduce any specific copyrighted track, use as a tonal/genre description only.

**Usage rights**: **Unconfirmed — verify before shipping.** Suno's free/basic tier commercial-use terms have historically differed from paid tiers; confirm the specific plan's license covers use in a distributed game (not just personal/non-commercial listening) before this track ships past internal alpha testing. Flag this explicitly to the user if generating under a free tier.

---

### Menu loop

```
Compose an upbeat, tropical-nautical instrumental loop for a mobile arena battle game's main menu, around 80-100 BPM. Steel drum and marimba melodic lead over a light, clean electronic percussion bed — confident, breezy, a little playful, like a beach-sport tournament about to start. No vocals. Should loop seamlessly with no audible gap or key change at the loop point. Duration: 60-90 seconds, designed to repeat indefinitely in the background of a menu screen without becoming fatiguing.
```

### Combat loop

```
Compose an upbeat, percussive tropical-electronic instrumental loop for a mobile arena battle game's combat music, around 120-140 BPM. Same steel-drum/marimba melodic identity as a calmer menu theme but faster, punchier, driven by a tighter electronic percussion groove — energetic and competitive, building tension without becoming aggressive or militaristic. No vocals. Should loop seamlessly under a variable-length match (no hard-timed musical stinger baked into the loop itself). Duration: 60-90 seconds.
```

### Sudden Death intensity variant (optional — a mix/filter pass on the combat loop is acceptable for alpha instead of a full separate composition)

```
A higher-tension variant of [reference the combat loop's melodic identity]: same tempo and instrumentation family, but with a filtered/muted low-end and an added subtle rising-tension pad or tremolo effect layered in, evoking rising water/urgency. Should still loop cleanly. This can be a mix/processing pass on the combat loop rather than a fully new composition if that's faster.
```

### File destinations

`Assets/Audio/Music/Menu_Loop.ogg`, `Assets/Audio/Music/Combat_Loop.ogg` (Sudden Death variant, if produced as a separate file, `Assets/Audio/Music/Combat_SuddenDeath_Loop.ogg`).
