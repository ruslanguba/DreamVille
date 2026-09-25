using System.Collections.Generic;

using DreamVille.Gameplay.Interactables;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Plain <see cref="IInteractionEffect"/> double: appends its id to a shared log (to check order across effects)
    /// and remembers the last context it received.
    /// </summary>
    public sealed class RecordingEffect : IInteractionEffect
    {
        private readonly int _id;
        private readonly List<int> _log;

        public RecordingEffect(int id, List<int> log)
        {
            _id = id;
            _log = log;
        }

        public int CallCount { get; private set; }

        public InteractionContext LastContext { get; private set; }

        public void Apply(in InteractionContext context)
        {
            CallCount++;
            LastContext = context;
            _log.Add(_id);
        }
    }
}
