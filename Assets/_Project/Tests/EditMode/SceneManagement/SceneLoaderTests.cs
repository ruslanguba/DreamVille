using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using DreamVille.Core.SceneManagement;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.SceneManagement
{
    public class SceneLoaderTests
    {
        [Test]
        public void LoadAsync_ForwardsSceneNameToBackendAndSetsIsLoadingUntilBackendCompletes()
        {
            var backend = new FakeSceneLoadingBackend();
            var loader = new SceneLoader(backend);

            UniTask task = loader.LoadAsync("Level1");

            Assert.That(backend.RequestedSceneNames, Is.EqualTo(new[] { "Level1" }));
            Assert.That(loader.IsLoading, Is.True);

            backend.CompleteRequest(0);

            Assert.That(loader.IsLoading, Is.False);
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
        }

        [Test]
        public void LoadAsync_SecondCallWhileLoading_IsIgnoredAndCompletesImmediately()
        {
            var backend = new FakeSceneLoadingBackend();
            var loader = new SceneLoader(backend);
            loader.LoadAsync("Level1");

            UniTask second = loader.LoadAsync("Level2");

            Assert.That(backend.RequestCount, Is.EqualTo(1));
            Assert.That(second.Status, Is.EqualTo(UniTaskStatus.Succeeded));
        }

        [Test]
        public void LoadAsync_ProgressChanged_ReceivesZeroAtStartAndOneAtEnd()
        {
            var backend = new FakeSceneLoadingBackend();
            var loader = new SceneLoader(backend);
            var progress = new List<float>();
            loader.ProgressChanged += progress.Add;

            loader.LoadAsync("Level1");
            backend.CompleteRequest(0);

            Assert.That(progress, Is.EqualTo(new[] { 0f, 1f }));
        }

        [Test]
        public void LoadAsync_ProgressListenerThrows_TransitionStillFinishes()
        {
            var backend = new FakeSceneLoadingBackend();
            var loader = new SceneLoader(backend);
            loader.ProgressChanged += _ => throw new InvalidOperationException("listener failure");
            LogAssert.Expect(LogType.Exception, new Regex(".*"));
            LogAssert.Expect(LogType.Exception, new Regex(".*"));

            UniTask task = loader.LoadAsync("Level1");
            backend.CompleteRequest(0);

            Assert.That(loader.IsLoading, Is.False);
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
        }

        [Test]
        public void LoadAsync_EmptySceneName_ThrowsArgumentException()
        {
            var backend = new FakeSceneLoadingBackend();
            var loader = new SceneLoader(backend);

            Assert.Throws<ArgumentException>(() => loader.LoadAsync(string.Empty));
        }

        [Test]
        public void LoadAsync_TokenAlreadyCancelled_ThrowsOperationCanceledExceptionAndNeverCallsBackend()
        {
            var backend = new FakeSceneLoadingBackend();
            var loader = new SceneLoader(backend);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            UniTask task = loader.LoadAsync("Level1", cts.Token);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
            Assert.That(backend.RequestedSceneNames, Is.Empty);
        }
    }
}
