namespace DreamVille.Dialogue
{
    /// <summary>A line as shown on screen: the speaker and the text already resolved for the current locale.</summary>
    public readonly struct SpokenLine
    {
        public SpokenLine(DialogueSpeaker speaker, string text)
        {
            Speaker = speaker;
            Text = text;
        }

        public DialogueSpeaker Speaker { get; }

        public string Text { get; }
    }
}
