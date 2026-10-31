using System.Collections.Generic;
using UnityEngine;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Enemies.Combat;
using DreamVille.Gameplay.Enemies.Presentation;
using DreamVille.Gameplay.Enemies.Steering;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Hand-written double for <see cref="IEnemyContext"/>. Every seam (<see cref="Steering"/>,
    /// <see cref="Animator"/>, <see cref="Audio"/>, <see cref="Attack"/>) is exposed both as its
    /// concrete fake type (for assertions) and, explicitly, as the interface the states consume.
    /// </summary>
    public sealed class FakeEnemyContext : IEnemyContext
    {
        public List<Vector3> PatrolPoints { get; } = new List<Vector3>();

        public List<Vector3> FacedTowards { get; } = new List<Vector3>();

        public Vector3? SnappedTo { get; private set; }

        public bool? AutoFacing { get; private set; }

        public bool? Vulnerable { get; private set; }

        public int HideoutEntered { get; private set; }

        public bool BodyReleased { get; private set; }

        public float? DespawnDelay { get; private set; }

        public FakeSteering Steering { get; } = new FakeSteering();

        public FakeEnemyAnimator Animator { get; } = new FakeEnemyAnimator();

        public FakeEnemyAudio Audio { get; } = new FakeEnemyAudio();

        public FakeEnemyAttack Attack { get; } = new FakeEnemyAttack();

        public EnemyConfig Config { get; set; }

        public Vector3 Position { get; set; }

        public bool HasPlayer { get; set; }

        public Vector3 PlayerPosition { get; set; }

        public bool HasFleeTarget { get; set; }

        public Vector3 FleeTargetPosition { get; set; }

        public bool FleeTargetIsHideout { get; set; }

        public int PatrolPointCount => PatrolPoints.Count;

        ISteering IEnemyContext.Steering => Steering;

        IEnemyAnimator IEnemyContext.Animator => Animator;

        IEnemyAudio IEnemyContext.Audio => Audio;

        IEnemyAttack IEnemyContext.Attack => Attack;

        public Vector3 GetPatrolPoint(int index) => PatrolPoints[index];

        public void EnterHideout() => HideoutEntered++;

        public void FaceTowards(Vector3 worldPoint) => FacedTowards.Add(worldPoint);

        public void SnapFacing(Vector3 worldPoint) => SnappedTo = worldPoint;

        public void SetAutoFacing(bool isEnabled) => AutoFacing = isEnabled;

        public void SetVulnerable(bool isVulnerable) => Vulnerable = isVulnerable;

        public void ReleaseBody() => BodyReleased = true;

        public void Despawn(float delay) => DespawnDelay = delay;
    }
}
