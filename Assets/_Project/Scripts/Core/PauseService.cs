using UnityEngine;

using DreamVille.Gameplay.Player;

namespace DreamVille.Core
{
    /// <summary>Пауза: время, управление игроком и курсор.</summary>
    public class PauseService
    {
        private readonly PlayerInputSwitch _playerInput;
        private readonly CursorService _cursor;

        public bool IsPaused { get; private set; }

        public PauseService(PlayerInputSwitch playerInput, CursorService cursor)
        {
            _playerInput = playerInput;
            _cursor = cursor;
        }

        public void Pause()
        {
            Time.timeScale = 0f;
            IsPaused = true;
            SetPlayerControl(false);
        }

        public void Resume()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            SetPlayerControl(true);
        }

        /// <summary>Отнять управление, не останавливая время — например, на время диалога.</summary>
        public void SetPlayerControl(bool isEnabled)
        {
            if (isEnabled)
            {
                _playerInput.EnableInput();
                _cursor.UseGameplayMode();
            }
            else
            {
                _playerInput.DisableInput();
                _cursor.UseUiMode();
            }
        }
    }
}
