using Newtonsoft.Json;
using System.Collections.Generic;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeFileOptions
    {
        [JsonProperty(Required = Required.Always)]
        public string Src { get; set; }

        public List<Range> Ranges { get; set; } = null;

        public List<Tag> Tags { get; set; } = null;

        public bool IsCode { get; set; } = false;

        public CodeOptions CodeOptions { get; set; } = null;
    }
}
