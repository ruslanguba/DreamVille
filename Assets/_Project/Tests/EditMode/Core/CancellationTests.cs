using System.Threading;
using NUnit.Framework;

using DreamVille.Core.Async;

namespace DreamVille.Tests.EditMode.Core
{
    /// <summary>The cancel-dispose-forget contract of <see cref="Cancellation.CancelAndDispose"/>.</summary>
    public class CancellationTests
    {
        [Test]
        public void CancelAndDispose_CancelsTokenAndClearsField()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            Cancellation.CancelAndDispose(ref source);

            Assert.That(token.IsCancellationRequested, Is.True);
            Assert.That(source, Is.Null);
        }

        [Test]
        public void CancelAndDispose_OnNull_DoesNothing()
        {
            CancellationTokenSource source = null;

            Assert.DoesNotThrow(() => Cancellation.CancelAndDispose(ref source));
            Assert.That(source, Is.Null);
        }
    }
}
