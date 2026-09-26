using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Progression.Lobby;

namespace DreamVille.Tests.EditMode.Progression
{
    /// <summary>The cast validation that <see cref="LobbyInstaller"/> relies on to fail the scene start loudly.</summary>
    public class LobbyActorsTests
    {
        private GameObject _host;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_host);
        }

        [Test]
        public void FindMissingReferences_OnAnEmptyCast_ListsEveryField()
        {
            _host = new GameObject("Lobby");
            var actors = _host.AddComponent<LobbyActors>();
            Assert.That(actors, Is.Not.Null);

            IReadOnlyList<string> missing = actors.FindMissingReferences();

            Assert.That(missing, Is.EquivalentTo(new[]
            {
                "_firstGroup", "_secondGroup", "_firstDialogue", "_secondDialogue", "_firstDoor", "_secondDoor", "_guide",
            }));
        }
    }
}
