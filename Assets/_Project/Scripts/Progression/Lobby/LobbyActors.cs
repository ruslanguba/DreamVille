using System.Collections.Generic;
using UnityEngine;

using DreamVille.Dialogue;
using DreamVille.Gameplay.Damage;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Environment;
using DreamVille.Gameplay.NPC;

namespace DreamVille.Progression.Lobby
{
    /// <summary>
    /// The lobby's cast: every cross-reference of the scenario, visible in one Inspector.
    /// Fields are typed to concrete components because Unity cannot serialize interfaces;
    /// the <see cref="ILobbyCast"/> properties are the only way out.
    /// </summary>
    public sealed class LobbyActors : MonoBehaviour, ILobbyCast
    {
        [SerializeField] private EnemyHealth[] _firstGroup;
        [SerializeField] private EnemyHealth[] _secondGroup;
        [SerializeField] private DialogueTrigger _firstDialogue;
        [SerializeField] private DialogueTrigger _secondDialogue;
        [SerializeField] private Door _firstDoor;
        [SerializeField] private Door _secondDoor;
        [SerializeField] private NPCReactionController _guide;

        public IReadOnlyList<IMortal> FirstGroup => _firstGroup;
        public IReadOnlyList<IMortal> SecondGroup => _secondGroup;

        // Unity's fake-null would become a non-null interface reference; normalise it to a true null.
        public IDialogueTrigger FirstDialogue => _firstDialogue != null ? _firstDialogue : null;
        public IDialogueTrigger SecondDialogue => _secondDialogue != null ? _secondDialogue : null;
        public IDoor FirstDoor => _firstDoor != null ? _firstDoor : null;
        public IDoor SecondDoor => _secondDoor != null ? _secondDoor : null;
        public INpcReactions Guide => _guide != null ? _guide : null;

        /// <summary>
        /// Names of unassigned fields, empty groups and null group entries (Unity null check, so a
        /// "Missing" reference counts). Empty when the cast is complete.
        /// </summary>
        public IReadOnlyList<string> FindMissingReferences()
        {
            var missing = new List<string>();

            AddMissingGroup(missing, nameof(_firstGroup), _firstGroup);
            AddMissingGroup(missing, nameof(_secondGroup), _secondGroup);
            AddIfMissing(missing, nameof(_firstDialogue), _firstDialogue);
            AddIfMissing(missing, nameof(_secondDialogue), _secondDialogue);
            AddIfMissing(missing, nameof(_firstDoor), _firstDoor);
            AddIfMissing(missing, nameof(_secondDoor), _secondDoor);
            AddIfMissing(missing, nameof(_guide), _guide);

            return missing;
        }

        private static void AddMissingGroup(List<string> missing, string fieldName, EnemyHealth[] group)
        {
            if (group == null || group.Length == 0)
            {
                missing.Add(fieldName);
                return;
            }

            for (int i = 0; i < group.Length; i++)
                AddIfMissing(missing, $"{fieldName}[{i}]", group[i]);
        }

        private static void AddIfMissing(List<string> missing, string fieldName, Object reference)
        {
            if (reference == null)
                missing.Add(fieldName);
        }
    }
}
