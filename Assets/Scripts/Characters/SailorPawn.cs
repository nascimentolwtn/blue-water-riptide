using BlueWaterRiptide.Core;
using UnityEngine;

namespace BlueWaterRiptide.Characters
{
    /// <summary>
    /// The pawn both the human player and the AI opponent use identically — it only ever
    /// talks to an IInputDriver and its Definition's IAbilityBehaviors, never knows if it's
    /// touch/keyboard/AI, or what kit it's running. Adding a Sailor is data (SailorDefinitionData
    /// + ability behavior instances), never a SailorPawn change.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SailorPawn : MonoBehaviour
    {
        public ParticipantId Id { get; private set; }
        public Team Team { get; private set; }
        public SailorDefinitionData Definition { get; private set; }
        public float CurrentHP { get; private set; }
        public float HpFraction => Definition == null ? 0f : Mathf.Clamp01(CurrentHP / Definition.MaxHP);
        public int CurrentAmmo { get; private set; }
        public bool IsKnockedOut { get; private set; }
        public float SuperCharge01 { get; private set; }

        /// <summary>Seconds of no-damage grace ResetForRound grants after placing this Sailor at a spawn pad. Set by the host from ArenaDefinition.</summary>
        public float SpawnImmunityDuration = 1.5f;
        public bool IsSpawnImmune => _spawnImmunityTimer > 0f;

        public Vector2 ArenaHalfExtents = new Vector2(1000f, 1000f);

        const float FireCooldownDuration = 0.35f;
        const float KnockbackDecayPerSecond = 6f;

        IInputDriver _driver;
        MatchController _matchController;
        float _reloadTimer;
        float _fireCooldownTimer;
        float _superCharge;

        Vector3 _knockbackVelocity;
        float _damageReductionFraction;
        float _damageReductionTimer;
        float _spawnImmunityTimer;

        public void Initialize(ParticipantId id, Team team, SailorDefinitionData definition, IInputDriver driver, MatchController matchController)
        {
            Id = id;
            Team = team;
            Definition = definition;
            _driver = driver;
            _matchController = matchController;

            CurrentHP = definition.MaxHP;
            CurrentAmmo = definition.MaxAmmo;
            IsKnockedOut = false;
            _reloadTimer = 0f;
            _fireCooldownTimer = 0f;
            _superCharge = 0f;
            SuperCharge01 = 0f;
            _knockbackVelocity = Vector3.zero;
            _damageReductionFraction = 0f;
            _damageReductionTimer = 0f;
        }

        public void ApplyDamage(float amount, ParticipantId source)
        {
            if (IsKnockedOut || IsSpawnImmune) return;

            float mitigated = _damageReductionTimer > 0f ? amount * (1f - _damageReductionFraction) : amount;
            CurrentHP -= mitigated;

            if (CurrentHP <= 0f)
            {
                CurrentHP = 0f;
                IsKnockedOut = true;

                var collider = GetComponent<Collider>();
                if (collider != null) collider.enabled = false;

                var renderer = GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = false;

                _matchController?.ReportKnockout(Id);
            }
        }

        /// <summary>Called by CombatResolver on whoever dealt damage — Super "charges by dealing damage" (00 §2).</summary>
        public void AddSuperCharge(float damageDealt)
        {
            if (Definition == null || Definition.Super == null) return;
            _superCharge = Mathf.Min(_superCharge + damageDealt, Definition.SuperChargeThreshold);
            SuperCharge01 = Definition.SuperChargeThreshold > 0f ? _superCharge / Definition.SuperChargeThreshold : 0f;
        }

        /// <summary>Instant velocity impulse that decays over a fraction of a second; no-op if this Sailor's Trait grants knockback immunity.</summary>
        public void ApplyKnockback(Vector3 direction, float force)
        {
            if (Definition != null && Definition.KnockbackImmune) return;
            _knockbackVelocity = direction.normalized * force;
        }

        /// <summary>Temporary incoming-damage reduction, e.g. Drop Anchor's ally aura.</summary>
        public void ApplyDamageReductionBuff(float fraction, float duration)
        {
            _damageReductionFraction = fraction;
            _damageReductionTimer = duration;
        }

        /// <summary>
        /// Resets this Sailor for a fresh round (MatchController.OnRoundCountdownStart): full HP/ammo,
        /// re-enabled collider/renderer, cleared knockout, Super charge carried over at
        /// superRetainFraction (00 §1's round-reset rule: 0% into round 1, 50% into round 2+), and a
        /// brief spawn-immunity window. The immunity timer is a plain countdown decremented by
        /// Time.deltaTime each Update — a duration relative to when this call happens, not a stored
        /// wall-clock timestamp, so it stays correct regardless of when in the match it's granted.
        /// </summary>
        public void ResetForRound(Vector3 spawnPosition, float superRetainFraction)
        {
            transform.position = spawnPosition;

            if (Definition != null)
            {
                CurrentHP = Definition.MaxHP;
                CurrentAmmo = Definition.MaxAmmo;
            }

            IsKnockedOut = false;
            _reloadTimer = 0f;
            _fireCooldownTimer = 0f;
            _knockbackVelocity = Vector3.zero;
            _damageReductionFraction = 0f;
            _damageReductionTimer = 0f;

            _superCharge *= Mathf.Clamp01(superRetainFraction);
            SuperCharge01 = (Definition != null && Definition.SuperChargeThreshold > 0f)
                ? _superCharge / Definition.SuperChargeThreshold
                : 0f;

            var collider = GetComponent<Collider>();
            if (collider != null) collider.enabled = true;

            var renderer = GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = true;

            _spawnImmunityTimer = SpawnImmunityDuration;
        }

        void Update()
        {
            if (IsKnockedOut) return;

            // Timers that run regardless of driver availability
            if (_damageReductionTimer > 0f) _damageReductionTimer -= Time.deltaTime;
            if (_spawnImmunityTimer > 0f) _spawnImmunityTimer -= Time.deltaTime;

            if (_driver == null || Definition == null) return;

            var cmd = _driver.Sample(Time.timeAsDouble);

            // Movement (input-driven + decaying knockback impulse)
            Vector3 moveDir = new Vector3(cmd.Move.x, 0f, cmd.Move.y);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            Vector3 pos = transform.position + moveDir * Definition.MoveSpeed * Time.deltaTime + _knockbackVelocity * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -ArenaHalfExtents.x, ArenaHalfExtents.x);
            pos.z = Mathf.Clamp(pos.z, -ArenaHalfExtents.y, ArenaHalfExtents.y);
            transform.position = pos;

            _knockbackVelocity = Vector3.MoveTowards(_knockbackVelocity, Vector3.zero, KnockbackDecayPerSecond * _knockbackVelocity.magnitude * Time.deltaTime + 0.01f);

            // Aim
            if (cmd.Aim.sqrMagnitude > 0.0001f)
            {
                Vector3 aimDir = new Vector3(cmd.Aim.x, 0f, cmd.Aim.y).normalized;
                transform.rotation = Quaternion.LookRotation(aimDir, Vector3.up);
            }

            // Ammo regen
            if (CurrentAmmo < Definition.MaxAmmo)
            {
                _reloadTimer += Time.deltaTime;
                if (_reloadTimer >= Definition.ReloadTime)
                {
                    _reloadTimer = 0f;
                    CurrentAmmo++;
                }
            }

            // Basic attack (fire-rate cooldown gates ammo-based firing; behavior itself is data-driven)
            if (_fireCooldownTimer > 0f) _fireCooldownTimer -= Time.deltaTime;

            if (cmd.FireHeld && CurrentAmmo > 0 && _fireCooldownTimer <= 0f && Definition.BasicAttack != null)
            {
                Definition.BasicAttack.Execute(this);
                CurrentAmmo--;
                _fireCooldownTimer = FireCooldownDuration;
            }

            // Super
            if (cmd.SuperPressed && SuperCharge01 >= 1f && Definition.Super != null)
            {
                Definition.Super.Execute(this);
                _superCharge = 0f;
                SuperCharge01 = 0f;
            }
        }

        /// <summary>
        /// GameObject.CreatePrimitive's default material uses the legacy "Standard" shader, which
        /// renders as solid magenta under URP (this project's active pipeline) instead of tinting.
        /// Build a pipeline-compatible material instead of touching the default one.
        /// </summary>
        internal static Material CreateUrpMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { color = color };
            return mat;
        }
    }
}
