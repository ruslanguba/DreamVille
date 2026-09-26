using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;

namespace DreamVille.Tests.EditMode.NPC
{
    /// <summary>
    /// Pins the fix of the "Pluh animation switching" bug: after applause the NPC must go straight to Talk,
    /// not fall through Idle and not wait for an exit time. The code sets the mood flags in one write, so any
    /// transition into Idle must require every mood flag off, and transitions into Talk must fire at once.
    /// </summary>
    public class PluhNpcControllerTests
    {
        private const string ControllerPath = "Assets/_Project/Art/CitizensModels/Animations/PluhNPC.controller";
        private static readonly string[] MoodStates = { "Applause", "Greetings", "Talk" };

        private AnimatorStateMachine _root;

        [SetUp]
        public void SetUp()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            Assert.That(controller, Is.Not.Null, ControllerPath);
            _root = controller.layers[0].stateMachine;
        }

        [Test]
        public void TransitionsIntoTalk_DoNotWaitForExitTime()
        {
            var intoTalk = _root.states
                .SelectMany(s => s.state.transitions.Select(t => (from: s.state.name, transition: t)))
                .Where(x => x.transition.destinationState != null && x.transition.destinationState.name == "Talk")
                .Where(x => x.transition.conditions.Any(c => c.parameter == "isTalk" && c.mode == AnimatorConditionMode.If))
                .ToList();

            Assert.That(intoTalk, Is.Not.Empty, "no flag-driven transition into Talk found");
            foreach (var (from, transition) in intoTalk)
                Assert.That(transition.hasExitTime, Is.False, $"{from} -> Talk waits for its exit time");
        }

        [Test]
        public void TransitionsIntoIdle_RequireEveryMoodFlagOff()
        {
            var expected = new[]
            {
                ("isTalk", AnimatorConditionMode.IfNot),
                ("isGreeting", AnimatorConditionMode.IfNot),
                ("isApplause", AnimatorConditionMode.IfNot),
            };

            var intoIdle = _root.states
                .Where(s => MoodStates.Contains(s.state.name))
                .SelectMany(s => s.state.transitions.Select(t => (from: s.state.name, transition: t)))
                .Where(x => x.transition.destinationState != null && x.transition.destinationState.name == "Idle")
                .ToList();

            Assert.That(intoIdle.Select(x => x.from), Is.EquivalentTo(MoodStates), "each mood state needs one way back to Idle");
            foreach (var (from, transition) in intoIdle)
            {
                var actual = transition.conditions.Select(c => (c.parameter, c.mode));
                Assert.That(actual, Is.EquivalentTo(expected), $"{from} -> Idle must require every mood flag off");
            }
        }
    }
}
