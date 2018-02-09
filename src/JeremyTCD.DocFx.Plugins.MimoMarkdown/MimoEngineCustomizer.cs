using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;
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

            int blockquoteRuleIndex = builder.BlockRules.FindIndex(r => r is MarkdownBlockquoteBlockRule);
            builder.BlockRules = builder.BlockRules.Insert(blockquoteRuleIndex, new CustomMarkdownBlockquoteBlockRule());
        }
    }
}
