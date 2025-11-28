using Microsoft.Extensions.Localization;
using Yanitor.Web.Domain.Models;

namespace Yanitor.Web.Domain.Services;

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
public class TaskProvider(IStringLocalizer<TaskProvider> localizer) : ITaskProvider
{
    private readonly IStringLocalizer<TaskProvider> _localizer = localizer;

    public IEnumerable<MaintenanceTask> GetTasksForItemType(string itemType)
    {
        return itemType.ToUpperInvariant() switch
        {
            "HVAC" => GetHvacTasks(),
            "PLUMBING" => GetPlumbingTasks(),
            "DOOR" => GetDoorTasks(),
            "WINDOW" => GetWindowTasks(),
            "GARAGE" => GetGarageTasks(),
            "SAFETY" => GetSafetyTasks(),
            "ROOF" => GetRoofTasks(),
            "EXTERIOR" => GetExteriorTasks(),
            "INSULATION" => GetInsulationTasks(),
            "WASHINGMACHINE" => GetWashingMachineTasks(),
            _ => GetGenericTasks()
        };
    }

    private List<MaintenanceTask> GetHvacTasks() =>
    [
        new MaintenanceTask
        {
            Name = _localizer["HVAC_ChangeAirFilter_Name"],
            Description = _localizer["HVAC_ChangeAirFilter_Description"],
            IntervalDays = 90
        },
        new MaintenanceTask
        {
            Name = _localizer["HVAC_ProfessionalInspection_Name"],
            Description = _localizer["HVAC_ProfessionalInspection_Description"],
            IntervalDays = 365
        },
        new MaintenanceTask
        {
            Name = _localizer["HVAC_CleanVentsAndDucts_Name"],
            Description = _localizer["HVAC_CleanVentsAndDucts_Description"],
            IntervalDays = 180
        }
    ];

    private List<MaintenanceTask> GetPlumbingTasks() =>
    [
        new MaintenanceTask
        {
            Name = _localizer["Plumbing_CheckForLeaks_Name"],
            Description = _localizer["Plumbing_CheckForLeaks_Description"],
            IntervalDays = 90
        },
        new MaintenanceTask
        {
            Name = _localizer["Plumbing_CleanDrains_Name"],
            Description = _localizer["Plumbing_CleanDrains_Description"],
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            Name = _localizer["Plumbing_TestWaterPressure_Name"],
            Description = _localizer["Plumbing_TestWaterPressure_Description"],
            IntervalDays = 365
        }
    ];

    private List<MaintenanceTask> GetDoorTasks() =>
    [
        new MaintenanceTask
        {
            Name = _localizer["Door_LubricateHinges_Name"],
            Description = _localizer["Door_LubricateHinges_Description"],
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            Name = _localizer["Door_CheckWeatherstripping_Name"],
            Description = _localizer["Door_CheckWeatherstripping_Description"],
            IntervalDays = 365
        },
        new MaintenanceTask
        {
            Name = _localizer["Door_TightenHardware_Name"],
            Description = _localizer["Door_TightenHardware_Description"],
            IntervalDays = 180
        }
    ];

    private List<MaintenanceTask> GetWindowTasks() =>
    [
        new MaintenanceTask
        {
            Name = _localizer["Window_CleanWindows_Name"],
            Description = _localizer["Window_CleanWindows_Description"],
            IntervalDays = 90
        },
        new MaintenanceTask
        {
            Name = _localizer["Window_CheckSeals_Name"],
            Description = _localizer["Window_CheckSeals_Description"],
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            Name = _localizer["Window_LubricateTracks_Name"],
            Description = _localizer["Window_LubricateTracks_Description"],
            IntervalDays = 180
        }
    ];

    private List<MaintenanceTask> GetGarageTasks() =>
    [
        new MaintenanceTask
        {
            Name = _localizer["Garage_LubricateMovingParts_Name"],
            Description = _localizer["Garage_LubricateMovingParts_Description"],
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            Name = _localizer["Garage_TestSafetyFeatures_Name"],
            Description = _localizer["Garage_TestSafetyFeatures_Description"],
            IntervalDays = 90
        },
        new MaintenanceTask
        {
            Name = _localizer["Garage_ProfessionalInspection_Name"],
            Description = _localizer["Garage_ProfessionalInspection_Description"],
            IntervalDays = 365
        }
    ];

    private List<MaintenanceTask> GetSafetyTasks() =>
    [
        new MaintenanceTask
        {
            Name = _localizer["Safety_TestAlarm_Name"],
            Description = _localizer["Safety_TestAlarm_Description"],
            IntervalDays = 30
        },
        new MaintenanceTask
        {
            Name = _localizer["Safety_ReplaceBatteries_Name"],
            Description = _localizer["Safety_ReplaceBatteries_Description"],
            IntervalDays = 365
        },
        new MaintenanceTask
        {
            Name = _localizer["Safety_CleanSensor_Name"],
            Description = _localizer["Safety_CleanSensor_Description"],
            IntervalDays = 180
        }
    ];

    private List<MaintenanceTask> GetRoofTasks() =>
    [
        new MaintenanceTask
        {
            Name = _localizer["Roof_InspectShingles_Name"],
            Description = _localizer["Roof_InspectShingles_Description"],
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            Name = _localizer["Roof_CheckForMoss_Name"],
            Description = _localizer["Roof_CheckForMoss_Description"],
            IntervalDays = 365
        },
        new MaintenanceTask
        {
            Name = _localizer["Roof_ProfessionalInspection_Name"],
            Description = _localizer["Roof_ProfessionalInspection_Description"],
            IntervalDays = 730
        }
    ];

    private List<MaintenanceTask> GetExteriorTasks() =>
    [
        new MaintenanceTask
        {
            Name = _localizer["Exterior_CleanGutters_Name"],
            Description = _localizer["Exterior_CleanGutters_Description"],
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            Name = _localizer["Exterior_InspectForDamage_Name"],
            Description = _localizer["Exterior_InspectForDamage_Description"],
            IntervalDays = 90
        },
        new MaintenanceTask
        {
            Name = _localizer["Exterior_PaintTouchUp_Name"],
            Description = _localizer["Exterior_PaintTouchUp_Description"],
            IntervalDays = 365
        }
    ];

    private List<MaintenanceTask> GetInsulationTasks() =>
    [
        new MaintenanceTask
        {
            Name = _localizer["Insulation_InspectForDamage_Name"],
            Description = _localizer["Insulation_InspectForDamage_Description"],
            IntervalDays = 365
        },
        new MaintenanceTask
        {
            Name = _localizer["Insulation_CheckRValue_Name"],
            Description = _localizer["Insulation_CheckRValue_Description"],
            IntervalDays = 1825
        },
        new MaintenanceTask
        {
            Name = _localizer["Insulation_SealAirLeaks_Name"],
            Description = _localizer["Insulation_SealAirLeaks_Description"],
            IntervalDays = 365
        }
    ];

    private List<MaintenanceTask> GetWashingMachineTasks() =>
    [
        new MaintenanceTask
        {
            Name = _localizer["WashingMachine_RinseDrumCompartment_Name"],
            Description = _localizer["WashingMachine_RinseDrumCompartment_Description"],
            IntervalDays = 30
        },
        new MaintenanceTask
        {
            Name = _localizer["WashingMachine_RinseSoapCompartment_Name"],
            Description = _localizer["WashingMachine_RinseSoapCompartment_Description"],
            IntervalDays = 30
        },
        new MaintenanceTask
        {
            Name = _localizer["WashingMachine_RinseDrainFilter_Name"],
            Description = _localizer["WashingMachine_RinseDrainFilter_Description"],
            IntervalDays = 90
        },
        new MaintenanceTask
        {
            Name = _localizer["WashingMachine_RinseDrainOutlet_Name"],
            Description = _localizer["WashingMachine_RinseDrainOutlet_Description"],
            IntervalDays = 180
        }
    ];

    private List<MaintenanceTask> GetGenericTasks() =>
    [
        new MaintenanceTask
        {
            Name = _localizer["Generic_RegularInspection_Name"],
            Description = _localizer["Generic_RegularInspection_Description"],
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            Name = _localizer["Generic_CleanAndMaintain_Name"],
            Description = _localizer["Generic_CleanAndMaintain_Description"],
            IntervalDays = 365
        }
    ];
}
