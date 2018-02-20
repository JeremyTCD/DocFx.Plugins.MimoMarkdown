using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;
using System.Collections.Concurrent;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class TableBlockRenderer : DfmCustomizedRendererPartBase<IMarkdownRenderer, MarkdownTableBlockToken, MarkdownBlockContext>
    {
        public override string Name => nameof(TableBlockRenderer);

        private ConcurrentDictionary<string, int> _imageBlockNums = new ConcurrentDictionary<string, int>();

        public override bool Match(IMarkdownRenderer renderer, MarkdownTableBlockToken token, MarkdownBlockContext context)
        {
            return true;
        }

        public override StringBuffer Render(IMarkdownRenderer renderer, MarkdownTableBlockToken token, MarkdownBlockContext context)
        {
            // TODO Needs to be more user friendly on narrow screens. Perhaps use flexbox instead, morph when narrow etc.

            StringBuffer result = "<div class=\"table-block\">\n";
            result += "<table>\n<thead>\n";
            // header
            result += "<tr>\n";
            var cell = StringBuffer.Empty;
            for (int i = 0; i < token.Header.Length; i++)
            {
                if (i < token.Align.Length && token.Align[i] != Align.NotSpec)
                {
                    result += "<th style=\"text-align:";
                    result += token.Align[i].ToString().ToLower();
                    result += "\">";
                }
                else
                {
                    result += "<th>";
                }
                foreach (var item in token.Header[i].Content.Tokens)
                {
                    result += renderer.Render(item);
                }
                result += "</th>\n";
            }
            result += "</tr>\n";
            result += "</thead>\n";
            result += "<tbody>\n";
            // body
            for (int i = 0; i < token.Cells.Length; i++)
            {
                var row = token.Cells[i];
                result += "<tr>\n";
                for (int j = 0; j < row.Length; j++)
                {
                    if (j < token.Align.Length && token.Align[j] != Align.NotSpec)
                    {
                        result += "<td style=\"text-align:";
                        result += token.Align[j].ToString().ToLower();
                        result += "\">";
                    }
                    else
                    {
                        result += "<td>";
                    }
                    foreach (var item in row[j].Content.Tokens)
                    {
                        result += renderer.Render(item);
                    }
                    result += "</td>\n";
                }
                result += "</tr>\n";
            }

            return result + "</tbody>\n" + "</table>\n" + "</div>\n";
        }
    }
}
