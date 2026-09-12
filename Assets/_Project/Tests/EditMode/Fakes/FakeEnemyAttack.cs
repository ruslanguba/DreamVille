using System.Collections.Generic;
using UnityEngine;

using DreamVille.Gameplay.Enemies.Combat;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="IEnemyAttack"/>: records every thrown direction.</summary>
    public sealed class FakeEnemyAttack : IEnemyAttack
    {
        public Vector3 Origin { get; set; }

        public List<Vector3> ThrownDirections { get; } = new List<Vector3>();

        public void Throw(Vector3 direction) => ThrownDirections.Add(direction);
    }
}
