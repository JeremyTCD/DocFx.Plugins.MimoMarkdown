using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public abstract class ClippingArea
    {
        public int DedentLength { get; set; } = -1;

        [JsonConverter(typeof(StringEnumConverter))]
        public LineBreakOption LineBreak { get; set; } = LineBreakOption.None;
    }
}
