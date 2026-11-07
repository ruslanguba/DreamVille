using System;
using UnityEngine;

using DreamVille.Gameplay.Weapons.Bullets;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Raises a bullet hit only when a test asks.</summary>
    public sealed class FakeBulletHitSource : IBulletHitSource
    {
        public event Action<BulletHitInfo> BulletHit;

        public void RaiseHit(int hitCount, Vector3 hitPoint)
        {
            BulletHit?.Invoke(new BulletHitInfo(null, hitPoint, 0f, hitCount));
        }
    }
}
