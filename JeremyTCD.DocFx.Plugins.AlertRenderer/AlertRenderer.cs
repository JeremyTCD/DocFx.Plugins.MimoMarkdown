using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;

namespace JeremyTCD.DocFx.Plugins.BlockquoteRenderer
{
    public class AlertRenderer : DfmCustomizedRendererPartBase<IMarkdownRenderer, MarkdownBlockquoteBlockToken, MarkdownBlockContext>
    {
        public override string Name => "BlockquoteRenderer";

        public override bool Match(IMarkdownRenderer renderer, MarkdownBlockquoteBlockToken blockToken, MarkdownBlockContext context)
        {
            return blockToken.Tokens[0] is AlertToken;
        }

        public override StringBuffer Render(IMarkdownRenderer renderer, MarkdownBlockquoteBlockToken blockToken, MarkdownBlockContext context)
        {
            StringBuffer content = string.Empty;
            AlertToken alertToken = blockToken.Tokens[0] as AlertToken;

            content += "<div class=\"";
            content += alertToken.AlertType;
            content += "\"";
            content += ">\n";

            content += "<svg>\n";
            content += "<use xlink:href=\"";
            switch (alertToken.AlertType)
            {
                case "alert-note":
                    content += "#material-design-info";
                    break;
                default:
                    content += "#material-design-warning";
                    break;
            }
            content += "\"></use>\n";
            content += "</svg>\n";


            for (int i = 1; i < blockToken.Tokens.Length; i++)
            {
                IMarkdownToken token = blockToken.Tokens[i];
                content += renderer.Render(token);
            }
            content += "</div>\n";

            return content;
        }
    }
}
