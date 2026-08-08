using Unity.Netcode;
using UnityEngine;

namespace BlueWaterRiptide.Networking.Runtime
{
    /// <summary>
    /// Local mirror of MatchController's phase concept, deliberately NOT a reference to
    /// Core.MatchController's actual state enum: that type is owned by parallel work on
    /// MatchController and this networking slice is scoped to not guess at or couple to its
    /// exact shape (see task scope notes). A future host-side integration layer maps
    /// Core.MatchController's real phase onto this one when it wires SetRoundState below.
    /// </summary>
    public enum MatchPhase
    {
        Setup,
        RoundCountdown,
        Combat,
        SuddenDeath,
        RoundEnd,
        MatchEnd,
        Results,
    }

    /// <summary>
    /// Match-flow replication (Plan 02 §1): round number, per-team round wins, the round timer, the
    /// Rising Tide ring's current step, and match phase — everything a client-side HUD needs to
    /// render without re-deriving match logic locally. SetRoundState/SetTideRing are intentionally
    /// simple setters rather than something that reacts to MatchController events itself; wiring
    /// them to the real event source is future host-side integration work, not this class's job.
    /// </summary>
    public sealed class NetworkMatchState : NetworkBehaviour
    {
        public readonly NetworkVariable<int> RoundNumber = new NetworkVariable<int>(1);
        public readonly NetworkVariable<int> RoundWinsTeamA = new NetworkVariable<int>(0);
        public readonly NetworkVariable<int> RoundWinsTeamB = new NetworkVariable<int>(0);
        public readonly NetworkVariable<float> RoundTimer = new NetworkVariable<float>(0f);
        public readonly NetworkVariable<int> TideRingIndex = new NetworkVariable<int>(0);
        public readonly NetworkVariable<MatchPhase> Phase = new NetworkVariable<MatchPhase>(MatchPhase.Setup);

        /// <summary>Host-only: mirrors MatchController's round bookkeeping onto the replicated variables in one call.</summary>
        public void SetRoundState(int round, int winsA, int winsB, float timer, MatchPhase phase)
        {
            if (!IsServer)
            {
                Debug.LogWarning("NetworkMatchState.SetRoundState called on a non-server instance; ignored.");
                return;
            }

            RoundNumber.Value = round;
            RoundWinsTeamA.Value = winsA;
            RoundWinsTeamB.Value = winsB;
            RoundTimer.Value = timer;
            Phase.Value = phase;
        }

        /// <summary>Host-only: mirrors the Rising Tide hazard's current ring step (see Gameplay/Court/RisingTideHazard) onto the replicated variable.</summary>
        public void SetTideRingIndex(int ringIndex)
        {
            if (!IsServer)
            {
                Debug.LogWarning("NetworkMatchState.SetTideRingIndex called on a non-server instance; ignored.");
                return;
            }

            TideRingIndex.Value = ringIndex;
        }
    }
}
