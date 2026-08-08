# Prompt: SFX (Combat, UI, Progression)

New ground. Sequencing/ownership: `.claude/plans/09-graphics-and-alpha-build.md` §3b, which has the full event-to-code-hook table (`MatchController` events, `SailorPawn.ApplyDamage`, ability `Execute` calls) — this file is prompt text only; see `09` for exactly which hook each clip attaches to.

**Art/audio style**: Punchy, cartoonish "arcade sports" hits — **not** military-realistic. Attacks are volleyball-serve/anchor-slam themed per the game's cosmetic theme (`CLAUDE.md`'s theme-vs-mechanics guardrail: the Serve is a projectile attack that *looks/sounds* like a served volleyball, it does not play like a sport mechanic).

**Sourcing priority**: Kenney.nl free UI/SFX packs first for generic sounds (button clicks, generic chimes) — check before generating, per the established free-asset-sourcing playbook. Generate only the bespoke combat/ability hits below that a generic pack won't have.

**Target service**: A dedicated SFX generator (e.g. ElevenLabs' sound-effects endpoint if available on a usable tier) or Gemini if it supports short audio-clip generation at the time of use — verify current capability via WebSearch before committing to a specific service, since this space changes quickly.

**Format**: WAV (short one-shots, decompressed-in-memory for low playback latency — Unity Audio Import Settings, not a file-format concern at generation time; generate as WAV or convert on import).

**Reference images**: N/A (audio).

**Usage rights**: Kenney packs are explicitly free/CC0 (verify per-pack) — safe. Generated SFX — confirm the specific service's commercial-use terms, same caveat as `audio_music.md`.

---

### Jump Serve — fire

```
A short, punchy cartoon sports "whoosh" sound effect for a volleyball serve attack, quick and light with a subtle synthetic energy undertone (this is a game ability, not a real sports sound) — under 0.5 seconds, crisp and readable in a busy mix.
```

### Jump Serve — impact

```
A short, soft "splash-thwack" impact sound for a stylized volleyball-serve projectile hitting a target in an arcade arena game — not a gunshot or realistic weapon impact, more like a satisfying sports-game hit-confirm sound. Under 0.4 seconds.
```

### Anchor Slam — swing

```
A heavy, cartoonish "whoosh" followed by a deep ground-thud sound effect for a large melee slam attack in an arcade arena game — weighty and impactful but stylized/exaggerated, not realistic. Around 0.6-0.8 seconds.
```

### Anchor Slam — impact/knockback

```
A deep, satisfying impact sound with a "sand kicking up" textural tail, for a heavy AOE ground-pound attack landing in a beach arena game. Exaggerated, cartoonish weight — think a "big stomp" game sound, not a realistic collision. Around 0.5 seconds.
```

### Drop Anchor Super — leap+slam+aura

```
A heavy metallic "anchor dropping and clanging" impact sound followed by a splash, for a character's special ability landing in an arcade arena game — dramatic and weighty, signaling a powerful move. Around 1 second, followed by (separate short loopable clip) a soft, warm sustained hum/shimmer texture for a 6-second protective aura buff effect.
```

### Knockout stinger (need 2 variants — one per team, for readability in a 3v3 mix)

```
A short, clear "character eliminated" stinger sound for a mobile arena battle game — a quick downward musical/tonal sting, distinct and readable even in a busy combat mix, not somber, matches an arcade game's energy. Under 1 second. [Generate a second variant with a subtly different tonal color/pitch for team-differentiation.]
```

### Round start / countdown — ship's bell

```
A clean, bright ship's bell "ding" sound, single clear strike, for a mobile game's round-start UI cue — nautical and crisp, reusing the bell-mark motif from the game's splash/icon branding. Under 0.5 seconds.
```

### Round end / match end fanfare (win) and stinger (loss)

```
A short, triumphant fanfare stinger for a mobile arena game's round-win moment — bright, brassy, celebratory, 1-2 seconds. [Separate clip:] A short, lower-register "round lost" stinger — not sad or somber, just a clear "that round's over, you didn't win it" tonal cue, 1-2 seconds, arcade-game appropriate (not punishing).
```

### Sudden Death start — alarm/tide cue

```
A short, urgent-but-not-alarming rising tonal cue signaling a "sudden death" phase starting in an arcade arena game — should evoke rising water/urgency without being a harsh alarm. 1-2 seconds.
```

### UI button tap

```
A short, clean, satisfying UI click sound with a subtle nautical-bell character (a soft, quick bell-like tick rather than a generic digital click), for consistent use across every button in a mobile game's menu system. Under 0.2 seconds.
```

### Doubloon/reward gain

```
A bright, short coin-chime sound effect for a currency-reward moment in a mobile game, cheerful and satisfying, under 0.5 seconds.
```

### File destinations

`Assets/Audio/SFX/`, named per `09` §3b's table (e.g. `SFX_JumpServe_Fire.wav`, `SFX_JumpServe_Impact.wav`, `SFX_AnchorSlam_Swing.wav`, `SFX_AnchorSlam_Impact.wav`, `SFX_DropAnchor_Impact.wav`, `SFX_DropAnchor_AuraLoop.wav`, `SFX_Knockout_TeamA.wav`, `SFX_Knockout_TeamB.wav`, `SFX_RoundStart_Bell.wav`, `SFX_RoundWin_Fanfare.wav`, `SFX_RoundLoss_Stinger.wav`, `SFX_SuddenDeath_Start.wav`, `SFX_UI_ButtonTap.wav`, `SFX_Reward_Doubloon.wav`).
