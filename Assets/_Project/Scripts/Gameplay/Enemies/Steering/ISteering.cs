using UnityEngine;

namespace DreamVille.Gameplay.Enemies.Steering
{
    /// <summary>Velocity-matching steering; the only motion primitive a state may use.</summary>
    public interface ISteering
    {
        Vector3 Velocity { get; }

        void Accelerate(Vector3 direction, float targetSpeed, float acceleration);

        void Stop();
    }
}
