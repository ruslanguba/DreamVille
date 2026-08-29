using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine;

namespace DreamVille.UI
{
    public class LocaleOnButton : MonoBehaviour
    {
        public void OnClickRu()
        {
            LocalizationSettings.SelectedLocale = Locale.CreateLocale("ru");
        }

        public void OnClickEn()
        {
            LocalizationSettings.SelectedLocale = Locale.CreateLocale("en");
        }
    }
}
