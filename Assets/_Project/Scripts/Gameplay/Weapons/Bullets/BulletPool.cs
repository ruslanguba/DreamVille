using System.Collections.Generic;
using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    public class BulletPool : MonoBehaviour
    {
        [SerializeField] private Bullet _prefab;
        private Queue<Bullet> _pool = new();

        public Bullet Get()
        {
            if (_pool.Count == 0)
                Create();

            Bullet bullet = _pool.Dequeue();
            bullet.gameObject.SetActive(true);
            bullet.SetPool(this);
            return bullet;
        }

        public void Return(Bullet bullet)
        {
            bullet.gameObject.SetActive(false);
            _pool.Enqueue(bullet);
        }

        private void Create()
        {
            var bullet = Instantiate(_prefab);
            bullet.gameObject.SetActive(false);
            _pool.Enqueue(bullet);
        }
    }
}
