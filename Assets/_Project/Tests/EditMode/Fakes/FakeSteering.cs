using UnityEngine;

using DreamVille.Gameplay.Enemies.Steering;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="ISteering"/>: records the last <see cref="Accelerate"/> call and how often each method fired.</summary>
    public sealed class FakeSteering : ISteering
    {
        public Vector3 LastDirection { get; private set; }

        public float LastTargetSpeed { get; private set; }

        public float LastAcceleration { get; private set; }

        public int AccelerateCount { get; private set; }

        public int StopCount { get; private set; }

        public void Accelerate(Vector3 direction, float targetSpeed, float acceleration)
        {
            LastDirection = direction;
            LastTargetSpeed = targetSpeed;
            LastAcceleration = acceleration;
            AccelerateCount++;
        }

        public void Stop() => StopCount++;
    }
}
