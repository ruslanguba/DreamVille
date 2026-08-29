using System;
using UnityEngine.InputSystem;
using UnityEngine;

namespace DreamVille.Core.Input
{
    public class GameInput : MonoBehaviour
    {
        [SerializeField] private InputActionReference _pauseAction;
        [SerializeField] private InputActionReference _tubAction;

        public event Action OnPause;
        public event Action OnTub;

        // Делегаты кэшируются: подписка и отписка должны ссылаться на один и тот же экземпляр.
        private Action<InputAction.CallbackContext> _onPausePerformed;
        private Action<InputAction.CallbackContext> _onTubPerformed;

        private void Awake()
        {
            _onPausePerformed = _ => OnPause?.Invoke();
            _onTubPerformed = _ => OnTub?.Invoke();
        }

        private void OnEnable()
        {
            EnableAllActions();
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
            DisableAllActions();
        }

        private void EnableAllActions()
        {
            _pauseAction?.action?.Enable();
            _tubAction?.action?.Enable();
        }

        private void DisableAllActions()
        {
            _pauseAction?.action?.Disable();
            _tubAction?.action?.Disable();
        }

        private void SubscribeEvents()
        {
            if (_pauseAction?.action != null) _pauseAction.action.performed += _onPausePerformed;
            if (_tubAction?.action != null) _tubAction.action.performed += _onTubPerformed;
        }

        private void UnsubscribeEvents()
        {
            if (_pauseAction?.action != null) _pauseAction.action.performed -= _onPausePerformed;
            if (_tubAction?.action != null) _tubAction.action.performed -= _onTubPerformed;
        }
    }
}
