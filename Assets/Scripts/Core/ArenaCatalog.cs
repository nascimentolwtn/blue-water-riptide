using System.Collections.Generic;

namespace BlueWaterRiptide.Core
{
    /// <summary>Id-keyed arena metadata. Geometry (the actual prefab/tile-spawn data under
    /// Assets/Prefabs/Environment) is Editor/Asset-bound and doesn't exist yet — this catalog
    /// only owns the id -> descriptor mapping so Session/MatchController and mode launchers
    /// never hardcode arena names, per Plan 00 §7.4.</summary>
    public sealed class ArenaDescriptor
    {
        public string Id { get; }
        public string DisplayName { get; }

        public ArenaDescriptor(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }
    }

    /// <summary>Single entry in v1: Tideline Cove (Plan 00 §5).</summary>
    public static class ArenaCatalog
    {
        static readonly Dictionary<string, ArenaDescriptor> ById = new Dictionary<string, ArenaDescriptor>
        {
            ["tideline-cove"] = new ArenaDescriptor("tideline-cove", "Tideline Cove"),
        };

        public static ArenaDescriptor Default => ById["tideline-cove"];

        public static bool TryGet(string id, out ArenaDescriptor descriptor) => ById.TryGetValue(id, out descriptor);
    }
}
