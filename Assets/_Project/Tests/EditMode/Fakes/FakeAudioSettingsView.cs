using System;
using System.Collections.Generic;

using DreamVille.UI.Menus;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Records every slider value the presenter showed; the player's slider edits are raised on request.</summary>
    public sealed class FakeAudioSettingsView : IAudioSettingsView
    {
        private readonly List<float> _musicShown = new List<float>();
        private readonly List<float> _sfxShown = new List<float>();

        public event Action<float> MusicVolumeEdited;
        public event Action<float> SfxVolumeEdited;

        public IReadOnlyList<float> MusicShown => _musicShown;

        public IReadOnlyList<float> SfxShown => _sfxShown;

        public void ShowMusicVolume(float normalized)
        {
            _musicShown.Add(normalized);
        }

        public void ShowSfxVolume(float normalized)
        {
            _sfxShown.Add(normalized);
        }

        public void EditMusicVolume(float normalized)
        {
            MusicVolumeEdited?.Invoke(normalized);
        }

        public void EditSfxVolume(float normalized)
        {
            SfxVolumeEdited?.Invoke(normalized);
        }
    }
}
