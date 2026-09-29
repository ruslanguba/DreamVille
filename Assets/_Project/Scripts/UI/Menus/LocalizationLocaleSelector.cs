using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace DreamVille.UI.Menus
{
    /// <summary>
    /// Default <see cref="ILocaleSelector"/>: selects the locale asset the project ships
    /// (<c>LocalizationSettings.AvailableLocales</c>), never a runtime-created <see cref="Locale"/>.
    /// </summary>
    public sealed class LocalizationLocaleSelector : ILocaleSelector
    {
        public void Select(string localeCode)
        {
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            if (locale == null)
            {
                Debug.LogError($"[{nameof(LocalizationLocaleSelector)}] The project has no locale '{localeCode}'.");
                return;
            }

            LocalizationSettings.SelectedLocale = locale;
        }
    }
}
