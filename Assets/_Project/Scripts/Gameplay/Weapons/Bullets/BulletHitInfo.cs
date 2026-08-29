using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    public struct BulletHitInfo
    {
        public GameObject Target;
        public Vector3 HitPoint;
        public float Damage;
        public int RicochetCount;
    }
}
