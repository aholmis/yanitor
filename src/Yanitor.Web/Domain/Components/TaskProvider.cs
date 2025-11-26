namespace Yanitor.Web.Domain.Components;

/// <summary>
/// Provides predefined maintenance tasks for different types of house items.
/// </summary>
public interface ITaskProvider
{
    /// <summary>
    /// Gets predefined maintenance tasks for a specific item type.
    /// </summary>
    /// <param name="itemType">The type of house item (e.g., HVAC, Plumbing, Door).</param>
    /// <returns>A collection of maintenance tasks for the item type.</returns>
    IEnumerable<MaintenanceTask> GetTasksForItemType(string itemType);
}

/// <summary>
/// Default implementation of ITaskProvider that provides predefined maintenance tasks
/// for common house item types.
/// </summary>
public class TaskProvider : ITaskProvider
{
    private readonly Dictionary<string, List<MaintenanceTask>> _tasksByType;

    public TaskProvider()
    {
        _tasksByType = new Dictionary<string, List<MaintenanceTask>>(StringComparer.OrdinalIgnoreCase)
        {
            ["HVAC"] = new List<MaintenanceTask>
            {
                new MaintenanceTask
                {
                    Name = "Change Air Filter",
                    Description = "Replace HVAC air filter to maintain air quality and system efficiency.",
                    IntervalDays = 90
                },
                new MaintenanceTask
                {
                    Name = "Professional Inspection",
                    Description = "Schedule professional HVAC system inspection and maintenance.",
                    IntervalDays = 365
                },
                new MaintenanceTask
                {
                    Name = "Clean Vents and Ducts",
                    Description = "Clean air vents and inspect ductwork for debris or blockages.",
                    IntervalDays = 180
                }
            },
            ["Plumbing"] = new List<MaintenanceTask>
            {
                new MaintenanceTask
                {
                    Name = "Check for Leaks",
                    Description = "Inspect pipes, faucets, and connections for any signs of leaks or water damage.",
                    IntervalDays = 90
                },
                new MaintenanceTask
                {
                    Name = "Clean Drains",
                    Description = "Clean and flush drains to prevent clogs and buildup.",
                    IntervalDays = 180
                },
                new MaintenanceTask
                {
                    Name = "Test Water Pressure",
                    Description = "Check water pressure and adjust if necessary to prevent pipe damage.",
                    IntervalDays = 365
                }
            },
            ["Door"] = new List<MaintenanceTask>
            {
                new MaintenanceTask
                {
                    Name = "Lubricate Hinges",
                    Description = "Apply lubricant to door hinges to prevent squeaking and ensure smooth operation.",
                    IntervalDays = 180
                },
                new MaintenanceTask
                {
                    Name = "Check Weatherstripping",
                    Description = "Inspect and replace weatherstripping to maintain energy efficiency.",
                    IntervalDays = 365
                },
                new MaintenanceTask
                {
                    Name = "Tighten Hardware",
                    Description = "Check and tighten all door hardware including handles, locks, and hinges.",
                    IntervalDays = 180
                }
            },
            ["Window"] = new List<MaintenanceTask>
            {
                new MaintenanceTask
                {
                    Name = "Clean Windows",
                    Description = "Clean glass, frames, and tracks for optimal view and operation.",
                    IntervalDays = 90
                },
                new MaintenanceTask
                {
                    Name = "Check Seals",
                    Description = "Inspect window seals and caulking for air leaks and water intrusion.",
                    IntervalDays = 180
                },
                new MaintenanceTask
                {
                    Name = "Lubricate Tracks",
                    Description = "Clean and lubricate window tracks for smooth opening and closing.",
                    IntervalDays = 180
                }
            },
            ["Garage"] = new List<MaintenanceTask>
            {
                new MaintenanceTask
                {
                    Name = "Lubricate Moving Parts",
                    Description = "Lubricate garage door opener chain, rollers, and hinges.",
                    IntervalDays = 180
                },
                new MaintenanceTask
                {
                    Name = "Test Safety Features",
                    Description = "Test auto-reverse and sensor features for safety compliance.",
                    IntervalDays = 90
                },
                new MaintenanceTask
                {
                    Name = "Professional Inspection",
                    Description = "Schedule professional inspection of garage door system and springs.",
                    IntervalDays = 365
                }
            },
            ["Safety"] = new List<MaintenanceTask>
            {
                new MaintenanceTask
                {
                    Name = "Test Alarm",
                    Description = "Press test button to ensure alarm is functioning properly.",
                    IntervalDays = 30
                },
                new MaintenanceTask
                {
                    Name = "Replace Batteries",
                    Description = "Replace batteries in smoke detector or carbon monoxide detector.",
                    IntervalDays = 365
                },
                new MaintenanceTask
                {
                    Name = "Clean Sensor",
                    Description = "Gently vacuum or wipe sensor to ensure accurate detection.",
                    IntervalDays = 180
                }
            },
            ["Roof"] = new List<MaintenanceTask>
            {
                new MaintenanceTask
                {
                    Name = "Inspect Shingles",
                    Description = "Check for missing, damaged, or curling shingles and repair as needed.",
                    IntervalDays = 180
                },
                new MaintenanceTask
                {
                    Name = "Check for Moss",
                    Description = "Inspect and remove any moss or algae growth on roof surface.",
                    IntervalDays = 365
                },
                new MaintenanceTask
                {
                    Name = "Professional Inspection",
                    Description = "Schedule professional roof inspection for overall condition assessment.",
                    IntervalDays = 730
                }
            },
            ["Exterior"] = new List<MaintenanceTask>
            {
                new MaintenanceTask
                {
                    Name = "Clean Gutters",
                    Description = "Remove leaves and debris from gutters and downspouts.",
                    IntervalDays = 180
                },
                new MaintenanceTask
                {
                    Name = "Inspect for Damage",
                    Description = "Check exterior for cracks, loose sections, or signs of wear.",
                    IntervalDays = 90
                },
                new MaintenanceTask
                {
                    Name = "Paint Touch-Up",
                    Description = "Inspect paint and apply touch-ups where needed to prevent deterioration.",
                    IntervalDays = 365
                }
            },
            ["Insulation"] = new List<MaintenanceTask>
            {
                new MaintenanceTask
                {
                    Name = "Inspect for Damage",
                    Description = "Check insulation for moisture damage, settling, or pest infestation.",
                    IntervalDays = 365
                },
                new MaintenanceTask
                {
                    Name = "Check R-Value",
                    Description = "Verify insulation levels meet recommended R-value for your climate zone.",
                    IntervalDays = 1825
                },
                new MaintenanceTask
                {
                    Name = "Seal Air Leaks",
                    Description = "Inspect and seal any air leaks around insulation areas.",
                    IntervalDays = 365
                }
            }
        };
    }

    public IEnumerable<MaintenanceTask> GetTasksForItemType(string itemType)
    {
        if (_tasksByType.TryGetValue(itemType, out var tasks))
        {
            return tasks;
        }

        // Return generic tasks for unknown item types
        return new List<MaintenanceTask>
        {
            new MaintenanceTask
            {
                Name = "Regular Inspection",
                Description = "Perform a visual inspection to check for any issues or needed maintenance.",
                IntervalDays = 180
            },
            new MaintenanceTask
            {
                Name = "Clean and Maintain",
                Description = "Clean and perform basic maintenance to keep item in good condition.",
                IntervalDays = 365
            }
        };
    }
}
