using UnityEngine;

using DreamVille.Gameplay.Player.Skins;

namespace DreamVille.Tests.Support
{
    /// <summary>Test-only <see cref="IPlayerSkins"/> component, so an effect can find it with <c>GetComponentInParent</c>.</summary>
    public sealed class FakePlayerSkinsBehaviour : MonoBehaviour, IPlayerSkins
    {
        public int CallCount { get; private set; }

        public int LastIndex { get; private set; } = -1;

        public void SetSkin(int index)
        {
            CallCount++;
            LastIndex = index;
        }
    }
}
