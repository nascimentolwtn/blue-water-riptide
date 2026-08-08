using Unity.Netcode;
using UnityEngine;

namespace BlueWaterRiptide.Networking.Runtime
{
    /// <summary>
    /// Thin per-pawn replicated state (Plan 02 §1): HP/ammo/Super charge/KO flag, nothing else — no
    /// combat logic lives here. The host-side combat code (Gameplay/Combat, unchanged from Single
    /// Player) keeps being the single source of truth and just also writes through these setters;
    /// clients only ever read. NetworkVariable's default permissions (server-write, everyone-read)
    /// are exactly right for this, so nothing is configured beyond field defaults (Plan 02 §1: "don't
    /// over-configure permissions").
    ///
    /// Deliberately doesn't replicate transform — that's NetworkTransform's job on the same
    /// NetworkObject (Editor-side component, out of scope here).
    /// </summary>
    public sealed class NetworkSailorState : NetworkBehaviour
    {
        public readonly NetworkVariable<float> Hp = new NetworkVariable<float>(0f);
        public readonly NetworkVariable<int> Ammo = new NetworkVariable<int>(0);
        public readonly NetworkVariable<float> SuperCharge = new NetworkVariable<float>(0f);
        public readonly NetworkVariable<bool> IsKnockedOut = new NetworkVariable<bool>(false);

        public void SetHp(float value)
        {
            if (!IsServer)
            {
                Debug.LogWarning("NetworkSailorState.SetHp called on a non-server instance; ignored.");
                return;
            }

            Hp.Value = value;
        }

        public void SetAmmo(int value)
        {
            if (!IsServer)
            {
                Debug.LogWarning("NetworkSailorState.SetAmmo called on a non-server instance; ignored.");
                return;
            }

            Ammo.Value = value;
        }

        public void SetSuperCharge(float value)
        {
            if (!IsServer)
            {
                Debug.LogWarning("NetworkSailorState.SetSuperCharge called on a non-server instance; ignored.");
                return;
            }

            SuperCharge.Value = value;
        }

        public void SetKnockedOut(bool value)
        {
            if (!IsServer)
            {
                Debug.LogWarning("NetworkSailorState.SetKnockedOut called on a non-server instance; ignored.");
                return;
            }

            IsKnockedOut.Value = value;
        }
    }
}
