# Plan 04 — Menu, Navigation & Lobby Flow

Defines every screen between app launch and `MatchController` taking over, and the path back out. Builds on the scene list in Plan 01 §1 (`Boot → MainMenu → TeamSelect → Match`) and the Local Play screens in Plan 02 §3 — this plan does not add scenes, it organizes the screens *inside* them and specifies the navigation layer. UI is a thin presentation layer over `Core` (per `00` §7): screens render Session/profile state and raise intents; they never own game logic.

## 1. Scenes vs screens

Four scenes total (unchanged from Plan 01). Everything menu-side is a **screen** — a UI prefab pushed onto a navigation stack inside the `MainMenu` scene. This keeps transitions instant (no scene loads between menu screens) and matches Plan 02's decision that Local Play screens live in MainMenu.

| Scene | Screens hosted |
|---|---|
| `Boot` | Splash (static logo while services init — save load, catalogs, audio) |
| `MainMenu` | Home, Sailor Locker, Voyage Road, Settings, Mode Select, Local Play stack (Host-or-Join, Host Config, Lobby), Online stub |
| `TeamSelect` | Single Player pre-match Sailor pick (Plan 01 §1) |
| `Match` | HUD + Results overlay (Plan 01 §1 — Results is an overlay, not a scene) |

**Two different "Sailor select" surfaces — don't merge them:**
- **Sailor Locker** (Home → Locker): browse roster, view stats, spend Doubloons on level-ups. Meta screen, no match context.
- **Pre-match pick**: `TeamSelect` scene for Single Player; the Lobby screen's pick row for LAN. These write the choice into the `Session`; the Locker never does.

## 2. Navigation map

```
App launch
  └─ Boot (splash) ──────────────────────────► Home
                                                │
      ┌─────────────┬──────────────┬───────────┼────────────┐
      ▼             ▼              ▼           ▼            ▼
  Sailor Locker  Voyage Road   Settings    [Doubloon     PLAY
  (browse,       (trophy       (audio,      counter —      │
   level-up)      track,        difficulty,  display       ▼
      │           claim         joystick     only)     Mode Select
      ▼           nodes)        side)                 ┌────┼─────────┐
  Sailor Detail                                       ▼    ▼         ▼
  (stats, level                                Single   Local     Online
   -up button)                                 Player   Network   (stub/
                                                  │        │      greyed)
                                                  ▼        ▼         │
                                            TeamSelect  Host-or-Join ▼
                                              (scene)      │      (future:
                                                  │   ┌────┴────┐  Join Code /
                                                  │   ▼         ▼  Solo Queue)
                                                  │  Host     Join list
                                                  │  Config   / by IP
                                                  │   └────┬────┘
                                                  │        ▼
                                                  │      Lobby
                                                  │  (teams, picks,
                                                  │   ready-up)
                                                  └────┬───┘
                                                       ▼
                                              Match scene (MatchController)
                                                       │
                                                       ▼
                                               Results overlay
                                                ├─ Rematch ──► Match (same Session*)
                                                └─ Exit ─────► Home
```
\* LAN rematch returns everyone to the Lobby (ready flags reset) rather than relaunching directly — the host may want to reshuffle teams.

Back navigation: hardware/UI back pops one screen off the stack; back on Home shows a quit-confirm dialog. Back from Lobby leaves the session (host: closes it, clients get "host left lobby").

## 3. Screen specs

### Boot / Splash
- Purpose: block until services are up (Plan 01 §1: "keep it dumb").
- UI: logo, version string, indeterminate progress. No interaction. Auto-advances to Home.
- On corrupted save: show a recover/reset dialog here, before anything reads the profile.

### Home
The hub. Everything v1's meta actually supports is one tap away; nothing else gets a button.
- **PLAY** — large, bottom-center. → Mode Select.
- **Sailor Locker** — portrait button of the currently selected Sailor. → Locker.
- **Voyage Road** — progress widget: total-trophy count + next-unlock preview. → Voyage Road screen. A claim badge appears when a node is claimable.
- **Trophy total** — account-level count (sum of per-Sailor trophies, `00` §6) next to the Fleet Rank badge (Plan 05).
- **Doubloon counter** — top corner, display only (spending happens in the Locker; no shop screen in v1).
- **Settings** — gear icon. → Settings.
- **Player name** — from profile; tap to rename (writes profile).
- *Roadmap placeholders (do not build UI): shop, clubs, events/quests board, news banner. Consistent with `00` §6's out-of-scope list — leave layout headroom, ship no dead buttons.*

### Sailor Locker
- Grid of roster tiles — **v1: exactly 2 tiles** (Ace, Anchor). Locked Sailors (Anchor pre-unlock) shown greyed with their Voyage Road unlock hint (Plan 01 §1 convention). Future-Sailor teaser tiles come from the roadmap data, not hard-coded.
- Tap → **Sailor Detail**: stats, ability descriptions (attack/Super/Trait from `SailorDefinition`), current level, level-up button with Doubloon cost (`ProgressionService.TryLevelUp`), per-Sailor trophy count.
- "Select" here sets the profile's default Sailor (pre-selected in TeamSelect/Lobby).

### Voyage Road
- Single horizontal linear track (`00` §6): nodes with trophy thresholds — Anchor unlock early, Doubloon caches, cosmetic flags/emblems; pre-labeled future nodes render as teasers.
- Current trophy total marks position; claimable nodes pulse; claim applies via `ProgressionService` and animates.

### Settings
- Audio (music/SFX sliders), AI difficulty (Deckhand/Bosun/Skipper, Plan 01 §3), joystick side, player name, credits, save-reset (double-confirm).

### Mode Select
- Three cards, one per **connection mode** (this screen never picks a ruleset — v1 has one, `00` §1):
  - **Single Player** → TeamSelect scene.
  - **Local Network** → Host-or-Join screen (Plan 02 flow).
  - **Online** → v1 ships this card greyed with "Coming soon" (or compiled out behind a build flag — decide at first release; greyed is preferred so the roadmap is visible).

### TeamSelect (Single Player, own scene per Plan 01)
- Sailor pick (2 tiles, default = Locker selection), difficulty display (from settings), Confirm → `SinglePlayerLauncher` builds the `Session` → Match scene.
- **No arena select** — `ArenaCatalog.Default` hard-wired (Plan 01 §1). When arenas multiply, arena choice becomes a field written into the Session here; match code never changes.

### Local Network screens (Plan 02 §3 — restated as UI spec)
1. **Host-or-Join**: `Host Game` button; auto-refreshing discovered-session list from `LanSessionScanner` (host name, mode, players n/max; version-mismatch rows greyed per Plan 02 §2); `Join by IP` manual-entry fallback (always visible).
2. **Host Config** (host only): Co-op vs PvP toggle; AI difficulty for co-op enemy team. Confirm → opens Lobby and starts `LanSessionAdvertiser`.
3. **Lobby** (all players): player rows grouped by team (host can drag players between teams in PvP), each row shows name, Sailor pick, ready flag. Local player's pick row offers *their own* unlocked Sailors; duplicates allowed. Ready toggle per player; host's **Start** enables when all ready → host builds `Session`, NGO scene-loads Match.
4. Connection-lost/host-left dialogs route back to Host-or-Join (Plan 02 §4).

### Online screens (future — sketch only, per Plan 03)
Deliberately thin; do not spec further until online is greenlit. The intent is screen reuse:
- **Online entry**: two cards — `Play with Friends` (create lobby → shows 6-char join code; or enter a code) and `Solo Queue` (queue with trophy-band matchmaking, searching spinner, cancel).
- Both paths land in **the same Lobby screen** as LAN — Plan 03 §3.2 requires the Host/Join UI to bind to `ISessionProvider`, not LAN classes, precisely so these screens are reused with a different provider. Only the entry screens (code entry, queue status) are new UI.

### Match HUD → Results
- Match scene UI is the HUD from `00` §7.6 (joystick, attack/Super buttons, round pips, timer, teammate bars), driven only by `MatchController` events.
- **Results overlay** on `OnMatchEnd`: winner banner, per-player KO line, trophy delta and Doubloon reward (computed locally by `ProgressionService` from the match result — each device its own profile, Plan 02 §5), Voyage Road progress tick if a node was crossed. Buttons: Rematch / Exit to Home.

## 4. UI architecture (`Assets/Scripts/UI`)

- **`UIStateController`** (`Assets/Scripts/UI/Navigation/`): owns a stack of `UIScreen` prefabs inside MainMenu; `Push(screenId)` / `Pop()` / `Reset(home)`; handles hardware back and transition animation. It is navigation plumbing only — it never touches gameplay or networking types.
- **`UIScreen`** base: `OnPush/OnPop/OnFocus`; screens declare which Core services they observe.
- **Data flow, strictly one-way per `00` §7**: screens *read* from `SaveService`/`ProgressionService`/`Session`/`LobbyState` and *subscribe* to their change events (`OnProfileChanged`, `OnTrophiesChanged`, `OnLobbyChanged`, `MatchController.OnMatchEnd`). Screens *write* only by calling intent methods (`TryLevelUp`, `ClaimVoyageNode`, `SetReady`, launcher `Start...`). No screen mutates profile or lobby state directly, and no gameplay code references a UI type.
- Mode launchers stay in their homes: `SinglePlayerLauncher` in `Core/Modes/`, `LanSessionLauncher` in `Networking/Session/` — Mode Select calls them; they build the `Session`.

| Path | Contents |
|---|---|
| `UI/Navigation/` | `UIStateController`, `UIScreen`, screen registry, transition tweens |
| `UI/Home/` | Home screen, Doubloon/trophy widgets, quit dialog |
| `UI/Locker/` | Locker grid, Sailor Detail, level-up flow |
| `UI/Progression/` | Voyage Road screen, node claim animation |
| `UI/ModeSelect/` | Mode Select cards, TeamSelect screen |
| `UI/LocalPlay/` | Host-or-Join, Host Config, Lobby, Join-by-IP dialog, connection dialogs (Plan 02 §5) |
| `UI/Match/` | HUD, aim indicators, Results overlay (Plan 01 §4) |
| `UI/Settings/` | Settings screen |

## 5. v1 scope honesty

- **No arena-select screen anywhere** — 1 arena (`00` §5); the id is hard-wired at Session build. Adding map select later = one new field + one new screen before Match; nothing here restructures.
- **Sailor select is trivial by design** — 2 tiles in TeamSelect/Lobby, 2 tiles + teasers in the Locker. Do not build filtering, sorting, or squadron tabs until the roster justifies them (`00` §4 roadmap).
- **Online is a greyed card + this sketch** — building its entry screens now would violate Plan 03's deferral.
- **No shop/clubs/quests screens** — roadmap placeholders only, matching `00` §6's exclusions. Plan 05 adds the small v1 gamification surfaces (first-win bonus, achievements list) as sub-sections of existing screens, not new top-level destinations.

## 6. Remote vs Editor-only work

`UIStateController`/`UIScreen` nav-stack plumbing (§4) is **[File]** work — pure C# writeable without Unity open. Every actual *screen* (Home, Locker, Voyage Road, Mode Select, Local Play stack, HUD, Results overlay — Canvas layout, prefab assembly, visual hierarchy) is **[Editor/Asset]**: it needs the Editor's UI tools. See the general rule in `00` §8 and the classified backlog in `.claude/napkin.md`.
