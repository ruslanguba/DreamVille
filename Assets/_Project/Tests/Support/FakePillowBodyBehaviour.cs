using UnityEngine;

using DreamVille.Gameplay.Weapons.Pillow;

namespace DreamVille.Tests.Support
{
    /// <summary>
    /// Test-only <see cref="MonoBehaviour"/> implementing <see cref="IPillowBody"/>, used to reproduce the
    /// fake-null case: a destroyed component still satisfies the interface reference but Unity's <c>==</c> sees it as null.
    /// </summary>
    public sealed class FakePillowBodyBehaviour : MonoBehaviour, IPillowBody
    {
        public Vector3 Position { get; set; }

        public void Launch(Vector3 position, Quaternion rotation, Vector3 impulse) { }

        public void Freeze() { }

        public void BeginReturn(Vector3 startPosition) { }

        public void MoveTo(Vector3 position) { }

        public void Stow() { }
    }
}
