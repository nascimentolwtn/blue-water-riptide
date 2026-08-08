# Prompt: Voice Lines (Sailor Callouts)

New ground. Sequencing/ownership: `.claude/plans/09-graphics-and-alpha-build.md` §3c. Lowest-priority audio asset — first to cut under time pressure per `09`.

**Art/audio style**: Short barks only, not full VO lines. Per-Sailor personality grounded in `docs/History.md`/`07-asset-prompts.md` §1: **Ace** — young, confident, "a half-step ahead of his own confidence," competitive but friendly, not toxic. **Anchor** — calm, weathered, gravelly, an elder-statesman mentor tone, unbothered.

**Target service**: TTS (per project direction) — a service offering multiple stock/licensed synthetic voice options with adjustable tone/delivery.

**Format**: Short WAV one-shots, same import treatment as SFX.

**Reference images**: N/A. Voice reference: pick one stock TTS voice per Sailor and reuse it consistently across every line for that character — don't vary voices between lines.

**Usage rights — hard constraint, not a preference**: Use only a TTS service's built-in stock/licensed synthetic voices. **Never use a voice-cloning feature to imitate a real person** (celebrity or otherwise) for either Sailor's voice. This is a rights/likeness issue that must be avoided outright, not a style tradeoff to weigh.

---

### Ensign Ace — line scripts

| Moment | Line | Delivery direction |
|---|---|---|
| Round start / spawn | "Let's move!" | Quick, energetic, confident |
| Round start / spawn (alt) | "Ace is up." | Light cockiness, not arrogant |
| Basic attack (occasional — don't trigger on every hit) | "Serve's up!" | Playful, competitive |
| Knockout scored | "That's game." | Friendly taunt, not mean-spirited — this is couch-LAN, keep it light |
| Own knockout | "Aw, come on—" | Short, mildly annoyed, not devastated |
| Match win | "Blue Water takes it!" | Genuinely excited |

### Admiral Anchor — line scripts

| Moment | Line | Delivery direction |
|---|---|---|
| Round start / spawn | "Anchor's down." | Steady, low-key, matter-of-fact |
| Round start / spawn (alt) | "Hold the line." | Calm authority |
| Basic attack (occasional — effort sound more than words) | *(grunt/effort exhale, minimal or no words)* | Short physical effort sound |
| Super use ("Drop Anchor") | "Drop anchor!" | Weighty, declarative, not shouted |
| Knockout scored | "Steady work." | Calm, respectful — matches his mentor role, no gloating |
| Own knockout | *(short exhale, unbothered)* | Dry, minimal |
| Match win | "Well fought." | Warm but understated |

### Generation notes

- Generate each line as a separate short clip — don't batch multiple lines into one audio file.
- Keep total voice-bark count small for alpha (the table above is the complete alpha-scope list per Sailor — resist adding more without a specific need, per `09` §3c's note that voice is the first cut under time pressure).
- Basic-attack barks should be used sparingly in-engine (not on every single hit) regardless of how many variants exist — this is an implementation note for whoever wires the AudioSource triggers, not a generation concern, but worth stating here so the *number* of variants generated matches actual planned usage frequency (1-2 variants per Sailor is enough; don't over-produce).

### File destinations

`Assets/Audio/SFX/VO/`, named `VO_Ace_<Moment>.wav` / `VO_Anchor_<Moment>.wav` (e.g. `VO_Ace_RoundStart_01.wav`, `VO_Anchor_Super_DropAnchor.wav`).
