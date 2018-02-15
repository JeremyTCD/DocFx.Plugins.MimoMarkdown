using HtmlAgilityPack;
using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;
using System.Collections.Immutable;
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

            // Get content
            string content;
            if (token.Options.Tags != null && token.Options.Tags.Any())
            {
                content = _fileClippingService.GetRegions(token, fileContent);
            }
            else if (token.Options.Ranges != null && token.Options.Ranges.Any())
            {
                content = _fileClippingService.GetRanges(token, fileContent);
            }
            else
            {
                content = fileContent;
            }

            // Code block, append content as is
            if (codeBlock)
            {
                AppendCodeBlockOpeningTags(result, renderer, token);
                result.Append(content);
                AppendCodeBlockClosingTags(result, renderer, token);
            }
            else
            {
                // Notes
                // Src could be a url or a relative path. If it is relative, the context is updated so its FilePathStack is accurate, allowing relative links in nested includes to be resolved correctly.
                // If src is a url, it is assumed that the file has no relative links. Allowing urls to have relative links is easy, such functionality should be added if the need arises.

                bool srcIsRelative = PathUtility.IsRelativePath(token.Options.Src);
                string srcFilePath = token.Options.Src;

                // Update file path stack
                // Note: src could also be an absolute path with a protocol
                if (srcIsRelative)
                {
                    // TODO when would it be empty? this logic is from DfmInclusionLoader
                    ImmutableStack<string> filePathStack = context.GetFilePathStack() ?? ImmutableStack<string>.Empty;

                    string currentFilePath = filePathStack.Peek();
                    if (currentFilePath != null)
                    {
                        // TODO FileRetrievalService.GetFile does the same thing, move this to start of function so it only needs to be done once
                        // Src is specified relative to current file path in markdown file, find src path relative to root
                        srcFilePath = ((RelativePath)token.Options.Src).BasedOn((RelativePath)currentFilePath);
                    }

                    if(filePathStack.Contains(srcFilePath, FilePathComparer.OSPlatformSensitiveComparer))
                    {
                        Logger.LogError($"Circular dependency in \"{currentFilePath}\"", currentFilePath, token.SourceInfo.LineNumber.ToString());
                    }

                    // This is required so files have accurate paths when traversing a tree of nested includes
                    filePathStack = filePathStack.Push(srcFilePath);
                    context = context.SetFilePathStack(filePathStack) as MarkdownBlockContext;
                }

                // TODO cache generated markup. Would make more sense to cache using token.src and clipping data, e.g src and tags. This way FileClippingService methods would not even need to be called.

                // Process as markdown
                DfmEngine currentEngine = (DfmEngine)renderer.Engine;
                // Mimics DfmEngine internal constructor, used by DfmInclusionLoader (handles Dfm's include tokens)
                DfmEngine newEngine = new DfmEngine(currentEngine.Context, currentEngine.Rewriter, currentEngine.RendererImpl, currentEngine.Options)
                {
                    TokenTreeValidator = currentEngine.TokenTreeValidator,
                    TokenAggregators = currentEngine.TokenAggregators
                };

                string markup = newEngine.Markup(content, context);

                // Fix relative hrefs
                if (srcIsRelative)
                {
                    markup = UpdateIncludedPaths(markup, srcFilePath);
                }


                result.Append(markup);
            }

            return result.ToString();
        }

        private string UpdateIncludedPaths(string markup, string filePath)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(markup);
            HtmlNode rootNode = doc.DocumentNode;
            HtmlNodeCollection nodesWithLinks = rootNode.
                SelectNodes("//*[@src|@href]");

            if (nodesWithLinks == null)
            {
                return markup;
            }

            foreach (HtmlNode nodeWithLink in nodesWithLinks)
            {
                bool hasSrcAttr = true;
                string path = nodeWithLink.GetAttributeValue("src", null);
                if (path == null)
                {
                    nodeWithLink.GetAttributeValue("href", null);
                    hasSrcAttr = false;
                }

                if (PathUtility.IsRelativePath(path) && !RelativePath.IsPathFromWorkingFolder(path) && !path.StartsWith("#"))
                {
                    nodeWithLink.SetAttributeValue(hasSrcAttr ? "src" : "href", ((RelativePath)path).BasedOn((RelativePath)filePath).GetPathFromWorkingFolder());
                }
            }
            return rootNode.WriteTo();
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
