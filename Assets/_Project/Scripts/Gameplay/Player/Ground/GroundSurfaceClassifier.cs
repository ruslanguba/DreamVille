using UnityEngine;

using DreamVille.Gameplay.Environment;
using DreamVille.Gameplay.Weapons.Pillow;

namespace DreamVille.Gameplay.Player.Ground
{
    /// <summary>Pure classification of overlap results; EditMode-testable with plain GameObjects.</summary>
    public static class GroundSurfaceClassifier
    {
        public static GroundSurface Classify(Collider[] hits, int count)
        {
            GroundSurface surface = GroundSurface.None;

            for (int i = 0; i < count; i++)
            {
                GameObject hitObject = hits[i].gameObject;

                if (hitObject.TryGetComponent(out Pillow _))
                    surface |= GroundSurface.Pillow;

                if (hitObject.TryGetComponent(out RotationObject _))
                    surface |= GroundSurface.RotatingPlatform;
            }

            return surface;
        }
    }
}
