using UnityEngine;

using DreamVille.Gameplay.Enemies;

namespace DreamVille.Gameplay.Enemies.StateMachine
{
    public class EnemySwitchCollider : MonoBehaviour
    {
        [SerializeField] private Collider _aliveCollider;
        [SerializeField] private Collider _deadCollider;
        [SerializeField] private EnemyHealth enemyHealth;

        private void Start()
        {
            _aliveCollider = GetComponent<CapsuleCollider>();
            _deadCollider = GetComponentInChildren<BoxCollider>();
            _aliveCollider.enabled = true;
            _deadCollider.enabled = false;
        }

        private void OnEnable()
        {
            enemyHealth = GetComponent<EnemyHealth>();
            enemyHealth.OnDeath += SwitchCollider;
        }

        private void OnDisable()
        {
            enemyHealth.OnDeath -= SwitchCollider;
        }

        private void SwitchCollider(EnemyHealth enemy, int collisionsCount)
        {
            _aliveCollider.enabled = false;
            _deadCollider.enabled = true;
        }
    }
}
