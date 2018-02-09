using Microsoft.DocAsCode.MarkdownLite;
using Microsoft.DocAsCode.MarkdownLite.Matchers;
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

        private static readonly Matcher _BlockquoteMatcher =
            (
                // @" *> *\n"
                (Matcher.WhiteSpacesOrEmpty + '>' + Matcher.WhiteSpacesOrEmpty + (Matcher.NewLine | Matcher.EndOfString)) |
                (
                    // @" *>[^\n]+(\n[^\n]+)*"
                    Matcher.WhiteSpacesOrEmpty + '>' + Matcher.AnyStringInSingleLine +
                    (
                        Matcher.NewLine +
                        // expect following:
                        (
                            // heading
                            (Matcher.WhiteSpacesOrEmpty + Matcher.Char('#').Repeat(1, 6) + Matcher.WhiteSpaces + Matcher.AnyStringInSingleLine + '\n') |
                            // hr
                            ((Matcher.WhiteSpacesOrEmpty + Matcher.AnyCharIn('-', '*', '_')).RepeatAtLeast(3) + Matcher.WhiteSpacesOrEmpty + '\n') |
                            // list
                            (Matcher.WhiteSpacesOrEmpty + Matcher.AnyCharIn('-', '*') + Matcher.WhiteSpaces + Matcher.AnyStringInSingleLine + '\n') |
                            (Matcher.WhiteSpacesOrEmpty + Matcher.AnyCharInRange('0', '9').RepeatAtLeast(1) + '.' + Matcher.WhiteSpaces + Matcher.AnyStringInSingleLine + '\n') |
                            // @" *>"
                            (Matcher.WhiteSpacesOrEmpty + '>')
                        ).ToNegativeTest() +
                        Matcher.AnyStringInSingleLine
                    ).RepeatAtLeast(0) +
                    (Matcher.NewLine | Matcher.EndOfString)
                )
            ).RepeatAtLeast(1) +
            Matcher.NewLine.RepeatAtLeast(0);

        public virtual IMarkdownToken TryMatch(IMarkdownParser parser, IMarkdownParsingContext context)
        {
            MarkdownBlockquoteBlockToken result = null;

            // Need to switch context (see DfmBlockquoteBlockRule.cs)
            IMarkdownContext initialContext = parser.SwitchContext(MarkdownBlockContext.IsBlockQuote, true);
            MatchResult match = context.Match(_BlockquoteMatcher);

            if (match?.Length > 0)
            {
                SourceInfo sourceInfo = context.Consume(match.Length);
                string capStr = LeadingBlockquote.Replace(sourceInfo.Markdown, string.Empty);
                ImmutableArray<IMarkdownToken> blockTokens = parser.Tokenize(sourceInfo.Copy(capStr));
                blockTokens = TokenHelper.CreateParagraghs(parser, this, blockTokens, true, sourceInfo);
                result = new MarkdownBlockquoteBlockToken(
                    this,
                    parser.Context,
                    blockTokens,
                    sourceInfo);
            }

            parser.SwitchContext(initialContext);

            return result;
        }
    }
}