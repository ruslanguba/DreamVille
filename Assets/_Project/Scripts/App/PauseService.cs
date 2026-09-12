using UnityEngine;

using DreamVille.Core.Players;

namespace DreamVille.App
{
    /// <summary>Pause: time, player control and cursor mode.</summary>
    public class PauseService : IPauseService
    {
        private readonly IPlayerControlSwitch _playerControl;
        private readonly ICursorService _cursor;

        public bool IsPaused { get; private set; }

        public PauseService(IPlayerControlSwitch playerControl, ICursorService cursor)
        {
            _playerControl = playerControl;
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

        public void SetPlayerControl(bool isEnabled)
        {
            if (isEnabled)
            {
                _playerControl.EnableControl();
                _cursor.UseGameplayMode();
            }
            else
            {
                _playerControl.DisableControl();
                _cursor.UseUiMode();
            }
        }
    }
}
