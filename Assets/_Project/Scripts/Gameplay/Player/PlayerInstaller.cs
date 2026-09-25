using System;

using Unity.Cinemachine;
using UnityEngine;
using Zenject;

using DreamVille.Audio;
using DreamVille.Configs;
using DreamVille.Gameplay.Damage;
using DreamVille.Gameplay.Player.Animation;
using DreamVille.Gameplay.Player.Ground;
using DreamVille.Input;

namespace DreamVille.Gameplay.Player
{
    /// <summary>
    /// Composition root of the player prefab, installed by its <see cref="GameObjectContext"/>. Binds the root's
    /// components and constructs the player's plain systems; scene services come from the parent scene container.
    /// </summary>
    public sealed class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private PlayerConfig _config;

        public override void InstallBindings()
        {
            if (_config == null)
            {
                Debug.LogError($"[{nameof(PlayerInstaller)}] {nameof(PlayerConfig)} is not assigned.", this);
                throw new InvalidOperationException($"{nameof(PlayerInstaller)} on '{name}' has no {nameof(PlayerConfig)}.");
            }

            BindPrefabComponents();
            BindPlayerSystems();
        }

        private void BindPrefabComponents()
        {
            Container.Bind<PlayerConfig>().FromInstance(_config).AsSingle();

            Container.BindInterfacesAndSelfTo<InputReader>().FromComponentOnRoot().AsSingle();
            Container.Bind<CharacterController>().FromComponentOnRoot().AsSingle();
            Container.Bind<Transform>().FromComponentOnRoot().AsSingle();
            Container.BindInterfacesTo<GroundSurfaceSensor>().FromComponentOnRoot().AsSingle();
            Container.Bind<IActorAudio>().To<ActorAudio>().FromComponentOnRoot().AsSingle();

            // Resolved once at composition; every gameplay scene has a MainCamera-tagged camera.
            Container.Bind<Camera>().FromMethod(_ => Camera.main).AsSingle();

            // Hierarchy lookups from a GameObjectContext search only the prefab's own subtree.
            Container.Bind<CinemachineCamera>().FromComponentInHierarchy().AsSingle();
            Container.Bind<CinemachineInputAxisController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<Animator>().FromComponentInHierarchy(includeInactive: false).AsSingle();
        }

        private void BindPlayerSystems()
        {
            Container.BindInterfacesAndSelfTo<PlayerAnimatorDriver>().AsSingle();

            Container.Bind<PlayerMovement>().AsSingle();
            Container.Bind<PlayerGravity>().AsSingle();
            Container.Bind<PlayerLook>().AsSingle();
            Container.Bind<PlayerKnockback>().AsSingle();
            Container.Bind<SafePositionTracker>().AsSingle();

            Container.BindInterfacesAndSelfTo<PlayerJump>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerHealth>().AsSingle();
            Container.BindInterfacesTo<PlayerMotor>().AsSingle();
        }
    }
}
