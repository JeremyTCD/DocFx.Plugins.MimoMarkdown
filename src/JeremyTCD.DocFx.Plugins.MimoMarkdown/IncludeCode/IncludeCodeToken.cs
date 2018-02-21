using Microsoft.DocAsCode.MarkdownLite;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeCodeToken : IMarkdownToken
    {
        public IncludeCodeToken(IMarkdownRule rule, IMarkdownContext context, IncludeCodeOptions options, SourceInfo sourceInfo)
        {
            Rule = rule;
            Context = context;
            SourceInfo = sourceInfo;
            Options = options;
        }

        public IMarkdownRule Rule { get; }

        public IMarkdownContext Context { get; }

        public IncludeCodeOptions Options { get; }

        public SourceInfo SourceInfo { get; }
    }
}
