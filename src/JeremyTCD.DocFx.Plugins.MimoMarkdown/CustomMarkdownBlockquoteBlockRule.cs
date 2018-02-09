using Microsoft.DocAsCode.MarkdownLite;
using System;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    /// <summary>
    /// Custom rule to get around consecutive blockquotes not working: https://github.com/dotnet/docfx/issues/2424
    /// </summary>
    public class CustomMarkdownBlockquoteBlockRule : IMarkdownRule
    {
        public virtual string Name => "CustomBlockquote";

        public virtual Regex Blockquote => new Regex(@"^( *>[^\n]+(\n(?! *\[([^\]]+)\]: *<?([^\s>]+)>?(?: +[""(]([^\n]+)["")])? *(?:\n+|$))[^\n]+)*\n)+\n*", RegexOptions.Compiled, TimeSpan.FromSeconds(10));

        public virtual Regex LeadingBlockquote => new Regex(@"^ *> ?", RegexOptions.Multiline | RegexOptions.Compiled, TimeSpan.FromSeconds(10));

        public virtual IMarkdownToken TryMatch(IMarkdownParser parser, IMarkdownParsingContext context)
        {
            Match match = Blockquote.Match(context.CurrentMarkdown);
            if (match.Length == 0)
            {
                return null;
            }
            SourceInfo sourceInfo = context.Consume(match.Length);
            string capStr = LeadingBlockquote.Replace(sourceInfo.Markdown, string.Empty);
            ImmutableArray<IMarkdownToken> blockTokens = parser.Tokenize(sourceInfo.Copy(capStr));
            blockTokens = TokenHelper.CreateParagraghs(parser, this, blockTokens, true, sourceInfo);

            return new MarkdownBlockquoteBlockToken(
                this,
                parser.Context,
                blockTokens,
                sourceInfo);
        }
    }
}
