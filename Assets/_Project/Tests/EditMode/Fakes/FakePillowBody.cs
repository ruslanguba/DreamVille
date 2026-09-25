using UnityEngine;

using DreamVille.Gameplay.Weapons.Pillow;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="IPillowBody"/>: settable position, records calls and their arguments.</summary>
    public sealed class FakePillowBody : IPillowBody
    {
        public Vector3 Position { get; set; }

        /// <summary>Name of the last <see cref="IPillowBody"/> method called, or null.</summary>
        public string LastCall { get; private set; }

        public int CallCount { get; private set; }

        public int StowCount { get; private set; }

        public Vector3 LaunchPosition { get; private set; }

        public Quaternion LaunchRotation { get; private set; }

        public Vector3 LaunchImpulse { get; private set; }

        public Vector3 ReturnStart { get; private set; }

        public void Launch(Vector3 position, Quaternion rotation, Vector3 impulse)
        {
            Record(nameof(Launch));
            Position = position;
            LaunchPosition = position;
            LaunchRotation = rotation;
            LaunchImpulse = impulse;
        }

        public void Freeze() => Record(nameof(Freeze));

        public void BeginReturn(Vector3 startPosition)
        {
            Record(nameof(BeginReturn));
            Position = startPosition;
            ReturnStart = startPosition;
        }

        public void MoveTo(Vector3 position)
        {
            Record(nameof(MoveTo));
            Position = position;
        }

        public void Stow()
        {
            Record(nameof(Stow));
            StowCount++;
        }

        /// <summary>Forgets recorded calls so a test can assert what its act step did.</summary>
        public void ResetCalls()
        {
            LastCall = null;
            CallCount = 0;
            StowCount = 0;
        }

        private void Record(string call)
        {
            LastCall = call;
            CallCount++;
        }
    }
}
