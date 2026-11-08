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

        // Resolved once in Awake; null when the reference is not assigned.
        private InputAction _pause;
        private InputAction _comics;

        // Delegates are cached: subscribe and unsubscribe must reference the same instance.
        private Action<InputAction.CallbackContext> _onPausePerformed;
        private Action<InputAction.CallbackContext> _onComicsPerformed;

        private void Awake()
        {
            _pause = Resolve(_pauseAction, nameof(_pauseAction));
            _comics = Resolve(_comicsAction, nameof(_comicsAction));

            _onPausePerformed = _ => PausePressed?.Invoke();
            _onComicsPerformed = _ => ComicsPressed?.Invoke();
        }

        private void OnEnable()
        {
            Enable(_pause, _onPausePerformed);
            Enable(_comics, _onComicsPerformed);
        }

        private void OnDisable()
        {
            Disable(_pause, _onPausePerformed);
            Disable(_comics, _onComicsPerformed);
        }

        // InputActionReference is a ScriptableObject: compared with Unity's null, never with ?.
        private InputAction Resolve(InputActionReference reference, string fieldName)
        {
            if (reference == null || reference.action == null)
            {
                Debug.LogError($"[{nameof(GameInput)}] '{name}': input action reference {fieldName} is not assigned.", this);
                return null;
            }

            return reference.action;
        }

        private static void Enable(InputAction action, Action<InputAction.CallbackContext> onPerformed)
        {
            if (action == null)
                return;

            action.Enable();
            action.performed += onPerformed;
        }

        private static void Disable(InputAction action, Action<InputAction.CallbackContext> onPerformed)
        {
            if (action == null)
                return;

            action.performed -= onPerformed;
            action.Disable();
        }
    }
}
