using System.Threading;
using Cysharp.Threading.Tasks;

namespace DreamVille.UI
{
    /// <summary>The intro splash the game plays before the main menu.</summary>
    public interface IGameIntro
    {
        /// <summary>Plays the whole intro. Completes when the last frame has been shown.</summary>
        UniTask PlayAsync(CancellationToken token);
    }
}
