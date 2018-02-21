using Newtonsoft.Json;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class Range
    {
        [JsonProperty(Required = Required.Always)]
        public int Start { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int End { get; set; }

        public int DedentLength { get; set; } = -1;
    }
}
