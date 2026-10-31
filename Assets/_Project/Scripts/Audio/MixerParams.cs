namespace DreamVille.Audio
{
    /// <summary>
    /// Exposed parameters of <c>Audio/MainMixer.mixer</c>. Renaming one in the mixer asset
    /// means renaming its constant here; <c>MixerParamsTests</c> fails when they drift apart.
    /// </summary>
    public static class MixerParams
    {
        public const string Music = "MusicVolume";
        public const string Sfx = "SFXVolume";
    }
}
