using System;
using System.Collections.Generic;

namespace AtcWeb.Domain.GitHub.Models
{
    public class DirectoryMetadata
    {
        public List<DirectoryItem> Directories { get; set; } = new List<DirectoryItem>();

        public List<FileItem> Files { get; set; } = new List<FileItem>();
    }
}