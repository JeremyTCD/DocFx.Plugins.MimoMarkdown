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
            FileRetrievalService fileRetrievalService = new FileRetrievalService(httpClient);
            FileClippingService fileClippingService = new FileClippingService(regionExtractor);
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            yield return new AlertRenderer();
            yield return new HeadingRenderer();
            yield return new CodeBlockRenderer(codeBlockRenderingService);
            yield return new IncludeCodeRenderer(fileRetrievalService, fileClippingService, codeBlockRenderingService);
            yield return new IncludeMarkdownRenderer(fileRetrievalService, fileClippingService);
            yield return new IncludeImageRenderer();
            yield return new TableBlockRenderer();
        }
    }
}
