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
            string[] labels = new string[token.Header.Length];
            StringBuffer result = "<div class=\"table-block\">\n";
            result += "<table>\n";

            result += "<thead>\n";
            // header
            result += "<tr>\n";
            for (int i = 0; i < token.Header.Length; i++)
            {
                result += "<th";
                if (i < token.Align.Length && token.Align[i] != Align.NotSpec)
                {
                    result += " class=\"align-";
                    result += token.Align[i].ToString().ToLower();
                    result += "\"";
                }
                result += ">\n";

                foreach (IMarkdownToken item in token.Header[i].Content.Tokens)
                {
                    string label = renderer.Render(item);
                    labels[i] = label;
                    result += label;
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
                    result += $"<td data-label=\"{labels[j]}\"";
                    if (j < token.Align.Length && token.Align[j] != Align.NotSpec)
                    {
                        string alignment = token.Align[j].ToString().ToLower();
                        result += $" class=\"align-{alignment}\"";
                    }
                    result += ">\n";

                    result += $"<span>";

                    foreach (IMarkdownToken item in row[j].Content.Tokens)
                    {
                        result += renderer.Render(item);
                    }
                    result += "</span>\n";
                    result += "</td>\n";
                }
                result += "</tr>\n";
            }
            result += "</tbody>\n";
            result += "</table>\n";
            result += "</div>\n"; // table-block

            return result;
        }
    }
}
