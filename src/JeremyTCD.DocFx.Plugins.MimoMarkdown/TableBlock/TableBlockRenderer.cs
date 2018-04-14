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
            result += "<div class=\"full-table split-block\">\n";

            result += "<div class=\"full-table-header-group split-none\">\n";
            // header
            result += "<div class=\"full-table-row\">\n";
            for (int i = 0; i < token.Header.Length; i++)
            {
                result += "<div class=\"full-table-cell";
                if (i < token.Align.Length && token.Align[i] != Align.NotSpec)
                {
                    result += " text-align-";
                    result += token.Align[i].ToString().ToLower();
                }
                result += "\">";

                foreach (IMarkdownToken item in token.Header[i].Content.Tokens)
                {
                    string label = renderer.Render(item);
                    labels[i] = label;
                    result += label;
                }
                result += "</div>\n";
            }
            result += "</div>\n"; // table-row
            result += "</div>\n"; // table-header-group

            result += "<div class=\"full-table-row-group split-table\">\n";
            // body
            for (int i = 0; i < token.Cells.Length; i++)
            {
                var row = token.Cells[i];
                result += "<div class=\"full-table-row split-table-row-group\">\n";
                for (int j = 0; j < row.Length; j++)
                {
                    result += $"<div class=\"full-table-cell split-table-row\">\n";
                    result += $"<div class=\"full-none split-table-cell\">\n";
                    result += labels[j];
                    result += $"</div>\n";

                    // TODO Can a row have fewer columns than other rows?
                    result += $"<div class=\"split-table-cell";
                    if (j < token.Align.Length && token.Align[j] != Align.NotSpec)
                    {
                        string alignment = token.Align[j].ToString().ToLower(); 
                        result += $" text-align-{alignment} vertical-align-{alignment}";
                    }
                    result += "\">\n"; // header table cell
                    foreach (IMarkdownToken item in row[j].Content.Tokens)
                    {
                        result += renderer.Render(item);
                    }
                    result += "</div>\n"; // content table cell
                    result += "</div>\n"; // outer table cell
                }
                result += "</div>\n"; // table-row
            }
            result += "</div>\n"; // table-row-group
            result += "</div>\n"; // table
            result += "</div>\n"; // table-block

            return result;
        }
    }
}
