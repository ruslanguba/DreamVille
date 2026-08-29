using UnityEngine;
using Zenject;

using DreamVille.Gameplay.Player;

namespace DreamVille.Gameplay.Enemies
{
    public class HenMove : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private float _timeToReachSpeed = 2;
        [SerializeField] private float _speed = 5;
        [SerializeField] private float _turnSpeed = 2;

        private PlayerContext _player;

        [Inject]
        public void Construct(PlayerContext player)
        {
            _player = player;
        }

        private void FixedUpdate()
        {
            if (_player == null) return;

            Vector3 toPlayer = (_player.BodyTransform.position - transform.position).normalized;
            Vector3 force = _rb.mass * (toPlayer * _speed - _rb.linearVelocity) / _timeToReachSpeed;
            Quaternion targetRotation = Quaternion.LookRotation(toPlayer, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _turnSpeed);
            _rb.AddForce(force);
        }
    }
}
