using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

using DreamVille.Configs;
using DreamVille.Input;

namespace DreamVille.Gameplay.Weapons.Aiming
{
    /// <summary>
    /// Draws the bounce path of the shot the equipped weapon would fire now, while aim is held.
    /// Lives on the gun's GameObject, so an unequipped gun draws nothing.
    /// </summary>
    public sealed class AimPreview : MonoBehaviour
    {
        [FormerlySerializedAs("line")]
        [SerializeField] private LineRenderer _line;

        private IPlayerInput _input;
        private IAimSource _aim;
        private ISphereCaster _caster;
        private Vector3[] _points;

        [Inject]
        public void Construct(IPlayerInput input, IAimSource aim, ISphereCaster caster)
        {
            _input = input;
            _aim = aim;
            _caster = caster;
        }

        private void Awake()
        {
            if (_line == null || _aim.Projectile == null)
            {
                Debug.LogError($"[{nameof(AimPreview)}] {nameof(_line)} and the weapon's projectile config must be assigned; the preview is disabled.", this);
                enabled = false;
                return;
            }

            _points = new Vector3[BouncePath.RequiredCapacity(_aim.Projectile.MaxCollisionCount)];
            Hide();
        }

        private void OnDisable()
        {
            Hide();
        }

        // LateUpdate draws after the player's Update turned the body towards the aim.
        private void LateUpdate()
        {
            if (!_input.IsAimHeld || !_aim.CanAim)
            {
                Hide();
                return;
            }

            BulletConfig projectile = _aim.Projectile;
            int count = BouncePath.Compute(
                _aim.Aim,
                projectile.CollisionRadius,
                projectile.MaxCollisionCount,
                projectile.MaxTravelDistance,
                _caster,
                _points);

            _line.positionCount = count;
            _line.SetPositions(_points);   // Unity copies only positionCount entries
        }

        private void Hide()
        {
            if (_line != null && _line.positionCount != 0)
                _line.positionCount = 0;
        }
    }
}
