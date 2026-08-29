using UnityEngine;

namespace DreamVille.Configs
{
    [CreateAssetMenu(fileName = "BulletConfig", menuName = "Scriptable Objects/BulletConfig")]
    public class BulletConfig : ScriptableObject
    {
        public float Damage;
        public float Speed;
        public float LifeTime;
        public int MaxCollisionCount;
        public ParticleSystem HitEffect;
        public AudioClip CollisionSound;
    }
}
