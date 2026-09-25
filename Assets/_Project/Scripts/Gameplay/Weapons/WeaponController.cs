using System;
using Zenject;

using DreamVille.Input;

namespace DreamVille.Gameplay.Weapons
{
    /// <summary>Maps the fire input to the equipped weapon. Plain class: it only needs input and a weapon.</summary>
    public sealed class WeaponController : IInitializable, IDisposable
    {
        private readonly IPlayerInput _input;
        private readonly IWeapon _weapon;

        public WeaponController(IPlayerInput input, IWeapon weapon)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _weapon = weapon ?? throw new ArgumentNullException(nameof(weapon));
        }

        public void Initialize()
        {
            _input.FirePressed += OnFirePressed;
        }

        public void Dispose()
        {
            _input.FirePressed -= OnFirePressed;
        }

        // The single readiness check lives in TryShoot.
        private void OnFirePressed() => _weapon.TryShoot();
    }
}
