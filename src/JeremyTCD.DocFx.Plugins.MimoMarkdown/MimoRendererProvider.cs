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
            RegionExtractorFactory regionExtractorFactory = new RegionExtractorFactory();
            RegionExtractor regionExtractor = regionExtractorFactory.BuildRegionExtractor();

            yield return new AlertRenderer();
            yield return new HeadingRenderer();
            yield return new CodeBlockRenderer();
            yield return new IncludeFileRenderer(new FileRetrievalService(), new FileClippingService(regionExtractor));
        }
    }
}
