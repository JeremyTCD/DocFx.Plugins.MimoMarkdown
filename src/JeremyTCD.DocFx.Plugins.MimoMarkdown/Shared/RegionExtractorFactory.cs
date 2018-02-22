using Microsoft.DocAsCode.Dfm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    /// <summary>
    /// Loosely based on - DocFx's TagNameBlockPathQueryOption.cs.
    /// </summary>
    public class RegionExtractorFactory
    {
        // C family code snippet comment block: // <[/]snippetname>
        private static readonly Regex CFamilyCodeSnippetCommentStartLineRegex = new Regex(@"^\s*\/{2}\s*\<\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CFamilyCodeSnippetCommentEndLineRegex = new Regex(@"^\s*\/{2}\s*\<\s*\/\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Basic family code snippet comment block: ' <[/]snippetname>
        private static readonly Regex BasicFamilyCodeSnippetCommentStartLineRegex = new Regex(@"^\s*\'\s*\<\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex BasicFamilyCodeSnippetCommentEndLineRegex = new Regex(@"^\s*\'\s*\<\s*\/\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Markup language family code snippet block: <!-- <[/]snippetname> -->
        private static readonly Regex MarkupLanguageFamilyCodeSnippetCommentStartLineRegex = new Regex(@"^\s*\<\!\-{2}\s*\<\s*(?<name>[\w\.\-]+)\s*\>\s*\-{2}\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex MarkupLanguageFamilyCodeSnippetCommentEndLineRegex = new Regex(@"^\s*\<\!\-{2}\s*\<\s*\/\s*(?<name>[\w\.\-]+)\s*\>\s*\-{2}\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Sql family code snippet block: -- <[/]snippetname>
        private static readonly Regex SqlFamilyCodeSnippetCommentStartLineRegex = new Regex(@"^\s*\-{2}\s*\<\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SqlFamilyCodeSnippetCommentEndLineRegex = new Regex(@"^\s*\-{2}\s*\<\s*\/\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Script family snippet comment block: # <[/]snippetname>
        private static readonly Regex ScriptFamilyCodeSnippetCommentStartLineRegex = new Regex(@"^\s*#\s*\<\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ScriptFamilyCodeSnippetCommentEndLineRegex = new Regex(@"^\s*#\s*\<\s*\/\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Lisp code snippet comment block: rem <[/]snippetname>
        private static readonly Regex BatchFileCodeSnippetRegionStartLineRegex = new Regex(@"^\s*rem\s+\<\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex BatchFileCodeSnippetRegionEndLineRegex = new Regex(@"^\s*rem\s+\<\s*\/\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // C# code snippet region block: start -> #region snippetname, end -> #endregion
        private static readonly Regex CSharpCodeSnippetRegionStartLineRegex = new Regex(@"^\s*#\s*region(?:\s+(?<name>.+?))?\s*$", RegexOptions.Compiled);
        private static readonly Regex CSharpCodeSnippetRegionEndLineRegex = new Regex(@"^\s*#\s*endregion(?:\s.*)?$", RegexOptions.Compiled);

        // Erlang code snippet comment block: % <[/]snippetname>
        private static readonly Regex ErlangCodeSnippetRegionStartLineRegex = new Regex(@"^\s*%\s*\<\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ErlangCodeSnippetRegionEndLineRegex = new Regex(@"^\s*%\s*\<\s*\/\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Lisp code snippet comment block: ; <[/]snippetname>
        private static readonly Regex LispCodeSnippetRegionStartLineRegex = new Regex(@"^\s*;\s*\<\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex LispCodeSnippetRegionEndLineRegex = new Regex(@"^\s*;\s*\<\s*\/\s*(?<name>[\w\.]+)\s*\>\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // VB code snippet Region block: start -> # Region "snippetname", end -> # End Region
        private static readonly Regex VBCodeSnippetRegionRegionStartLineRegex = new Regex(@"^\s*#\s*Region(?:\s+(?<name>.+?))?\s*$", RegexOptions.Compiled);
        private static readonly Regex VBCodeSnippetRegionRegionEndLineRegex = new Regex(@"^\s*#\s*End\s+Region(?:\s.*)?$", RegexOptions.Compiled);

        private IDictionary<string, List<ICodeSnippetExtractor>> _languageExtractorsMap = new Dictionary<string, List<ICodeSnippetExtractor>>();
        private IDictionary<string, List<string>> _languageAliasesMap = new Dictionary<string, List<string>>();

        public RegionExtractor BuildRegionExtractor()
        {
            // Extractors
            AddExtractor(
                new FlatNameCodeSnippetExtractor(BasicFamilyCodeSnippetCommentStartLineRegex, BasicFamilyCodeSnippetCommentEndLineRegex),
                "vb", "vbhtml");
            AddExtractor(
                new FlatNameCodeSnippetExtractor(CFamilyCodeSnippetCommentStartLineRegex, CFamilyCodeSnippetCommentEndLineRegex),
                "actionscript", "arduino", "assembly", "cpp", "csharp", "cshtml", "cuda", "d", "fsharp", "go", "java", "javascript", "pascal", "php", "processing", "rust", "scala", "smalltalk", "swift", "typescript");
            AddExtractor(
                new FlatNameCodeSnippetExtractor(MarkupLanguageFamilyCodeSnippetCommentStartLineRegex, MarkupLanguageFamilyCodeSnippetCommentEndLineRegex),
                "xml", "xaml", "html", "cshtml", "vbhtml", "markdown");
            AddExtractor(
                new FlatNameCodeSnippetExtractor(SqlFamilyCodeSnippetCommentStartLineRegex, SqlFamilyCodeSnippetCommentEndLineRegex),
                "haskell", "lua", "sql");
            AddExtractor(
                new FlatNameCodeSnippetExtractor(ScriptFamilyCodeSnippetCommentStartLineRegex, ScriptFamilyCodeSnippetCommentEndLineRegex),
                "perl", "powershell", "python", "r", "ruby", "shell");
            AddExtractor(
                new FlatNameCodeSnippetExtractor(BatchFileCodeSnippetRegionStartLineRegex, BatchFileCodeSnippetRegionEndLineRegex),
                "batchfile");
            AddExtractor(
                new RecursiveNameCodeSnippetExtractor(CSharpCodeSnippetRegionStartLineRegex, CSharpCodeSnippetRegionEndLineRegex),
                "csharp", "cshtml");
            AddExtractor(
                new FlatNameCodeSnippetExtractor(ErlangCodeSnippetRegionStartLineRegex, ErlangCodeSnippetRegionEndLineRegex),
                "erlang", "matlab");
            AddExtractor(
                new FlatNameCodeSnippetExtractor(LispCodeSnippetRegionStartLineRegex, LispCodeSnippetRegionEndLineRegex),
                "lisp");
            AddExtractor(
                new RecursiveNameCodeSnippetExtractor(VBCodeSnippetRegionRegionStartLineRegex, VBCodeSnippetRegionRegionEndLineRegex),
                "vb", "vbhtml");

            // Aliases
            AddAliases("actionscript", ".as");
            AddAliases("arduino", ".ino");
            AddAliases("assembly", "nasm", ".asm");
            AddAliases("batchfile", ".bat", ".cmd");
            AddAliases("cpp", "c", "c++", "objective-c", "obj-c", "objc", "objectivec", ".c", ".cpp", ".h", ".hpp", ".cc");
            AddAliases("csharp", "cs", ".cs");
            AddAliases("cuda", ".cu", ".cuh");
            AddAliases("d", "dlang", ".d");
            AddAliases("erlang", ".erl");
            AddAliases("fsharp", "fs", ".fs", ".fsi", ".fsx");
            AddAliases("go", "golang", ".go");
            AddAliases("haskell", ".hs");
            AddAliases("html", ".html", ".jsp", ".asp", ".aspx", ".ascx");
            AddAliases("markdown", ".md");
            AddAliases("cshtml", ".cshtml", "aspx-cs", "aspx-csharp");
            AddAliases("vbhtml", ".vbhtml", "aspx-vb");
            AddAliases("java", ".java");
            AddAliases("javascript", "js", "node", ".js");
            AddAliases("lisp", ".lisp", ".lsp");
            AddAliases("lua", ".lua");
            AddAliases("matlab", ".matlab");
            AddAliases("pascal", ".pas");
            AddAliases("perl", ".pl");
            AddAliases("php", ".php");
            AddAliases("powershell", "posh", ".ps1");
            AddAliases("processing", ".pde");
            AddAliases("python", ".py");
            AddAliases("r", ".r");
            AddAliases("ruby", "ru", ".ru", ".ruby");
            AddAliases("rust", ".rs");
            AddAliases("scala", ".scala");
            AddAliases("shell", "sh", "bash", ".sh", ".bash");
            AddAliases("smalltalk", ".st");
            AddAliases("sql", ".sql");
            AddAliases("swift", ".swift");
            AddAliases("typescript", "ts", ".ts");
            AddAliases("xaml", ".xaml");
            AddAliases("xml", "xsl", "xslt", "xsd", "wsdl", ".xml", ".csdl", ".edmx", ".xsl", ".xslt", ".xsd", ".wsdl");
            AddAliases("vb", "vbnet", "vbscript", ".vb", ".bas", ".vbs", ".vba");

            return new RegionExtractor(CreateKeyExtractorsMap());
        }

        private Dictionary<string, List<ICodeSnippetExtractor>> CreateKeyExtractorsMap()
        {
            Dictionary<string, List<ICodeSnippetExtractor>> result = new Dictionary<string, List<ICodeSnippetExtractor>>();

            foreach (KeyValuePair<string, List<string>> languageAliases in _languageAliasesMap)
            {
                string language = languageAliases.Key;

                if (!_languageExtractorsMap.TryGetValue(language, out List<ICodeSnippetExtractor> extractors))
                {
                    throw new InvalidOperationException($"Language {language} has no corresponding extractors.");
                }

                foreach (string alias in languageAliases.Value)
                {
                    result.Add(alias, extractors);
                }

                result.Add(language, extractors);
            }

            return result;
        }

        private void AddAliases(string language, params string[] newLanguageAliases)
        {
            if (_languageAliasesMap.TryGetValue(language, out List<string> existingLanguageAliases))
            {
                existingLanguageAliases.AddRange(newLanguageAliases);
            }
            else
            {
                _languageAliasesMap.Add(language, newLanguageAliases.ToList());
            }
        }

        private void AddExtractor(ICodeSnippetExtractor extractor, params string[] languages)
        {
            foreach (string language in languages)
            {
                if (_languageExtractorsMap.TryGetValue(language, out List<ICodeSnippetExtractor> existingLanguageExtractors))
                {
                    existingLanguageExtractors.Add(extractor);
                }
                else
                {
                    _languageExtractorsMap.Add(language, new List<ICodeSnippetExtractor> { extractor });
                }
            }
        }
    }
}
