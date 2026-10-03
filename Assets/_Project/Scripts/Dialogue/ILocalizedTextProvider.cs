using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;

namespace DreamVille.Dialogue
{
    /// <summary>Resolves a localized string for the selected locale. Seam so the dialogue service is testable without Localization.</summary>
    public interface ILocalizedTextProvider
    {
        UniTask<string> GetTextAsync(LocalizedString text, CancellationToken cancellationToken);
    }
}
