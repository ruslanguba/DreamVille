using System;
using UnityEngine.InputSystem;
using UnityEngine;

namespace DreamVille.Input
{
    /// <summary>IPlayerInput over Input System action references; an action left unassigned in the Inspector reads as idle.</summary>
    public sealed class InputReader : MonoBehaviour, IPlayerInput
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _jumpAction;
        [SerializeField] private InputActionReference _fireAction;
        [SerializeField] private InputActionReference _secondaryFireAction;
        [SerializeField] private InputActionReference _stopAction;
        [SerializeField] private InputActionReference _throwAction;
        [SerializeField] private InputActionReference _scrollAction;

        // --- IPlayerInput ---
        public Vector2 Move => _move?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool IsAimHeld => _secondary?.IsPressed() ?? false;

        public event Action JumpPressed;
        public event Action FirePressed;
        public event Action StopPressed;
        public event Action ThrowPressed;
        public event Action<float> Scrolled;

        // Resolved actions: null when the reference is not assigned in the inspector.
        private InputAction _move;
        private InputAction _jump;
        private InputAction _fire;
        private InputAction _secondary;
        private InputAction _stop;
        private InputAction _throw;
        private InputAction _scroll;

        // Delegates are cached: subscribe and unsubscribe must reference the same instance.
        private Action<InputAction.CallbackContext> _onJump;
        private Action<InputAction.CallbackContext> _onFirePerformed;
        private Action<InputAction.CallbackContext> _onStop;
        private Action<InputAction.CallbackContext> _onThrow;
        private Action<InputAction.CallbackContext> _onScroll;

        private void Awake()
        {
            _move = Resolve(_moveAction, nameof(_moveAction));
            _jump = Resolve(_jumpAction, nameof(_jumpAction));
            _fire = Resolve(_fireAction, nameof(_fireAction));
            _secondary = Resolve(_secondaryFireAction, nameof(_secondaryFireAction));
            _stop = Resolve(_stopAction, nameof(_stopAction));
            _throw = Resolve(_throwAction, nameof(_throwAction));
            _scroll = Resolve(_scrollAction, nameof(_scrollAction));

            _onJump = _ => JumpPressed?.Invoke();
            _onFirePerformed = _ => FirePressed?.Invoke();
            _onStop = _ => StopPressed?.Invoke();
            _onThrow = _ => ThrowPressed?.Invoke();
            _onScroll = ctx => Scrolled?.Invoke(ctx.ReadValue<Vector2>().y);
        }

        private InputAction Resolve(InputActionReference reference, string fieldName)
        {
            if (reference == null || reference.action == null)
            {
                Debug.LogWarning($"[{nameof(InputReader)}] '{name}': input action reference {fieldName} is not assigned.", this);
                return null;
            }

            return reference.action;
        }

        private void OnEnable()
        {
            EnableActions();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            DisableActions();
        }

        private void EnableActions()
        {
            _move?.Enable();
            _jump?.Enable();
            _fire?.Enable();
            _secondary?.Enable();
            _stop?.Enable();
            _throw?.Enable();
            _scroll?.Enable();
        }

        private void DisableActions()
        {
            _move?.Disable();
            _jump?.Disable();
            _fire?.Disable();
            _secondary?.Disable();
            _stop?.Disable();
            _throw?.Disable();
            _scroll?.Disable();
        }

        private void Subscribe()
        {
            if (_jump != null) _jump.performed += _onJump;
            if (_fire != null) _fire.performed += _onFirePerformed;
            if (_stop != null) _stop.performed += _onStop;
            if (_throw != null) _throw.performed += _onThrow;
            if (_scroll != null) _scroll.performed += _onScroll;
        }

        private void Unsubscribe()
        {
            if (_jump != null) _jump.performed -= _onJump;
            if (_fire != null) _fire.performed -= _onFirePerformed;
            if (_stop != null) _stop.performed -= _onStop;
            if (_throw != null) _throw.performed -= _onThrow;
            if (_scroll != null) _scroll.performed -= _onScroll;
        }
    }
}
