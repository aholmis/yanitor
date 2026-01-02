using System.Collections.Immutable;

namespace Yanitor.Web.Domain.Models;

/// <summary>
/// Represents the users house configuration: selected item types in the house.
/// </summary>
public record HouseConfiguration
{
    /// <summary>
    /// The set of item Type identifiers that exist in the house.
    /// </summary>
    public HashSet<string> SelectedItemTypes { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// UTC timestamp when configuration was last modified.
    /// </summary>
    public DateTime LastModifiedAt { get; init; } = DateTime.UtcNow;
}
