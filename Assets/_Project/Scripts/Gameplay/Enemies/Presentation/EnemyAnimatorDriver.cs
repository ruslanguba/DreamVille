using System.Collections.Generic;
using UnityEngine;

namespace DreamVille.Gameplay.Enemies.Presentation
{
    /// <summary>
    /// Drives an enemy's <see cref="Animator"/> through <see cref="AnimParams"/>. Enemy controllers
    /// declare different parameter sets, so every call no-ops on a parameter or state the controller
    /// does not declare instead of logging a Unity warning per call.
    /// </summary>
    public sealed class EnemyAnimatorDriver : IEnemyAnimator
    {
        private const int BaseLayer = 0;

        private readonly Animator _animator;
        private readonly HashSet<int> _parameterHashes = new HashSet<int>();

        public EnemyAnimatorDriver(Animator animator)
        {
            _animator = animator;

            if (_animator == null)
                return;

            foreach (var parameter in _animator.parameters)
                _parameterHashes.Add(parameter.nameHash);
        }

        public void SetMoving(bool isMoving) => SetBool(AnimParams.IsMoving, isMoving);

        public void TriggerDeath() => SetTrigger(AnimParams.Dead);

        public void PlayLocomotion(float normalizedOffset)
        {
            if (_animator != null && _animator.HasState(BaseLayer, AnimParams.WalkState))
                _animator.Play(AnimParams.WalkState, BaseLayer, normalizedOffset);
        }

        private void SetBool(int hash, bool value)
        {
            if (_animator != null && _parameterHashes.Contains(hash))
                _animator.SetBool(hash, value);
        }

        private void SetTrigger(int hash)
        {
            if (_animator != null && _parameterHashes.Contains(hash))
                _animator.SetTrigger(hash);
        }
    }
}
