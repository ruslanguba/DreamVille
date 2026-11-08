using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>
    /// Guards two rules: the language the player picks is remembered between launches (the
    /// Localization package's PlayerPrefs selector runs before the operating-system language), and
    /// an English line that has no English text falls back to Russian instead of showing
    /// "No translation found".
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

        [Test]
        public void English_FallsBackToRussian()
        {
            Locale english = LocalizationEditorSettings.GetLocale(new LocaleIdentifier("en"));
            Locale russian = LocalizationEditorSettings.GetLocale(new LocaleIdentifier("ru"));
            Assume.That(english, Is.Not.Null, "The English locale is missing.");
            Assume.That(russian, Is.Not.Null, "The Russian locale is missing.");

            FallbackLocale fallback = english.Metadata.GetMetadata<FallbackLocale>();

            Assert.That(fallback, Is.Not.Null, "English has no FallbackLocale metadata: lines without an English entry show 'No translation found'.");
            Assert.That(fallback.Locale, Is.EqualTo(russian), "English must fall back to Russian.");
        }

        [Test]
        public void StringDatabase_UsesFallback()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsAssetPath);
            Assume.That(settings, Is.Not.Null, $"{SettingsAssetPath} is missing.");

            Assert.That(settings.GetStringDatabase().UseFallback, Is.True,
                "The string database ignores fallback locales, so the FallbackLocale metadata on English has no effect.");
        }

        [Test]
        public void AssetDatabase_UsesFallback()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsAssetPath);
            Assume.That(settings, Is.Not.Null, $"{SettingsAssetPath} is missing.");

            Assert.That(settings.GetAssetDatabase().UseFallback, Is.True,
                "The asset database ignores fallback locales; localized images would be missing in English.");
        }
    }
}
