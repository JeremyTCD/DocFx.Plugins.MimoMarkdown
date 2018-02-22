using Newtonsoft.Json;
using System.Collections.Generic;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeCodeOptions
    {
        [JsonProperty(Required = Required.Always)]
        public string Src { get; set; }

        public List<Range> Ranges { get; set; } = null;

        public List<Tag> Tags { get; set; } = null;

        public string Language { get; set; } = null;

        public bool ShowLineNumbers { get; set; } = false;

        public bool Highlight { get; set; } = true;

        public string Title { get; set; } = null;

        public string BlockID { get; set; }
    }
}
