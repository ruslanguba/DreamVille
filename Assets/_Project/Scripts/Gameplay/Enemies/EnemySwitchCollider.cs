using UnityEngine;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>Swaps an enemy's upright collider for its ragdoll collider when it dies.</summary>
    [RequireComponent(typeof(EnemyHealth))]
    public sealed class EnemySwitchCollider : MonoBehaviour
    {
        [SerializeField] private Collider _aliveCollider;
        [SerializeField] private Collider _deadCollider;

        private EnemyHealth _health;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();

            if (_aliveCollider == null || _deadCollider == null)
            {
                Debug.LogError($"[{nameof(EnemySwitchCollider)}] Both the alive and the dead collider must be assigned.", this);
                enabled = false;
                return;
            }

            _aliveCollider.enabled = true;
            _deadCollider.enabled = false;
        }

        private void OnEnable() => _health.OnDeath += OnEnemyDeath;

        private void OnDisable() => _health.OnDeath -= OnEnemyDeath;

        private void OnEnemyDeath(EnemyHealth enemy, int ricochetCount)
        {
            _aliveCollider.enabled = false;
            _deadCollider.enabled = true;
        }
    }
}
