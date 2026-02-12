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
    /// <param name="itemType">The house item type.</param>
    /// <returns>A collection of maintenance tasks for the item type.</returns>
    IEnumerable<MaintenanceTask> GetTasksForItemType(HouseItemType itemType);
}

/// <summary>
/// Default implementation of ITaskProvider that provides predefined maintenance tasks
/// for common house item types.
/// </summary>
public class TaskProvider(IStringLocalizer<TaskProvider> localizer) : ITaskProvider
{
    private readonly IStringLocalizer<TaskProvider> _localizer = localizer;

    public IEnumerable<MaintenanceTask> GetTasksForItemType(HouseItemType itemType)
    {
        return itemType switch
        {
            HouseItemType.Ventilation => GetHvacTasks(),
            HouseItemType.Shower => GetPlumbingTasks(),
            HouseItemType.WashingMachine => GetWashingMachineTasks(),
            HouseItemType.Dishwasher => GetDishwasherTasks(),
            HouseItemType.BathroomSink => GetBathroomSinkTasks(),
            HouseItemType.BathtubDrain => GetBathtubDrainTasks(),
            HouseItemType.InteriorDoor => GetDoorTasks(),
            HouseItemType.SmokeDetector => GetSafetyTasks(),
            _ => GetGenericTasks()
        };
    }

    private List<MaintenanceTask> GetHvacTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "HVAC_ChangeAirFilter_Name",
            DescriptionKey = "HVAC_ChangeAirFilter_Description",
            DetailedDescriptionKey = "HVAC_ChangeAirFilter_DetailedDescription",
            IntervalDays = 180,
            VideoUrl = "https://www.youtube.com/watch?v=fxxyMF7gOIo",
            ProductLinks =
            [
                new ProductLink
                {
                    NameKey = "Product_AirFilter_HEPA_Name",
                    DescriptionKey = "Product_AirFilter_HEPA_Description",
                    Url = "https://www.amazon.com/s?k=hvac+air+filter"
                },
                new ProductLink
                {
                    NameKey = "Product_AirFilter_Electrostatic_Name",
                    DescriptionKey = "Product_AirFilter_Electrostatic_Description",
                    Url = "https://www.amazon.com/s?k=electrostatic+air+filter"
                }
            ]
        },
        new MaintenanceTask
        {
            NameKey = "HVAC_ProfessionalInspection_Name",
            DescriptionKey = "HVAC_ProfessionalInspection_Description",
            IntervalDays = 365
        },
        new MaintenanceTask
        {
            NameKey = "HVAC_CleanVentsAndDucts_Name",
            DescriptionKey = "HVAC_CleanVentsAndDucts_Description",
            IntervalDays = 180
        }
    ];

    private List<MaintenanceTask> GetPlumbingTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "Plumbing_CheckForLeaks_Name",
            DescriptionKey = "Plumbing_CheckForLeaks_Description",
            IntervalDays = 90
        },
        new MaintenanceTask
        {
            NameKey = "Plumbing_CleanDrains_Name",
            DescriptionKey = "Plumbing_CleanDrains_Description",
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            NameKey = "Plumbing_TestWaterPressure_Name",
            DescriptionKey = "Plumbing_TestWaterPressure_Description",
            IntervalDays = 365
        }
    ];

    private List<MaintenanceTask> GetDoorTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "Door_LubricateHinges_Name",
            DescriptionKey = "Door_LubricateHinges_Description",
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            NameKey = "Door_CheckWeatherstripping_Name",
            DescriptionKey = "Door_CheckWeatherstripping_Description",
            IntervalDays = 365
        },
        new MaintenanceTask
        {
            NameKey = "Door_TightenHardware_Name",
            DescriptionKey = "Door_TightenHardware_Description",
            IntervalDays = 180
        }
    ];

    private List<MaintenanceTask> GetWindowTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "Window_CleanWindows_Name",
            DescriptionKey = "Window_CleanWindows_Description",
            IntervalDays = 90
        },
        new MaintenanceTask
        {
            NameKey = "Window_CheckSeals_Name",
            DescriptionKey = "Window_CheckSeals_Description",
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            NameKey = "Window_LubricateTracks_Name",
            DescriptionKey = "Window_LubricateTracks_Description",
            IntervalDays = 180
        }
    ];

    private List<MaintenanceTask> GetGarageTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "Garage_LubricateMovingParts_Name",
            DescriptionKey = "Garage_LubricateMovingParts_Description",
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            NameKey = "Garage_TestSafetyFeatures_Name",
            DescriptionKey = "Garage_TestSafetyFeatures_Description",
            IntervalDays = 90
        },
        new MaintenanceTask
        {
            NameKey = "Garage_ProfessionalInspection_Name",
            DescriptionKey = "Garage_ProfessionalInspection_Description",
            IntervalDays = 365
        }
    ];

    private List<MaintenanceTask> GetSafetyTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "Safety_TestAlarm_Name",
            DescriptionKey = "Safety_TestAlarm_Description",
            DetailedDescriptionKey = "Safety_TestAlarm_DetailedDescription",
            IntervalDays = 30,
            VideoUrl = "https://www.youtube.com/watch?v=HJDwjHdZe-0",
            ProductLinks =
            [
                new ProductLink
                {
                    NameKey = "Product_SmokeDetector_Photoelectric_Name",
                    DescriptionKey = "Product_SmokeDetector_Photoelectric_Description",
                    Url = "https://www.amazon.com/s?k=photoelectric+smoke+detector"
                },
                new ProductLink
                {
                    NameKey = "Product_SmokeDetector_DualSensor_Name",
                    DescriptionKey = "Product_SmokeDetector_DualSensor_Description",
                    Url = "https://www.amazon.com/s?k=dual+sensor+smoke+detector"
                }
            ]
        },
        new MaintenanceTask
        {
            NameKey = "Safety_ReplaceBatteries_Name",
            DescriptionKey = "Safety_ReplaceBatteries_Description",
            IntervalDays = 365
        },
        new MaintenanceTask
        {
            NameKey = "Safety_CleanSensor_Name",
            DescriptionKey = "Safety_CleanSensor_Description",
            IntervalDays = 180
        }
    ];

    private List<MaintenanceTask> GetRoofTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "Roof_InspectShingles_Name",
            DescriptionKey = "Roof_InspectShingles_Description",
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            NameKey = "Roof_CheckForMoss_Name",
            DescriptionKey = "Roof_CheckForMoss_Description",
            IntervalDays = 365
        },
        new MaintenanceTask
        {
            NameKey = "Roof_ProfessionalInspection_Name",
            DescriptionKey = "Roof_ProfessionalInspection_Description",
            IntervalDays = 730
        }
    ];

    private List<MaintenanceTask> GetExteriorTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "Exterior_CleanGutters_Name",
            DescriptionKey = "Exterior_CleanGutters_Description",
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            NameKey = "Exterior_InspectForDamage_Name",
            DescriptionKey = "Exterior_InspectForDamage_Description",
            IntervalDays = 90
        },
        new MaintenanceTask
        {
            NameKey = "Exterior_PaintTouchUp_Name",
            DescriptionKey = "Exterior_PaintTouchUp_Description",
            IntervalDays = 365
        }
    ];

    private List<MaintenanceTask> GetInsulationTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "Insulation_InspectForDamage_Name",
            DescriptionKey = "Insulation_InspectForDamage_Description",
            IntervalDays = 365
        },
        new MaintenanceTask
        {
            NameKey = "Insulation_CheckRValue_Name",
            DescriptionKey = "Insulation_CheckRValue_Description",
            IntervalDays = 1825
        },
        new MaintenanceTask
        {
            NameKey = "Insulation_SealAirLeaks_Name",
            DescriptionKey = "Insulation_SealAirLeaks_Description",
            IntervalDays = 365
        }
    ];

    private List<MaintenanceTask> GetWashingMachineTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "WashingMachine_RinseDrumCompartment_Name",
            DescriptionKey = "WashingMachine_RinseDrumCompartment_Description",
            IntervalDays = 30
        },
        new MaintenanceTask
        {
            NameKey = "WashingMachine_RinseSoapCompartment_Name",
            DescriptionKey = "WashingMachine_RinseSoapCompartment_Description",
            IntervalDays = 30
        },
        new MaintenanceTask
        {
            NameKey = "WashingMachine_RinseDrainFilter_Name",
            DescriptionKey = "WashingMachine_RinseDrainFilter_Description",
            IntervalDays = 90
        },
        new MaintenanceTask
        {
            NameKey = "WashingMachine_RinseDrainOutlet_Name",
            DescriptionKey = "WashingMachine_RinseDrainOutlet_Description",
            IntervalDays = 180
        }
    ];

    private List<MaintenanceTask> GetDishwasherTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "Dishwasher_CleanFilter_Name",
            DescriptionKey = "Dishwasher_CleanFilter_Description",
            DetailedDescriptionKey = "Dishwasher_CleanFilter_DetailedDescription",
            IntervalDays = 14,
            VideoUrl = "https://www.youtube.com/watch?v=NW3asK4epk4",
            ProductLinks =
            [
                new ProductLink
                {
                    NameKey = "Product_DishwasherCleaner_Name",
                    DescriptionKey = "Product_DishwasherCleaner_Description",
                    Url = "https://www.amazon.com/s?k=dishwasher+cleaner"
                },
                new ProductLink
                {
                    NameKey = "Product_DishwasherFilter_Name",
                    DescriptionKey = "Product_DishwasherFilter_Description",
                    Url = "https://www.amazon.com/s?k=dishwasher+filter+replacement"
                }
            ]
        },
        new MaintenanceTask
        {
            NameKey = "Dishwasher_CleanDoorAndSeals_Name",
            DescriptionKey = "Dishwasher_CleanDoorAndSeals_Description",
            IntervalDays = 30
        }
    ];

    private List<MaintenanceTask> GetBathroomSinkTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "BathroomSink_CleanDrain_Name",
            DescriptionKey = "BathroomSink_CleanDrain_Description",
            IntervalDays = 90
        }
    ];

    private List<MaintenanceTask> GetBathtubDrainTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "BathtubDrain_CleanDrain_Name",
            DescriptionKey = "BathtubDrain_CleanDrain_Description",
            IntervalDays = 90
        }
    ];

    private List<MaintenanceTask> GetGenericTasks() =>
    [
        new MaintenanceTask
        {
            NameKey = "Generic_RegularInspection_Name",
            DescriptionKey = "Generic_RegularInspection_Description",
            IntervalDays = 180
        },
        new MaintenanceTask
        {
            NameKey = "Generic_CleanAndMaintain_Name",
            DescriptionKey = "Generic_CleanAndMaintain_Description",
            IntervalDays = 365
        }
    ];
}
