using System.Threading;
using NUnit.Framework;

using DreamVille.Core.Async;

namespace DreamVille.Tests.EditMode.Core
{
    /// <summary>
    /// The cancel-dispose-forget contract of <see cref="Cancellation.CancelAndDispose"/> and the
    /// "newest caller owns the operation" rule of <see cref="Cancellation.Restart"/>.
    /// </summary>
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

        [Test]
        public void Restart_CancelsThePreviousRun()
        {
            CancellationTokenSource source = null;
            CancellationToken first = Cancellation.Restart(ref source, CancellationToken.None);

            CancellationToken second = Cancellation.Restart(ref source, CancellationToken.None);

            Assert.That(first.IsCancellationRequested, Is.True);
            Assert.That(second.IsCancellationRequested, Is.False);

            Cancellation.CancelAndDispose(ref source);
        }

        [Test]
        public void Restart_NewTokenIsCancelledWithLifetime()
        {
            using var lifetime = new CancellationTokenSource();
            CancellationTokenSource source = null;
            CancellationToken token = Cancellation.Restart(ref source, lifetime.Token);

            lifetime.Cancel();

            Assert.That(token.IsCancellationRequested, Is.True);

            Cancellation.CancelAndDispose(ref source);
        }

        [Test]
        public void Restart_WithCancelledLifetime_ReturnsCancelledToken()
        {
            using var lifetime = new CancellationTokenSource();
            lifetime.Cancel();
            CancellationTokenSource source = null;

            CancellationToken token = Cancellation.Restart(ref source, lifetime.Token);

            Assert.That(token.IsCancellationRequested, Is.True);

            Cancellation.CancelAndDispose(ref source);
        }

        [Test]
        public void Restart_OnArrayElement_OnlyTouchesThatElement()
        {
            var sources = new CancellationTokenSource[2];
            CancellationToken firstSlot = Cancellation.Restart(ref sources[0], CancellationToken.None);
            CancellationToken secondSlot = Cancellation.Restart(ref sources[1], CancellationToken.None);

            Cancellation.Restart(ref sources[0], CancellationToken.None);

            Assert.That(firstSlot.IsCancellationRequested, Is.True);
            Assert.That(secondSlot.IsCancellationRequested, Is.False);

            Cancellation.CancelAndDispose(ref sources[0]);
            Cancellation.CancelAndDispose(ref sources[1]);
        }
    }
}
