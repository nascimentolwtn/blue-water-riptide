using UnityEngine;

namespace BlueWaterRiptide.AI
{
    /// <summary>The six behavior states from Plan 01 §3. A plain enum + scorer functions is enough —
    /// no class hierarchy needed since every state is just "read perception, return a float".</summary>
    public enum UtilityStateKind
    {
        Engage,
        Retreat,
        Regroup,
        Flank,
        HoldChoke,
        SuddenDeathPush
    }

    /// <summary>
    /// Scores each utility state against a world snapshot; AIInputDriver picks the highest-scoring
    /// state (with a minimum dwell time applied by the driver itself, not here — scoring is stateless).
    /// </summary>
    public static class UtilityStateScorer
    {
        public static UtilityStateKind SelectBestState(AIPerception perception, AIBehaviorProfile profile, bool playSafe)
        {
            UtilityStateKind best = UtilityStateKind.Engage;
            float bestScore = float.NegativeInfinity;

            void Consider(UtilityStateKind kind, float score)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    best = kind;
                }
            }

            Consider(UtilityStateKind.Engage, ScoreEngage(perception, profile));
            Consider(UtilityStateKind.Retreat, ScoreRetreat(perception, profile));
            Consider(UtilityStateKind.Regroup, ScoreRegroup(perception, profile));
            Consider(UtilityStateKind.Flank, ScoreFlank(perception, profile));
            Consider(UtilityStateKind.HoldChoke, ScoreHoldChoke(perception, profile, playSafe));
            Consider(UtilityStateKind.SuddenDeathPush, ScoreSuddenDeathPush(perception));

            return best;
        }

        public static float ScoreEngage(AIPerception perception, AIBehaviorProfile profile)
        {
            var nearest = perception.NearestEnemy();
            if (!nearest.HasValue) return 0f;

            float score = 0.5f;

            var definition = perception.Self.Definition;
            if (definition != null)
            {
                float dist = Vector3.Distance(perception.Self.Position, nearest.Value.Position);
                float preferredRange = profile.PreferredRangeFraction * definition.AttackRange;
                float rangeError = Mathf.Abs(dist - preferredRange) / Mathf.Max(definition.AttackRange, 0.001f);
                score += Mathf.Clamp01(1f - rangeError) * 0.5f;
            }

            if (perception.Self.HpFraction < profile.RetreatHpThreshold) score -= 0.4f;

            if (profile.Archetype == AttackArchetype.Serve && perception.Self.CurrentAmmo < profile.MinAmmoToFire)
            {
                score -= 0.3f;
            }

            return Mathf.Clamp01(score);
        }

        public static float ScoreRetreat(AIPerception perception, AIBehaviorProfile profile)
        {
            float score = 0f;

            if (perception.Self.HpFraction < profile.RetreatHpThreshold) score += 0.7f;
            score += (1f - perception.Self.HpFraction) * 0.3f;

            if (profile.Archetype == AttackArchetype.Serve && perception.Self.CurrentAmmo <= 0)
            {
                score += 0.5f;
            }

            return Mathf.Clamp01(score);
        }

        public static float ScoreRegroup(AIPerception perception, AIBehaviorProfile profile)
        {
            float nearestAllyDist = float.MaxValue;
            for (int i = 0; i < perception.Allies.Count; i++)
            {
                var ally = perception.Allies[i];
                if (ally.IsKnockedOut) continue;
                float dist = Vector3.Distance(perception.Self.Position, ally.Position);
                if (dist < nearestAllyDist) nearestAllyDist = dist;
            }

            if (nearestAllyDist == float.MaxValue) return 0f; // no living allies left, regrouping is moot

            float score = 0.2f;

            // "Far from allies" is arena-relative — use the larger arena half-extent as the yardstick.
            float isolationYardstick = Mathf.Max(perception.ArenaHalfExtents.x, perception.ArenaHalfExtents.y);
            if (nearestAllyDist > isolationYardstick * 0.5f) score += 0.3f;

            if (perception.Self.HpFraction < 0.6f && perception.Self.HpFraction >= profile.RetreatHpThreshold)
            {
                score += 0.3f;
            }

            return Mathf.Clamp01(score);
        }

        public static float ScoreFlank(AIPerception perception, AIBehaviorProfile profile)
        {
            var nearestEnemy = perception.NearestEnemy();
            if (!nearestEnemy.HasValue) return 0f;

            int livingEnemies = 0;
            for (int i = 0; i < perception.Enemies.Count; i++)
            {
                if (!perception.Enemies[i].IsKnockedOut) livingEnemies++;
            }
            if (livingEnemies < 2) return 0f;

            float score = 0.15f;

            // Flanking makes sense when a teammate is already closer to the focus target — let them hold
            // attention while this Sailor works the angle instead of everyone stacking the same approach.
            float selfDist = Vector3.Distance(perception.Self.Position, nearestEnemy.Value.Position);
            for (int i = 0; i < perception.Allies.Count; i++)
            {
                var ally = perception.Allies[i];
                if (ally.IsKnockedOut) continue;
                float allyDist = Vector3.Distance(ally.Position, nearestEnemy.Value.Position);
                if (allyDist < selfDist)
                {
                    score += 0.35f;
                    break;
                }
            }

            if (profile.Archetype == AttackArchetype.Serve) score += 0.1f;

            return Mathf.Clamp01(score);
        }

        public static float ScoreHoldChoke(AIPerception perception, AIBehaviorProfile profile, bool playSafe)
        {
            float score = 0f;

            if (playSafe) score += 0.6f;
            if (profile.Archetype == AttackArchetype.Spike) score += 0.15f; // body-blocking is Spike-flavored

            return Mathf.Clamp01(score);
        }

        public static float ScoreSuddenDeathPush(AIPerception perception)
        {
            return perception.IsSuddenDeath ? 0.9f : 0f;
        }
    }
}
