using UnityEngine;

namespace DreamVille.Core
{
    /// <summary>Режим курсора. Раньше эти четыре строки были размазаны по GameManager.</summary>
    public class CursorService
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
