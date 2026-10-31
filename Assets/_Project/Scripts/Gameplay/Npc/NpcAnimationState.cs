using System;

namespace DreamVille.Gameplay.Npc
{
    /// <summary>
    /// Writes an NPC's mood as one operation (exactly one of isTalk/isGreeting/isApplause true, none for Idle)
    /// and owns the authored start pose: it is applied once at Start unless a state was requested earlier,
    /// in which case the request wins, so directors may react before the NPC's Start.
    /// </summary>
    public sealed class NpcAnimationState
    {
        private readonly INpcAnimatorParameters _parameters;

        public NpcAnimationState(INpcAnimatorParameters parameters)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        /// <summary>Last written mood; null until the start pose or a request has been applied.</summary>
        public NpcState? Current { get; private set; }

        /// <summary>Writes the three mood flags for <paramref name="state"/>; the swim flag is left untouched.</summary>
        public void Set(NpcState state)
        {
            _parameters.SetBool(AnimParams.IsTalk, state == NpcState.Talk);
            _parameters.SetBool(AnimParams.IsGreeting, state == NpcState.Greeting);
            _parameters.SetBool(AnimParams.IsApplause, state == NpcState.Applause);
            Current = state;
        }

        /// <summary>
        /// Always writes the swim flag; writes <paramref name="startState"/> only if no state has been set yet.
        /// </summary>
        public void ApplyStartPose(NpcState startState, bool swim)
        {
            _parameters.SetBool(AnimParams.IsSwim, swim);

            if (Current == null)
                Set(startState);
        }
    }
}
