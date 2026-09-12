using UnityEngine;

using DreamVille.Core.Input;
using DreamVille.Gameplay.Weapons.Aiming;

namespace DreamVille.Gameplay.Weapons
{
    public class AimingHandler : MonoBehaviour
    {
        [SerializeField] private BounceRayTrajectory _rayTrajectory;
        [SerializeField] private Transform _aimingPivot;
        private bool _isAnimating = false;
        private IPlayerInput _input;

        private void Awake()
        {
            _input = GetComponent<IPlayerInput>();
        }

        // Subscribe in OnEnable (mirrored by OnDisable) so aiming survives disable/enable cycles.
        private void OnEnable()
        {
            if (_input == null)
                return;

            _input.SecondaryFirePressed += ShowTrajectory;
            _input.SecondaryFireReleased += ClearTrajectory;
        }

        private void OnDisable()
        {
            if (_input == null)
                return;

            _input.SecondaryFirePressed -= ShowTrajectory;
            _input.SecondaryFireReleased -= ClearTrajectory;
        }

        private void ShowTrajectory()
        {
            _isAnimating = true;
        }

        private void ClearTrajectory()
        {
            _isAnimating = false;
            _rayTrajectory?.Clear();
        }

        private void Update()
        {
            if (_isAnimating && _rayTrajectory != null && _aimingPivot != null)
            {
                _rayTrajectory.DrawTrajectory(_aimingPivot.forward);
            }
        }
    }
}
