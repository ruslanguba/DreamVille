namespace DreamVille.Gameplay.Player.Ground
{
    /// <summary>Single per-frame ground query shared by movement, jump and respawn logic.</summary>
    public interface IGroundSensor
    {
        /// <summary>CharacterController.isGrounded, live.</summary>
        bool IsGrounded { get; }

        /// <summary>Result of the last <see cref="Sample"/>.</summary>
        GroundSurface Surface { get; }

        /// <summary><see cref="IsGrounded"/> and standing on no special surface.</summary>
        bool IsSafeGround { get; }

        /// <summary>Runs one overlap query. Called once per frame.</summary>
        void Sample();
    }
}
