using UnityEngine;
using Zenject;

using DreamVille.Input;
using DreamVille.Gameplay.Weapons.Aiming;

namespace DreamVille.Gameplay.Weapons
{
    public class AimingHandler : MonoBehaviour
    {
        [SerializeField] private BounceRayTrajectory _rayTrajectory;
        [SerializeField] private Transform _aimingPivot;
        private bool _isAnimating = false;
        private IPlayerInput _input;

        [Inject]
        private void Construct(IPlayerInput input)
        {
            _input = input;
        }

        // Subscribe in OnEnable (mirrored by OnDisable) so aiming survives disable/enable cycles.
        private void OnEnable()
        {
            _input.SecondaryFirePressed += ShowTrajectory;
            _input.SecondaryFireReleased += ClearTrajectory;
        }

        private void OnDisable()
        {
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
