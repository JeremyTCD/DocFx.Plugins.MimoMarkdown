using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.MarkdownLite;
using System;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class RangeExtractor
    {
        public string[] ExtractRange(string[] lines, Range range, IMarkdownToken token)
        {
            if (range.Start < 1 || range.End > lines.Length || range.End < range.Start)
            {
                Logger.LogError($"Invalid range [{range.Start}, {range.End}].", file: token.SourceInfo.File, line: token.SourceInfo.LineNumber.ToString());
                throw new InvalidOperationException();
            }

            int startIndex = range.Start - 1;
            int length = range.End - startIndex;
            string[] result = new string[length];
            Array.Copy(lines, startIndex, result, 0, length);

            return result;
        }
    }
}
