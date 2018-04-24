using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class RegionExtractor
    {
        private IDictionary<string, List<ICodeSnippetExtractor>> _keyExtractorsMap;
        private ConcurrentDictionary<string, Lazy<Dictionary<string, List<DfmTagNameResolveResult>>>> _cache = new ConcurrentDictionary<string, Lazy<Dictionary<string, List<DfmTagNameResolveResult>>>>();

        public RegionExtractor(IDictionary<string, List<ICodeSnippetExtractor>> keyExtractors)
        {
            _keyExtractorsMap = keyExtractors;
        }

        public string[] ExtractRegion(string[] fileLines,
            Dictionary<string, List<DfmTagNameResolveResult>> resolveResultsMap,
            Region region,
            IMarkdownToken token)
        {
            // Region name does not exist
            if (!resolveResultsMap.TryGetValue(region.Name, out List<DfmTagNameResolveResult> resolveResults) &&
                !resolveResultsMap.TryGetValue($"snippet{region.Name}", out resolveResults))
            {
                Logger.LogError($"{region.Name} does not exist.", token.SourceInfo.File, token.SourceInfo.LineNumber.ToString());
                throw new InvalidOperationException();
            }

            // Same region name used multiple times
            if (resolveResults.Count > 1)
            {
                Logger.LogError($"Multiple occurences of {region.Name}.", token.SourceInfo.File, token.SourceInfo.LineNumber.ToString());
                throw new InvalidOperationException();
            }

            DfmTagNameResolveResult resolveResult = resolveResults[0];

            // Resolve error
            if (!string.IsNullOrEmpty(resolveResult.ErrorMessage))
            {
                Logger.LogError($"Error retrieving content for region with name \"{region.Name}\": {resolveResult.ErrorMessage}", token.SourceInfo.File, token.SourceInfo.LineNumber.ToString());
                throw new InvalidOperationException();
            }

            int startIndex = resolveResult.StartLine - 1;
            int length = resolveResult.EndLine - startIndex;
            string[] result = new string[length];
            Array.Copy(fileLines, startIndex, result, 0, length);

            return result;
        }

        public Dictionary<string, List<DfmTagNameResolveResult>> GetResolveResultsMap(string[] lines, string language, string src, IMarkdownToken token)
        {
            string key = language;
            if (string.IsNullOrEmpty(key))
            {
                try
                {
                    // TODO remote sources
                    key = Path.GetExtension(src);
                }
                catch (Exception exception)
                {
                    Logger.LogError($"Unable to retrieve a language or file extension: {exception.Message}", token.SourceInfo.File, token.SourceInfo.LineNumber.ToString());
                    throw;
                }
            }

            // Get extractors
            if (!_keyExtractorsMap.TryGetValue(key, out List<ICodeSnippetExtractor> extractors))
            {
                Logger.LogError($"{key} does not have region extractors.", token.SourceInfo.File, token.SourceInfo.LineNumber.ToString());
                throw new InvalidOperationException();
            }

            // Generate or retrieve resolve results (regions and their locations)
            return _cache.
                GetOrAdd(src, new Lazy<Dictionary<string, List<DfmTagNameResolveResult>>>(() => GetTagResolveResultsForFile(extractors, lines))).
                Value;
        }

        private Dictionary<string, List<DfmTagNameResolveResult>> GetTagResolveResultsForFile(List<ICodeSnippetExtractor> keyExtractors, string[] fileLines)
        {
            // GetAll retrieves all tag resolve results, not all will be used, so don't throw here if a region name has multiple corresponding results or if there are
            // resolve errors.
            Dictionary<string, List<DfmTagNameResolveResult>> resolveResultsMap = new Dictionary<string, List<DfmTagNameResolveResult>>();

            foreach (ICodeSnippetExtractor keyExtractor in keyExtractors)
            {
                Dictionary<string, DfmTagNameResolveResult> extractorResultMap = keyExtractor.GetAll(fileLines);

                foreach (KeyValuePair<string, DfmTagNameResolveResult> extractorResult in extractorResultMap)
                {
                    if (resolveResultsMap.TryGetValue(extractorResult.Key, out List<DfmTagNameResolveResult> resolveResults))
                    {
                        resolveResults.Add(extractorResult.Value);
                    }
                    else
                    {
                        resolveResultsMap.Add(extractorResult.Key, new List<DfmTagNameResolveResult> { extractorResult.Value });
                    }
                }
            }

            return resolveResultsMap;
        }
    }
}
