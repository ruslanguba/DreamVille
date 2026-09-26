namespace DreamVille.Gameplay.NPC
{
    /// <summary>The Animator writes an NPC state needs; exists so EditMode tests can record them.</summary>
    public interface INpcAnimatorParameters
    {
        void SetBool(int parameterHash, bool value);
    }
}
