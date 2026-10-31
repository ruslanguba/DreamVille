using NUnit.Framework;
using UnityEditor;
using UnityEngine.Audio;

using DreamVille.Audio;

namespace DreamVille.Tests.EditMode.Audio
{
    /// <summary>
    /// Pins <see cref="MixerParams"/> to the exposed parameters of the shipped
    /// Audio/MainMixer.mixer: a rename on either side becomes a red test here instead of
    /// a volume slider that silently stops reaching the mixer.
    /// </summary>
    public class MixerParamsTests
    {
        private const string MainMixerAssetPath = "Assets/_Project/Audio/MainMixer.mixer";

        [TestCase(MixerParams.Music)]
        [TestCase(MixerParams.Sfx)]
        public void MixerParam_IsExposedByMainMixer(string name)
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MainMixerAssetPath);
            Assert.That(mixer, Is.Not.Null, $"{MainMixerAssetPath} is missing.");

            bool isExposed = mixer.GetFloat(name, out _);

            Assert.That(isExposed, Is.True, $"'{name}' is not an exposed parameter of {MainMixerAssetPath}.");
        }
    }
}
