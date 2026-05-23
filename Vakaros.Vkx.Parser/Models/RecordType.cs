namespace Vakaros.Vkx.Parser.Models;

/// <summary>Identifies the type of a VKX record by its 1-byte key.</summary>
public enum RecordType : byte
{
    /// <summary>Internal Vakaros message type (0x01). Payload is skipped.</summary>
    InternalType01 = 0x01,
    /// <summary>Position, Velocity, and Orientation (0x02). Primary telemetry record.</summary>
    PositionVelocityOrientation = 0x02,
    /// <summary>Magnetic declination (0x03).</summary>
    Declination = 0x03,
    /// <summary>Race timer event (0x04).</summary>
    RaceTimerEvent = 0x04,
    /// <summary>Start line pin or boat end position (0x05).</summary>
    LinePosition = 0x05,
    /// <summary>Port or starboard tack shift angle (0x06).</summary>
    ShiftAngle = 0x06,
    /// <summary>Internal Vakaros message type (0x07). Payload is skipped.</summary>
    InternalType07 = 0x07,
    /// <summary>Device configuration (0x08).</summary>
    DeviceConfiguration = 0x08,
    /// <summary>Apparent wind reading from a Calypso sensor (0x0A).</summary>
    Wind = 0x0A,
    /// <summary>Speed through water from a transducer (0x0B).</summary>
    SpeedThroughWater = 0x0B,
    /// <summary>Water depth from a transducer (0x0C).</summary>
    Depth = 0x0C,
    /// <summary>Internal Vakaros message type (0x0E). Payload is skipped.</summary>
    InternalType0E = 0x0E,
    /// <summary>Load cell reading from a Cyclops sensor (0x0F).</summary>
    Load = 0x0F,
    /// <summary>Temperature from a transducer (0x10).</summary>
    Temperature = 0x10,
    /// <summary>Internal Vakaros message type (0x20). Payload is skipped.</summary>
    InternalType20 = 0x20,
    /// <summary>Internal Vakaros message type (0x21). Payload is skipped.</summary>
    InternalType21 = 0x21,
    /// <summary>Page terminator (0xFE). Carries the previous page length for backward iteration.</summary>
    PageTerminator = 0xFE,
    /// <summary>Page header (0xFF). Carries the VKX format version number.</summary>
    PageHeader = 0xFF,
}

