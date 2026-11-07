using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Pillows
{
    /// <summary>The physical side of one pillow; implemented by the Pillow component, faked in tests.</summary>
    public interface IPillowBody
    {
        Vector3 Position { get; }

        /// <summary>Places and activates the body, makes it dynamic with collisions, and applies <paramref name="impulse"/>.</summary>
        void Launch(Vector3 position, Quaternion rotation, Vector3 impulse);

        /// <summary>Stops the body where it is; it stays a solid platform.</summary>
        void Freeze();

        /// <summary>Stops the body, turns off its collisions and teleports it to <paramref name="startPosition"/>.</summary>
        void BeginReturn(Vector3 startPosition);

        void MoveTo(Vector3 position);

        /// <summary>Stops the body, unparents it and deactivates it.</summary>
        void Stow();
    }
}
