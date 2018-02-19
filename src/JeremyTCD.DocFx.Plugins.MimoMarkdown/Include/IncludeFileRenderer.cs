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
        private readonly FileRetrievalService _fileRetrievalService;
        private readonly FileClippingService _fileClippingService;
        private readonly CodeBlockRenderingService _codeBlockRenderingService;

        public IncludeFileRenderer(FileRetrievalService fileRetrievalService, 
            FileClippingService fileClippingService,
            CodeBlockRenderingService codeBlockRenderingService)
        {
            _fileRetrievalService = fileRetrievalService;
            _fileClippingService = fileClippingService;
            _codeBlockRenderingService = codeBlockRenderingService;
        }

        public override string Name => nameof(IncludeFileRenderer);

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
                CodeOptions codeOptions = token.Options.CodeOptions ?? new CodeOptions();

                _codeBlockRenderingService.
                    AppendCodeBlock(
                        result,
                        token.SourceInfo.File,
                        content,
                        codeOptions.ShowLineNumbers,
                        codeOptions.Highlight,
                        codeOptions.Language,
                        renderer.Options.LangPrefix);
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

                // By setting context to include, the engine will remove any yaml header
                string markup = newEngine.Markup(content, context.SetIsInclude());

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
    }
}
