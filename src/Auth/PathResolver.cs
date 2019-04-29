using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace Auth
{
    public static class PathResolver
    {
        public static string ResolveCertConfigPath(string configPath, IHostingEnvironment environment)
        {
            var start = new[] { "./", ".\\", "~/", "~\\" };
            if (start.Any(d => configPath.StartsWith(d)))
            {
                foreach (var item in start)
                {
                    configPath = configPath.TrimStart(item);
                }
                return Path.Combine(environment.ContentRootPath, configPath);
            }
            return configPath;
        }

        public static string TrimStart(this string target, string trimString)
        {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            while (result.StartsWith(trimString))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }
    }
}