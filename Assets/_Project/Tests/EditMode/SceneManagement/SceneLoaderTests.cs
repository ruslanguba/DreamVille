using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using DreamVille.SceneManagement;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.SceneManagement
{
    public class SceneLoaderTests
    {
        [Test]
        public void LoadAsync_ForwardsSceneNameToBackendAndSetsIsLoading()
        {
            var backend = new FakeSceneLoadingBackend();
            var loader = new SceneLoader(backend);

            loader.LoadAsync("Level1");

            Assert.That(backend.RequestedSceneNames, Is.EqualTo(new[] { "Level1" }));
            Assert.That(loader.IsLoading, Is.True);
        }

        [Test]
        public void LoadAsync_WhenBackendCompletes_ReleasesIsLoadingAndSucceeds()
        {
            var backend = new FakeSceneLoadingBackend();
            var loader = new SceneLoader(backend);
            UniTask task = loader.LoadAsync("Level1");

            backend.CompleteRequest(0);

            Assert.That(loader.IsLoading, Is.False);
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
        }

        [Test]
        public void LoadAsync_WhenBackendThrows_ReleasesIsLoadingAndRethrows()
        {
            var backend = new FakeSceneLoadingBackend();
            var loader = new SceneLoader(backend);
            UniTask task = loader.LoadAsync("Level1");

            backend.FailRequest(0, new InvalidOperationException("load failed"));

            Assert.That(loader.IsLoading, Is.False);
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Faulted));
            Assert.Throws<InvalidOperationException>(() => task.GetAwaiter().GetResult());
        }

        [Test]
        public void LoadAsync_CancelledMidLoad_HoldsIsLoadingUntilTheBackendFinishes()
        {
            var backend = new FakeSceneLoadingBackend();
            var loader = new SceneLoader(backend);
            using var cts = new CancellationTokenSource();
            UniTask task = loader.LoadAsync("Level1", cts.Token);

            cts.Cancel();

            Assert.That(loader.IsLoading, Is.True);
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending));

            backend.CompleteRequest(0);

            Assert.That(loader.IsLoading, Is.False);
            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
        }

        [Test]
        public void ReloadActiveAsync_LoadsTheActiveScene()
        {
            var backend = new FakeSceneLoadingBackend { ActiveSceneName = "Level2" };
            var loader = new SceneLoader(backend);

            loader.ReloadActiveAsync();

            Assert.That(backend.RequestedSceneNames, Is.EqualTo(new[] { "Level2" }));
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
            // Both progress reports (0 and 1) reach the throwing listener.
            LogAssert.Expect(LogType.Exception, new Regex("listener failure"));
            LogAssert.Expect(LogType.Exception, new Regex("listener failure"));

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
