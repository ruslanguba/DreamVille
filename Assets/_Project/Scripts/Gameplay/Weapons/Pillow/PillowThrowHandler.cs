using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

using DreamVille.Core.Bootstrap;
using DreamVille.Core.Input;
using DreamVille.Gameplay.Trajectory;

namespace DreamVille.Gameplay.Weapons.Pillow
{
    public class PillowThrowHandler : MonoBehaviour
    {
        public event Action OnThrow;
        public event Action OnRecall;
        public event Action<int> OnPillowAdded;

        [Header("References")]
        [SerializeField] private Transform _spawnPoint; // точка броска
        [SerializeField] private Transform _character;
        [SerializeField] private List<GameObject> _pillows; // заранее созданные подушки
        //[SerializeField] private PillowUiBinder _uiBinder;
        //[SerializeField] private DynamicObjectsRegistry _dynamicObjectsRegistry;
        private Stack<GameObject> _thrownPillows = new Stack<GameObject>();

        [Header("Settings")]
        [SerializeField] private float _throwForce = 10f;
        [SerializeField] private float _recallSpeed = 12f;

        private IPlayerInput _input;

        private int _currentIndex = -1; // индекс текущей летящей подушки
        private Rigidbody _currentRb;

        public void Initialize()
        {
            _input = GetComponent<IPlayerInput>();
            foreach (var pillow in _pillows)
            {
                pillow.SetActive(false);
                var rb = pillow.GetComponent<Rigidbody>();
                pillow.transform.parent = null;
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
            _input.Recall += HandleThrowOrStop;
            _input.Throw += HandleRecall;

            OnPillowAdded?.Invoke(_pillows.Count);
        }

        //private void Awake()
        //{
        //    _input = GetComponent<IPlayerInput>();
        //    foreach (var pillow in _pillows)
        //    {
        //        pillow.SetActive(false);
        //        var rb = pillow.GetComponent<Rigidbody>();
        //        pillow.transform.parent = null;
        //        if (rb != null)
        //        {
        //            rb.isKinematic = true;
        //            rb.linearVelocity = Vector3.zero;
        //            rb.angularVelocity = Vector3.zero;
        //        }
        //    }
        //}

        //private void OnEnable()
        //{
        //    _input.Recall += HandleThrowOrStop;
        //    _input.Throw += HandleRecall;
        //}

        private void OnDisable()
        {
            // Initialize() вызывает GameplaySceneEntryPoint. В сценах без него подписки не создавались.
            if (_input == null)
                return;

            _input.Recall -= HandleThrowOrStop;
            _input.Throw -= HandleRecall;
            //_uiBinder.Unbind();
        }

        //private void Start()
        //{
        //    //_uiBinder.Bind();
        //    OnPillowAdded?.Invoke(_pillows.Count);
        //}

        private void HandleThrowOrStop()
        {
            // если сейчас летит подушка, останавливаем её
            if (_currentRb != null)
            {
                StopCurrentPillow();
                return;
            }

            // иначе ищем следующую доступную подушку
            GameObject nextPillow = GetNextPillow();
            if (nextPillow != null)
            {
                ThrowPillow(nextPillow);
            }
        }

        private GameObject GetNextPillow()
        {
            for (int i = 0; i < _pillows.Count; i++)
            {
                int index = (i + _currentIndex + 1) % _pillows.Count;
                if (!_pillows[index].activeInHierarchy)
                {
                    _currentIndex = index;
                    return _pillows[index];
                }
            }
            return null; // все подушки в полёте
        }

        private void ThrowPillow(GameObject pillow)
        {
            pillow.transform.position = _spawnPoint.position;
            pillow.transform.rotation = _character.rotation;
            pillow.SetActive(true);
            _currentRb = pillow.GetComponent<Rigidbody>();
            _currentRb.isKinematic = false;
            _currentRb.linearVelocity = Vector3.zero;
            _currentRb.angularVelocity = Vector3.zero;
            _currentRb.AddForce(_character.forward * _throwForce, ForceMode.Impulse);
            _currentRb.detectCollisions = true;
            _thrownPillows.Push(pillow); // кладём подушку в стек
            OnThrow?.Invoke();
        }

        private void StopCurrentPillow()
        {
            if (_currentRb == null) return;

            _currentRb.linearVelocity = Vector3.zero;
            _currentRb.angularVelocity = Vector3.zero;
            _currentRb.isKinematic = true;

            _currentRb = null;
        }

        private void HandleRecall()
        {
            if (_thrownPillows.Count == 0) return;

            GameObject pillowToRecall = _thrownPillows.Pop();
            Rigidbody rb = pillowToRecall.GetComponent<Rigidbody>();

            StartCoroutine(RecallPillow(rb));
            _currentRb = null;
        }

        private IEnumerator RecallPillow(Rigidbody pillowRb)
        {
            pillowRb.isKinematic = true;
            pillowRb.detectCollisions = false;
            float distance = Vector3.Distance(pillowRb.position, _spawnPoint.position);

            if (distance > 10f)
            {
                // Получаем направление от подушки к игроку
                Vector3 dir = (_spawnPoint.position - pillowRb.position).normalized;

                // Новая позиция на расстоянии 10 м от игрока (в сторону подушки)
                pillowRb.position = _spawnPoint.position - dir * 10f;
            }

            while (Vector3.Distance(pillowRb.position, _spawnPoint.position) > 0.1f)
            {
                pillowRb.position = Vector3.MoveTowards(pillowRb.position, _spawnPoint.position, _recallSpeed * Time.deltaTime);
                yield return null;
            }

            pillowRb.position = _spawnPoint.position;
            pillowRb.gameObject.SetActive(false);
            OnRecall?.Invoke();
        }
    }

}
