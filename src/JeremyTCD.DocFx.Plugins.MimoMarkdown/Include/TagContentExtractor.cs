using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.Dfm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class TagContentExtractor
    {
        private IDictionary<string, List<ICodeSnippetExtractor>> _keyExtractorsMap;

        public TagContentExtractor(IDictionary<string, List<ICodeSnippetExtractor>> keyExtractors)
        {
            _keyExtractorsMap = keyExtractors;
        }

        public void AppendTagContents(StringBuilder result, IncludeFileToken token, string[] fileLines)
        {
            string key = token.Options.CodeOptions.Language;
            if (string.IsNullOrEmpty(key))
            {
                try
                {
                    key = Path.GetExtension(token.SourceInfo.File);
                }
                catch (Exception exception)
                {
                    Logger.LogError($"Unable to retrieve a language or file extension: {exception.Message}", token.SourceInfo.File, token.SourceInfo.LineNumber.ToString());
                    throw;
                }
            }

            // Invalid key
            if (!_keyExtractorsMap.TryGetValue(key, out List<ICodeSnippetExtractor> keyExtractors))
            {
                Logger.LogError($"{key} does not have tag extractors.", token.SourceInfo.File, token.SourceInfo.LineNumber.ToString());
                throw new InvalidOperationException();
            }

            Dictionary<string, List<DfmTagNameResolveResult>> resolveResultsMap = GetTagResolveResultsForFile(token.SourceInfo.File, keyExtractors, fileLines);

            // Tag name does not exist
            if (!resolveResultsMap.TryGetValue(token.Options.Tag, out List<DfmTagNameResolveResult> resolveResults) &&
                !resolveResultsMap.TryGetValue($"snippet{token.Options.Tag}", out resolveResults))
            {
                Logger.LogError($"{token.Options.Tag} does not exist.", token.SourceInfo.File, token.SourceInfo.LineNumber.ToString());
                throw new InvalidOperationException();
            }

            // Same tag name used multiple times
            if (resolveResults.Count > 1)
            {
                Logger.LogError($"Multiple occurences of {token.Options.Tag}.", token.SourceInfo.File, token.SourceInfo.LineNumber.ToString());
                throw new InvalidOperationException();
            }

            DfmTagNameResolveResult resolveResult = resolveResults[0];

            // Resolve error
            if (!string.IsNullOrEmpty(resolveResult.ErrorMessage))
            {
                Logger.LogError($"Error retrieving content for tag \"{token.Options.Tag}\": {resolveResult.ErrorMessage}", token.SourceInfo.File, token.SourceInfo.LineNumber.ToString());
                throw new InvalidOperationException();
            }

            for (int i = resolveResult.StartLine - 1; i < resolveResult.EndLine; i++)
            {
                result.AppendLine(fileLines[i]);
            }
        }

        public Dictionary<string, List<DfmTagNameResolveResult>> GetTagResolveResultsForFile(string fileName, List<ICodeSnippetExtractor> keyExtractors, string[] fileLines)
        {
            // TODO cache for each file

            // GetAll retrieves all tags, not all will be used, so don't throw here if a tag name has multiple corresponding results
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
