using System;
using System.Collections.Generic;

using DreamVille.Gameplay.Damage;

namespace DreamVille.Progression.Objectives
{
    /// <summary>
    /// Complete once every member of a fixed group is dead. Members already dead when the objective starts
    /// count as killed; a member listed twice counts once; completion is raised exactly once.
    /// </summary>
    public sealed class KillGroupObjective : IDisposable
    {
        private readonly HashSet<IMortal> _members;
        private readonly HashSet<IMortal> _alive = new HashSet<IMortal>();
        private bool _started;

        /// <exception cref="ArgumentNullException"><paramref name="members"/> is null.</exception>
        /// <exception cref="ArgumentException">The group is empty or contains a null member.</exception>
        public KillGroupObjective(IReadOnlyList<IMortal> members)
        {
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            if (members.Count == 0)
                throw new ArgumentException("A kill group needs at least one member.", nameof(members));

            _members = new HashSet<IMortal>();
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] == null)
                    throw new ArgumentException($"Member at index {i} is null.", nameof(members));

                _members.Add(members[i]);
            }
        }

        /// <summary>
        /// Raised once when the last living member dies. Raised synchronously inside <see cref="Start"/>
        /// for a group that is already cleared, so subscribe before starting.
        /// </summary>
        public event Action Completed;

        public bool IsComplete { get; private set; }

        /// <summary>Distinct members still alive. Before <see cref="Start"/> every member counts.</summary>
        public int Remaining => _started ? _alive.Count : _members.Count;

        /// <summary>Subscribes to the living members; completes immediately if none is left. Throws if called twice.</summary>
        public void Start()
        {
            if (_started)
                throw new InvalidOperationException($"[{nameof(KillGroupObjective)}] Start was already called.");

            _started = true;

            foreach (IMortal member in _members)
            {
                if (member.IsDead)
                    continue;

                _alive.Add(member);
                member.Died += OnMemberDied;
            }

            if (_alive.Count == 0)
                Complete();
        }

        /// <summary>Unsubscribes from every member still alive; later deaths are ignored. Idempotent.</summary>
        public void Dispose()
        {
            // Removing a handler that is not attached is a no-op, so a second call is harmless.
            foreach (IMortal member in _alive)
                member.Died -= OnMemberDied;
        }

        private void OnMemberDied(IMortal member)
        {
            member.Died -= OnMemberDied;

            if (!_alive.Remove(member))
                return;

            if (_alive.Count == 0)
                Complete();
        }

        private void Complete()
        {
            if (IsComplete)
                return;

            IsComplete = true;
            Completed?.Invoke();
        }
    }
}
