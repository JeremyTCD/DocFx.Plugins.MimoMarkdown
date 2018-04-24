using Newtonsoft.Json;
using System.Collections.Generic;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public abstract class IncludeFileOptions : ClippingArea
    {
        [JsonProperty(Required = Required.Always)]
        public string Src { get; set; }

        public List<Range> Ranges { get; set; } = null;

        public List<Region> Regions { get; set; } = null;
    }
}
