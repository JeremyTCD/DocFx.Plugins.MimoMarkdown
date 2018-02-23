using Microsoft.DocAsCode.MarkdownLite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    /// <summary>
    /// Has state - must be used as a singleton
    /// </summary>
    public class CodeBlockRenderingService
    {
        private ConcurrentDictionary<string, int> _codeBlockNums;
        private IDictionary<string, string> _languageDisplayNames;

        public CodeBlockRenderingService()
        {
            _codeBlockNums = new ConcurrentDictionary<string, int>();

            // TODO Not ideal to initialize here
            // Note: these are just the langauges supported by region extractor. This list should be configurable.
            IDictionary<string, IEnumerable<string>> _temp = new Dictionary<string, IEnumerable<string>>
            {
                { "ActionScript", new string[]{"actionscript", ".as"} },
                { "Arduino", new string[]{"arduino", ".ino"} },
                { "Assembly", new string[]{"assembly", "nasm", ".asm"}},
                { "Batch Script", new string[]{"batchfile", ".bat", ".cmd"}},
                { "C++", new string[]{"cpp", "c", "c++", "objective-c", "obj-c", "objc", "objectivec", ".c", ".cpp", ".h", ".hpp", ".cc"}},
                { "C#", new string[]{"csharp", "cs", ".cs"}},
                { "CUDA", new string[]{"cuda", ".cu", ".cuh"}},
                { "D", new string[]{"d", "dlang", ".d"}},
                { "Erlang", new string[]{"erlang", ".erl"}},
                { "F#", new string[]{"fsharp", "fs", ".fs", ".fsi", ".fsx"}},
                { "Go", new string[]{"go", "golang", ".go"}},
                { "Haskell", new string[]{"haskell", ".hs"}},
                { "HTML", new string[]{"html", ".html", ".jsp", ".asp", ".aspx", ".ascx"}},
                { "Markdown", new string[]{"markdown", ".md"}},
                { "CSHTML", new string[]{"cshtml", ".cshtml", "aspx-cs", "aspx-csharp"}},
                { "VBHTML", new string[]{"vbhtml", ".vbhtml", "aspx-vb"}},
                { "Java", new string[]{"java", ".java"}},
                { "Javascript", new string[]{"javascript", "js", "node", ".js"}},
                { "Lisp", new string[]{"lisp", ".lisp", ".lsp"}},
                { "Lua", new string[]{"lua", ".lua"}},
                { "MATLAB", new string[]{"matlab", ".matlab"}},
                { "Pascal", new string[]{"pascal", ".pas"}},
                { "Perl", new string[]{"perl", ".pl"}},
                { "PHP", new string[]{"php", ".php"}},
                { "PowerShell", new string[]{"powershell", "posh", ".ps1"}},
                { "Processing", new string[]{"processing", ".pde"}},
                { "Python", new string[]{"python", ".py"}},
                { "R", new string[]{"r", ".r"}},
                { "Ruby", new string[]{"ruby", "ru", ".ru", ".ruby"}},
                { "Rust", new string[]{"rust", ".rs"}},
                { "Scala", new string[]{"scala", ".scala"}},
                { "Bash", new string[]{"shell", "sh", "bash", ".sh", ".bash"}},
                { "Smalltalk", new string[]{"smalltalk", ".st"}},
                { "SQL", new string[]{"sql", ".sql"}},
                { "Swift", new string[]{"swift", ".swift"}},
                { "Typescript", new string[]{"typescript", "ts", ".ts"}},
                { "XAML", new string[]{"xaml", ".xaml"}},
                { "XML", new string[]{"xml", "xsl", "xslt", "xsd", "wsdl", ".xml", ".csdl", ".edmx", ".xsl", ".xslt", ".xsd", ".wsdl"}},
                { "VB", new string[]{"vb", "vbnet", "vbscript", ".vb", ".bas", ".vbs", ".vba"}}
            };

            _languageDisplayNames = new Dictionary<string, string>();

            foreach (KeyValuePair<string, IEnumerable<string>> keyValuePair in _temp)
            {
                foreach (string alias in keyValuePair.Value)
                {
                    _languageDisplayNames.Add(alias, keyValuePair.Key);
                }
            }
        }

        public void AppendCodeBlock(StringBuilder result,
            string src,
            string fileName,
            string content,
            string blockID,
            string title,
            bool showLineNumbers,
            bool highlight,
            string language,
            string languagePrefix)
        {
            // AddOrUpdate uses an optimistic concurrency model. It grabs the current value, runs the delegate and tries to update the initial value. If the initial value changed while the delegate was running,
            // it runs the delegate again, using the new value - https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-add-and-remove-items.
            int num = _codeBlockNums.AddOrUpdate(fileName, 1, (key, codeBlockNum) => ++codeBlockNum);
            blockID = !String.IsNullOrEmpty(blockID) ? blockID : $"code-block-{num}";
            string codeElementID = $"{blockID}-code";
            string encodedContent = StringHelper.HtmlEncode(content);

            // TODO Title
            result.Append($"<div id=\"{blockID}\" class=\"code-block\">\n");

            result.Append("<header>\n");

            if (!string.IsNullOrEmpty(title))
            {
                result.Append("<span class=\"code-block-title\">\n");
                result.Append(GetTitle(title, src, language));
                result.Append("</span>\n");
            }

            // Firefox does not support hover events for svgs within button elements, so use a div and assign 'button' to its role attribute
            // data-clipboard-target used by clipboard.js. title used by tippy.js
            result.Append($"<div data-clipboard-target=\"#{codeElementID}\" role=\"button\" title=\"Code copied\">\n");
            result.Append("<svg>\n");
            result.Append("<use xlink:href=\"#material-design-copy\"></use>\n");
            result.Append("</svg>\n");
            result.Append("</div>\n"); // button
            result.Append("</header>\n");

            if (showLineNumbers)
            {
                result.Append("<div>\n");
                AppendLineNumbers(result, encodedContent);
            }

            // No unecessary white space within pre element
            result.Append("<pre class=\"code\">");
            string noHighlight = "no-highlight";
            string hljsLanguageClass = !string.IsNullOrEmpty(language) && highlight && language != noHighlight ?
                languagePrefix + StringHelper.Escape(language, true) :
                noHighlight;
            result.Append($"<code id=\"{codeElementID}\" class=\"{hljsLanguageClass}\">");

            result.Append(encodedContent);
            result.Append("</code>");
            result.Append("</pre>\n");

            if (showLineNumbers)
            {
                result.Append("</div>\n");
            }
            result.Append("</div>\n"); // code block
        }

        private string GetTitle(string title, string src, string language)
        {
            if (title.Contains("<fileName>"))
            {
                string fileName = Path.GetFileName(src);
                title = title.Replace("<fileName>", fileName);
            }

            if (title.Contains("<language>"))
            {
                string languageAlias = string.IsNullOrEmpty(language) ? Path.GetExtension(src) : language;
                if (!_languageDisplayNames.TryGetValue(languageAlias, out string languageDisplayName))
                {
                    // TODO catch this at the renderer level and log file name and token line number
                    throw new InvalidOperationException($"No display name for language alias {languageAlias}");
                }
                title = title.Replace("<language>", languageDisplayName);
            }

            return title;
        }

        private void AppendLineNumbers(StringBuilder result, string encodedContent)
        {
            int numLines = GetNumLines(encodedContent);
            int maxNumDigits = GetNumberNumDigits(numLines);

            result.Append("<pre class=\"line-numbers\">");
            for (int i = 1; i <= numLines; i++)
            {
                // Right align
                int numDigits = GetNumberNumDigits(i);
                for (int j = 0; j < maxNumDigits - numDigits; j++)
                {
                    result.Append(' ');
                }

                result.Append(i);
                if (i != numLines)
                {
                    result.Append('\n');
                }
            }
            result.Append("</pre>");
        }

        private int GetNumberNumDigits(int num)
        {
            int result = 0;

            do
            {
                result++;
                num /= 10;
            }
            while (num != 0);

            return result;
        }

        private int GetNumLines(string str)
        {
            int result = 1;

            for (int i = 0; i < str.Length; i++)
                switch (str[i])
                {
                    case '\r':
                        {
                            result++;

                            // \r\n counts as 1 line break
                            if (i + 1 < str.Length && str[i + 1] == '\n')
                            {
                                i++;
                            }

                            break;
                        }
                    case '\n':
                        result++;
                        break;
                }

            return result;
        }
    }
}
