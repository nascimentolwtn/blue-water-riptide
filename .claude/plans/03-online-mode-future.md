# Plan 03 — Online Mode (Future / Design Direction Only)

Internet play is explicitly deferred. This document sets the architectural direction so modes 1–2 leave the right seams in place, without speccing a backend build. Nothing here is scheduled; treat it as constraints on today's code plus a sketch of tomorrow's.

## 1. Player-facing concept

Two entry paths, same match once connected:
- **Play with friends**: a player opens a lobby and shares a short **join code** (6 chars); friends enter the code and land in the same lobby (same lobby UI as LAN — co-op vs AI, or private PvP). Join codes first, friend lists later.
- **Solo/duo queue**: player (or a pre-made pair) queues; matchmaking fills a 3v3 PvP match with strangers of similar trophy count.
- Later layers (not v1 of online either): persistent friend list, invites via push, ranked queue.

## 2. Architecture direction

### Topology: keep client-hosted, add a relay
The LAN architecture (one player device is the authoritative host, others send inputs) carries over; the internet problem is NAT traversal and discovery, not simulation. The natural Unity-ecosystem fit, given NGO + UTP are already in use:
- **Unity Relay** — replaces direct IP connectivity; UTP has first-class Relay support, so switching transports is configuration, not gameplay code.
- **Unity Lobby service** — replaces UDP broadcast discovery; provides the join-code flow and lobby metadata out of the box.
- **Unity Matchmaker (or a thin custom ticket service)** — for the solo-queue path; trophy-based fairness rules.
- **Unity Authentication (anonymous first)** — stable player id for lobbies/matchmaking; upgradeable to platform accounts (Google Play Games) later; unlocks cloud save for the existing id-keyed profile.

These are defaults, not commitments — the seams below keep alternatives (self-hosted relay, Steam-style P2P later on other platforms) open.

### Honest caveats to design around (when the time comes)
- Client-hosted online means host-advantage latency and host-quit fragility; acceptable for casual play-with-friends, marginal for ranked. If ranked ever matters, budget for dedicated game servers (Unity Game Server Hosting or containerized Linux headless builds — the transport-agnostic sim already runs headless for AI batch testing, which is most of the porting work).
- Cheating: host-authoritative protects against client cheats but not a cheating host. Casual-first scope makes this acceptable initially; don't promise competitive integrity before server hosting exists.
- Progression trust: the "each client computes its own rewards from replicated results" model (Plan 02) stays fine until rewards have real value; cloud-validated results are a dedicated-server-era problem.

## 3. Do this now (in modes 1–2) to make online easier later

Checklist — each item is cheap now and expensive to retrofit:

1. **Keep the simulation transport-agnostic** (already mandated in `00` §7): all gameplay driven by `InputCommand`s through `IInputDriver`, no NGO types outside `Assets/Scripts/Networking/`, enforced by asmdef direction. This is the single highest-leverage item.
2. **Abstract session discovery/joining behind one interface**: `ISessionProvider` with operations {create, advertise, browse/resolve, join, leave}. LAN implements it with UDP broadcast + direct connect; online implements it with Lobby + Relay. The Host/Join UI should bind to the interface, not to the LAN classes, so the online version reuses the screens.
3. **Make the transport a swappable config**: isolate UTP endpoint setup in `NetworkBootstrap` behind a small factory (`direct(ip,port)` today, `relay(allocation)` tomorrow). Never scatter transport configuration.
4. **Stable content ids**: Sailors, abilities, arenas, and ruleset referenced by string/hash id in all wire messages and save data (never array index or asset reference) — protects cross-version matches and server-side validation later.
5. **Protocol versioning from day one**: the LAN advertisement's `protocolVersion` gate (Plan 02) is the same mechanism online compatibility checks will use; keep the constant and the reject-UX generic.
6. **Id-keyed, serializable player profile** (already in `00` §6): profile keyed by a locally generated GUID so adopting Unity Authentication becomes "map GUID → cloud id + merge", not a save-format rewrite.
7. **Keep lobby state a plain serializable model**: `LobbyState` (players, teams, picks, ready) should be a data object replicated by NGO, not logic embedded in NetworkBehaviours — the online Lobby service will need to mirror the same model through a different channel.
8. **Latency headroom in game feel**: LAN hides latency; make aim/hit feedback tolerate ~80–120ms now (server-confirmed hits with client-side anticipation VFX, generous projectile visuals) so online doesn't require re-tuning every ability. Cheap to bake into ability-framework conventions; painful later.
9. **Rejoin token flow** (Plan 02) is the reconnect story online too — keep it independent of how the connection was established.
10. **Headless sim runs** (Plan 01 M4's AI-vs-AI batches) double as the seed of dedicated-server viability — keep the sim bootable without rendering/UI.

## 4. Rough sequencing (when online is greenlit)

1. Anonymous auth + Relay + join-code Lobby behind `ISessionProvider` → "play with friends over the internet" with LAN-parity features. This alone is most of the user value.
2. Solo-queue matchmaking (trophy-banded) on the same foundation.
3. Cloud save adoption; friend list/invites.
4. Evaluate dedicated servers only if ranked/competitive play becomes a goal.

## 5. Remote vs Editor-only work

The "do this now" checklist (§3) is entirely **[File]** work — interfaces, transport-agnostic architecture, id-keying conventions, no scenes/prefabs/services to wire yet. When online is actually greenlit, expect the split to look like Plan 02's: session/relay C# logic is [File], service dashboard setup (Unity Relay/Lobby/Matchmaker/Authentication project config) and any new lobby-entry screens are **[Editor/Asset]**. See the general rule in `00` §8 and the classified backlog in `.claude/napkin.md`.
