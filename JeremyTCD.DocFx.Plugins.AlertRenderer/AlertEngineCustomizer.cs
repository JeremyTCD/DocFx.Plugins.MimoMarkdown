using Microsoft.DocAsCode.Dfm;
using System.Collections.Generic;
using System.Composition;
namespace JeremyTCD.DocFx.Plugins.BlockquoteRenderer
{
    [Export(typeof(IDfmEngineCustomizer))]
    public class AlertEngineCustomizer : IDfmEngineCustomizer
    {
        public void Customize(DfmEngineBuilder builder, IReadOnlyDictionary<string, object> parameters)
        {
            builder.BlockRules = builder.BlockRules.Insert(0, new AlertRule());
        }
    }
}
