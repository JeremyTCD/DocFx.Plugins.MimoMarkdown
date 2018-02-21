using Newtonsoft.Json;
using System.Collections.Generic;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeMarkdownOptions
    {
        [JsonProperty(Required = Required.Always)]
        public string Src { get; set; }

        public List<Range> Ranges { get; set; } = null;

        public List<Tag> Tags { get; set; } = null;
    }
}
