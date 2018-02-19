using Microsoft.DocAsCode.MarkdownLite;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeImageToken : IMarkdownToken
    {
        public IncludeImageToken(IMarkdownRule rule, IMarkdownContext context, IncludeImageOptions options, SourceInfo sourceInfo)
        {
            Rule = rule;
            Context = context;
            SourceInfo = sourceInfo;
            Options = options;
        }

        public IMarkdownRule Rule { get; }

        public IMarkdownContext Context { get; }

        public IncludeImageOptions Options { get; }

        public SourceInfo SourceInfo { get; }
    }
}
