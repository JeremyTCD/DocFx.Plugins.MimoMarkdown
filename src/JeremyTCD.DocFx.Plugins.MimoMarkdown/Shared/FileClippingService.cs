using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class FileClippingService
    {
        private RegionExtractor _regionExtractor;
        private RangeExtractor _rangeExtractor;
        private DedentingService _dedentingService;

        public FileClippingService(RangeExtractor rangeExtractor, RegionExtractor regionExtractor, DedentingService dedentingService)
        {
            _regionExtractor = regionExtractor;
            _rangeExtractor = rangeExtractor;
            _dedentingService = dedentingService;
        }

        public string GetRegions(string language, string src, List<Region> regions, string fileContent, IMarkdownToken token)
        {
            string[] fileLines = GetFileLines(fileContent);

            Dictionary<string, List<DfmTagNameResolveResult>> resolveResultsMap = _regionExtractor.GetResolveResultsMap(fileLines, language, src, token);

            StringBuilder result = new StringBuilder();

            foreach (Region region in regions)
            {
                string[] regionLines = _regionExtractor.ExtractRegion(fileLines, resolveResultsMap, region, token);
                AppendLines(result, regionLines, region);
            }

            return result.ToString();
        }

        public string GetRanges(List<Range> ranges, string fileContent, IMarkdownToken token)
        {
            StringBuilder result = new StringBuilder();

            // Split 
            string[] fileLines = GetFileLines(fileContent);

            foreach (Range range in ranges)
            {
                string[] rangeLines = _rangeExtractor.ExtractRange(fileLines, range, token);
                AppendLines(result, rangeLines, range);
            }

            return result.ToString();
        }

        public string GetFile(IncludeFileOptions includeFileOptions, string fileContent)
        {
            StringBuilder result = new StringBuilder();
            string[] fileLines = GetFileLines(fileContent);
            AppendLines(result, fileLines, includeFileOptions);

            return result.ToString();
        }

        private void AppendLines(StringBuilder result, string[] lines, ClippingArea clippingArea)
        {
            _dedentingService.Dedent(lines, clippingArea);

            if (!string.IsNullOrEmpty(clippingArea.Before))
            {
                result.Append(clippingArea.Before);
            }

            foreach (string line in lines)
            {
                result.AppendLine(line);
            }

            if (!string.IsNullOrEmpty(clippingArea.After))
            {
                result.Append(clippingArea.After);
            }
        }

        private string[] GetFileLines(string content)
        {
            return content.Split(
                new[] { "\r\n", "\r", "\n" },
                // Preserve empty lines
                StringSplitOptions.None
            );
        }
    }
}
