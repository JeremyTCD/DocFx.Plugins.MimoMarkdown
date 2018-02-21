using Microsoft.DocAsCode.MarkdownLite;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeMarkdownToken : IMarkdownToken
    {
        public IncludeMarkdownToken(IMarkdownRule rule, IMarkdownContext context, IncludeMarkdownOptions options, SourceInfo sourceInfo)
        {
            Rule = rule;
            Context = context;
            SourceInfo = sourceInfo;
            Options = options;
        }

        public IMarkdownRule Rule { get; }

        public IMarkdownContext Context { get; }

        public IncludeMarkdownOptions Options { get; }

        public SourceInfo SourceInfo { get; }
    }
}
