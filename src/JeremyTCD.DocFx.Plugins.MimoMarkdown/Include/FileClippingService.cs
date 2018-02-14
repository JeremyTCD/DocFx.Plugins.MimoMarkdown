using Microsoft.DocAsCode.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class FileClippingService
    {
        private RegionExtractor _regionExtractor;

        public FileClippingService(RegionExtractor regionExtractor)
        {
            _regionExtractor = regionExtractor;
        }

        public void AppendRegions(StringBuilder result, IncludeFileToken token, string fileContent)
        {
            // Split
            string[] fileLines = GetFileLines(fileContent);

            _regionExtractor.AppendRegions(result, token, fileLines);
        }

        public void AppendRanges(StringBuilder result, IncludeFileToken token, string fileContent)
        {
            // Split 
            string[] fileLines = GetFileLines(fileContent);

            foreach (Range range in token.Options.Ranges)
            {
                if (range.Start - 1 < 0 || range.End > fileLines.Length
                    || range.End < range.Start)
                {
                    Logger.LogError($"Invalid range [{range.Start}, {range.End}].", file: token.SourceInfo.File, line: token.SourceInfo.LineNumber.ToString());
                    throw new InvalidOperationException();
                }

                AppendRange(result, range, fileLines);
            }
        }

        public void AppendRange(StringBuilder result, Range range, string[] fileLines)
        {
            bool autoDedent = range.DedentLength < 0;
            List<string> linesForRange = new List<string>(range.End - range.Start + 1);
            for (int i = range.Start - 1; i < range.End; i++)
            {
                if (autoDedent)
                {
                    // Assume that all lines either begin with spaces or tabs
                    int numSpaces = fileLines[i].TakeWhile(c => char.IsWhiteSpace(c)).Count();
                    range.DedentLength = numSpaces < range.DedentLength || range.DedentLength < 0 ? numSpaces : range.DedentLength;
                }

                linesForRange.Add(fileLines[i]);
            }

            foreach (string line in linesForRange)
            {
                // remove whitespace from start of line
                result.AppendLine(line.Substring(range.DedentLength));
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
