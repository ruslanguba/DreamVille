using UnityEngine;
using Zenject;

using DreamVille.Gameplay.Player.Animation;

namespace DreamVille.Gameplay.Player.Skins
{
    /// <summary>Swaps the active player skin among a fixed set of Animator variants.</summary>
    public sealed class PlayerSkins : MonoBehaviour, IPlayerSkins
    {
        [SerializeField] private Animator[] _skins;

        private IPlayerAnimator _playerAnimator;
        private int _skinIndex;

        [Inject]
        private void Construct(IPlayerAnimator playerAnimator)
        {
            _playerAnimator = playerAnimator;
        }

        private void Awake()
        {
            if (_skins == null || _skins.Length == 0)
            {
                Debug.LogError($"[{nameof(PlayerSkins)}] No skins assigned.", this);
                return;
            }

            // One source of truth: the visible skin and the animator driver both follow _skinIndex from the start.
            ApplySkin();
        }

        public void SetSkin(int index)
        {
            if (_skins == null || index < 0 || index >= _skins.Length)
            {
                Debug.LogError($"[{nameof(PlayerSkins)}] Skin index {index} is out of range.", this);
                return;
            }

            _skinIndex = index;
            ApplySkin();
        }

        private void ApplySkin()
        {
            foreach (Animator skin in _skins)
                skin.gameObject.SetActive(false);

            _skins[_skinIndex].gameObject.SetActive(true);
            _playerAnimator.SetAnimator(_skins[_skinIndex]);
        }
    }
}
