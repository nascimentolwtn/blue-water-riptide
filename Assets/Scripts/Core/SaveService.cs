using System;
using System.IO;
using UnityEngine;

namespace BlueWaterRiptide.Core
{
    /// <summary>
    /// Local JSON save/load for the player's profile (Plan 05 schema, M1-scoped basics only per
    /// Plan 08 step 7). Writes to a temp file and swaps it in so a mid-write kill (e.g. Android
    /// backgrounding the app) can't corrupt the real save file.
    /// </summary>
    public static class SaveService
    {
        const string SaveFileName = "savegame.json";

        static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public static ProfileData CreateDefault()
        {
            return new ProfileData
            {
                schemaVersion = 1,
                profileId = Guid.NewGuid().ToString(),
                displayName = "Sailor",
                createdAtUtc = DateTime.UtcNow.ToString("o"),
                doubloons = 0,
            };
        }

        /// <summary>Loads the save file, creating (and persisting) a fresh default profile on first run or on a parse failure.</summary>
        public static ProfileData Load()
        {
            try
            {
                if (!File.Exists(SavePath))
                {
                    var fresh = CreateDefault();
                    Save(fresh);
                    return fresh;
                }

                string json = File.ReadAllText(SavePath);
                var loaded = JsonUtility.FromJson<ProfileData>(json);
                if (loaded == null || string.IsNullOrEmpty(loaded.profileId))
                {
                    Debug.LogWarning("SaveService: existing save file failed to parse — creating a fresh default profile.");
                    var fresh = CreateDefault();
                    Save(fresh);
                    return fresh;
                }

                return loaded;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveService: load failed ({e.GetType().Name}: {e.Message}) — falling back to an in-memory default profile.");
                return CreateDefault();
            }
        }

        /// <summary>Atomic-ish write: full write to a temp file first, then swap it in, so the previous save survives a failed write.</summary>
        public static void Save(ProfileData profile)
        {
            if (profile == null) return;

            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                string json = JsonUtility.ToJson(profile, true);
                string tempPath = SavePath + ".tmp";

                File.WriteAllText(tempPath, json);

                if (File.Exists(SavePath))
                {
                    try
                    {
                        File.Replace(tempPath, SavePath, null);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        // File.Replace isn't guaranteed on every Android filesystem under IL2CPP —
                        // fall back to delete+move; the temp file is already fully written by now.
                        File.Delete(SavePath);
                        File.Move(tempPath, SavePath);
                    }
                }
                else
                {
                    File.Move(tempPath, SavePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveService: save failed ({e.GetType().Name}: {e.Message}).");
            }
        }
    }
}
