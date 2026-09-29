namespace DreamVille.UI.Menus
{
    /// <summary>Switches the UI language.</summary>
    public interface ILocaleSelector
    {
        /// <summary>Selects the project locale with this code; logs an error if the project has none.</summary>
        void Select(string localeCode);
    }
}
