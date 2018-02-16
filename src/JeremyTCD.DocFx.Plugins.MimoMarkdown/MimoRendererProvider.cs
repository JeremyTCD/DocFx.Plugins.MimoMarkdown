using System;
using System.Collections.Generic;
using System.Composition;
using System.Net.Http;
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
            CodeBlockRenderingService codeBlockRenderingService = new CodeBlockRenderingService();
            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            yield return new AlertRenderer();
            yield return new HeadingRenderer();
            yield return new CodeBlockRenderer(codeBlockRenderingService);
            yield return new IncludeFileRenderer(new FileRetrievalService(httpClient), 
                new FileClippingService(regionExtractor), 
                codeBlockRenderingService);
        }
    }
}
