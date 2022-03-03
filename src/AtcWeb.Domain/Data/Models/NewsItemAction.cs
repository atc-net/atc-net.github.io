using System.ComponentModel;

namespace AtcWeb.Domain.Data.Models;

public enum NewsItemAction
{
    Organization,

    [Description("New repository")]
    RepositoryNew,

    [Description("New feature")]
    FeatureNew,
}