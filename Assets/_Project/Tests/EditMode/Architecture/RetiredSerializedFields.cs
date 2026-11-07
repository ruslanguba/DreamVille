using System;

using DreamVille.Audio;
using DreamVille.Bootstrap;
using DreamVille.Configs;
using DreamVille.Dialogue;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Enemies.Combat;
using DreamVille.Gameplay.Environment;
using DreamVille.Gameplay.Interactables;
using DreamVille.Gameplay.Interactables.Effects;
using DreamVille.Gameplay.Npc;
using DreamVille.Gameplay.Player.Ground;
using DreamVille.Gameplay.Weapons;
using DreamVille.Gameplay.Weapons.Aiming;
using DreamVille.Input;
using DreamVille.Progression;
using DreamVille.UI;
using DreamVille.UI.Dialogues;
using DreamVille.UI.Hud;
using DreamVille.UI.Menus;

namespace DreamVille.Tests.EditMode.Architecture
{
    /// <summary>
    /// Ledger of every serialized field the project has renamed or removed: the owner type and the names its data used to be
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
            // 6.3: tuning moved to EnemyConfig archetypes (the underscored names, and the handler's rotation speed).
            (typeof(Enemy), new[] { "moveSpeed", "chaseRange", "attackRange", "attackCooldown", "patrolPoints", "attackSpawn", "target", "throwForce", "_moveSpeed", "_chaseRange", "_attackRange", "_attackCooldown" }),
            (typeof(EnemyRotationHandler), new[] { "rotationSpeed", "_rotationSpeed" }),

            // Renamed in phases 2-5 behind [FormerlySerializedAs]; 6.2 re-serialized the data and retired the attributes.
            (typeof(AudioPlayer), new[] { "audioMixer", "musicSource", "sfxSource" }),
            (typeof(ProjectInstaller), new[] { "_audioManagerPrefab" }),
            (typeof(AudioConfig), new[] { "StartVolume" }),
            (typeof(BulletConfig), new[] { "Damage", "Speed", "LifeTime", "MaxCollisionCount" }),
            (typeof(PlayerConfig), new[] { "walkSpeed", "acceleration", "rotationSpeed", "jumpHeight", "gravity" }),
            (typeof(ScoreRules), new[] { "scorePerKill" }),
            (typeof(StarRules), new[] { "starThresholds" }),
            (typeof(DialogueTrigger), new[] { "deleteOnEnd" }),
            (typeof(EnemyHealth), new[] { "_health" }),
            (typeof(EnemyProjectile), new[] { "knockbackForce" }),
            (typeof(RandomFloating), new[] { "minY", "maxY", "stopTime" }),
            (typeof(Pickup), new[] { "_upScaleTime", "_hideScaleTime" }),
            (typeof(EquipWeaponEffect), new[] { "pillowGun" }),
            (typeof(PaintEffect), new[] { "_maxHitPoints" }),
            (typeof(NpcDialogueHandler), new[] { "_timer", "_handler" }),
            (typeof(NpcMover), new[] { "jumpForce", "forwardForce" }),
            (typeof(NpcQueue), new[] { "jumpers", "moveDuration", "delayBetweenJumps" }),
            (typeof(GroundSurfaceSensor), new[] { "_checkSurfacePivot", "radius" }),
            (typeof(PillowGun), new[] { "_spawn", "_shotPeriod" }),
            (typeof(AimPreview), new[] { "line" }),
            (typeof(GameInput), new[] { "_tubAction" }),
            (typeof(InputReader), new[] { "_recallAction" }),
            (typeof(ImageFadeOut), new[] { "image" }),
            (typeof(AudioSettingsView), new[] { "musicSlider", "sfxSlider" }),
            (typeof(DialogueView), new[] { "panel", "leftImage", "rightImage", "leftTextBackground", "rightTextBackground", "leftText", "rightText", "sizeOffset" }),
            (typeof(GameUIRoot), new[] { "_ricochetUI", "_pillowUI", "_enemyCounterUI", "_scorePopupUI", "_starsUI" }),
            // 6.4: same-object components are required and fetched in Awake; runtime state is not data.
            (typeof(FinalZone), new[] { "_animator", "_audio" }),
            (typeof(ToggleDoorEffect), new[] { "_isOpen" }),
            (typeof(CameraZoom), new[] { "_currentZoom" }),
        };

        /// <summary>
        /// Dead overrides on a ledger owner that address a field which no longer exists and are too widespread for a
        /// rename batch to drop (spec 14 B6.2: more than about five assets). The override scan tolerates exactly these
        /// owner-field pairs; retired names stay forbidden. The list can only shrink.
        /// </summary>
        public static readonly (Type Owner, string Field, string Reason)[] KnownDeadOverrides =
        {
            (typeof(InputReader), "_interractAction", "the player instance in 9 assets; dropped with the other dead overrides in roadmap 6.5"),
        };
    }
}
