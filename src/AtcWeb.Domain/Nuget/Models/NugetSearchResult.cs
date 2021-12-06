using System.Collections.Generic;

namespace AtcWeb.Domain.Nuget.Models
{
    public class NugetSearchResult
    {
        public int TotalHits { get; set; }

        public List<NugetSearchResultDataPackageMetadata> Data { get; set; }

        public override string ToString()
            => $"{nameof(TotalHits)}: {TotalHits}, {nameof(Data)}: {Data}";
    }
}