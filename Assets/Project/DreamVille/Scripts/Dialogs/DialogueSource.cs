using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
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
