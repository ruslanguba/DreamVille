using System;

using DreamVille.Audio;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Environment;
using DreamVille.Gameplay.Npc;
using DreamVille.UI;

namespace DreamVille.Tests.EditMode.Architecture
{
    /// <summary>
    /// Ledger of every serialized field the project has renamed: the owner type and the names its data used to be
    /// stored under. <see cref="SerializedDataHygieneTests"/> proves that no asset still carries data under one of these
    /// names and that no field takes one of them again. A rename batch adds its entry in the same commit as the data.
    /// </summary>
    internal static class RetiredSerializedFields
    {
        public static readonly (Type Owner, string[] OldNames)[] All =
        {
            (typeof(GameStartUI), new[] { "firstImage", "collageImages", "fadeTime", "appearScale", "firstScale" }),
            (typeof(NpcCanvasToggle), new[] { "Canvas" }),

            // _isInterracted was runtime state stored as data; it became the non-serialized _isFloating.
            (typeof(PropsMover), new[] { "_baloon", "_isInterracted" }),

            // Two fields were both called "pitch random"; each now names the clip group it jitters.
            (typeof(ActorAudio), new[] { "_pitchRandom", "randomClips", "randomInterval", "pitchRandom" }),

            // target and throwForce left Enemy in step 4.2; their instance overrides were dead data until 6.2 dropped them.
            (typeof(Enemy), new[] { "moveSpeed", "chaseRange", "attackRange", "attackCooldown", "patrolPoints", "attackSpawn", "target", "throwForce" }),
            (typeof(EnemyRotationHandler), new[] { "rotationSpeed" }),
        };
    }
}
