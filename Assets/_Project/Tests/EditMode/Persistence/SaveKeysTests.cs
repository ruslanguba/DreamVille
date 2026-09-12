using NUnit.Framework;

using DreamVille.Persistence;

namespace DreamVille.Tests.EditMode.Persistence
{
    public class SaveKeysTests
    {
        [Test]
        public void Stars_ReturnsLevelIdWithLegacySuffix()
        {
            string key = SaveKeys.Stars("2");

            Assert.That(key, Is.EqualTo("2_Stars"));
        }

        [Test]
        public void Completed_ReturnsLevelIdWithLegacySuffix()
        {
            string key = SaveKeys.Completed("2");

            Assert.That(key, Is.EqualTo("2_Completed"));
        }

        [Test]
        public void BestScore_KeepsLegacyPrefix()
        {
            string key = SaveKeys.BestScore("2");

            Assert.That(key, Is.EqualTo("BestScore_LVL_2"));
        }

        [Test]
        public void VolumeKeys_MatchTheirFixedNames()
        {
            Assert.That(SaveKeys.MusicVolume, Is.EqualTo("MusicVolume"));
            Assert.That(SaveKeys.SfxVolume, Is.EqualTo("SFXVolume"));
        }
    }
}
