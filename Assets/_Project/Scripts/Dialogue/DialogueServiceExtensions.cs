using System.Threading;
using Cysharp.Threading.Tasks;

namespace DreamVille.Dialogue
{
    /// <summary>Awaitable form of <see cref="IDialogueService.TryStart"/> for callers that do not need the acceptance moment.</summary>
    public static class DialogueServiceExtensions
    {
        /// <summary>Runs a conversation to its end. Returns false if another conversation was running.</summary>
        public static async UniTask<bool> RunAsync(this IDialogueService service, DialogueDefinition dialogue, CancellationToken cancellationToken)
        {
            if (!service.TryStart(dialogue, cancellationToken, out UniTask completion))
                return false;

            await completion;
            return true;
        }
    }
}
