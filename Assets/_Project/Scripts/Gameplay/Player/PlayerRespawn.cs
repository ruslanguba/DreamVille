using UnityEngine;
using Zenject;

using DreamVille.Configs;

namespace DreamVille.Gameplay.Player
{
    /// <summary>
    /// Teleports the player back above the last safe position. Lives on the child whose trigger collider
    /// a <see cref="Environment.DeathTrigger"/> touches.
    /// </summary>
    public sealed class PlayerRespawn : MonoBehaviour
    {
        private SafePositionTracker _tracker;
        private PlayerConfig _config;
        private CharacterController _controller;
        private Transform _root;

        [Inject]
        private void Construct(SafePositionTracker tracker, PlayerConfig config, CharacterController controller, Transform root)
        {
            _tracker = tracker;
            _config = config;
            _controller = controller;
            _root = root;
        }

        private void Start()
        {
            _tracker.Reset(_root.position);
        }

        public void ReturnToSafePosition()
        {
            // A CharacterController overrides transform writes while enabled.
            _controller.enabled = false;
            _root.position = _tracker.LastSafePosition + Vector3.up * _config.RespawnHeightOffset;
            _controller.enabled = true;
        }
    }
}
