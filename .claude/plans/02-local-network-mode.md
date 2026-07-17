# Plan 02 — Local Network (LAN) Mode

Nearby devices on the same Wi-Fi play together with no internet dependency: **PvP** (players split across both teams) or **co-op** (players together on Team A vs an AI Team B). Built on Unity Netcode for GameObjects (NGO) + Unity Transport (UTP/UDP), with UDP broadcast for host discovery. Reuses the entire simulation from Plan 01 unchanged — this plan adds a transport layer and session UI, not new gameplay.

## 1. Architecture

### Host/client model
- **Client-hosted, server-authoritative**: one phone is host (server+client in one process, NGO host mode). The host runs the authoritative simulation — the same `MatchController`/combat code Single Player runs locally.
- **Clients send `InputCommand`s, receive state.** A client's touch input goes through the same `TouchInputDriver`, but its commands are forwarded to the host via a `NetworkInputDriver` (ServerRpc, unreliable-sequenced channel). The host feeds them into the simulation exactly as if they were local. Down the wire, clients receive replicated state (positions, HP, ammo, Super charge, match state) and render it.
- **Client-side presentation smoothing only**: interpolation on remote pawns, immediate local movement prediction for the client's own pawn with server reconciliation (position snap-with-lerp on divergence > threshold). Keep prediction minimal in v1 — LAN latency (<10ms typical) makes this forgiving; do not build rollback.
- Empty player slots in either mode are filled with `AIInputDriver` participants **on the host** — AI is host-side only, clients just see replicated pawns. This is the same AI from Plan 01, which is why the input-driver seam matters.

### What gets networked (NGO specifics)
- `NetworkManager` + UTP configured for LAN (direct connect to host IP:port, default port e.g. 7777).
- Pawns: `NetworkObject` + `NetworkTransform` (server-authoritative) + a thin `NetworkSailorState` (`NetworkVariable`s: HP, ammo, super charge, KO flag). Ability activations are broadcast as ClientRpc events carrying (ability id, aim, origin, tick) so clients play the full visual locally instead of syncing every projectile transform; projectiles are deterministic-enough from their launch parameters, with server-authoritative hit results.
- Match flow: `NetworkMatchState` (round number, round wins, timer, tide ring index, match phase) as NetworkVariables; round/match transitions as ClientRpcs mirrored from `MatchController` events.
- Scene flow uses NGO scene management for the Match scene load so late-loading clients synchronize.

## 2. LAN discovery (UDP broadcast)

NGO has no built-in discovery; build a small standalone component (no NGO dependency, reusable):

- **`LanSessionAdvertiser`** (host): broadcasts a JSON datagram to `255.255.255.255:47777` every 1s while the lobby is open: `{ gameId, protocolVersion, hostName, ip, port, mode: "coop"|"pvp", playersCurrent, playersMax, lobbyState }`. Stops on match start (configurable — allow mid-lobby joins only).
- **`LanSessionScanner`** (client): listens on 47777, aggregates advertisements into a session list (expire entries not re-seen for 3s), surfaces them to the Join screen.
- `protocolVersion` mismatch → session shown greyed with "version mismatch" (both devices must update). Bump this constant on any wire-format change.
- Android specifics: acquire `WifiManager.MulticastLock` (broadcast receive is filtered on many devices without it); request it only while the scanner is active. Document that AP/client isolation on some routers silently breaks discovery — the Join screen therefore always includes a **"Join by IP"** manual-entry fallback.

## 3. Session setup & UI flow

From MainMenu, "Local Play" leads to:

1. **Host or Join screen** — `Host Game` | list of discovered sessions (auto-refreshing) | `Join by IP`.
2. **Host config** (host only): pick mode — **Co-op** (all humans on Team A, AI fills Team A remainder and all of Team B) or **PvP** (humans distributed across teams, AI fills gaps; host can drag players between teams). v1 team size is fixed 3v3.
3. **Lobby** (all players): shows joined players, team assignment, each player's Sailor pick (Ace/Anchor from *their own* profile's unlocked list), ready flags. Host's Start button enables when all ready. Character locking is not needed (duplicates allowed, consistent with Plan 01 tinting).
4. **Match** — host builds the `Session` (participants: local human, remote humans, AI fills) and drives the networked match; no arena choice (v1 loads Tideline Cove directly — when arenas multiply, the arena id becomes a host-config field written into the Session and advertised in the lobby, nothing else changes).
5. **Results** — replicated results overlay; each device applies progression to its **own local profile** (LAN grants SP-tier trophy rewards; host is not trusted to write other players' progression — each client computes rewards from the replicated result).

## 4. Disconnects & reconnects

- **Client disconnect mid-match** (host view): pawn is immediately taken over by an `AIInputDriver` (Bosun difficulty) — the match never stalls. A 60s **rejoin window** keeps the participant slot reserved, keyed by a `rejoinToken` (GUID issued at lobby join, cached on the client).
- **Client reconnect**: client reconnects to the same host endpoint, presents its rejoinToken in the connection-approval payload; host swaps the AI driver back out and NGO resyncs state (full NetworkVariable snapshot covers it — no custom state transfer needed).
- **Host loss = match over.** No host migration in v1 (out of scope; note as a future option). Clients detect transport disconnect, show "Host left — match ended", return to Host/Join screen. The match counts as neither win nor loss.
- **App backgrounding** (mobile reality): treat OS-backgrounded clients as disconnects after a 10s grace (UTP heartbeat timeout); rejoin flow covers the "phone call interrupted my game" case.
- Connection approval (NGO callback) validates: protocolVersion, rejoinToken (if reconnecting), lobby capacity, lobby state (reject joins after start unless rejoining).

## 5. Script layout

All new code in `Assets/Scripts/Networking/` (plus UI screens in `UI/`). Nothing outside this folder may reference NGO types — enforce with an asmdef: `Networking.asmdef` references `Core`/`Gameplay`/`Characters`, never the reverse.

| Path | Contents |
|---|---|
| `Networking/Discovery/` | `LanSessionAdvertiser`, `LanSessionScanner`, `SessionAdvertisement` (serializable), multicast-lock helper |
| `Networking/Session/` | `LanSessionLauncher` (builds `Session` from lobby state, mirrors `SinglePlayerLauncher`), `LobbyState` (NetworkVariables: players, teams, picks, ready flags), connection approval, `rejoinToken` handling |
| `Networking/Runtime/` | `NetworkInputDriver` (client input → ServerRpc → host-side `IInputDriver`), `NetworkSailorState`, `NetworkMatchState`, ability-event RPC relay, pawn interpolation/prediction |
| `Networking/` | `NetworkBootstrap` (NetworkManager config, UTP settings, host/join entry points), disconnect/timeout handling |
| `UI/LocalPlay/` | Host-or-Join screen, host config, lobby screen, "Join by IP" dialog, connection-lost dialogs |
| `Assets/Scenes/` | no new scenes — MainMenu hosts the Local Play screens; Match scene is shared (NGO scene-managed when networked) |

## 6. Milestones

**M1 — Wire-up spike**
- NGO host + 1 client on two devices, direct IP connect, both controlling Ace pawns in Tideline Cove; movement + basic attack replicated; host-authoritative damage/KO.
- Exit criteria: a playable 1v1 over Wi-Fi with no visible rubber-banding at LAN latency.

**M2 — Discovery + lobby**
- UDP advertise/scan, session list UI, Join by IP fallback, multicast lock, protocol versioning.
- Full lobby: co-op/PvP config, team assignment, Sailor picks, ready/start, AI backfill on the host.
- Full Sink or Swim match flow replicated (rounds, timer, sudden death, results), Supers and both Sailors working networked.

**M3 — Robustness**
- Disconnect → AI takeover; rejoin window + token flow; host-loss handling; backgrounding grace.
- Late-join rejection, capacity limits, version-mismatch UX.
- Two-device soak testing on real hardware (including a deliberately hostile router setup for the Join-by-IP path).

**M4 — Polish**
- Lobby QoL (player name from profile, team-color preview, connection-quality indicator), reconnect UX, error-message pass.
- Performance: bandwidth audit (target well under 30KB/s per client — trivial at this scale, but establish the measuring habit), CPU cost of host+client on the weakest supported device (the host phone runs sim + AI + rendering).
- Co-op difficulty tuning (AI enemy team at selectable difficulty, shared by both human players).
