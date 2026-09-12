using UnityEngine;

using DreamVille.Gameplay.Player;

namespace DreamVille.Gameplay.Interactables
{
    /// <summary>Swaps the active player skin animator among a fixed set of variants.</summary>
    [RequireComponent(typeof(PlayerContext))]
    public class SkinHandler : MonoBehaviour
    {
        [SerializeField] private Animator[] _skins;

        private PlayerContext _playerContext;
        private int _skinIndex;

        private void Awake()
        {
            _playerContext = GetComponent<PlayerContext>();

            if (_skins == null || _skins.Length == 0)
            {
                Debug.LogError($"[{nameof(SkinHandler)}] No skins assigned.", this);
                return;
            }

            _playerContext.SetAnimator(_skins[_skinIndex]);
        }

        private void ChangeSkin()
        {
            foreach (var skin in _skins)
            {
                skin.gameObject.SetActive(false);
            }
            _skins[_skinIndex].gameObject.SetActive(true);
            _playerContext.SetAnimator(_skins[_skinIndex]);
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
