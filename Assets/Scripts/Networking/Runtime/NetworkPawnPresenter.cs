using BlueWaterRiptide.Characters;
using BlueWaterRiptide.Core;
using Unity.Netcode;
using UnityEngine;

namespace BlueWaterRiptide.Networking.Runtime
{
    /// <summary>
    /// Per-pawn host/client split for the LAN M1 spike (Plan 10 M1, Plan 02 §1 "client-side
    /// presentation smoothing only"). LanMatchBootstrap wires the host's own copy of each spawned
    /// pawn as the authoritative simulation directly (SailorPawn.Initialize + a real/buffered
    /// IInputDriver); this component only ever does anything on every OTHER peer's local copy of
    /// that same NetworkObject, where SailorPawn.Update() must NOT run — that would be a second,
    /// non-authoritative combat simulation racing the host's. Disabling SailorPawn here (rather than
    /// never adding it on non-host peers) keeps one prefab/one code path for both roles; position
    /// then comes from NetworkTransform alone, HP/ammo/Super/KO from NetworkSailorState.
    /// </summary>
    [RequireComponent(typeof(SailorPawn))]
    [RequireComponent(typeof(NetworkInputDriver))]
    public sealed class NetworkPawnPresenter : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            if (IsServer) return; // host: LanMatchBootstrap already configured this instance as the authoritative pawn.

            GetComponent<SailorPawn>().enabled = false;

            if (!IsOwner) return; // the other peer's pawn as seen from here — nothing else to configure locally.

            // Lazily falls back to Camera.main inside KeyboardMouseInputDriver if the client's own
            // presentation camera (LanMatchBootstrap.ClientPreparePresentation) hasn't built one yet
            // by the time this spawn message arrives.
            IInputDriver localSource = Application.isEditor
                ? new KeyboardMouseInputDriver(transform, Camera.main)
                : new TouchInputDriver();

            GetComponent<NetworkInputDriver>().InitializeLocalSource(localSource);
        }
    }
}
