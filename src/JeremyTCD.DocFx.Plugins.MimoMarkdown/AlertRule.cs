using Microsoft.DocAsCode.MarkdownLite;
using System.Text.RegularExpressions;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class AlertRule : IMarkdownRule
    {
        public virtual string Name => "Alert";

        private static readonly Regex _regex = new Regex(@"\[!(alert-(?:note|warning|critical-warning))\]\s*?\n", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public virtual IMarkdownToken TryMatch(IMarkdownParser parser, IMarkdownParsingContext context)
        {
            if (!parser.Context.Variables.ContainsKey(MarkdownBlockContext.IsBlockQuote) || !(bool)parser.Context.Variables[MarkdownBlockContext.IsBlockQuote])
            {
                return null;
            }

            Match match = _regex.Match(context.CurrentMarkdown);
            if (match.Length > 0)
            {
                SourceInfo sourceInfo = context.Consume(match.Length);
                return new AlertToken(this, parser.Context, match.Groups[1].Value, sourceInfo.Markdown, sourceInfo);
            }
            return null;
        }
    }
}
