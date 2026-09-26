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
    }
}
