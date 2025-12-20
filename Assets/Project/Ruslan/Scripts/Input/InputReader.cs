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
    //[SerializeField] private InputActionReference _pauseAction;

    // --- IPlayerInput ---
    public Vector2 Move => _moveAction.action.ReadValue<Vector2>();
    public Vector2 Look => _lookAction.action.ReadValue<Vector2>();
    public bool IsFireHeld => _fireAction.action.IsPressed();
    public bool IsAimHeld => _secondaryFire.action.IsPressed();

    public event Action Jump;
    public event Action FirePressed;
    public event Action FireReleased;
    public event Action SecondaryFirePressed;
    public event Action SecondaryFireReleased;
    public event Action Recall;
    public event Action Throw;
    public event Action Interact;
    //public event Action Pause;
    public event Action<float> Scroll;

    // Cached delegates (важно!)
    private Action<InputAction.CallbackContext> _onFirePerformed;
    private Action<InputAction.CallbackContext> _onFireCanceled;
    private Action<InputAction.CallbackContext> _onSecondaryFirePerformed;
    private Action<InputAction.CallbackContext> _onSecondaryFireCanceled;

    private void Awake()
    {
        _onFirePerformed = _ => FirePressed?.Invoke();
        _onFireCanceled = _ => FireReleased?.Invoke();
        _onSecondaryFirePerformed = _ => SecondaryFirePressed?.Invoke();
        _onSecondaryFireCanceled = _ => SecondaryFireReleased?.Invoke();
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
        _moveAction.action?.Enable();
        _lookAction.action?.Enable();
        _jumpAction.action?.Enable();
        _fireAction.action?.Enable();
        _secondaryFire.action?.Enable();
        _recallAction.action?.Enable();
        _throwAction.action?.Enable();
        _interactAction.action?.Enable();
        _scrollAction.action?.Enable();
        //_pauseAction.action?.Enable();
    }

    private void DisableActions()
    {
        _moveAction.action?.Disable();
        _lookAction.action?.Disable();
        _jumpAction.action?.Disable();
        _fireAction.action?.Disable();
        _secondaryFire.action?.Disable();
        _recallAction.action?.Disable();
        _throwAction.action?.Disable();
        _interactAction.action?.Disable();
        _scrollAction.action?.Disable();
        //_pauseAction.action?.Disable();
    }

    private void Subscribe()
    {
        _jumpAction.action.performed += _ => Jump?.Invoke();

        _fireAction.action.performed += _onFirePerformed;
        _fireAction.action.canceled += _onFireCanceled;

        _secondaryFire.action.performed += _onSecondaryFirePerformed;
        _secondaryFire.action.canceled += _onSecondaryFireCanceled;

        _recallAction.action.performed += _ => Recall?.Invoke();
        _throwAction.action.performed += _ => Throw?.Invoke();
        _interactAction.action.performed += _ => Interact?.Invoke();
        //_pauseAction.action.performed += _ => Pause?.Invoke();

        _scrollAction.action.performed += ctx =>
            Scroll?.Invoke(ctx.ReadValue<Vector2>().y);
    }

    private void Unsubscribe()
    {
        _jumpAction.action.performed -= _ => Jump?.Invoke();

        _fireAction.action.performed -= _onFirePerformed;
        _fireAction.action.canceled -= _onFireCanceled;

        _secondaryFire.action.performed -= _onSecondaryFirePerformed;
        _secondaryFire.action.canceled -= _onSecondaryFireCanceled;

        _recallAction.action.performed -= _ => Recall?.Invoke();
        _throwAction.action.performed -= _ => Throw?.Invoke();
        _interactAction.action.performed -= _ => Interact?.Invoke();
        //_pauseAction.action.performed -= _ => Pause?.Invoke();
    }
}
