using System.Collections.Generic;
using System.Linq;

namespace DreamVille.Tests.EditMode.UI
{
    /// <summary>One <c>ProximityHint</c> as it must read in a scene: where it is, what it shows and for how long.</summary>
    internal sealed class HintRow
    {
        public HintRow(string scene, string triggerPath, string targetPath, float showSeconds, float fadeSeconds, bool showOnce, string text)
        {
            Scene = scene;
            TriggerPath = triggerPath;
            TargetPath = targetPath;
            ShowSeconds = showSeconds;
            FadeSeconds = fadeSeconds;
            ShowOnce = showOnce;
            Text = text;
        }

        /// <summary>Scene name without folder and extension.</summary>
        public string Scene { get; }

        /// <summary>Hierarchy path of the trigger (the object that carries the hint) from the scene root.</summary>
        public string TriggerPath { get; }

        /// <summary>Hierarchy path of the UI object the hint shows, from the scene root.</summary>
        public string TargetPath { get; }

        public float ShowSeconds { get; }

        public float FadeSeconds { get; }

        public bool ShowOnce { get; }

        /// <summary>Every <c>TMP_Text.text</c> under the target joined with " | ", real line breaks kept.</summary>
        public string Text { get; }
    }

    /// <summary>
    /// The resolved settings of every live <c>ProximityHint</c> (spec 12 §2.2, measured in batch B0): scene
    /// overrides on top of the prefab, including the values that only exist as C# initializers.
    /// A changed trigger, target, duration or text must be changed here on purpose.
    /// </summary>
    internal static class ProximityHintSnapshot
    {
        public static readonly IReadOnlyList<HintRow> Rows = new[]
        {
            new HintRow("Lobby_01", "hints/TriggerForText", "Canvas/Image", 3f, 1f, false,
                "WASD - передвижение"),
            new HintRow("Lobby_01", "hints/TriggerForText (1)", "Canvas/Image (3)", 3f, 1f, false,
                "Бах! Бабах! \nХэ-хэ!"),
            new HintRow("Lobby_01", "hints/TriggerForText (2)", "Canvas/Image (2)", 3f, 1f, false,
                "Я смогу показать себя и стать шерифом!"),
            new HintRow("Lobby_01", "hints/TriggerForText (3)", "Canvas/Image (1)", 3f, 1f, false,
                "ПРОБЕЛ - прыжок"),
            new HintRow("Lobby_01", "hints/TriggerForText (4)", "Canvas/Image (4)", 3f, 1f, false,
                "А что, если мне нужно стрельнуть вверх?"),

            new HintRow("Lobby_02", "TriggerForText", "Canvas/Image", 2f, 6f, false,
                "Не забывай возвращать подушку клавишей F"),
            new HintRow("Level_04", "TriggerForText", "Canvas/Image", 2f, 6f, false,
                "Забыл куда нажимать - открой инструкцию нажми на TAB"),

            new HintRow("Level_02", "TriggerForText", "AllLevel_1/Image", 2f, 2f, false,
                "А что, если пульнуть в ракету?\nОна полетит?"),
            new HintRow("Level_02", "TriggerForText (1)", "AllLevel_1/Image (1)", 2f, 2f, false,
                "Покрути колесиком мыши - осмотрись."),
            new HintRow("Level_02", "TriggerForText (2)", "AllLevel_1/Image (2)", 2f, 1f, true,
                "Подушечка! Она мне пригодится!"),

            new HintRow("Final Level 1_Max", "TriggerForText", "AllLevel_1/Image", 2f, 6f, false,
                "Нажми TAB - открыть инструкцию"),
            new HintRow("Final Level 1_Max", "TriggerForText (1)", "AllLevel_1/Image (1)", 2f, 6f, false,
                "Покрути колесиком мыши - посмотри что вокруг"),
            new HintRow("Level_2_Max", "TriggerForText", "Canvas/Image", 2f, 6f, false,
                "Забыл куда нажимать - открой инструкцию нажми на TAB"),
            new HintRow("Lobby_2_Max", "TriggerForText", "Canvas/Image", 2f, 6f, false,
                "Не забывай возвращать подушки обратно - нажми F"),
        };

        /// <summary>NUnit case data: one <c>(scene, triggerPath)</c> pair per row.</summary>
        public static IEnumerable<object[]> Cases()
            => Rows.Select(row => new object[] { row.Scene, row.TriggerPath }).ToArray();

        public static HintRow RowOf(string scene, string triggerPath)
            => Rows.Single(row => row.Scene == scene && row.TriggerPath == triggerPath);
    }
}
