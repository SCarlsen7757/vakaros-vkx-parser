using Vakaros.Vkx.Parser.Models;

namespace Vakaros.Vkx.Parser;

/// <summary>
/// Represents a fully parsed VKX log file. All records are available via <see cref="Records"/>;
/// per-type convenience properties allow direct access to each record category.
/// </summary>
public sealed class VkxSession
{
    private readonly List<VkxRecord> _records;

    internal VkxSession(byte formatVersion, List<VkxRecord> records, bool isPartial = false)
    {
        FormatVersion = formatVersion;
        _records = records;
        IsPartial = isPartial;
    }

    /// <summary>VKX format version number read from the first page header in the file.
    /// See the revision history in the VKX format specification.
    /// </summary>
    public byte FormatVersion { get; }

    /// <summary>
    /// The VKX specification revision this file was written against, derived from
    /// <see cref="FormatVersion"/>. Returns <see cref="VkxSpecVersion.Unknown"/> when
    /// the format version byte does not match any known revision.
    /// </summary>
    public VkxSpecVersion SpecVersion => VkxFormatVersion.ToSpecVersion(FormatVersion);

    /// <summary>
    /// <see langword="true"/> when parsing stopped early and the session contains only the records
    /// collected up to the stopping point. This happens in two cases:
    /// <list type="bullet">
    ///   <item>The file uses a format version newer than <see cref="VkxFormatVersion.MaxKnown"/> and
    ///   an unrecognised record key was encountered.</item>
    ///   <item>The stream ended mid-payload (e.g. a file that was still being written when copied).</item>
    /// </list>
    /// </summary>
    public bool IsPartial { get; }

    /// <summary>All parsed records in file order, including page headers and terminators.</summary>
    public IReadOnlyList<VkxRecord> Records => _records;

    // ── Telemetry ───────────────────────────────────────────────────────────

    /// <summary>Position, Velocity, and Orientation records (0x02).</summary>
    public IEnumerable<PositionRecord> PositionRecords =>
        _records.OfType<PositionRecord>();

    /// <summary>Declination records (0x03).</summary>
    public IEnumerable<DeclinationRecord> DeclinationRecords =>
        _records.OfType<DeclinationRecord>();

    /// <summary>
    /// Wind records (0x0A). Only populated when a Calypso Wind Sensor was attached.
    /// </summary>
    public IEnumerable<WindRecord> WindRecords =>
        _records.OfType<WindRecord>();

    /// <summary>
    /// Speed Through Water records (0x0B). Only populated when a transducer was attached.
    /// </summary>
    public IEnumerable<SpeedThroughWaterRecord> SpeedThroughWaterRecords =>
        _records.OfType<SpeedThroughWaterRecord>();

    /// <summary>
    /// Depth records (0x0C). Only populated when a transducer was attached.
    /// </summary>
    public IEnumerable<DepthRecord> DepthRecords =>
        _records.OfType<DepthRecord>();

    /// <summary>
    /// Temperature records (0x10). Only populated when a transducer was attached.
    /// </summary>
    public IEnumerable<TemperatureRecord> TemperatureRecords =>
        _records.OfType<TemperatureRecord>();

    /// <summary>
    /// Load records (0x0F). Only populated when a Cyclops load cell was attached.
    /// </summary>
    public IEnumerable<LoadRecord> LoadRecords =>
        _records.OfType<LoadRecord>();

    // ── Race / Navigation ───────────────────────────────────────────────────

    /// <summary>Race Timer Event records (0x04).</summary>
    public IEnumerable<RaceTimerEventRecord> RaceTimerEventRecords =>
        _records.OfType<RaceTimerEventRecord>();

    /// <summary>Start line position records (0x05).</summary>
    public IEnumerable<LinePositionRecord> LinePositionRecords =>
        _records.OfType<LinePositionRecord>();

    /// <summary>Shift Angle records (0x06).</summary>
    public IEnumerable<ShiftAngleRecord> ShiftAngleRecords =>
        _records.OfType<ShiftAngleRecord>();

    // ── System ──────────────────────────────────────────────────────────────

    /// <summary>Device Configuration records (0x08).</summary>
    public IEnumerable<DeviceConfigurationRecord> DeviceConfigurationRecords =>
        _records.OfType<DeviceConfigurationRecord>();
}
