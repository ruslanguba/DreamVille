using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>What a bullet hit: the target, the point, the damage and how many ricochets it made before the hit.</summary>
    public struct BulletHitInfo
    {
        public GameObject Target;
        public Vector3 HitPoint;
        public float Damage;
        public int RicochetCount;
    }
}
