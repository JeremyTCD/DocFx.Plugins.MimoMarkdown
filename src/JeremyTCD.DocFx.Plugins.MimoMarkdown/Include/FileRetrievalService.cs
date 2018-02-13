using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.MarkdownLite;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class FileRetrievalService
    {
        // TODO would be great if this could be async
        public string GetFile(IncludeFileToken token, MarkdownBlockContext context)
        {
            // TODO is there some way to get line nubmer?
            string src = token.Options.Src;
            string currentFile = (context.Variables["FilePathStack"] as ImmutableStack<string>).Peek();

            // Remote
            bool isUrl = Uri.TryCreate(src, UriKind.Absolute, out Uri uriResult) && (uriResult?.Scheme == Uri.UriSchemeHttp || uriResult?.Scheme == Uri.UriSchemeHttps);

            if (isUrl)
            {
                using (var client = new WebClient())
                {
                    try
                    {
                        byte[] data = client.DownloadData(uriResult);
                        return Encoding.UTF8.GetString(data);
                    }
                    catch (Exception exception)
                    {
                        Logger.LogError($"Unable to retrieve or decode data from \"{src}\": {exception.Message}", file: token.SourceInfo.File, line: token.SourceInfo.LineNumber.ToString());
                        throw;
                    }
                }
            }

            // Local
            // src is an absolute path
            if (uriResult != null)
            {
                Logger.LogError($"Reading from absolute urls such as \"{src}\" is not supported.", file: token.SourceInfo.File, line: token.SourceInfo.LineNumber.ToString());
                throw new InvalidOperationException();
            }
            // Assume src is a valid relative path
            string root = (context.Variables["BaseFolder"] as string);
            // TODO is it safe to assume that current file is always valid?
            string currentDirectory = Directory.GetParent(Path.Combine(root, currentFile)).FullName;
            string file = Path.Combine(currentDirectory, src);
            try
            {
                return File.ReadAllText(file);
            }
            catch (Exception exception)
            {
                Logger.LogError($"Unable to read from \"{src}\": {exception.Message}", file: token.SourceInfo.File, line: token.SourceInfo.LineNumber.ToString());
                throw;
            }
        }
    }
}
