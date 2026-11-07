using NUnit.Framework;
using UnityEditor;

using DreamVille.Configs;

namespace DreamVille.Tests.EditMode.Scoring
{
    /// <summary>Content guard: the star meter divides by the gap between two thresholds, so every shipped rule must have a positive one.</summary>
    public class StarRulesContentTests
    {
        [Test]
        public void EveryStarRules_HasPositiveAscendingThresholds()
        {
            string[] guids = AssetDatabase.FindAssets("t:StarRules", new[] { "Assets/_Project" });
            Assert.That(guids, Is.Not.Empty, "No StarRules asset found under Assets/_Project.");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var rules = AssetDatabase.LoadAssetAtPath<StarRules>(path);
                Assert.That(rules.Thresholds, Is.Not.Null, $"{path} has no thresholds array.");

                float previous = 0f;
                for (int i = 0; i < rules.Thresholds.Count; i++)
                {
                    Assert.That(rules.Thresholds[i], Is.GreaterThan(previous), $"{path}: threshold #{i} must be positive and above the one before it.");
                    previous = rules.Thresholds[i];
                }
            }
        }
    }
}
