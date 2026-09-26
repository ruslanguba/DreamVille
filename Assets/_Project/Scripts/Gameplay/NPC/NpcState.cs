namespace DreamVille.Gameplay.NPC
{
    /// <summary>Mutually exclusive mood of an NPC. Swimming is an independent pose flag, not a mood.</summary>
    public enum NpcState
    {
        Idle = 0,
        Talk = 1,
        Greeting = 2,
        Applause = 3
    }
}
