using UnityEngine;
using Zenject;

using DreamVille.Gameplay.Player.Animation;

namespace DreamVille.Gameplay.Interactables
{
    /// <summary>Swaps the active player skin animator among a fixed set of variants.</summary>
    public class SkinHandler : MonoBehaviour
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
                Debug.LogError($"[{nameof(SkinHandler)}] No skins assigned.", this);
                return;
            }

            _playerAnimator.SetAnimator(_skins[_skinIndex]);
        }

        private void ChangeSkin()
        {
            foreach (var skin in _skins)
            {
                skin.gameObject.SetActive(false);
            }
            _skins[_skinIndex].gameObject.SetActive(true);
            _playerAnimator.SetAnimator(_skins[_skinIndex]);
        }

        public void SetSkin(int skinIndex)
        {
            if (skinIndex < 0 || skinIndex >= _skins.Length)
            {
                Debug.LogError($"[{nameof(SkinHandler)}] Skin index {skinIndex} is out of range.", this);
                return;
            }

            _skinIndex = skinIndex;
            ChangeSkin();
        }
    }
}
