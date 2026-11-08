using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay;

namespace DreamVille.Tests.EditMode.Gameplay
{
    /// <summary>
    /// Pins <see cref="Layers"/> to ProjectSettings/TagManager.asset: a renamed layer makes
    /// <c>LayerMask.NameToLayer</c> return -1 and every mask built from it silently match nothing.
    /// </summary>
    public class LayersTests
    {
        [TestCase(Layers.Player)]
        [TestCase(Layers.PlayerBullet)]
        public void Names_ExistInTagManager(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);

            Assert.That(layer, Is.GreaterThanOrEqualTo(0), $"Layer '{layerName}' is not defined in the project.");
        }
    }
}
