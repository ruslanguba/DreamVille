namespace DreamVille.Core.Players
{
    /// <summary>Turns player input and camera control on and off without stopping time.</summary>
    public interface IPlayerControlSwitch
    {
        void EnableControl();

        void DisableControl();
    }
}
