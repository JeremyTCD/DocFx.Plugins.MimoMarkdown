namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class CodeOptions
    {
        public string Language { get; set; } = null;

        public bool ShowLanguage { get; set; } = false;

        public bool ShowLineNumbers { get; set; } = false;

        public bool Highlight { get; set; } = false;

        public string Title { get; set; } = null;
    }
}
