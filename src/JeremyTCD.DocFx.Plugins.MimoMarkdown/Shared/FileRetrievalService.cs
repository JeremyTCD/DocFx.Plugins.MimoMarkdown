using Microsoft.DocAsCode.Common;
using Microsoft.DocAsCode.Dfm;
using Microsoft.DocAsCode.MarkdownLite;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace JeremyTCD.DocFx.Plugins.MimoMarkdown
{
    public class FileRetrievalService
    {
        // https://andrewlock.net/making-getoradd-on-concurrentdictionary-thread-safe-using-lazy/
        private ConcurrentDictionary<string, Lazy<string>> _cache = new ConcurrentDictionary<string, Lazy<string>>();

        private HttpClient _httpClient;

        public FileRetrievalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public string GetFile(string src, IMarkdownToken token, MarkdownBlockContext context)
        {
            return _cache.
                GetOrAdd(src, new Lazy<string>(() => GetFileCore(src, token, context))).
                Value;
        }

        // TODO would be great if this could be async, but methods down the stack are not async
        private string GetFileCore(string src, IMarkdownToken token, MarkdownBlockContext context)
        {
            // Remote
            bool isUrl = Uri.TryCreate(src, UriKind.Absolute, out Uri uriResult) && (uriResult?.Scheme == Uri.UriSchemeHttp || uriResult?.Scheme == Uri.UriSchemeHttps);
            if (isUrl)
            {
                int remainingTries = 3;

                do
                {
                    remainingTries--;

                    try
                    {
                        HttpResponseMessage response = _httpClient.GetAsync(uriResult).Result;
                        string result = response.Content.ReadAsStringAsync().Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new HttpRequestException($"Request to \"{src}\" failed with status code \"{response.StatusCode}\" and message \"{result}\".");
                        }

                        return result;
                    }
                    catch(TaskCanceledException taskCanceledException)
                    {
                        if (remainingTries > 0)
                        {
                            Logger.LogWarning($"Request to \"{src}\" timed out: {taskCanceledException.Message}, {remainingTries} attempts remaining.");
                        }
                        else
                        {
                            Logger.LogError($"Multiple attempts to get data from \"{src}\" have failed, please ensure that the url is valid and that your network connection is stable.", 
                                file: token.SourceInfo.File, line: token.SourceInfo.LineNumber.ToString());
                            throw;
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.LogError($"Unable to retrieve or decode data from \"{src}\": {exception.Message}", file: token.SourceInfo.File, line: token.SourceInfo.LineNumber.ToString());
                        throw;
                    }
                }
                while (remainingTries > 0);
            }

            // Local
            // src is an absolute path
            if (uriResult != null)
            {
                Logger.LogError($"Reading from absolute urls such as \"{src}\" is not supported.", file: token.SourceInfo.File, line: token.SourceInfo.LineNumber.ToString());
                throw new InvalidOperationException();
            }

            if (PathUtility.IsRelativePath(src))
            {
                string root = context.GetBaseFolder();
                string currentDirectory = Directory.GetParent(Path.Combine(root, token.SourceInfo.File)).FullName;
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

            // Raw
            return src;
        }
    }
}
