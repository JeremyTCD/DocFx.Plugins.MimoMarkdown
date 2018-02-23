using HtmlAgilityPack;
using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeMarkdownRenderer : DfmCustomizedRendererPartBase<IMarkdownRenderer, IncludeMarkdownToken, MarkdownBlockContext>
    {
        private readonly FileRetrievalService _fileRetrievalService;
        private readonly FileClippingService _fileClippingService;

        public IncludeMarkdownRenderer(FileRetrievalService fileRetrievalService,
            FileClippingService fileClippingService)
        {
            _fileRetrievalService = fileRetrievalService;
            _fileClippingService = fileClippingService;
        }

        public override string Name => nameof(IncludeMarkdownRenderer);

        public override bool Match(IMarkdownRenderer renderer, IncludeMarkdownToken token, MarkdownBlockContext context)
        {
            return true;
        }

        public override StringBuffer Render(IMarkdownRenderer renderer, IncludeMarkdownToken token, MarkdownBlockContext context)
        {
            IncludeMarkdownOptions options = token.Options;
            string fileContent = _fileRetrievalService.GetFile(options.Src, token, context);
            StringBuilder result = new StringBuilder();

            // Get content
            string content;
            if (options.Regions != null && options.Regions.Any())
            {
                content = _fileClippingService.GetRegions("markdown", options.Src, options.Regions, fileContent, token);
            }
            else if (options.Ranges != null && options.Ranges.Any())
            {
                content = _fileClippingService.GetRanges(options.Ranges, fileContent, token);
            }
            else
            {
                content = fileContent;
            }

            // TODO
            // Src could be a url or a relative path. If it is relative, the context is updated so its FilePathStack is accurate, allowing relative links in nested includes to be resolved correctly.
            // If src is a url, it is assumed that the file has no relative links. Allowing urls to have relative links is easy, such functionality should be added if the need arises.
            bool srcIsRelative = PathUtility.IsRelativePath(options.Src);
            string srcFilePath = options.Src;

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
                    srcFilePath = ((RelativePath)options.Src).BasedOn((RelativePath)currentFilePath);
                }

                if (filePathStack.Contains(srcFilePath, FilePathComparer.OSPlatformSensitiveComparer))
                {
                    Logger.LogError($"Circular dependency in \"{currentFilePath}\"", currentFilePath, token.SourceInfo.LineNumber.ToString());
                }

                // This is required so files have accurate paths when traversing a tree of nested includes
                filePathStack = filePathStack.Push(srcFilePath);
                context = context.SetFilePathStack(filePathStack) as MarkdownBlockContext;
            }

            // TODO cache generated markup. Would make more sense to cache using token.src and clipping data, e.g src and Regions. This way FileClippingService methods would not even need to be called.

            // Process as markdown
            DfmEngine currentEngine = (DfmEngine)renderer.Engine;
            // Mimics DfmEngine internal constructor, used by DfmInclusionLoader (handles Dfm's include tokens)
            DfmEngine newEngine = new DfmEngine(currentEngine.Context, currentEngine.Rewriter, currentEngine.RendererImpl, currentEngine.Options)
            {
                TokenTreeValidator = currentEngine.TokenTreeValidator,
                TokenAggregators = currentEngine.TokenAggregators
            };

            // By setting context to include, the engine will remove any yaml header
            string markup = newEngine.Markup(content, context.SetIsInclude());

            // Fix relative hrefs
            if (srcIsRelative)
            {
                markup = UpdateIncludedPaths(markup, srcFilePath);
            }


            result.Append(markup);


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
    }
}
