namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    using Markdig;
    using Markdig.Extensions.AutoIdentifiers;
    using Microsoft.DocAsCode.Plugins;
    using System;

    // TODO add roslynator
    // TODO catch exceptions and log using Logger
    // TODO clean up exception messages
    // TODO look through default extensions
    //  - do we need yamlheaderextension?
    //  - what other extensions are useful
    // TODO add lexiblocks

    public class MimoMarkdownService : IMarkdownService
    {
        public string Name => "mimo-markdown";

        private readonly MarkdownServiceParameters _parameters;

        public MimoMarkdownService(MarkdownServiceParameters parameters)
        {
            _parameters = parameters;
        }

        public MarkupResult Markup(string src, string path)
        {
            return Markup(src, path, false);
        }

        public MarkupResult Markup(string src, string path, bool enableValidation)
        {
            if (src == null)
            {
                throw new ArgumentNullException(nameof(src));
            }

            if (path == null)
            {
                throw new ArgumentException("file path can't be null or empty.");
            }

            MarkdownPipeline markdownPipeline = CreatePipeline();

            return new MarkupResult
            {
                Html = Markdown.ToHtml(src, markdownPipeline)
            };
        }

        private MarkdownPipeline CreatePipeline()
        {
            var builder = new MarkdownPipelineBuilder().
                // UsePipeTables(). - create custom responsive pipe tables
                // UseGridTables(). - create custom responsive, free width grid tables
                UseEmphasisExtras().
                UseDefinitionLists().
                UseFootnotes().
                UseAutoIdentifiers(AutoIdentifierOptions.GitHub | AutoIdentifierOptions.AutoLink).
                UseAutoLinks().
                UseTaskLists().
                UseListExtras().
                UseMediaLinks().
                UseAbbreviations().
                UseFooters().
                UseFigures().
                UseCitations().
                UseCustomContainers().
                UseGenericAttributes().
                UseMathematics().
                UseSmartyPants().
                UseDiagrams().
                UseYamlFrontMatter();



            return builder.Build();
        }
    }
}
