using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.Localization.Settings;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// Guards spec 10 §8 Q1: the language the player picks is remembered between launches, so the
    /// Localization package's PlayerPrefs selector must run before the operating-system language.
    /// </summary>
    public class LocalizationSettingsContentTests
    {
        private const string SettingsAssetPath = "Assets/_Project/Localization/Localization Settings.asset";

        [Test]
        public void LocalizationSettings_RemembersTheSelectedLocale()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsAssetPath);
            Assume.That(settings, Is.Not.Null, $"{SettingsAssetPath} is missing.");

            List<IStartupLocaleSelector> selectors = settings.GetStartupLocaleSelectors();
            int playerPrefIndex = selectors.FindIndex(selector => selector is PlayerPrefLocaleSelector);
            int systemIndex = selectors.FindIndex(selector => selector is SystemLocaleSelector);

            Assert.That(playerPrefIndex, Is.Not.EqualTo(-1), "No PlayerPrefLocaleSelector: the chosen language is forgotten on restart.");
            Assert.That(playerPrefIndex, Is.LessThan(systemIndex), "The saved language must win over the system language.");
            Assert.That(((PlayerPrefLocaleSelector)selectors[playerPrefIndex]).PlayerPreferenceKey, Is.Not.Empty);
        }
    }
}
