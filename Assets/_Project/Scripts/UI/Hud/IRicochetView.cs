using UnityEngine;

namespace DreamVille.UI.Hud
{
    /// <summary>Short-lived "xN" badges drawn over a world position.</summary>
    public interface IRicochetView
    {
        /// <summary>Number of badges the view has; badge 0 is the first one.</summary>
        int BadgeCount { get; }

        /// <summary>Shows one badge over <paramref name="worldPoint"/>; showing it again restarts its timer.</summary>
        void ShowBadge(int badgeIndex, Vector3 worldPoint);
    }
}
