using Newtonsoft.Json;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class Tag : ClippingArea
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }
    }
}
