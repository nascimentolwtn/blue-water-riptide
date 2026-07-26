using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BlueWaterRiptide.Core.Progression
{
    /// <summary>
    /// Loads/saves the local <see cref="SaveProfile"/> under Application.persistentDataPath
    /// (Plan 05 §6). One profile per device — multi-profile is explicitly out of scope for v1.
    /// </summary>
    public sealed class SaveService
    {
        public const int CurrentSchemaVersion = 2;
        public const string StarterSailorId = "sailor.ace";

        const string SaveFileName = "profile.json";

        readonly string _savePath;

        public SaveService() : this(Path.Combine(Application.persistentDataPath, SaveFileName)) { }

        /// <summary>Test/tooling entry point — pass an explicit path instead of persistentDataPath.</summary>
        public SaveService(string savePath)
        {
            _savePath = savePath;
        }

        public SaveProfile Load()
        {
            if (!File.Exists(_savePath)) return CreateDefaultProfile();

            try
            {
                string json = File.ReadAllText(_savePath);
                var profile = JsonUtility.FromJson<SaveProfile>(json);
                if (profile == null) return CreateDefaultProfile();

                Migrate(profile);
                return profile;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveService: failed to load '{_savePath}', starting a fresh profile: {e.Message}");
                return CreateDefaultProfile();
            }
        }

        /// <summary>Writes via a temp file + swap so a crash mid-write can't corrupt the previous save.</summary>
        public void Save(SaveProfile profile)
        {
            string json = JsonUtility.ToJson(profile, prettyPrint: true);
            string tmpPath = _savePath + ".tmp";

            File.WriteAllText(tmpPath, json);
            if (File.Exists(_savePath)) File.Delete(_savePath);
            File.Move(tmpPath, _savePath);
        }

        static SaveProfile CreateDefaultProfile()
        {
            return new SaveProfile
            {
                schemaVersion = CurrentSchemaVersion,
                profileId = Guid.NewGuid().ToString(),
                displayName = "Player",
                createdAtUtc = DateTime.UtcNow.ToString("o"),
                doubloons = 0,
                sailors = new List<SailorProgressionRecord>
                {
                    new SailorProgressionRecord { sailorId = StarterSailorId }
                }
            };
        }

        /// <summary>No prior schema version has ever shipped, so there is nothing to migrate yet —
        /// this just stamps the current version and is the seam future field/shape changes hook into.</summary>
        static void Migrate(SaveProfile profile)
        {
            if (profile.schemaVersion >= CurrentSchemaVersion) return;
            profile.schemaVersion = CurrentSchemaVersion;
        }
    }
}
