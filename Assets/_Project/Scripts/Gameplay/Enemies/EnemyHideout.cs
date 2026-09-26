using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamVille.Gameplay.Interactables;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>
    /// A box or fridge that hides enemies: an interaction opens it and sends every hidden enemy fleeing to a random
    /// point; an enemy that comes back is swallowed again.
    /// </summary>
    public sealed class EnemyHideout : MonoBehaviour, IInteractionEffect, IEnemyHideout
    {
        // Frozen names: prefab-instance overrides in Level_03, Level_04 and Max/* address these fields by property path.
        [SerializeField] private List<Enemy> _enemies;
        [SerializeField] private List<Transform> _nextPointsTransform;

        [SerializeField] private float _hideDuration;
        [SerializeField] private float _openDuration;
        [SerializeField] private float _closeDuration;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
                Debug.LogError($"[{nameof(EnemyHideout)}] No Animator on this object or its children.", this);
        }

        private void Start()
        {
            foreach (Enemy enemy in _enemies)
            {
                enemy.transform.position = transform.position;
                enemy.gameObject.SetActive(false);
            }

            CloseDoor();
        }

        public void Apply(in InteractionContext context)
        {
            StartEnemyRun();
        }

        public void Receive(Enemy enemy)
        {
            StartCoroutine(MoveAndShrink(enemy));
        }

        private void CloseDoor()
        {
            _animator.SetBool(AnimParams.IsOpen, false);
            if (_enemies.Count > 0)
                _animator.SetBool(AnimParams.IsEnemy, true);
        }

        private void StartEnemyRun()
        {
            // The closet and fridge controllers declare isOpen as a Bool; the trigger call is kept as it always was.
            _animator.SetTrigger(AnimParams.IsOpen);
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                int pointIndex = Random.Range(0, _nextPointsTransform.Count);
                Enemy enemy = _enemies[i];
                enemy.transform.position = transform.position + transform.forward;
                enemy.gameObject.SetActive(true);
                enemy.transform.localScale = Vector3.one;
                enemy.FleeTo(_nextPointsTransform[pointIndex]);
                _enemies.RemoveAt(i);
            }
        }

        private IEnumerator MoveAndShrink(Enemy enemy)
        {
            Vector3 startPos = enemy.transform.position;
            Vector3 endPos = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            Vector3 startScale = enemy.transform.localScale;
            Vector3 endScale = Vector3.one * 0.01f;
            _animator.SetBool(AnimParams.IsOpen, true);
            yield return new WaitForSeconds(_openDuration);
            float t = 0f;

            while (enemy != null && t < _hideDuration)
            {
                t += Time.deltaTime;
                float normalized = t / _hideDuration;

                enemy.transform.position = Vector3.Lerp(startPos, endPos, normalized);
                enemy.transform.localScale = Vector3.Lerp(startScale, endScale, normalized);
                yield return null;
            }
            yield return new WaitForSeconds(_closeDuration);

            if (enemy == null)
                yield break;

            if (!_enemies.Contains(enemy))
                _enemies.Add(enemy);

            enemy.Destroyed -= RemoveEnemy;
            enemy.Destroyed += RemoveEnemy;
            CloseDoor();

            enemy.transform.position = endPos;
            enemy.transform.localScale = endScale;
            enemy.gameObject.SetActive(false);
        }

        private void RemoveEnemy(Enemy enemy)
        {
            _enemies.Remove(enemy);
        }
    }
}
