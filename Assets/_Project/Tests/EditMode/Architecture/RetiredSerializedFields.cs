using System;

using DreamVille.Gameplay.Environment;
using DreamVille.Gameplay.Npc;
using DreamVille.UI;

namespace DreamVille.Tests.EditMode.Architecture
{
    /// <summary>
    /// Ledger of every serialized field the project has renamed: the owner type and the names its data used to be
    /// stored under. <see cref="SerializedDataHygieneTests"/> proves that no asset still carries data under one of these
    /// names and that no field takes one of them again. A rename batch adds its entry in the same commit as the data.
    /// </summary>
    internal static class RetiredSerializedFields
    {
        public static readonly (Type Owner, string[] OldNames)[] All =
        {
            (typeof(GameStartUI), new[] { "firstImage", "collageImages", "fadeTime", "appearScale", "firstScale" }),
            (typeof(NpcCanvasToggle), new[] { "Canvas" }),

            // _isInterracted was runtime state stored as data; it became the non-serialized _isFloating.
            (typeof(PropsMover), new[] { "_baloon", "_isInterracted" }),
        };
    }
}
