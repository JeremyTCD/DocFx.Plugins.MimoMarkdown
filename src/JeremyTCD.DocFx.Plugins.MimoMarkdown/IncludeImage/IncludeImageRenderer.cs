using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;
using System;
using System.Collections.Concurrent;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeImageRenderer : DfmCustomizedRendererPartBase<IMarkdownRenderer, IncludeImageToken, MarkdownBlockContext>
    {
        public override string Name => nameof(IncludeImageRenderer);

        private ConcurrentDictionary<string, int> _imageBlockNums = new ConcurrentDictionary<string, int>();

        public override bool Match(IMarkdownRenderer renderer, IncludeImageToken token, MarkdownBlockContext context)
        {
            return true;
        }

        public override StringBuffer Render(IMarkdownRenderer renderer, IncludeImageToken token, MarkdownBlockContext context)
        {
            int num = _imageBlockNums.AddOrUpdate(token.SourceInfo.File, 1, (key, codeBlockNum) => ++codeBlockNum);
            string blockID = !String.IsNullOrEmpty(token.Options.BlockID) ? 
                token.Options.BlockID : 
                $"code-block-{num}";

            StringBuffer result = $"<div class=\"image-block\" id=\"{blockID}\">\n";

            result += $"<img src=\"{token.Options.Src}\"";
            if (!String.IsNullOrEmpty(token.Options.Title))
            {
                result += $" title=\"{token.Options.Title}\"";
            }
            if (!String.IsNullOrEmpty(token.Options.Alt))
            {
                result += $" alt=\"{token.Options.Title}\"";
            }
            result += " >\n";

            bool hasFooterContent = !String.IsNullOrEmpty(token.Options.FooterContent);
            bool hasDescription = !String.IsNullOrEmpty(token.Options.Description);
            bool hasCredits = !String.IsNullOrEmpty(token.Options.Credits);
            if (hasFooterContent || hasDescription || hasCredits)
            {
                // Add footer element: https://developer.mozilla.org/en-US/docs/Web/HTML/Element/footer
                result += "<footer>\n";

                if (hasFooterContent)
                {
                    result += token.Options.FooterContent;
                }
                else
                {
                    if (hasDescription)
                    {
                        result += $"<span class=\"image-description\">{StringHelper.HtmlEncode(token.Options.Description)}</span>";
                    }

                    if (hasCredits)
                    {
                        result += $"<span class=\"image-credits\">{StringHelper.HtmlEncode(token.Options.Credits)}</span>";
                    }
                }
                result += "</footer>\n";
            }

            result += "</div>\n";

            return result;
        }
    }
}
