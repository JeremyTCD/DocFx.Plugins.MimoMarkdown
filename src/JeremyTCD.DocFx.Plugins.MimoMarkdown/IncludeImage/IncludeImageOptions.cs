using Newtonsoft.Json;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeImageOptions
    {
        [JsonProperty(Required = Required.Always)]
        public string Src { get; set; }

        public string Title { get; set; }

        public string Alt { get; set; }

        public string FooterContent { get; set; }

        public string Description { get; set; }

        public string Credits { get; set; }

        public string BlockID { get; set; }
    }
}
