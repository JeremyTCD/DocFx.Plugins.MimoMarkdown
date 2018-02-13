using Microsoft.DocAsCode.MarkdownLite;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeFileToken : IMarkdownToken
    {
        public IncludeFileToken(IMarkdownRule rule, IMarkdownContext context, IncludeFileOptions options, SourceInfo sourceInfo)
        {
            Rule = rule;
            Context = context;
            SourceInfo = sourceInfo;
            Options = options;
        }

        public IMarkdownRule Rule { get; }

        public IMarkdownContext Context { get; }

        public IncludeFileOptions Options { get; }

        public SourceInfo SourceInfo { get; }
    }
}
