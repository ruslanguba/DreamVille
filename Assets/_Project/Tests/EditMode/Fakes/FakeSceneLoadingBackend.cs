using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DreamVille.Core.SceneManagement;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Hand-written double for <see cref="ISceneLoadingBackend"/>. Every call records the
    /// requested scene name or build index and hands back a <see cref="UniTaskCompletionSource"/>
    /// that the test resolves or fails on its own schedule. <see cref="ReportProgress"/> invokes
    /// the progress delegate of the most recently requested load, mirroring the fact that only one
    /// load can ever be in flight through <c>SceneLoader</c> at a time.
    /// </summary>
    public sealed class FakeSceneLoadingBackend : ISceneLoadingBackend
    {
        private readonly List<UniTaskCompletionSource> _requests = new List<UniTaskCompletionSource>();
        private Action<float> _lastProgress;

        public int ActiveSceneBuildIndex { get; set; }

        public List<string> RequestedSceneNames { get; } = new List<string>();
        public List<int> RequestedBuildIndices { get; } = new List<int>();

        public int RequestCount => _requests.Count;

        public UniTask LoadAsync(string sceneName, Action<float> progress)
        {
            RequestedSceneNames.Add(sceneName);
            return Enqueue(progress);
        }

        public UniTask LoadAsync(int buildIndex, Action<float> progress)
        {
            RequestedBuildIndices.Add(buildIndex);
            return Enqueue(progress);
        }

        /// <summary>Invokes the progress callback of the most recently requested load.</summary>
        public void ReportProgress(float value) => _lastProgress?.Invoke(value);

        public void CompleteRequest(int requestIndex) => _requests[requestIndex].TrySetResult();

        public void FailRequest(int requestIndex, Exception exception) => _requests[requestIndex].TrySetException(exception);

        private UniTask Enqueue(Action<float> progress)
        {
            _lastProgress = progress;
            var completionSource = new UniTaskCompletionSource();
            _requests.Add(completionSource);
            return completionSource.Task;
        }
    }
}
