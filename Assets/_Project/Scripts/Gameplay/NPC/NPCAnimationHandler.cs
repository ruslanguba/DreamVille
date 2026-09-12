using System.Collections.Generic;
using UnityEngine;

namespace DreamVille.Gameplay.NPC
{
    /// <summary>Drives an NPC's bool animator parameters for talk/greeting/applause/swim/idle states.</summary>
    public class NPCAnimationHandler : MonoBehaviour
    {
        private Animator _animator;
        [SerializeField] private bool _isTalk;
        [SerializeField] private bool _isGreeting;
        [SerializeField] private bool _isApplause;
        [SerializeField] private bool _isSwim;
        [SerializeField] private bool _isIdle;

        private HashSet<string> _boolParameters;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _boolParameters = new HashSet<string>();

            if (_animator == null)
            {
                Debug.LogError($"[{nameof(NPCAnimationHandler)}] '{name}' has no Animator in its children.", this);
                return;
            }

            foreach (AnimatorControllerParameter parameter in _animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Bool)
                    _boolParameters.Add(parameter.name);
            }
        }

        private void Start()
        {
            SetStartAnim();
        }

        public void SetIdleAnim()
        {
            SetBool("isTalk", false);
            SetBool("isGreeting", false);
            SetBool("isApplause", false);
        }

        public void SetStartAnim()
        {
            SetBool("isTalk", _isTalk);
            SetBool("isGreeting", _isGreeting);
            SetBool("isApplause", _isApplause);
            SetBool("isSwim", _isSwim);
        }

        public void SetisApplauseAnim()
        {
            SetBool("isApplause", true);
            SetBool("isTalk", false);
            SetBool("isGreeting", false);
        }

        public void SetGreetingAnim()
        {
            SetBool("isGreeting", true);
            SetBool("isTalk", false);
            SetBool("isApplause", false);
        }

        public void SetTalkAnim()
        {
            SetBool("isTalk", true);
            SetBool("isGreeting", false);
            SetBool("isApplause", false);
        }

        // Not every NPC animator controller declares the same set of parameters
        // (e.g. isSwim), so unknown parameter names are silently ignored.
        private void SetBool(string name, bool value)
        {
            if (_animator == null || !_boolParameters.Contains(name))
                return;

            _animator.SetBool(name, value);
        }
    }
}
