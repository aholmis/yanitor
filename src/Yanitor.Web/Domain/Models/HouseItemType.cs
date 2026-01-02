namespace Yanitor.Web.Domain.Models;

/// <summary>
/// Defines the supported house item categories.
/// </summary>
public enum HouseItemType
{
    /// <summary>
    /// A ventilation system (e.g., balanced ventilation, air exchanger).
    /// </summary>
    Ventilation,

    /// <summary>
    /// A shower installation.
    /// </summary>
    Shower,

    /// <summary>
    /// A washing machine appliance.
    /// </summary>
    WashingMachine,

    /// <summary>
    /// A dishwasher appliance.
    /// </summary>
    Dishwasher,

    /// <summary>
    /// A bathroom sink and its drain.
    /// </summary>
    BathroomSink,

    /// <summary>
    /// A bathtub drain.
    /// </summary>
    BathtubDrain,

    /// <summary>
    /// Interior doors inside the home.
    /// </summary>
    InteriorDoor,

    /// <summary>
    /// Smoke detectors / smoke alarms.
    /// </summary>
    SmokeDetector
}
