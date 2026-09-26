using System;

using DreamVille.Gameplay.Weapons.Pillow;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Pillow state set by the test; <see cref="Changed"/> is raised only on request.</summary>
    public sealed class FakePillowThrower : IPillowThrower
    {
        public event Action Changed;

        public int Available { get; set; }
        public int InHand { get; set; }

        public void RaiseChanged()
        {
            Changed?.Invoke();
        }
    }
}
