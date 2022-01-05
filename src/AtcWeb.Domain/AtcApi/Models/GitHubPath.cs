using System;
using System.Linq;

namespace AtcWeb.Domain.AtcApi.Models
{
    public class GitHubPath
    {
        public string Path { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Sha { get; set; } = string.Empty;

        public int Size { get; set; }

        public bool IsDirectory => "tree".Equals(Type, StringComparison.Ordinal);

        public bool IsFile => "blob".Equals(Type, StringComparison.Ordinal);

        public string GetFileName()
        {
            if (!IsFile)
            {
                return string.Empty;
            }

            if (!Path.Contains('/', StringComparison.Ordinal))
            {
                return Path;
            }

            return Path
                .Split('/')
                .Last();
        }

        public string GetFileExtension()
        {
            var fileName = GetFileName();
            if (string.IsNullOrEmpty(fileName) ||
                !fileName.Contains('.', StringComparison.Ordinal))
            {
                return string.Empty;
            }

            return fileName
                .Split('.')
                .Last();
        }

        public override string ToString()
            => $"{nameof(Path)}: {Path}, {nameof(Type)}: {Type}, {nameof(Size)}: {Size}, {nameof(IsDirectory)}: {IsDirectory}";
    }
}