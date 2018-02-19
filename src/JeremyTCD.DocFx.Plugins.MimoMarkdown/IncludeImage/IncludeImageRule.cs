using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.MarkdownLite;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeImageRule : IMarkdownRule
    {
        public virtual string Name => nameof(IncludeImageRule);

        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/grouping-constructs-in-regular-expressions#balancing-group-definitions
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/alternation-constructs-in-regular-expressions#conditional-matching-with-an-expression
        private static readonly Regex _regex = new Regex(@"^\s*\[!include-image\] *({([^{}]|(?<level>{)|(?<-level>}))*(?(level)(?!))})\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromSeconds(10));

        public virtual IMarkdownToken TryMatch(IMarkdownParser parser, IMarkdownParsingContext context)
        {
            Match match = _regex.Match(context.CurrentMarkdown);
            if (match.Length == 0)
            {
                return null;
            }

            string rawOptions = match.Groups[1].Value;
            IncludeImageOptions includeImageOptions = null;

            try
            {
                 includeImageOptions = JsonConvert.DeserializeObject<IncludeImageOptions>(rawOptions);
            }
            catch (JsonException exception)
            {
                Logger.LogError(exception.Message, context.File, context.LineNumber.ToString());
                throw;
            }

            SourceInfo sourceInfo = context.Consume(match.Length);

            return new IncludeImageToken(this, parser.Context, includeImageOptions, sourceInfo);
        }
    }
}
