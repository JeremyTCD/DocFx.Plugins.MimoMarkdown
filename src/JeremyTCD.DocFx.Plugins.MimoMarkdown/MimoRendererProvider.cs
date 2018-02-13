using System.Collections.Generic;
using System.Composition;
using Microsoft.DocAsCode.Dfm;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    [Export(typeof(IDfmCustomizedRendererPartProvider))]
    public class MimoMarkdownProvider : IDfmCustomizedRendererPartProvider
    {
        public IEnumerable<IDfmCustomizedRendererPart> CreateParts(IReadOnlyDictionary<string, object> parameters)
        {
            // TODO use a container for all of the following
            TagContentExtractorFactory tagContentExtractorFactory = new TagContentExtractorFactory();
            TagContentExtractor tagContentExtractor = tagContentExtractorFactory.BuildTagExtractor();

            yield return new AlertRenderer();
            yield return new HeadingRenderer();
            yield return new CodeBlockRenderer();
            yield return new IncludeFileRenderer(new FileRetrievalService(), new FileClippingService(tagContentExtractor));
        }
    }
}
