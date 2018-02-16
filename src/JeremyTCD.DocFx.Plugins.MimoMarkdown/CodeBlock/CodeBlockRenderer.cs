﻿using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;
using System.Text;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class CodeBlockRenderer : DfmCustomizedRendererPartBase<IMarkdownRenderer, MarkdownCodeBlockToken, MarkdownBlockContext>
    {
        private readonly CodeBlockRenderingService _codeBlockRenderingService;

        public override string Name => "CodeBlockRenderer";

        public CodeBlockRenderer(CodeBlockRenderingService codeBlockRenderingService)
        {
            _codeBlockRenderingService = codeBlockRenderingService;
        }

        public override bool Match(IMarkdownRenderer renderer, MarkdownCodeBlockToken token, MarkdownBlockContext context)
        {
            return true;
        }

        public override StringBuffer Render(IMarkdownRenderer renderer, MarkdownCodeBlockToken token, MarkdownBlockContext context)
        {
            StringBuilder result = new StringBuilder();

            _codeBlockRenderingService.
                AppendCodeBlock(result,
                    token.SourceInfo.File,
                    StringHelper.Escape(token.Code, true),
                    false,
                    true,
                    token.Lang,
                    renderer.Options.LangPrefix);

            return result.ToString();
        }
    }
}
