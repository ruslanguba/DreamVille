using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

using DreamVille.SceneManagement;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="ISceneLoader"/>. Every call completes
    /// immediately and is recorded, in order, as a string (for example "Load:Main menu 1",
    /// "ReloadActive"). Accepts an external log so a test can interleave its calls with
    /// another fake's, to assert cross-service call order.</summary>
    public sealed class FakeSceneLoader : ISceneLoader
    {
        private readonly List<string> _calls;

        public FakeSceneLoader(List<string> calls = null)
        {
            _calls = calls ?? new List<string>();
        }

        public IReadOnlyList<string> Calls => _calls;

#pragma warning disable CS0067 // never raised: no test drives progress reporting through this fake.
        public event Action<float> ProgressChanged;
#pragma warning restore CS0067

        public bool IsLoading => false;

        public UniTask LoadAsync(string sceneName, CancellationToken token = default)
        {
            _calls.Add($"Load:{sceneName}");
            return UniTask.CompletedTask;
        }

        public UniTask ReloadActiveAsync(CancellationToken token = default)
        {
            _calls.Add("ReloadActive");
            return UniTask.CompletedTask;
        }
    }
}
