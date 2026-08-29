using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, IPlayerInput
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _lookAction;
    [SerializeField] private InputActionReference _jumpAction;
    [SerializeField] private InputActionReference _fireAction;
    [SerializeField] private InputActionReference _secondaryFire;
    [SerializeField] private InputActionReference _recallAction;
    [SerializeField] private InputActionReference _throwAction;
    [SerializeField] private InputActionReference _interactAction;
    [SerializeField] private InputActionReference _scrollAction;

    // --- IPlayerInput ---
    public Vector2 Move => _move?.ReadValue<Vector2>() ?? Vector2.zero;
    public Vector2 Look => _look?.ReadValue<Vector2>() ?? Vector2.zero;
    public bool IsFireHeld => _fire?.IsPressed() ?? false;
    public bool IsAimHeld => _secondary?.IsPressed() ?? false;

    public event Action Jump;
    public event Action FirePressed;
    public event Action FireReleased;
    public event Action SecondaryFirePressed;
    public event Action SecondaryFireReleased;
    public event Action Recall;
    public event Action Throw;
    public event Action Interact;
    public event Action<float> Scroll;

    // Разрешённые действия: null, если ссылка не назначена в инспекторе.
    private InputAction _move;
    private InputAction _look;
    private InputAction _jump;
    private InputAction _fire;
    private InputAction _secondary;
    private InputAction _recall;
    private InputAction _throw;
    private InputAction _interact;
    private InputAction _scroll;

    // Делегаты кэшируются: подписка и отписка должны ссылаться на один и тот же экземпляр.
    private Action<InputAction.CallbackContext> _onJump;
    private Action<InputAction.CallbackContext> _onFirePerformed;
    private Action<InputAction.CallbackContext> _onFireCanceled;
    private Action<InputAction.CallbackContext> _onSecondaryFirePerformed;
    private Action<InputAction.CallbackContext> _onSecondaryFireCanceled;
    private Action<InputAction.CallbackContext> _onRecall;
    private Action<InputAction.CallbackContext> _onThrow;
    private Action<InputAction.CallbackContext> _onInteract;
    private Action<InputAction.CallbackContext> _onScroll;

    private void Awake()
    {
        _move = Resolve(_moveAction, nameof(_moveAction));
        _look = Resolve(_lookAction, nameof(_lookAction));
        _jump = Resolve(_jumpAction, nameof(_jumpAction));
        _fire = Resolve(_fireAction, nameof(_fireAction));
        _secondary = Resolve(_secondaryFire, nameof(_secondaryFire));
        _recall = Resolve(_recallAction, nameof(_recallAction));
        _throw = Resolve(_throwAction, nameof(_throwAction));
        _interact = Resolve(_interactAction, nameof(_interactAction));
        _scroll = Resolve(_scrollAction, nameof(_scrollAction));

        _onJump = _ => Jump?.Invoke();
        _onFirePerformed = _ => FirePressed?.Invoke();
        _onFireCanceled = _ => FireReleased?.Invoke();
        _onSecondaryFirePerformed = _ => SecondaryFirePressed?.Invoke();
        _onSecondaryFireCanceled = _ => SecondaryFireReleased?.Invoke();
        _onRecall = _ => Recall?.Invoke();
        _onThrow = _ => Throw?.Invoke();
        _onInteract = _ => Interact?.Invoke();
        _onScroll = ctx => Scroll?.Invoke(ctx.ReadValue<Vector2>().y);
    }

    private InputAction Resolve(InputActionReference reference, string fieldName)
    {
        if (reference == null || reference.action == null)
        {
            Debug.LogWarning($"{nameof(InputReader)} на '{name}': не назначена ссылка {fieldName}.", this);
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
        _look?.Enable();
        _jump?.Enable();
        _fire?.Enable();
        _secondary?.Enable();
        _recall?.Enable();
        _throw?.Enable();
        _interact?.Enable();
        _scroll?.Enable();
    }

    private void DisableActions()
    {
        _move?.Disable();
        _look?.Disable();
        _jump?.Disable();
        _fire?.Disable();
        _secondary?.Disable();
        _recall?.Disable();
        _throw?.Disable();
        _interact?.Disable();
        _scroll?.Disable();
    }

    private void Subscribe()
    {
        if (_jump != null) _jump.performed += _onJump;

        if (_fire != null)
        {
            _fire.performed += _onFirePerformed;
            _fire.canceled += _onFireCanceled;
        }

        if (_secondary != null)
        {
            _secondary.performed += _onSecondaryFirePerformed;
            _secondary.canceled += _onSecondaryFireCanceled;
        }

        if (_recall != null) _recall.performed += _onRecall;
        if (_throw != null) _throw.performed += _onThrow;
        if (_interact != null) _interact.performed += _onInteract;
        if (_scroll != null) _scroll.performed += _onScroll;
    }

    private void Unsubscribe()
    {
        if (_jump != null) _jump.performed -= _onJump;

        if (_fire != null)
        {
            _fire.performed -= _onFirePerformed;
            _fire.canceled -= _onFireCanceled;
        }

        if (_secondary != null)
        {
            _secondary.performed -= _onSecondaryFirePerformed;
            _secondary.canceled -= _onSecondaryFireCanceled;
        }

        if (_recall != null) _recall.performed -= _onRecall;
        if (_throw != null) _throw.performed -= _onThrow;
        if (_interact != null) _interact.performed -= _onInteract;
        if (_scroll != null) _scroll.performed -= _onScroll;
    }
}
