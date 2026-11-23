using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SeptemberCountdown
{
    internal sealed class UpdateInfo
    {
        internal string Version;
        internal string Url;
        internal string Page;
        internal string Notes;
        internal string Source;
        internal bool FromLocal;
        internal string Notice;
    }

    internal static class UpdateService
    {
        internal const string DefaultFeedUrl =
            "https://raw.githubusercontent.com/wyft2/SeptemberCountdown/main/version.json";

        // GitHub 直连、jsDelivr、Fastly、国内 GitHub 代理、Gitee 镜像、GitHub Pages。
        // 检查时会并行尝试，优先返回版本号最高的合法 version.json。
        internal static readonly string[] DefaultSources = new string[]
        {
            DefaultFeedUrl,
            "https://cdn.jsdelivr.net/gh/wyft2/SeptemberCountdown@main/version.json",
            "https://fastly.jsdelivr.net/gh/wyft2/SeptemberCountdown@main/version.json",
            "https://gh-proxy.com/https://raw.githubusercontent.com/wyft2/SeptemberCountdown/main/version.json",
            "https://gitee.com/wyft2/SeptemberCountdown/raw/main/version.json",
            "https://wyft2.github.io/SeptemberCountdown/version.json",
            "https://wyft2.github.io/Echo-Layer-Page-7/SeptemberCountdown/version.json"
        };

        internal static Version CurrentVersion
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return Normalize(version);
            }
        }

        internal static UpdateInfo Check(string customFeed)
        {
            List<string> sources = new List<string>();
            if (!string.IsNullOrWhiteSpace(customFeed))
            {
                sources.Add(customFeed.Trim());
            }
            for (int i = 0; i < DefaultSources.Length; i++)
            {
                if (!sources.Contains(DefaultSources[i]))
                {
                    sources.Add(DefaultSources[i]);
                }
            }

            List<Task<FetchResult>> tasks = new List<Task<FetchResult>>();
            for (int i = 0; i < sources.Count; i++)
            {
                string url = sources[i];
                tasks.Add(Task.Run(delegate { return Fetch(url); }));
            }

            DateTime hardDeadline = DateTime.UtcNow.AddMilliseconds(5600);
            DateTime softDeadline = DateTime.MaxValue;
            string lastError = null;
            UpdateInfo best = null;
            Version bestVersion = null;

            while (tasks.Count > 0)
            {
                DateTime effectiveDeadline = softDeadline < hardDeadline ? softDeadline : hardDeadline;
                int remaining = (int)(effectiveDeadline - DateTime.UtcNow).TotalMilliseconds;
                if (remaining <= 0)
                {
                    break;
                }

                Task[] waitArray = new Task[tasks.Count];
                for (int i = 0; i < tasks.Count; i++)
                {
                    waitArray[i] = tasks[i];
                }

                int index = Task.WaitAny(waitArray, remaining);
                if (index < 0)
                {
                    break;
                }

                Task<FetchResult> task = tasks[index];
                tasks.RemoveAt(index);
                if (task.Status != TaskStatus.RanToCompletion)
                {
                    if (task.Exception != null)
                    {
                        lastError = task.Exception.GetBaseException().Message;
                    }
                    continue;
                }

                FetchResult result = task.Result;
                if (result == null)
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(result.Text))
                {
                    if (!string.IsNullOrWhiteSpace(result.Error))
                    {
                        lastError = result.Error;
                    }
                    continue;
                }

                UpdateInfo info = TryParse(result.Text, result.Url, false);
                if (info == null)
                {
                    lastError = "更新源返回的内容格式不正确：" + result.Url;
                    continue;
                }

                Version infoVersion = ParseVersion(info.Version);
                if (infoVersion == null)
                {
                    continue;
                }
                if (bestVersion == null || infoVersion > bestVersion)
                {
                    best = info;
                    bestVersion = infoVersion;
                }
                if (softDeadline == DateTime.MaxValue)
                {
                    // 已经拿到一个可用版本，再给镜像 700ms 的时间返回更高版本。
                    softDeadline = DateTime.UtcNow.AddMilliseconds(700);
                }
            }

            if (best != null)
            {
                return best;
            }

            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.json");
            if (File.Exists(localPath))
            {
                try
                {
                    UpdateInfo info = TryParse(File.ReadAllText(localPath, Encoding.UTF8), localPath, true);
                    if (info != null)
                    {
                        info.Notice = "远程更新源暂时无法访问，已使用程序目录下的 version.json 作为离线回退。";
                        if (!string.IsNullOrWhiteSpace(lastError))
                        {
                            info.Notice += "\n\n最后一次错误：" + lastError;
                        }
                        return info;
                    }
                }
                catch (Exception ex)
                {
                    lastError = ex.GetBaseException().Message;
                }
            }

            throw new InvalidOperationException(lastError ?? "无法连接更新服务器，请检查网络后重试。");
        }

        internal static Version ParseVersion(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string value = text.Trim();
            if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("V", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(1);
            }
            int cut = value.IndexOfAny(new char[] { '-', '+', ' ' });
            if (cut > 0)
            {
                value = value.Substring(0, cut);
            }

            Version parsed;
            if (!Version.TryParse(value, out parsed))
            {
                return null;
            }
            return Normalize(parsed);
        }

        private static Version Normalize(Version version)
        {
            if (version == null)
            {
                return new Version(0, 0, 0, 0);
            }
            return new Version(
                version.Major,
                version.Minor,
                version.Build < 0 ? 0 : version.Build,
                version.Revision < 0 ? 0 : version.Revision);
        }

        private static FetchResult Fetch(string url)
        {
            try
            {
                return new FetchResult { Url = url, Text = Download(url) };
            }
            catch (Exception ex)
            {
                return new FetchResult { Url = url, Error = ex.GetBaseException().Message };
            }
        }

        private static string Download(string url)
        {
            if (string.IsNullOrWhiteSpace(url) ||
                (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                 !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("更新源地址需要以 http:// 或 https:// 开头。");
            }

            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            catch
            {
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = "SeptemberCountdown/1.7";
            request.Accept = "application/json, text/plain, */*";
            request.Timeout = 4000;
            request.ReadWriteTimeout = 4000;
            request.AllowAutoRedirect = true;
            request.KeepAlive = false;
            if (url.IndexOf("127.0.0.1", StringComparison.Ordinal) >= 0 ||
                url.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                request.Proxy = null;
            }

            using (WebResponse response = request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static UpdateInfo TryParse(string text, string source, bool fromLocal)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string version = null;
            string url = null;
            string page = null;
            string notes = null;
            bool parsedJson = false;

            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> data =
                    serializer.Deserialize<Dictionary<string, object>>(text);
                if (data != null)
                {
                    version = GetString(data, "version");
                    url = GetString(data, "url");
                    page = GetString(data, "page");
                    notes = GetString(data, "notes");
                    parsedJson = !string.IsNullOrWhiteSpace(version);
                }
            }
            catch
            {
            }

            if (!parsedJson)
            {
                string[] lines = text.Replace("\r", "").Split('\n');
                if (lines.Length > 0)
                {
                    string[] parts = lines[0].Trim().Split(
                        new char[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0)
                    {
                        version = parts[0];
                    }
                    if (parts.Length > 1)
                    {
                        url = parts[1].Trim();
                    }
                }
                if (lines.Length > 1)
                {
                    string second = lines[1].Trim();
                    if (second.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                        second.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.IsNullOrWhiteSpace(url))
                        {
                            url = second;
                        }
                        if (lines.Length > 2)
                        {
                            notes = string.Join("\n", lines, 2, lines.Length - 2);
                        }
                    }
                    else
                    {
                        notes = string.Join("\n", lines, 1, lines.Length - 1);
                    }
                }
            }

            Version parsed = ParseVersion(version);
            if (parsed == null)
            {
                return null;
            }

            return new UpdateInfo
            {
                Version = parsed.ToString(3),
                Url = SafeUrl(url),
                Page = SafeUrl(page),
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                Source = source,
                FromLocal = fromLocal
            };
        }

        private static string GetString(Dictionary<string, object> data, string key)
        {
            foreach (KeyValuePair<string, object> pair in data)
            {
                if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Value == null
                        ? null
                        : Convert.ToString(pair.Value, CultureInfo.InvariantCulture);
                }
            }
            return null;
        }

        private static string SafeUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            string url = value.Trim();
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }
            return null;
        }

        private sealed class FetchResult
        {
            public string Url;
            public string Text;
            public string Error;
        }
    }
}
