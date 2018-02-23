using Newtonsoft.Json;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class Region : ClippingArea
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }
    }
}
