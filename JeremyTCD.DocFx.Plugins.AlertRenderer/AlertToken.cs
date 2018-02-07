using Microsoft.DocAsCode.MarkdownLite;

namespace JeremyTCD.DocFx.Plugins.BlockquoteRenderer
{
    public class AlertToken : IMarkdownToken
    {
        public AlertToken(IMarkdownRule rule, IMarkdownContext context, string alertType, string content, SourceInfo sourceInfo)
        {
            Rule = rule;
            Context = context;
            SourceInfo = sourceInfo;
            AlertType = alertType;
            SourceInfo = sourceInfo;
        }

        public IMarkdownRule Rule { get; }

        public IMarkdownContext Context { get; }

        public string Content { get; }

        public string AlertType { get; }

        public SourceInfo SourceInfo { get; }
    }
}
