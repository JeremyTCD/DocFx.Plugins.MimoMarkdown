using Microsoft.DocAsCode.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class FileClippingService
    {
        private TagContentExtractor _tagContentExtractor;

        public FileClippingService(TagContentExtractor tagContentExtractor)
        {
            _tagContentExtractor = tagContentExtractor;
        }

        public string AppendTagContents(StringBuilder result, IncludeFileToken token, string fileContent)
        {
            // Split
            string[] fileLines = GetFileLines(fileContent);

            _tagContentExtractor.AppendTagContents(result, token, fileLines);

            return null;
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

        public void AppendRange(StringBuilder result, Range range, string[] lines)
        {
            bool autoDedent = range.Dedent < 0;
            List<string> linesForRange = new List<string>(range.End - range.Start + 1);
            for (int i = range.Start - 1; i < range.End; i++)
            {
                if (autoDedent)
                {
                    // Assume that all lines either begin with spaces or tabs
                    int numSpaces = lines[i].TakeWhile(c => char.IsWhiteSpace(c)).Count();
                    range.Dedent = numSpaces < range.Dedent || range.Dedent < 0 ? numSpaces : range.Dedent;
                }

                linesForRange.Add(lines[i]);
            }

            foreach(string line in linesForRange)
            {
                // remove whitespace from start of line
                result.AppendLine(line.Substring(range.Dedent));
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
