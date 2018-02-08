using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class CodeBlockRenderer : DfmCustomizedRendererPartBase<IMarkdownRenderer, MarkdownCodeBlockToken, MarkdownBlockContext>
    {
        private int _codeBlockNum = 0;

        public override string Name => "AlertRenderer";

        public override bool Match(IMarkdownRenderer renderer, MarkdownCodeBlockToken token, MarkdownBlockContext context)
        {
            return true;
        }

        public override StringBuffer Render(IMarkdownRenderer renderer, MarkdownCodeBlockToken token, MarkdownBlockContext context)
        {
            // TODO how is name handled?
            // TODO check why highlighting does not work
            // The above two issues are handled by DfmCodeRenderer, a different renderer for code from external sources.
            // Eventually, all code rendering should be handled by a custom token that allows for
            //  - title
            //  - source
            //  - language (for highlighting, also converted to display language)

            string codeID = $"code-block-{_codeBlockNum++}";
            bool escaped = false;
            string code = token.Code;
            if (renderer.Options.Highlight != null)
            {
                var highlightCode = renderer.Options.Highlight(code, token.Lang);
                if (highlightCode != null && highlightCode != code)
                {
                    escaped = true;
                    code = highlightCode;
                }
            }

            StringBuffer result = "<div class=\"code-block\">\n";
            // No unecessary white space within pre element
            result = result + "<pre>";
            result += $"<code id=\"{codeID}\"";
            if (!string.IsNullOrEmpty(token.Lang))
            {
                result = result + " class=\"" + renderer.Options.LangPrefix + StringHelper.Escape(token.Lang, true) + "\"";
            }
            result += ">";
            result += (escaped ? code : StringHelper.Escape(code, true));
            result += "</code>";
            result += "</pre>\n";
            // Firefox does not support hover events for svgs within button elements, so use a div and assign 'button' to its role attribute
            // data-clipboard-target used by clipboard.js. title used by tippy.js
            result += $"<div data-clipboard-target=\"#{codeID}\" role=\"button\" title=\"Code copied\">\n"; 
            result += "<svg>\n";
            result += "<use xlink:href=\"#material-design-copy\"></use>\n";
            result += "</svg>\n";
            result += "</div>\n"; // button
            result += "</div>\n"; // code block

            return result;
        }
    }
}
