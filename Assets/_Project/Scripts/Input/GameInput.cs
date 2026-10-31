using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DreamVille.Input
{
    /// <summary>
    /// Menu keys of a gameplay scene, read from the input actions assigned in the Inspector.
    /// Lives on the <c>GameManager</c> prefab; the actions are enabled only while this component is enabled.
    /// </summary>
    public sealed class GameInput : MonoBehaviour, IMenuInput
    {
        [SerializeField] private InputActionReference _pauseAction;
        [SerializeField] private InputActionReference _comicsAction;

        public event Action PausePressed;
        public event Action ComicsPressed;

        // Delegates are cached: subscribe and unsubscribe must reference the same instance.
        private Action<InputAction.CallbackContext> _onPausePerformed;
        private Action<InputAction.CallbackContext> _onComicsPerformed;

        private void Awake()
        {
            _onPausePerformed = _ => PausePressed?.Invoke();
            _onComicsPerformed = _ => ComicsPressed?.Invoke();
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
            _comicsAction?.action?.Enable();
        }

        private void DisableAllActions()
        {
            _pauseAction?.action?.Disable();
            _comicsAction?.action?.Disable();
        }

        private void SubscribeEvents()
        {
            if (_pauseAction?.action != null) _pauseAction.action.performed += _onPausePerformed;
            if (_comicsAction?.action != null) _comicsAction.action.performed += _onComicsPerformed;
        }

        private void UnsubscribeEvents()
        {
            if (_pauseAction?.action != null) _pauseAction.action.performed -= _onPausePerformed;
            if (_comicsAction?.action != null) _comicsAction.action.performed -= _onComicsPerformed;
        }
    }
}
