namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeCodeOptions : IncludeFileOptions
    {
        public string Language { get; set; } = null;

        public bool ShowLineNumbers { get; set; } = false;

        public bool Highlight { get; set; } = true;

        public string Title { get; set; } = null;

        public string BlockID { get; set; }
    }
}
