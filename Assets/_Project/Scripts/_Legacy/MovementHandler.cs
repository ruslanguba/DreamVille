using UnityEngine;

using DreamVille.Core.Input;

namespace DreamVille.Legacy
{
    public class MovementHandler : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed;

        private CharacterController _characterController;
        private IPlayerInput _inputHandler;
        private GravityHandler _gravityHandler;

        private Vector3 _moveDirection; 

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _inputHandler = GetComponent<IPlayerInput>();
            _gravityHandler = GetComponent<GravityHandler>();
        }

        void Update()
        {
            GetInputMoveVector();
        }

        private void FixedUpdate()
        {
            MoveCharacter();
        }

        private void GetInputMoveVector()
        {
            _moveDirection = _inputHandler.Move;
        }

        private void MoveCharacter()
        {
           Vector3 velocity =
           new Vector3(_moveDirection.x * _moveSpeed,
                       _gravityHandler.GetYVelocity(),
                       _moveDirection.y * _moveSpeed);

            _characterController.Move(velocity * Time.fixedDeltaTime);
        }
    }
}
