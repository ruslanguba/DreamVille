using UnityEngine;

namespace DreamVille.Gameplay.Npc
{
    /// <summary>
    /// Scene-facing shell of <see cref="NpcAnimationState"/>: finds the NPC's Animator and applies the authored pose.
    /// </summary>
    public sealed class NpcAnimationHandler : MonoBehaviour
    {
        [SerializeField, Tooltip("Mood the NPC shows from its first frame, unless a reaction was requested before Start.")]
        private NpcState _startState;

        [SerializeField, Tooltip("Swimming pose; independent of the mood. Ignored by controllers without an isSwim bool.")]
        private bool _isSwim;

        private NpcAnimationState _state;

        /// <summary>Authored start mood; read by content tests.</summary>
        public NpcState StartState => _startState;

        private void Awake()
        {
            Animator animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError($"[{nameof(NpcAnimationHandler)}] '{name}' has no Animator in its children.", this);
                enabled = false;
                return;
            }

            _state = new NpcAnimationState(new AnimatorBoolParameters(animator));
        }

        private void Start()
        {
            _state?.ApplyStartPose(_startState, _isSwim);
        }

        /// <summary>Switches the NPC's mood. A request made before Start wins over the authored start pose.</summary>
        public void SetState(NpcState state)
        {
            // Null only when Awake found no Animator; that was already reported once.
            _state?.Set(state);
        }
    }
}
