using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>What a bullet hit: the target, the point, the damage and the hit count (1 for the first contact, 2 after one ricochet, and so on).</summary>
    public readonly struct BulletHitInfo
    {
        public BulletHitInfo(GameObject target, Vector3 hitPoint, float damage, int hitCount)
        {
            Target = target;
            HitPoint = hitPoint;
            Damage = damage;
            HitCount = hitCount;
        }

        public GameObject Target { get; }

        public Vector3 HitPoint { get; }

        public float Damage { get; }

        public int HitCount { get; }
    }
}
