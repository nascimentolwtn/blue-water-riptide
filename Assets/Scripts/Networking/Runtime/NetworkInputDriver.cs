using BlueWaterRiptide.Core;
using Unity.Netcode;
using UnityEngine;

namespace BlueWaterRiptide.Networking.Runtime
{
    /// <summary>
    /// The client half and the host half of the same seam (Plan 02 §1): on the owning client,
    /// Sample() reads a wrapped local <see cref="IInputDriver"/> (whatever TouchInputDriver/
    /// KeyboardMouseInputDriver Single Player already uses) and relays each command to the host over
    /// an unreliable-sequenced ServerRpc; on the host, Sample() ignores the wrapped driver entirely
    /// and returns the latest command received for this pawn, so MatchController's simulation loop
    /// can treat a remote human exactly like a local one — it only ever calls IInputDriver.Sample.
    ///
    /// One instance per networked pawn, attached alongside NetworkSailorState. RpcDelivery.Unreliable
    /// is correct here because input is a continuous stream (Plan 02 §1) — a dropped sample is
    /// superseded by the next one a tick later, so paying for reliable retransmission would only add
    /// latency without improving anything.
    /// </summary>
    public sealed class NetworkInputDriver : NetworkBehaviour, IInputDriver
    {
        IInputDriver _localSource;
        InputCommand _lastReceived;
        bool _hasReceivedAny;

        /// <summary>Wires in the local driver to sample from and relay. Only meaningful on the owning client — call this after the object is spawned and IsOwner is known.</summary>
        public void InitializeLocalSource(IInputDriver localSource)
        {
            _localSource = localSource;
        }

        /// <summary>
        /// The one method the simulation calls, on both sides of the connection. Owning client:
        /// samples + relays + returns the fresh command (so the client's own prediction has zero
        /// extra latency waiting on a round trip). Host, for a remote-owned pawn: returns whatever
        /// arrived last, defaulting to InputCommand.None before the first packet lands.
        /// </summary>
        public InputCommand Sample(double time)
        {
            if (IsOwner && _localSource != null)
            {
                InputCommand command = _localSource.Sample(time);
                SubmitInputServerRpc(command.Move, command.Aim, command.FirePressed, command.FireHeld, command.SuperPressed, command.Timestamp);
                return command;
            }

            return _hasReceivedAny ? _lastReceived : InputCommand.None(time);
        }

        [ServerRpc(Delivery = RpcDelivery.Unreliable)]
        void SubmitInputServerRpc(Vector2 move, Vector2 aim, bool firePressed, bool fireHeld, bool superPressed, double timestamp)
        {
            // No sequence-number/staleness check: UTP's unreliable-sequenced channel already drops
            // out-of-order packets at the transport layer, so whatever arrives here is already the
            // newest the network delivered.
            _lastReceived = new InputCommand(move, aim, firePressed, fireHeld, superPressed, timestamp);
            _hasReceivedAny = true;
        }
    }
}
