using System;

namespace DreamVille.UI.Menus
{
    /// <summary>Music and effects sliders, values normalized to 0..1.</summary>
    public interface IAudioSettingsView
    {
        /// <summary>The player moved the music slider.</summary>
        event Action<float> MusicVolumeEdited;

        /// <summary>The player moved the effects slider.</summary>
        event Action<float> SfxVolumeEdited;

        /// <summary>Moves the music slider without raising <see cref="MusicVolumeEdited"/>.</summary>
        void ShowMusicVolume(float normalized);

        /// <summary>Moves the effects slider without raising <see cref="SfxVolumeEdited"/>.</summary>
        void ShowSfxVolume(float normalized);
    }
}
