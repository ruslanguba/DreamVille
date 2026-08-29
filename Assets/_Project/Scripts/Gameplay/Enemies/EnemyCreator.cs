using System.Collections.Generic;
using System.Collections;
using UnityEngine;

using DreamVille.Gameplay.Enemies.StateMachine;
using DreamVille.Gameplay.Player;
using DreamVille.Gameplay.Trajectory;

namespace DreamVille.Gameplay.Enemies
{
    public class EnemyCreator : MonoBehaviour
    {
        //[SerializeField] private PhysicsObjectsRegistry _physicsObjectsRegistry;
        [SerializeField] private GameObject _enemyPrefab;
        [SerializeField] private Transform _spawn;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private float _timeCreate = 5;


        private void Start()
        {
            //if (_physicsObjectsRegistry == null)
            //{
            //    _physicsObjectsRegistry = FindAnyObjectByType<PhysicsObjectsRegistry>();
            //}
            if (_playerTransform == null)
            {
                _playerTransform = FindFirstObjectByType<PlayerContext>().transform;
            }
            InvokeRepeating("EnemyCreate", 10, _timeCreate);
        }


        public void EnemyCreate()
        {
            GameObject newEnemy = Instantiate(_enemyPrefab, _spawn.position, _spawn.rotation);
            newEnemy.GetComponent<Enemy>().SetPlayerTransform(_playerTransform);
            //if (newEnemy.TryGetComponent(out Rigidbody rb))
            //{
            //    _physicsObjectsRegistry.RegisterNewRigitbody(rb);
            //}
        }
    }
}
