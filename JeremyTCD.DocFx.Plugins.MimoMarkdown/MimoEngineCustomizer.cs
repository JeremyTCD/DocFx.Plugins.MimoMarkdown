using Microsoft.DocAsCode.Dfm;
using System.Collections.Generic;
using System.Composition;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    [Export(typeof(IDfmEngineCustomizer))]
    public class MimoEngineCustomizer : IDfmEngineCustomizer
    {
        public void Customize(DfmEngineBuilder builder, IReadOnlyDictionary<string, object> parameters)
        {
            builder.BlockRules = builder.BlockRules.Insert(0, new AlertRule());
        }
    }
}
