using System;
using System.Collections.Generic;

namespace BlueWaterRiptide.Networking.Session
{
    /// <summary>
    /// Host-side rejoin-token bookkeeping (Plan 02 §4): a disconnected client's slot stays reserved
    /// for a 60s window, keyed by the GUID issued when it first joined the lobby, so a reconnect can
    /// reclaim its team/Sailor pick instead of joining as a fresh participant. Pure C#, no NGO
    /// dependency — actually swapping the AI driver back out on reclaim, and validating the token in
    /// NGO's connection-approval callback, are host-integration concerns for a future Editor-side
    /// layer (this registry only tracks reservations and expiry).
    ///
    /// Externally ticked (Tick(float), same pattern as RisingTideHazard/LanSessionAdvertiser) rather
    /// than using a real-time Timer, so the sweep stays deterministic and testable without touching
    /// the wall clock.
    /// </summary>
    public sealed class RejoinTokenRegistry
    {
        public const float ExpirySeconds = 60f;

        sealed class Reservation
        {
            public LobbyPlayerEntry Entry;
            public float SecondsUntilExpiry;
        }

        readonly Dictionary<Guid, Reservation> _reservations = new Dictionary<Guid, Reservation>();

        /// <summary>Reserves a slot for a participant (typically called right after they take over an
        /// AI-controlled entry on disconnect), returning the token they must present to reclaim it.</summary>
        public Guid Reserve(LobbyPlayerEntry entry)
        {
            var token = Guid.NewGuid();
            _reservations[token] = new Reservation { Entry = entry, SecondsUntilExpiry = ExpirySeconds };
            return token;
        }

        /// <summary>Attempts to reclaim a reserved slot. Consumes the reservation on success — a token is single-use.</summary>
        public bool TryReclaim(Guid token, out LobbyPlayerEntry entry)
        {
            if (_reservations.TryGetValue(token, out var reservation))
            {
                _reservations.Remove(token);
                entry = reservation.Entry;
                return true;
            }

            entry = null;
            return false;
        }

        /// <summary>Drops a reservation without reclaiming it (e.g. the host cancels the lobby). No-op if the token is unknown/already expired.</summary>
        public void Cancel(Guid token) => _reservations.Remove(token);

        /// <summary>Advances all reservations' expiry clocks and drops any that have timed out. Call once per second (or every frame) from the host's lobby update loop.</summary>
        public void Tick(float deltaSeconds)
        {
            if (_reservations.Count == 0) return;

            List<Guid> expired = null;
            foreach (var kvp in _reservations)
            {
                kvp.Value.SecondsUntilExpiry -= deltaSeconds;
                if (kvp.Value.SecondsUntilExpiry <= 0f)
                {
                    (expired ??= new List<Guid>()).Add(kvp.Key);
                }
            }

            if (expired == null) return;
            foreach (var token in expired)
            {
                _reservations.Remove(token);
            }
        }
    }
}
