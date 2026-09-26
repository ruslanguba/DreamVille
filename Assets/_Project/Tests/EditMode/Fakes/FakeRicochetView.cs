using System.Collections.Generic;
using UnityEngine;

using DreamVille.UI.Hud;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>A badge row of configurable size that records every badge shown.</summary>
    public sealed class FakeRicochetView : IRicochetView
    {
        private readonly List<(int BadgeIndex, Vector3 WorldPoint)> _shown = new List<(int, Vector3)>();

        public FakeRicochetView(int badgeCount)
        {
            BadgeCount = badgeCount;
        }

        public int BadgeCount { get; }

        public IReadOnlyList<(int BadgeIndex, Vector3 WorldPoint)> Shown => _shown;

        public void ShowBadge(int badgeIndex, Vector3 worldPoint)
        {
            _shown.Add((badgeIndex, worldPoint));
        }
    }
}
