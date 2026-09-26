using System;
using System.Collections.Generic;

namespace DreamVille.Gameplay.NPC
{
    /// <summary>Fixed-membership queue whose front can be moved to the back; the NPC queue's order rule.</summary>
    public sealed class RotatingQueue<T>
    {
        private readonly List<T> _members;

        /// <summary>Copies <paramref name="members"/>; later changes to the source do not affect the queue.</summary>
        public RotatingQueue(IEnumerable<T> members)
        {
            if (members == null)
                throw new ArgumentNullException(nameof(members));

            _members = new List<T>(members);

            if (_members.Count == 0)
                throw new ArgumentException("A rotating queue needs at least one member.", nameof(members));
        }

        public int Count => _members.Count;

        public T Front => _members[0];

        /// <summary>Member at queue position <paramref name="index"/>; 0 is the front.</summary>
        public T this[int index] => _members[index];

        /// <summary>Moves the front member to the back; everybody else moves one position forward.</summary>
        public void RotateFrontToBack()
        {
            T front = _members[0];
            _members.RemoveAt(0);
            _members.Add(front);
        }
    }
}
