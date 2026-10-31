using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>What a bullet hit: the target, the point, the damage and how many ricochets it made before the hit.</summary>
    public readonly struct BulletHitInfo
    {
        public BulletHitInfo(GameObject target, Vector3 hitPoint, float damage, int ricochetCount)
        {
            Target = target;
            HitPoint = hitPoint;
            Damage = damage;
            RicochetCount = ricochetCount;
        }

        public GameObject Target { get; }

        public Vector3 HitPoint { get; }

        public float Damage { get; }

        public int RicochetCount { get; }
    }
}
