using Microsoft.DocAsCode.MarkdownLite;
using System.Collections.Concurrent;
using System.Text;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    /// <summary>
    /// Has state - must be used as a singleton
    /// </summary>
    public class CodeBlockRenderingService
    {
        // Could use GUIDs but this makes for nice links (while documents are processed in parallel, code blocks within a document are processed
        // in series).
        private ConcurrentDictionary<string, int> _codeBlockNums = new ConcurrentDictionary<string, int>();

        public void AppendCodeBlock(StringBuilder result,
            string fileName,
            string content,
            bool showLineNumbers,
            bool highlight,
            string language,
            string languagePrefix)
        {
            // AddOrUpdate uses an optimistic concurrency model. It grabs the current value, runs the delegate and tries to update the initial value. If the initial value changed while the delegate was running,
            // it runs the delegate again, using the new value - https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-add-and-remove-items.
            string id = $"code-block-{_codeBlockNums.AddOrUpdate(fileName, 1, (key, codeBlockNum) => ++codeBlockNum)}";

            AppendCodeBlockOpeningTags(result, id, showLineNumbers, highlight, language, languagePrefix);
            result.Append(StringHelper.HtmlEncode(content));
            AppendCodeBlockClosingTags(result);
        }

        public void AppendCodeBlockOpeningTags(StringBuilder result,
            string id,
            bool showLineNumbers,
            bool highlight,
            string language,
            string languagePrefix)
        {
            // TODO ShowLanguage, Title
            result.Append($"<div class=\"code-block{(showLineNumbers ? " show-line-numbers" : "")}\">\n");

            result.Append("<header>\n");
            // Firefox does not support hover events for svgs within button elements, so use a div and assign 'button' to its role attribute
            // data-clipboard-target used by clipboard.js. title used by tippy.js
            result.Append($"<div data-clipboard-target=\"#{id}\" role=\"button\" title=\"Code copied\">\n");
            result.Append("<svg>\n");
            result.Append("<use xlink:href=\"#material-design-copy\"></use>\n");
            result.Append("</svg>\n");
            result.Append("</div>\n"); // button
            result.Append("</header>\n");

            // No unecessary white space within pre element
            result.Append("<pre>");
            string noHighlight = "no-highlight";
            string hljsLanguageClass = !string.IsNullOrEmpty(language) && highlight && language != noHighlight ? 
                languagePrefix + StringHelper.Escape(language, true) : 
                noHighlight;
            result.Append($"<code id=\"{id}\" class=\"{hljsLanguageClass}\">");
        }

        public void AppendCodeBlockClosingTags(StringBuilder result)
        {
            result.Append("</code>");
            result.Append("</pre>\n");
            result.Append("</div>\n"); // code block
        }
    }
}
