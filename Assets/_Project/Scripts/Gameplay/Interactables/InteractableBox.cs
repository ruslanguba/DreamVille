using System.Collections.Generic;
using System.Collections;
using UnityEngine;

using DreamVille.Gameplay.Enemies;

namespace DreamVille.Gameplay.Interactables
{
    /// <summary>Box that hides enemies, releases them when interacted with and swallows them again on their return.</summary>
    public class InteractableBox : InteractableBase, IEnemyHideout
    {
        [SerializeField] Animator _animator;
        [SerializeField] private List<Enemy> _enemies;
        [SerializeField] private List<Transform> _nextPointsTransform;

        [SerializeField] private bool _isRandomNextPoin;
        [SerializeField] private float _hideDuration;
        [SerializeField] private float _openDuration;
        [SerializeField] private float _closeDuration;

        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            foreach (var enemy in _enemies)
            {
                enemy.transform.position = transform.position;
                enemy.gameObject.SetActive(false);
            }
            CloseDoor();
        }

        public override void OnInteract()
        {
            StartEnemyRun();
        }

        public void Receive(Enemy enemy)
        {
            StartCoroutine(MoveAndShrink(enemy));
        }


        public void CloseDoor()
        {
            _animator.SetBool("isOpen", false);
            if(_enemies.Count > 0)
                _animator.SetBool("isEnemy", true);
        }

        private void StartEnemyRun()
        {
            _animator.SetTrigger("isOpen");
            for (int i = _enemies.Count -1; i >= 0; i--)
            {
                int rndIndex = Random.Range(0, _nextPointsTransform.Count);
                Enemy enemy = _enemies[i];
                enemy.transform.position = transform.position + transform.forward;
                enemy.gameObject.SetActive(true);
                enemy.transform.localScale = Vector3.one;
                enemy.FleeTo(_nextPointsTransform[rndIndex]);
                _enemies.RemoveAt(i);
            }
        }

        public IEnumerator MoveAndShrink(Enemy enemy)
        {
            Vector3 startPos = enemy.transform.position;
            Vector3 endPos = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            Vector3 startScale = enemy.transform.localScale;
            Vector3 endScale = Vector3.one * 0.01f;
            _animator.SetBool("isOpen", true);
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
