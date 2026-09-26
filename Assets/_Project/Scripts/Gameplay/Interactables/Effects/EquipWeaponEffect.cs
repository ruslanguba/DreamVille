using UnityEngine;
using UnityEngine.Serialization;

using DreamVille.Gameplay.Weapons;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>Keeps the player's weapon hidden until this pickup is collected, then equips it.</summary>
    public sealed class EquipWeaponEffect : MonoBehaviour, IInteractionEffect
    {
        [FormerlySerializedAs("pillowGun")]
        [SerializeField] private PillowGun _weapon;

        private void Awake()
        {
            if (_weapon == null)
            {
                Debug.LogError($"[{nameof(EquipWeaponEffect)}] Weapon is not assigned.", this);
                return;
            }

            _weapon.Deactivate();
        }

        public void Apply(in InteractionContext context)
        {
            if (_weapon == null)
                return;   // reported once in Awake

            _weapon.Activate();
        }
    }
}
