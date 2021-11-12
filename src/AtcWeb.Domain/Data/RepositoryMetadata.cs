using System;
using System.Collections.Generic;
using System.Linq;

namespace AtcWeb.Domain.Data
{
    public static class RepositoryMetadata
    {
        private static readonly List<Tuple<string, string>> ResponsibleMembers = new List<Tuple<string, string>>
        {
            Tuple.Create("atc", "davidkallesen"),
            Tuple.Create("atc", "perkops"),
            Tuple.Create("atc-auto-formatter", "rickykaare"),
            Tuple.Create("atc-azure-options", "kimlundjohansen "),
            Tuple.Create("atc-coding-rules", "davidkallesen"),
            Tuple.Create("atc-coding-rules", "perkops"),
            Tuple.Create("atc-coding-rules-updater", "davidkallesen"),
            Tuple.Create("atc-coding-rules-updater", "perkops"),
            Tuple.Create("atc-cosmos", "rickykaare"),
            Tuple.Create("atc-cosmos-eventstore", "LarsSkovslund"),
            Tuple.Create("atc-cosmos-sql-api-repository", "davidkallesen"),
            Tuple.Create("atc-cosmos-sql-api-repository", "perkops"),
            Tuple.Create("atc-data-platform", "mrmasterplan"),
            Tuple.Create("atc-docs", "davidkallesen"),
            Tuple.Create("atc-docs", "perkops"),
            Tuple.Create("atc-iot-digital-twin", "perkops"),
            Tuple.Create("atc-rest-api-generator", "davidkallesen"),
            Tuple.Create("atc-rest-api-generator", "perkops"),
            Tuple.Create("atc-rest-client", "davidkallesen"),
            Tuple.Create("atc-rest-client", "perkops"),
            Tuple.Create("atc-rest-client", "LarsSkovslund"),
            Tuple.Create("atc-rest-client", "egil"),
            Tuple.Create("atc-snippets", "perkops"),
            Tuple.Create("atc-snippets", "Mikael O. Hansen"),
            Tuple.Create("atc-snippets", "lupusbytes"),
            Tuple.Create("atc-test", "rickykaare"),
            Tuple.Create("atc-test", "egil"),
            Tuple.Create("atc-wpf", "davidkallesen"),
        };

        public static string[] GetResponsibleMembersByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return ResponsibleMembers
                    .Select(x => x.Item2)
                    .OrderBy(x => x)
                    .ToArray();
            }

            return ResponsibleMembers
                .Where(x => x.Item1.Equals(name, StringComparison.Ordinal))
                .Select(x => x.Item2)
                .OrderBy(x => x)
                .ToArray();
        }

        public static bool HasNotRequiredResponsibleMembersByName(string name)
        {
            return GetResponsibleMembersByName(name).Length < 2;
        }
    }
}