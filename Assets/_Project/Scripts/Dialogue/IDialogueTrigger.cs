using System;

namespace DreamVille.Dialogue
{
    /// <summary>
    /// A dialogue zone as seen by level logic: when it runs, and whether it listens yet. Each listener of its
    /// events runs on its own: one that throws is logged and the others still run, so listener order is not
    /// part of the contract.
    /// </summary>
    public interface IDialogueTrigger
    {
        /// <summary>Raised after the dialogue service accepted this trigger's lines.</summary>
        event Action DialogueStarted;

        /// <summary>Raised when the last line of this trigger's dialogue has been dismissed.</summary>
        event Action DialogueEnded;

        /// <summary>A disarmed trigger ignores the player; the object stays active.</summary>
        bool IsArmed { get; set; }
    }
}
