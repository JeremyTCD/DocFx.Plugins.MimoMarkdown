using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class HeadingRenderer : DfmCustomizedRendererPartBase<IMarkdownRenderer, MarkdownHeadingBlockToken, MarkdownBlockContext>
    {
        public override string Name => "AlertRenderer";

        public override bool Match(IMarkdownRenderer renderer, MarkdownHeadingBlockToken token, MarkdownBlockContext context)
        {
            // h1 and h2 elements need wrappers for their "copy link" buttons
            return token.Depth <= 2;
        }

        public override StringBuffer Render(IMarkdownRenderer renderer, MarkdownHeadingBlockToken token, MarkdownBlockContext context)
        {
            string level = token.Depth.ToString();
            StringBuffer result = $"<header class=\"header-{level}\" id=\"{renderer.Options.HeaderPrefix}{token.Id}\">\n";
            result += "<div class=\"wrapper\">\n";
            result += $"<h{level}>\n";
            foreach (var item in token.Content.Tokens)
            {
                result += renderer.Render(item);
            }
            result += $"\n</h{level}>\n";
            result += $"<div data-clipboard-text=\"#{token.Id}\" data-clipboard-action=\"copy\" role=\"button\" title=\"Link copied\">\n"; // data-clipboard-action used by clipboard.js. title used by tippy.js
            result += "<svg>\n";
            result += "<use xlink:href=\"#material-design-link\"></use>\n";
            result += "</svg>\n";
            result += "</div>\n"; // button
            result += "</div>\n"; // wrapper
            result += "</header>\n"; // header

            return result;
        }
    }
}
