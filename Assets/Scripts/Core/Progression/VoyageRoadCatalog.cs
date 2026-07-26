using System.Collections.Generic;
using System.Linq;

namespace BlueWaterRiptide.Core.Progression
{
    public sealed class VoyageRoadNode
    {
        public string Id { get; }
        public int TrophyThreshold { get; }
        /// <summary>Sailor id this node unlocks, or null if it doesn't unlock a Sailor.</summary>
        public string UnlocksSailorId { get; }
        public int DoubloonReward { get; }

        public VoyageRoadNode(string id, int trophyThreshold, string unlocksSailorId = null, int doubloonReward = 0)
        {
            Id = id;
            TrophyThreshold = trophyThreshold;
            UnlocksSailorId = unlocksSailorId;
            DoubloonReward = doubloonReward;
        }
    }

    /// <summary>
    /// v1 Voyage Road (Plan 00 §6): a single linear track, Admiral Anchor at an early milestone
    /// then Doubloon caches. Thresholds/rewards below are placeholder tuning defaults, not
    /// balanced numbers — easy to retune later since they're plain code, not baked assets.
    /// </summary>
    public static class VoyageRoadCatalog
    {
        static readonly List<VoyageRoadNode> NodesAscending = new List<VoyageRoadNode>
        {
            new VoyageRoadNode("voyage.anchor-unlock", 20, unlocksSailorId: "sailor.anchor"),
            new VoyageRoadNode("voyage.doubloons-1", 40, doubloonReward: 100),
            new VoyageRoadNode("voyage.doubloons-2", 100, doubloonReward: 200),
            new VoyageRoadNode("voyage.doubloons-3", 200, doubloonReward: 400),
        };

        public static IReadOnlyList<VoyageRoadNode> AllAscending => NodesAscending;

        public static bool TryGet(string nodeId, out VoyageRoadNode node)
        {
            node = NodesAscending.FirstOrDefault(n => n.Id == nodeId);
            return node != null;
        }

        /// <summary>Nodes reached but not yet claimed, ascending.</summary>
        public static IEnumerable<VoyageRoadNode> GetClaimableNodes(int accountTrophyTotal, IReadOnlyCollection<string> claimedNodeIds)
        {
            return NodesAscending.Where(n => n.TrophyThreshold <= accountTrophyTotal && !claimedNodeIds.Contains(n.Id));
        }
    }
}
