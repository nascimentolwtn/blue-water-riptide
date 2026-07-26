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
        string TmpPath => _savePath + ".tmp";

        public SaveService() : this(Path.Combine(Application.persistentDataPath, SaveFileName)) { }

        /// <summary>Test/tooling entry point — pass an explicit path instead of persistentDataPath.</summary>
        public SaveService(string savePath)
        {
            _savePath = savePath;
        }

        public SaveProfile Load()
        {
            if (!File.Exists(_savePath))
            {
                // Save() writes tmp -> delete old -> move tmp into place; a crash between the
                // delete and the move leaves only the tmp file behind. Recover from it rather
                // than silently starting over.
                var recovered = File.Exists(TmpPath) ? TryParse(TmpPath) : null;
                if (recovered != null)
                {
                    Migrate(recovered);
                    return recovered;
                }
                return CreateDefaultProfile();
            }

            var profile = TryParse(_savePath);
            if (profile != null)
            {
                Migrate(profile);
                return profile;
            }

            QuarantineUnreadableFile();
            return CreateDefaultProfile();
        }

        /// <summary>Writes via a temp file + swap so a crash mid-write can't corrupt the previous save.</summary>
        public void Save(SaveProfile profile)
        {
            string json = JsonUtility.ToJson(profile, prettyPrint: true);

            File.WriteAllText(TmpPath, json);
            if (File.Exists(_savePath)) File.Delete(_savePath);
            File.Move(TmpPath, _savePath);
        }

        SaveProfile TryParse(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<SaveProfile>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveService: failed to parse '{path}': {e.Message}");
                return null;
            }
        }

        /// <summary>Renames an unreadable save aside instead of leaving it to be silently
        /// overwritten by the next Save() — preserves it for a future "recover or reset" prompt
        /// (Plan 04's Boot flow) instead of destroying whatever was salvageable.</summary>
        void QuarantineUnreadableFile()
        {
            try
            {
                string quarantinePath = _savePath + ".corrupt";
                if (File.Exists(quarantinePath)) File.Delete(quarantinePath);
                File.Move(_savePath, quarantinePath);
                Debug.LogWarning($"SaveService: quarantined unreadable save as '{quarantinePath}', starting a fresh profile.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveService: failed to quarantine unreadable save '{_savePath}': {e.Message}");
            }
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
