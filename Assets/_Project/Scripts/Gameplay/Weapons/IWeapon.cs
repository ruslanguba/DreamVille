using UnityEngine;

namespace DreamVille.Gameplay.Weapons
{
    public interface IWeapon
    {
        void TryShoot();
        void Activate();
        void Deactivate();
    }
}
