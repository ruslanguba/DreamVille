using UnityEngine;

namespace DreamVille.Gameplay.Damage
{
    /// <summary>Anything that reacts to a directional hit. The player today; props tomorrow.</summary>
    public interface IHitReceiver
    {
        void ReceiveHit(Vector3 direction, float force);
    }
}
