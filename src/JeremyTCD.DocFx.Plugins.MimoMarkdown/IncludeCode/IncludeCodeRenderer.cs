﻿using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;
using System.Linq;
using System.Text;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class IncludeCodeRenderer : DfmCustomizedRendererPartBase<IMarkdownRenderer, IncludeCodeToken, MarkdownBlockContext>
    {
        private readonly FileRetrievalService _fileRetrievalService;
        private readonly FileClippingService _fileClippingService;
        private readonly CodeBlockRenderingService _codeBlockRenderingService;

        public IncludeCodeRenderer(FileRetrievalService fileRetrievalService,
            FileClippingService fileClippingService,
            CodeBlockRenderingService codeBlockRenderingService)
        {
            _fileRetrievalService = fileRetrievalService;
            _fileClippingService = fileClippingService;
            _codeBlockRenderingService = codeBlockRenderingService;
        }

        public override string Name => nameof(IncludeCodeRenderer);

        public override bool Match(IMarkdownRenderer renderer, IncludeCodeToken token, MarkdownBlockContext context)
        {
            return true;
        }

        public override StringBuffer Render(IMarkdownRenderer renderer, IncludeCodeToken token, MarkdownBlockContext context)
        {
            IncludeCodeOptions options = token.Options;
            string fileContent = _fileRetrievalService.GetFile(options.Src, token, context);
            StringBuilder result = new StringBuilder();

            // Get content
            string content;
            if (options.Regions != null && options.Regions.Any())
            {
                content = _fileClippingService.GetRegions(options.Language, options.Src, options.Regions, fileContent, token);
            }
            else if (options.Ranges != null && options.Ranges.Any())
            {
                content = _fileClippingService.GetRanges(options.Ranges, fileContent, token);
            }
            else
            {
                content = fileContent;
            }

            _codeBlockRenderingService.
                AppendCodeBlock(
                    result,
                    options.Src,
                    token.SourceInfo.File,
                    content,
                    options.BlockID,
                    options.Title,
                    options.ShowLineNumbers,
                    options.Highlight,
                    options.Language,
                    renderer.Options.LangPrefix);

            return result.ToString();
        }
    }
}
