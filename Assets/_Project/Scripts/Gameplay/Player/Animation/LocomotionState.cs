namespace DreamVille.Gameplay.Player.Animation
{
    /// <summary>Snapshot of the locomotion the animator should play this frame.</summary>
    public readonly struct LocomotionState
    {
        public LocomotionState(bool isMoving, MoveDirection direction, float speedMultiplier)
        {
            IsMoving = isMoving;
            Direction = direction;
            SpeedMultiplier = speedMultiplier;
        }

        public bool IsMoving { get; }
        public MoveDirection Direction { get; }
        public float SpeedMultiplier { get; }
    }
}
