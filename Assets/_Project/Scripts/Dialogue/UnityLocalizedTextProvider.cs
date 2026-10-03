using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace DreamVille.Dialogue
{
    /// <summary>Resolves text through the Localization package for the selected locale.</summary>
    public sealed class UnityLocalizedTextProvider : ILocalizedTextProvider
    {
        public async UniTask<string> GetTextAsync(LocalizedString text, CancellationToken cancellationToken)
        {
            // Localization releases this handle itself a frame after it completes: read it here, never cache or release it.
            AsyncOperationHandle<string> handle = text.GetLocalizedStringAsync();

            // No WaitForCompletion: it blocks the main thread and is not available on WebGL.
            if (!handle.IsDone)
                await UniTask.WaitUntil(() => handle.IsDone, cancellationToken: cancellationToken);

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                throw handle.OperationException
                    ?? new InvalidOperationException($"[{nameof(UnityLocalizedTextProvider)}] A localized string failed to load.");
            }

            return handle.Result;
        }
    }
}
