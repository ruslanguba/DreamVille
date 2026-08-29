using System.Collections.Generic;
using System;
using UnityEngine.Localization.Settings;

namespace DreamVille.Dialogue
{
    [Serializable]
    public class DialogueSource
    {
        public List<Dialogue> ru;
        public List<Dialogue> en;

        public List<Dialogue> GetByLocale()
        {
            var locale = LocalizationSettings.SelectedLocale.Identifier;
            return locale == "en" ? en : ru;
        }
    }
}
