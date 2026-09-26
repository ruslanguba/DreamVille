using System.Threading;

namespace DreamVille.Core.Async
{
    /// <summary>The cancel-dispose-forget sequence every restartable async operation needs.</summary>
    public static class Cancellation
    {
        /// <summary>Cancels and disposes <paramref name="source"/> and clears the field. Safe on null.</summary>
        public static void CancelAndDispose(ref CancellationTokenSource source)
        {
            if (source == null)
                return;

            source.Cancel();
            source.Dispose();
            source = null;
        }

        /// <summary>
        /// Cancels and disposes the previous run in <paramref name="source"/>, starts a new source linked to
        /// <paramref name="lifetime"/> and returns its token. The newest caller owns the operation.
        /// </summary>
        /// <remarks>
        /// Works on array elements (<c>ref _sources[i]</c>). If <paramref name="lifetime"/> is already
        /// cancelled, the returned token is cancelled too and the operation ends at its first await.
        /// </remarks>
        public static CancellationToken Restart(ref CancellationTokenSource source, CancellationToken lifetime)
        {
            CancelAndDispose(ref source);
            source = CancellationTokenSource.CreateLinkedTokenSource(lifetime);
            return source.Token;
        }
    }
}
