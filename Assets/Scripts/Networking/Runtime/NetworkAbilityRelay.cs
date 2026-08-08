using System;
using Unity.Netcode;
using UnityEngine;

namespace BlueWaterRiptide.Networking.Runtime
{
    /// <summary>
    /// Broadcasts ability activations as events, not transforms (Plan 02 §1): rather than syncing
    /// every projectile's position every tick, the host tells everyone "ability X fired from here,
    /// aimed there, at this tick" and each client plays the full local visual/VFX/audio for it — the
    /// host alone stays authoritative for hit results (damage/knockback land through
    /// NetworkSailorState, not through anything in this class). Sibling component to
    /// NetworkSailorState rather than folded into it, since ability relaying is a one-shot broadcast
    /// concern while sailor state is continuously-synced data — different enough shapes to not share
    /// a class.
    ///
    /// [ClientRpc]'s default target is every connected client EXCEPT the server, so the host's own
    /// client wouldn't otherwise see the event — BroadcastAbilityActivated raises it locally first
    /// and then fans it out over the RPC, giving host and clients the same event on every peer via
    /// one subscription in the (Editor-side, out of scope here) ability-visuals layer.
    /// </summary>
    public sealed class NetworkAbilityRelay : NetworkBehaviour
    {
        /// <summary>Raised on every peer, including the host's own client, when BroadcastAbilityActivated fires server-side.</summary>
        public event Action<string, Vector2, Vector3, uint> AbilityActivated;

        /// <summary>Host-only: call from wherever combat code resolves an ability activation to fan it out for local visuals everywhere.</summary>
        public void BroadcastAbilityActivated(string abilityId, Vector2 aim, Vector3 origin, uint tick)
        {
            if (!IsServer)
            {
                Debug.LogWarning("NetworkAbilityRelay.BroadcastAbilityActivated called on a non-server instance; ignored.");
                return;
            }

            AbilityActivated?.Invoke(abilityId, aim, origin, tick);
            AbilityActivatedClientRpc(abilityId, aim, origin, tick);
        }

        [ClientRpc]
        void AbilityActivatedClientRpc(string abilityId, Vector2 aim, Vector3 origin, uint tick)
        {
            AbilityActivated?.Invoke(abilityId, aim, origin, tick);
        }
    }
}
