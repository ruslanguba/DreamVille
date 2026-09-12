using UnityEngine;

namespace DreamVille.Gameplay.Weapons
{
    public interface IWeapon
    {
        bool CanShoot();
        void TryShoot();
        void Activate();
        void Deactivate();
    }
}
