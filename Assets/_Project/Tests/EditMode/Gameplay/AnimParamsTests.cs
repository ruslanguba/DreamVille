using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

using DreamVille.Gameplay;

namespace DreamVille.Tests.EditMode.Gameplay
{
    /// <summary>
    /// Pins <see cref="AnimParams"/> to the shipped animator controllers: a hash that names no parameter, or one
    /// of another type, makes the driver's <c>SetBool</c> or <c>SetTrigger</c> a silent no-op at runtime, so a
    /// rename on either side becomes a red test here. Each row is a controller and a parameter its consumer drives.
    /// </summary>
    public class AnimParamsTests
    {
        private const string Character = "Assets/_Project/Animations/Ruslan/Character.controller";
        private const string PluhNpc = "Assets/_Project/Art/CitizensModels/Animations/PluhNPC.controller";
        private const string Hatter = "Assets/_Project/Art/CitizensModels/Animations/Hatter.controller";
        private const string Townsman = "Assets/_Project/Art/CitizensModels/Animations/Townsman.controller";
        private const string TownsmanGreeting = "Assets/_Project/Art/CitizensModels/Animations/Townsman_2_greetings_only.controller";
        private const string FinalLevel = "Assets/_Project/Animations/SandWattch/FinalLevel.controller";
        private const string Shadow = "Assets/_Project/Art/Max/tenka/Animations/Shedow.controller";
        private const string Rotor = "Assets/_Project/Characters/Enemies/vertushek/Animation/vertusher_03(RIG).controller";
        private const string Gates = "Assets/_Project/Animations/Max/gates.controller";
        private const string Box = "Assets/_Project/Animations/Ruslan/Box.controller";
        private const string Closet = "Assets/_Project/Animations/closet/closet.controller";
        private const string Fridge = "Assets/_Project/Animations/fridge/fridge.controller";
        private const string FridgeAnimated = "Assets/_Project/Animations/Pavel/Fridge/Fridge.controller";

        private static readonly object[][] Rows =
        {
            new object[] { Character, nameof(AnimParams.Speed), AnimatorControllerParameterType.Float },
            new object[] { Character, nameof(AnimParams.IsMoving), AnimatorControllerParameterType.Bool },
            new object[] { Character, nameof(AnimParams.MoveDirection), AnimatorControllerParameterType.Int },
            new object[] { Character, nameof(AnimParams.AnimSpeedMultiplier), AnimatorControllerParameterType.Float },
            new object[] { Character, nameof(AnimParams.IsJump), AnimatorControllerParameterType.Trigger },

            new object[] { PluhNpc, nameof(AnimParams.Jump), AnimatorControllerParameterType.Trigger },
            new object[] { PluhNpc, nameof(AnimParams.IsMoving), AnimatorControllerParameterType.Bool },
            new object[] { PluhNpc, nameof(AnimParams.IsTalk), AnimatorControllerParameterType.Bool },
            new object[] { PluhNpc, nameof(AnimParams.IsGreeting), AnimatorControllerParameterType.Bool },
            new object[] { PluhNpc, nameof(AnimParams.IsApplause), AnimatorControllerParameterType.Bool },

            new object[] { Hatter, nameof(AnimParams.IsTalk), AnimatorControllerParameterType.Bool },
            new object[] { Townsman, nameof(AnimParams.IsTalk), AnimatorControllerParameterType.Bool },

            new object[] { TownsmanGreeting, nameof(AnimParams.IsGreeting), AnimatorControllerParameterType.Bool },
            new object[] { TownsmanGreeting, nameof(AnimParams.IsApplause), AnimatorControllerParameterType.Bool },
            new object[] { TownsmanGreeting, nameof(AnimParams.IsSwim), AnimatorControllerParameterType.Bool },

            new object[] { FinalLevel, nameof(AnimParams.Start), AnimatorControllerParameterType.Trigger },

            new object[] { Shadow, nameof(AnimParams.Dead), AnimatorControllerParameterType.Trigger },
            new object[] { Rotor, nameof(AnimParams.Dead), AnimatorControllerParameterType.Trigger },
            new object[] { Rotor, nameof(AnimParams.IsMoving), AnimatorControllerParameterType.Bool },

            new object[] { Gates, nameof(AnimParams.Open), AnimatorControllerParameterType.Trigger },
            new object[] { Box, nameof(AnimParams.Open), AnimatorControllerParameterType.Trigger },

            new object[] { Closet, nameof(AnimParams.IsOpen), AnimatorControllerParameterType.Bool },
            new object[] { Closet, nameof(AnimParams.IsEnemy), AnimatorControllerParameterType.Bool },
            new object[] { Fridge, nameof(AnimParams.IsOpen), AnimatorControllerParameterType.Bool },
            new object[] { Fridge, nameof(AnimParams.IsEnemy), AnimatorControllerParameterType.Bool },
            new object[] { FridgeAnimated, nameof(AnimParams.IsOpen), AnimatorControllerParameterType.Bool },
            new object[] { FridgeAnimated, nameof(AnimParams.IsEnemy), AnimatorControllerParameterType.Bool },
        };

        [TestCaseSource(nameof(Rows))]
        public void EveryHash_NamesAParameterOfItsTypeInEveryControllerThatUsesIt(
            string controllerPath, string hashName, AnimatorControllerParameterType expectedType)
        {
            var controller = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            Assert.That(controller, Is.Not.Null, $"{controllerPath} is missing.");
            FieldInfo field = typeof(AnimParams).GetField(hashName, BindingFlags.Public | BindingFlags.Static);
            Assert.That(field, Is.Not.Null, $"AnimParams.{hashName} does not exist.");
            var hash = (int)field.GetValue(null);

            AnimatorControllerParameter parameter = controller.parameters.FirstOrDefault(candidate => candidate.nameHash == hash);

            Assert.That(parameter, Is.Not.Null, $"{controllerPath} has no parameter behind AnimParams.{hashName}.");
            Assert.That(parameter.type, Is.EqualTo(expectedType), $"{controllerPath}: parameter '{parameter.name}' has the wrong type.");
        }
    }
}
