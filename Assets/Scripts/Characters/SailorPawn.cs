using BlueWaterRiptide.Core;
using BlueWaterRiptide.Gameplay.Combat;
using UnityEngine;

namespace BlueWaterRiptide.Characters
{
    /// <summary>
    /// The pawn both the human player and the AI opponent use identically — it only ever
    /// talks to an IInputDriver, never knows if it's touch/keyboard/AI.
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

        public Vector2 ArenaHalfExtents = new Vector2(1000f, 1000f);

        const float FireCooldownDuration = 0.35f;

        IInputDriver _driver;
        MatchController _matchController;
        float _reloadTimer;
        float _fireCooldownTimer;

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
        }

        public void ApplyDamage(float amount, ParticipantId source)
        {
            if (IsKnockedOut) return;

            CurrentHP -= amount;

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

        void Update()
        {
            if (IsKnockedOut) return;
            if (_driver == null || Definition == null) return;

            var cmd = _driver.Sample(Time.timeAsDouble);

            // Movement
            Vector3 moveDir = new Vector3(cmd.Move.x, 0f, cmd.Move.y);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            Vector3 pos = transform.position + moveDir * Definition.MoveSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -ArenaHalfExtents.x, ArenaHalfExtents.x);
            pos.z = Mathf.Clamp(pos.z, -ArenaHalfExtents.y, ArenaHalfExtents.y);
            transform.position = pos;

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

            // Fire cooldown
            if (_fireCooldownTimer > 0f) _fireCooldownTimer -= Time.deltaTime;

            if (cmd.FireHeld && CurrentAmmo > 0 && _fireCooldownTimer <= 0f)
            {
                Fire();
                CurrentAmmo--;
                _fireCooldownTimer = FireCooldownDuration;
            }
        }

        void Fire()
        {
            GameObject projectileObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObj.name = "Projectile";
            projectileObj.transform.position = transform.position + transform.forward * 1.2f + Vector3.up * 0.5f;
            projectileObj.transform.localScale = Vector3.one * 0.4f;

            var renderer = projectileObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = CreateUrpMaterial(Team == Team.A ? Color.cyan : Color.magenta);
            }

            var sphereCollider = projectileObj.GetComponent<SphereCollider>();
            if (sphereCollider != null) sphereCollider.isTrigger = true;

            var rigidbody = projectileObj.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            var projectile = projectileObj.AddComponent<Projectile>();
            projectile.Initialize(Id, Team, transform.forward, Definition.ProjectileSpeed, Definition.AttackDamage, Definition.AttackRange, _matchController);
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
