using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace DreamVille.Progression.Lobby
{
    /// <summary>
    /// Composition root of the lobby scenario, installed by the lobby's own <see cref="GameObjectContext"/>
    /// so that only scenes with a lobby get one. An incomplete cast fails the scene start loudly.
    /// </summary>
    [RequireComponent(typeof(LobbyActors))]
    public sealed class LobbyInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var actors = GetComponent<LobbyActors>();
            IReadOnlyList<string> missing = actors.FindMissingReferences();
            if (missing.Count > 0)
                throw new InvalidOperationException($"[{nameof(LobbyInstaller)}] '{name}' is missing: {string.Join(", ", missing)}.");

            Container.Bind<ILobbyCast>().FromInstance(actors).AsSingle();
            Container.BindInterfacesTo<LobbyDirector>().AsSingle();
        }
    }
}
