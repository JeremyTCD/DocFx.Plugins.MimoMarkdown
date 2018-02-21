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
            builder.BlockRules = builder.BlockRules.Insert(0, new IncludeCodeRule());
            builder.BlockRules = builder.BlockRules.Insert(0, new IncludeMarkdownRule());
            builder.BlockRules = builder.BlockRules.Insert(0, new IncludeImageRule());
            builder.BlockRules = builder.BlockRules.Insert(0, new AlertRule());

            IMarkdownRule blockquoteRule = builder.BlockRules.Find(r => r is MarkdownBlockquoteBlockRule);
            builder.BlockRules = builder.BlockRules.Replace(blockquoteRule, new CustomMarkdownBlockquoteBlockRule());
        }
    }
}
