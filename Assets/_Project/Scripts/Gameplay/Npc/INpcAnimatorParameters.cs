namespace DreamVille.Gameplay.Npc
{
    /// <summary>The Animator writes an NPC state needs; exists so EditMode tests can record them.</summary>
    public interface INpcAnimatorParameters
    {
        void SetBool(int parameterHash, bool value);
    }
}
