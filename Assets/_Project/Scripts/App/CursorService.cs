using UnityEngine;

namespace DreamVille.App
{
    /// <summary>Cursor mode for gameplay and for menus.</summary>
    public sealed class CursorService : ICursorService
    {
        public void UseGameplayMode()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UseUiMode()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}
