using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.MarkdownLite;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeFileRule : IMarkdownRule
    {
        public virtual string Name => "IncludeFile";

        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/grouping-constructs-in-regular-expressions#balancing-group-definitions
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/alternation-constructs-in-regular-expressions#conditional-matching-with-an-expression
        private static readonly Regex _regex = new Regex(@"^\s*\[!include-file\] *({([^{}]|(?<level>{)|(?<-level>}))*(?(level)(?!))})\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromSeconds(10));

        public virtual IMarkdownToken TryMatch(IMarkdownParser parser, IMarkdownParsingContext context)
        {
            Match match = _regex.Match(context.CurrentMarkdown);
            if (match.Length == 0)
            {
                return null;
            }

            string rawOptions = match.Groups[1].Value;
            IncludeFileOptions includeFileOptions = null;

            try
            {
                 includeFileOptions = JsonConvert.DeserializeObject<IncludeFileOptions>(rawOptions);
            }
            catch (JsonException exception)
            {
                Logger.LogError(exception.Message, context.File, context.LineNumber.ToString());
                throw;
            }

            SourceInfo sourceInfo = context.Consume(match.Length);

            return new IncludeFileToken(this, parser.Context, includeFileOptions, sourceInfo);
        }
    }
}
