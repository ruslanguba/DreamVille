using UnityEngine;
using Zenject;

using DreamVille.Core.Players;

namespace DreamVille.Gameplay.Player
{
    /// <summary>
    /// Root of the player prefab and the player marker for non-gameplay code.
    /// The player's systems are composed by <see cref="PlayerInstaller"/> in the sibling <see cref="GameObjectContext"/>.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(GameObjectContext))]
    public sealed class PlayerCharacter : MonoBehaviour, IPlayerMarker
    {
        public Transform Transform => transform;
    }
}
