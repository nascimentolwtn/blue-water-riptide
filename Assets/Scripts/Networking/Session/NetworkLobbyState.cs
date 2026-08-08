using System;
using System.Collections.Generic;
using BlueWaterRiptide.Core;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace BlueWaterRiptide.Networking.Session
{
    /// <summary>
    /// Wire-replicated mirror of one <see cref="LobbyPlayerEntry"/>. NetworkList/NetworkVariable
    /// require an unmanaged, IEquatable value type, so plain C# strings become fixed-capacity
    /// FixedStrings here — this struct exists purely as the NGO-visible shadow of the plain entry,
    /// never constructed or read outside <see cref="NetworkLobbyState"/>.
    /// </summary>
    public struct NetworkLobbyPlayerEntry : INetworkSerializable, IEquatable<NetworkLobbyPlayerEntry>
    {
        public FixedString64Bytes ParticipantIdOrToken;
        public Team Team;
        public FixedString32Bytes SailorId;
        public bool IsReady;
        public bool IsAI;
        public FixedString64Bytes DisplayName;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ParticipantIdOrToken);
            serializer.SerializeValue(ref Team);
            serializer.SerializeValue(ref SailorId);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref IsAI);
            serializer.SerializeValue(ref DisplayName);
        }

        public bool Equals(NetworkLobbyPlayerEntry other) =>
            ParticipantIdOrToken.Equals(other.ParticipantIdOrToken) &&
            Team == other.Team &&
            SailorId.Equals(other.SailorId) &&
            IsReady == other.IsReady &&
            IsAI == other.IsAI &&
            DisplayName.Equals(other.DisplayName);

        public override bool Equals(object obj) => obj is NetworkLobbyPlayerEntry other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(ParticipantIdOrToken, Team, SailorId, IsReady, IsAI, DisplayName);

        public static NetworkLobbyPlayerEntry FromPlain(LobbyPlayerEntry entry) => new NetworkLobbyPlayerEntry
        {
            ParticipantIdOrToken = entry.ParticipantIdOrToken ?? string.Empty,
            Team = entry.Team,
            SailorId = entry.SailorId ?? string.Empty,
            IsReady = entry.IsReady,
            IsAI = entry.IsAI,
            DisplayName = entry.DisplayName ?? string.Empty,
        };

        public LobbyPlayerEntry ToPlain() => new LobbyPlayerEntry
        {
            ParticipantIdOrToken = ParticipantIdOrToken.ToString(),
            Team = Team,
            SailorId = SailorId.ToString(),
            IsReady = IsReady,
            IsAI = IsAI,
            DisplayName = DisplayName.ToString(),
        };
    }

    /// <summary>
    /// Thin NGO replication wrapper around <see cref="LobbyState"/> (Plan 02 §5: "LobbyState should
    /// be a data object, not logic embedded in NetworkBehaviours"). This class does no lobby logic
    /// itself (no ready-check rules, no team-balancing) — it only mirrors the host's authoritative
    /// LobbyState onto NetworkList/NetworkVariable so clients can render it, and mirrors it back to
    /// plain data for BuildSession/UI to consume without ever touching an NGO type.
    ///
    /// Not wired to a live NetworkManager here — that requires an in-scene NetworkObject (Editor
    /// work, out of scope). ApplyFrom/ToPlain are the shape a future Editor-side lobby controller
    /// would call; both are safe to leave untested against a real NetworkManager for now.
    /// </summary>
    public sealed class NetworkLobbyState : NetworkBehaviour
    {
        readonly NetworkList<NetworkLobbyPlayerEntry> _players = new NetworkList<NetworkLobbyPlayerEntry>();

        public readonly NetworkVariable<SessionMode> Mode =
            new NetworkVariable<SessionMode>(SessionMode.Coop);

        public readonly NetworkVariable<bool> Started =
            new NetworkVariable<bool>(false);

        public int PlayerCount => _players.Count;

        /// <summary>Host-only: overwrites the replicated list/variables from an authoritative plain snapshot.</summary>
        public void ApplyFrom(LobbyState state)
        {
            if (!IsServer)
            {
                Debug.LogWarning("NetworkLobbyState.ApplyFrom called on a non-server instance; ignored.");
                return;
            }

            _players.Clear();
            foreach (var entry in state.Players)
            {
                _players.Add(NetworkLobbyPlayerEntry.FromPlain(entry));
            }

            Mode.Value = state.Mode;
            Started.Value = state.Started;
        }

        /// <summary>Snapshots the current replicated state back into a plain LobbyState (safe to call anywhere, host or client).</summary>
        public LobbyState ToPlain()
        {
            var state = new LobbyState
            {
                Players = new List<LobbyPlayerEntry>(_players.Count),
                Mode = Mode.Value,
                Started = Started.Value,
            };

            foreach (var networkEntry in _players)
            {
                state.Players.Add(networkEntry.ToPlain());
            }

            return state;
        }
    }
}
