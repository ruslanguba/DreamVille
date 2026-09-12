namespace DreamVille.Audio
{
    /// <summary>
    /// Exposed parameter names of the project's AudioMixer. They are spelled exactly like the
    /// matching save keys, so a rename here breaks stored volumes too.
    /// </summary>
    public static class MixerParams
    {
        public const string Music = "MusicVolume";
        public const string Sfx = "SFXVolume";
    }
}
