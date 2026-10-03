using System;
using System.Collections.Generic;
using UnityEngine;

namespace DreamVille.Dialogue
{
    /// <summary>An ordered conversation. Text lives in the "Dialogues" string table; this asset holds order and speakers.</summary>
    [CreateAssetMenu(menuName = "DreamVille/Dialogue/Dialogue Definition", fileName = "Dialogue")]
    public sealed class DialogueDefinition : ScriptableObject
    {
        [SerializeField] private DialogueLine[] _lines = Array.Empty<DialogueLine>();

        public IReadOnlyList<DialogueLine> Lines => _lines;

#if UNITY_INCLUDE_TESTS
        /// <summary>Test seam (InternalsVisibleTo DreamVille.Tests.EditMode); runtime code never mutates the asset.</summary>
        internal static DialogueDefinition Create(params DialogueLine[] lines)
        {
            var definition = CreateInstance<DialogueDefinition>();
            definition._lines = lines;
            return definition;
        }
#endif
    }
}
