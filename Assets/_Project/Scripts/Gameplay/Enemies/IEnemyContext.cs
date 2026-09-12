using UnityEngine;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies.Combat;
using DreamVille.Gameplay.Enemies.Presentation;
using DreamVille.Gameplay.Enemies.Steering;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>Everything a state may see. Implemented by the Enemy component, faked in EditMode tests.</summary>
    public interface IEnemyContext
    {
        EnemyConfig Config { get; }
        EnemyTuning Tuning { get; }
        Vector3 Position { get; }
        bool HasPlayer { get; }
        Vector3 PlayerPosition { get; }
        ISteering Steering { get; }
        IEnemyAnimator Animator { get; }
        IEnemyAudio Audio { get; }
        IEnemyAttack Attack { get; }
        int PatrolPointCount { get; }
        Vector3 GetPatrolPoint(int index);
        bool HasFleeTarget { get; }
        Vector3 FleeTargetPosition { get; }
        bool FleeTargetIsHideout { get; }

        /// <summary>Hands this enemy to the flee target's <see cref="IEnemyHideout"/>.</summary>
        void EnterHideout();

        /// <summary>Slerps facing towards a world point at the enemy's rotation speed.</summary>
        void FaceTowards(Vector3 worldPoint);

        /// <summary>Immediately snaps facing towards a world point, ignoring rotation speed.</summary>
        void SnapFacing(Vector3 worldPoint);

        /// <summary>Enables or disables the automatic velocity-facing rotation.</summary>
        void SetAutoFacing(bool isEnabled);

        void SetVulnerable(bool isVulnerable);

        /// <summary>Restores full physics simulation after death (gravity, non-kinematic, free rotation).</summary>
        void ReleaseBody();

        void Despawn(float delay);
    }
}
