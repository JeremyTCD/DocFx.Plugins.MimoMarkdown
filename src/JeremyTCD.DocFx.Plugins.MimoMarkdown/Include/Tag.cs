using Newtonsoft.Json;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class Tag
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        public int DedentLength { get; set; } = -1;
    }
}
