using System;
using System.Linq;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class DedentingService
    {
        public void Dedent(string[] lines, ClippingArea clippingArea)
        {
            bool autoDedent = clippingArea.DedentLength < 0;
            int[] linesLeadingSpacesOrTabs = new int[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                int lineLeadingSpacesOrTabs = line.TakeWhile(c => char.IsWhiteSpace(c)).Count();
                linesLeadingSpacesOrTabs[i] = lineLeadingSpacesOrTabs;

                if (autoDedent && (lineLeadingSpacesOrTabs < clippingArea.DedentLength || clippingArea.DedentLength < 0))
                {
                    clippingArea.DedentLength = lineLeadingSpacesOrTabs;
                }
            }

            bool collapse = clippingArea.CollapseLength > 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                int lineLeadingSpacesOrTabs = linesLeadingSpacesOrTabs[i];

                if (lineLeadingSpacesOrTabs < clippingArea.DedentLength)
                {
                    lines[i] = lines[i].Substring(lineLeadingSpacesOrTabs);
                }
                else
                {
                    int lineDedentLength = clippingArea.DedentLength;

                    if (collapse)
                    {
                        int spareSpacesOrTabs = lineLeadingSpacesOrTabs - lineDedentLength;

                        lineDedentLength += spareSpacesOrTabs - (int)Math.Round((float)spareSpacesOrTabs / clippingArea.CollapseLength);
                    }

                    lines[i] = lines[i].Substring(lineDedentLength);
                }
            }
        }
    }
}
