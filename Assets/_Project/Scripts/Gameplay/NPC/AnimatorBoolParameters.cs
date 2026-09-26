using System;
using System.Collections.Generic;
using UnityEngine;

namespace DreamVille.Gameplay.NPC
{
    /// <summary>
    /// Animator adapter that ignores bool parameters the controller does not declare
    /// (PluhNPC has talk/greeting/applause, Townsman_2 has greeting/applause/swim).
    /// </summary>
    public sealed class AnimatorBoolParameters : INpcAnimatorParameters
    {
        private readonly Animator _animator;
        private readonly HashSet<int> _declaredBools = new HashSet<int>();

        public AnimatorBoolParameters(Animator animator)
        {
            _animator = animator != null ? animator : throw new ArgumentNullException(nameof(animator));

            foreach (AnimatorControllerParameter parameter in _animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Bool)
                    _declaredBools.Add(parameter.nameHash);
            }
        }

        /// <summary>Writes the parameter if the controller declares it as a bool; otherwise does nothing.</summary>
        public void SetBool(int parameterHash, bool value)
        {
            if (_declaredBools.Contains(parameterHash))
                _animator.SetBool(parameterHash, value);
        }
    }
}
