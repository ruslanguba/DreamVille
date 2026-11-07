using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

using DreamVille.Core.Async;
using DreamVille.Gameplay.Interactables;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>
    /// A box or fridge that hides enemies: an interaction opens it and sends every hidden enemy fleeing to a random
    /// point; an enemy that comes back is swallowed again: the door opens, the enemy shrinks into the box, the door
    /// closes. All three phases run in scaled game time, so pause freezes them.
    /// </summary>
    public sealed class EnemyHideout : MonoBehaviour, IInteractionEffect, IEnemyHideout
    {
        private const float HiddenScale = 0.01f;

        [SerializeField] private List<Enemy> _enemies;
        [SerializeField] private List<Transform> _nextPointsTransform;

        [SerializeField, Min(0f), Tooltip("Seconds of game time the enemy takes to shrink into the hideout.")]
        private float _hideDuration;

        [SerializeField, Min(0f), Tooltip("Seconds of game time between the door opening and the shrink.")]
        private float _openDuration;

        [SerializeField, Min(0f), Tooltip("Seconds of game time between the shrink and the door closing.")]
        private float _closeDuration;

        // The enemies hidden right now. Starts as a copy of the authored _enemies, which is never written at runtime.
        private readonly List<Enemy> _hidden = new();

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
                Debug.LogError($"[{nameof(EnemyHideout)}] No Animator on this object or its children.", this);

            if (_enemies == null)
                return;

            foreach (Enemy enemy in _enemies)
                Track(enemy);
        }

        private void Start()
        {
            foreach (Enemy enemy in _hidden)
            {
                enemy.transform.position = transform.position;
                enemy.gameObject.SetActive(false);
            }

            CloseDoor();
        }

        private void OnDestroy()
        {
            foreach (Enemy enemy in _hidden)
                enemy.Destroyed -= OnEnemyDestroyed;

            _hidden.Clear();
        }

        public void Apply(in InteractionContext context)
        {
            StartEnemyRun();
        }

        public void Receive(Enemy enemy)
        {
            if (enemy == null)
                return;

            // One absorption per arriving enemy; concurrent absorptions share the door animator.
            AbsorbAsync(enemy, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private void CloseDoor()
        {
            _animator.SetBool(AnimParams.IsOpen, false);
            if (_hidden.Count > 0)
                _animator.SetBool(AnimParams.IsEnemy, true);
        }

        private void StartEnemyRun()
        {
            _animator.SetBool(AnimParams.IsOpen, true);
            for (int i = _hidden.Count - 1; i >= 0; i--)
            {
                int pointIndex = Random.Range(0, _nextPointsTransform.Count);
                Enemy enemy = _hidden[i];
                enemy.transform.position = transform.position + transform.forward;
                enemy.gameObject.SetActive(true);
                enemy.transform.localScale = Vector3.one;
                enemy.FleeTo(_nextPointsTransform[pointIndex]);
                enemy.Destroyed -= OnEnemyDestroyed;
                _hidden.RemoveAt(i);
            }
        }

        private async UniTaskVoid AbsorbAsync(Enemy enemy, CancellationToken hideoutToken)
        {
            // The absorption ends when either the hideout or the enemy is destroyed.
            using CancellationTokenSource lifetime =
                CancellationTokenSource.CreateLinkedTokenSource(hideoutToken, enemy.GetCancellationTokenOnDestroy());
            CancellationToken token = lifetime.Token;

            try
            {
                Transform body = enemy.transform;
                Vector3 endPosition = transform.position + Vector3.up;
                Vector3 endScale = Vector3.one * HiddenScale;

                _animator.SetBool(AnimParams.IsOpen, true);
                await GameTime.DelayAsync(_openDuration, token);

                // Read after the door has opened, where the enemy actually stands, so the shrink starts without a jump.
                Vector3 startPosition = body.position;
                Vector3 startScale = body.localScale;

                await Tween.RunAsync(_hideDuration, progress =>
                {
                    body.position = Vector3.Lerp(startPosition, endPosition, progress);
                    body.localScale = Vector3.Lerp(startScale, endScale, progress);
                }, token);

                await GameTime.DelayAsync(_closeDuration, token);

                Stow(enemy, endPosition, endScale);
            }
            catch (OperationCanceledException)
            {
                // Hideout or enemy destroyed mid-way: nothing is stowed and the door stays as it is.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void Stow(Enemy enemy, Vector3 position, Vector3 scale)
        {
            Track(enemy);
            CloseDoor();

            Transform body = enemy.transform;
            body.position = position;
            body.localScale = scale;
            enemy.gameObject.SetActive(false);
        }

        private void Track(Enemy enemy)
        {
            if (_hidden.Contains(enemy))
                return;

            _hidden.Add(enemy);
            enemy.Destroyed += OnEnemyDestroyed;
        }

        private void OnEnemyDestroyed(Enemy enemy)
        {
            _hidden.Remove(enemy);
        }
    }
}
