using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

using DreamVille.Core.Async;

namespace DreamVille.Tests.EditMode.Core
{
    /// <summary><see cref="GameTime"/> waits yield at least one frame, so a zero delay in a loop cannot spin.</summary>
    public class GameTimeTests
    {
        [TestCase(0f)]
        [TestCase(-1f)]
        public void DelayAsync_NonPositive_DoesNotCompleteSynchronously(float seconds)
        {
            using var cts = new CancellationTokenSource();

            UniTask task = GameTime.DelayAsync(seconds, cts.Token);

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending));

            cts.Cancel();
        }
    }
}
