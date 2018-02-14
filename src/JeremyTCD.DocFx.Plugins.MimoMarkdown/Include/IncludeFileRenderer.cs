using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;
using System.Linq;
using System.Text;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeFileRenderer : DfmCustomizedRendererPartBase<IMarkdownRenderer, IncludeFileToken, MarkdownBlockContext>
    {
        private int _externalCodeBlockNum = 0;

        private readonly FileRetrievalService _fileRetrievalService;
        private readonly FileClippingService _fileClippingService;

        public IncludeFileRenderer(FileRetrievalService fileRetrievalService, FileClippingService fileClippingService)
        {
            _fileRetrievalService = fileRetrievalService;
            _fileClippingService = fileClippingService;
        }

        public override string Name => throw new System.NotImplementedException();

        public override bool Match(IMarkdownRenderer renderer, IncludeFileToken token, MarkdownBlockContext context)
        {
            return true;
        }

        public override StringBuffer Render(IMarkdownRenderer renderer, IncludeFileToken token, MarkdownBlockContext context)
        {
            string fileContent = _fileRetrievalService.GetFile(token, context);
            StringBuilder result = new StringBuilder();
            bool codeBlock = token.Options.IsCode || token.Options.CodeOptions != null;

            // If code block, render opening tags
            if (codeBlock)
            {
                AppendCodeBlockOpeningTags(result, renderer, token);
            }

            // Render content
            if(token.Options.Tags != null && token.Options.Tags.Any())
            {
                _fileClippingService.AppendRegions(result, token, fileContent);
            }
            else if(token.Options.Ranges != null && token.Options.Ranges.Any())
            {
                _fileClippingService.AppendRanges(result, token, fileContent);
            }
            else
            {
                result.Append(fileContent);
            }

            // If codeblock , render closing tags
            if (codeBlock)
            {
                AppendCodeBlockClosingTags(result, renderer, token);
            }

            return result.ToString();
        }

        private void AppendCodeBlockOpeningTags(StringBuilder result, IMarkdownRenderer renderer, IncludeFileToken token)
        {
            CodeOptions codeOptions = token.Options.CodeOptions ?? new CodeOptions();

            // TODO ShowLanguage, Title

            result.Append($"<div class=\"code-block{(codeOptions.ShowLineNumbers ? " show-line-numbers" : "")}\">\n");
            // No unecessary white space within pre element
            result.Append("<pre>");
            result.Append($"<code id=\"external-code-block-{_externalCodeBlockNum}\"");

            if (!string.IsNullOrEmpty(codeOptions.Language) && codeOptions.Highlight)
            {
                result.Append(" class=\"" + renderer.Options.LangPrefix + StringHelper.Escape(codeOptions.Language, true) + "\"");
            }
            result.Append(">");
        }

        private void AppendCodeBlockClosingTags(StringBuilder result, IMarkdownRenderer renderer, IncludeFileToken token)
        {
            result.Append("</code>");
            result.Append("</pre>\n");
            // Firefox does not support hover events for svgs within button elements, so use a div and assign 'button' to its role attribute
            // data-clipboard-target used by clipboard.js. title used by tippy.js
            result.Append($"<div data-clipboard-target=\"#external-code-block-{_externalCodeBlockNum}\" role=\"button\" title=\"Code copied\">\n");
            result.Append("<svg>\n");
            result.Append("<use xlink:href=\"#material-design-copy\"></use>\n");
            result.Append("</svg>\n");
            result.Append("</div>\n"); // button
            result.Append("</div>\n"); // code block
        }
    }
}
