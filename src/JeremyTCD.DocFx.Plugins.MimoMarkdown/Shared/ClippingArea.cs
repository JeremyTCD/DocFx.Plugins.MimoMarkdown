namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public abstract class ClippingArea
    {
        public int DedentLength { get; set; } = -1;

        public int CollapseLength { get; set; } = -1;

        public string Before { get; set; } = null;

        public string After { get; set; } = null;
    }
}
