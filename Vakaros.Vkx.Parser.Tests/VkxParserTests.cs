using System.Reflection;
using System.Text;
using Vakaros.Vkx.Parser;
using Vakaros.Vkx.Parser.Models;
using Xunit;

namespace Vakaros.Vkx.Parser.Tests;

/// <summary>
/// Unit and integration tests for <see cref="VkxParser"/>.
/// All tests that don't use the real .vkx fixture build their payloads programmatically
/// using little-endian <see cref="BinaryWriter"/> — matching the format spec exactly.
/// </summary>
public class VkxParserTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Wraps an action over a BinaryWriter and returns the resulting byte array.</summary>
    private static byte[] Build(Action<BinaryWriter> write)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true);
        write(bw);
        bw.Flush();
        return ms.ToArray();
    }

    /// <summary>Writes a 0xFF page header with the given format version (+ 6 zero internal bytes).</summary>
    private static void WritePageHeader(BinaryWriter bw, byte version = VkxFormatVersion.V1_4)
    {
        bw.Write((byte)0xFF);
        bw.Write(version);
        bw.Write(new byte[6]);
    }

    /// <summary>Writes a 0xFE page terminator.</summary>
    private static void WritePageTerminator(BinaryWriter bw, ushort previousPageLength = 0)
    {
        bw.Write((byte)0xFE);
        bw.Write(previousPageLength);
    }

    private static VkxSession ParseBytes(byte[] data) => VkxParser.Parse(data);

    // ── Session / empty stream ────────────────────────────────────────────────

    [Fact]
    public void EmptyStream_ReturnsEmptySession()
    {
        var session = ParseBytes([]);
        Assert.Empty(session.Records);
        Assert.Equal(0, session.FormatVersion);
        Assert.False(session.IsPartial);
    }

    [Fact]
    public void PageHeaderOnly_ReturnsSingleRecord_WithFormatVersion()
    {
        var data = Build(bw => WritePageHeader(bw, VkxFormatVersion.V1_4));
        var session = ParseBytes(data);

        Assert.Equal(VkxFormatVersion.V1_4, session.FormatVersion);
        Assert.Single(session.Records);
        Assert.IsType<PageHeaderRecord>(session.Records[0]);
        Assert.False(session.IsPartial);
    }

    [Fact]
    public void IsPartial_IsFalse_ForCompleteKnownVersionFile()
    {
        var data = Build(bw =>
        {
            WritePageHeader(bw);
            WritePageTerminator(bw);
        });

        var session = ParseBytes(data);
        Assert.False(session.IsPartial);
    }

    // ── Version enforcement ───────────────────────────────────────────────────

    [Fact]
    public void Version_BelowMinimum_ThrowsNotSupportedException()
    {
        var data = Build(bw => WritePageHeader(bw, VkxFormatVersion.V1_3));

        var ex = Assert.Throws<NotSupportedException>(() => ParseBytes(data));
        Assert.Contains("0x04", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1.4", ex.Message);
    }

    [Fact]
    public void Version_1_4_ParsesSuccessfully()
    {
        var data = Build(bw => WritePageHeader(bw, VkxFormatVersion.V1_4));
        var session = ParseBytes(data);
        Assert.Equal(VkxFormatVersion.V1_4, session.FormatVersion);
    }

    [Fact]
    public void FutureVersion_WithUnknownKey_ReturnsPartialSession()
    {
        const byte futureVersion = VkxFormatVersion.MaxKnown + 1;
        var data = Build(bw =>
        {
            WritePageHeader(bw, futureVersion);
            // Unknown key that the parser has never seen.
            bw.Write((byte)0x99);
        });

        var session = ParseBytes(data);
        Assert.True(session.IsPartial);
        // The page header was parsed; the unknown record stopped parsing.
        Assert.Single(session.Records);
        Assert.IsType<PageHeaderRecord>(session.Records[0]);
    }

    [Fact]
    public void FutureVersion_WithOnlyKnownKeys_IsNotPartial()
    {
        const byte futureVersion = VkxFormatVersion.MaxKnown + 1;
        var data = Build(bw =>
        {
            WritePageHeader(bw, futureVersion);
            // Write a known record (Wind = 0x0A).
            bw.Write((byte)0x0A);
            bw.Write(1_000_000_000_000UL); // timestamp
            bw.Write(180.0f);              // direction
            bw.Write(5.0f);               // speed
        });

        var session = ParseBytes(data);
        Assert.False(session.IsPartial);
        Assert.Equal(2, session.Records.Count); // header + wind
    }

    // ── Page records ──────────────────────────────────────────────────────────

    [Fact]
    public void ParsesPageTerminatorRecord()
    {
        var data = Build(bw =>
        {
            WritePageHeader(bw);
            WritePageTerminator(bw, previousPageLength: 1024);
        });

        var session = ParseBytes(data);
        var terminator = Assert.Single(session.Records.OfType<PageTerminatorRecord>());
        Assert.Equal(1024, terminator.PreviousPageLength);
    }

    // ── Telemetry record round-trips ──────────────────────────────────────────

    [Fact]
    public void ParsesPositionRecord()
    {
        const ulong ts = 1_000_000_000_000UL;
        const int rawLat = 537_000_000;   // 53.7°
        const int rawLon = 100_000_000;   // 10.0°
        const float sog = 2.5f;
        const float cog = 1.2f;
        const float alt = 3.0f;
        const float qw = 1.0f, qx = 0.1f, qy = 0.2f, qz = 0.3f;

        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write((byte)0x02);
            bw.Write(ts);
            bw.Write(rawLat);
            bw.Write(rawLon);
            bw.Write(sog);
            bw.Write(cog);
            bw.Write(alt);
            bw.Write(qw);
            bw.Write(qx);
            bw.Write(qy);
            bw.Write(qz);
        });

        var session = ParseBytes(data);
        var record = Assert.Single(session.Records.OfType<PositionRecord>());

        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)ts), record.Timestamp);
        Assert.Equal(rawLat * 1e-7, record.Latitude, precision: 10);
        Assert.Equal(rawLon * 1e-7, record.Longitude, precision: 10);
        Assert.Equal(sog, record.SpeedOverGround);
        Assert.Equal(cog, record.CourseOverGround);
        Assert.Equal(alt, record.Altitude);
        Assert.Equal(qw, record.QuaternionW);
        Assert.Equal(qx, record.QuaternionX);
        Assert.Equal(qy, record.QuaternionY);
        Assert.Equal(qz, record.QuaternionZ);
    }

    [Fact]
    public void ParsesDeclinationRecord()
    {
        const ulong ts = 1_000_000_000_000UL;
        const float decl = 0.05f;
        const int rawLat = 537_000_000;
        const int rawLon = 100_000_000;

        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write((byte)0x03);
            bw.Write(ts);
            bw.Write(decl);
            bw.Write(rawLat);
            bw.Write(rawLon);
        });

        var session = ParseBytes(data);
        var record = Assert.Single(session.Records.OfType<DeclinationRecord>());

        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)ts), record.Timestamp);
        Assert.Equal(decl, record.DeclinationOffset);
        Assert.Equal(rawLat * 1e-7, record.Latitude, precision: 10);
        Assert.Equal(rawLon * 1e-7, record.Longitude, precision: 10);
    }

    [Fact]
    public void ParsesRaceTimerEventRecord()
    {
        const ulong ts = 1_000_000_000_000UL;
        const byte eventType = (byte)TimerEventType.RaceStart;
        const int timerValue = -300;

        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write((byte)0x04);
            bw.Write(ts);
            bw.Write(eventType);
            bw.Write(timerValue);
        });

        var session = ParseBytes(data);
        var record = Assert.Single(session.Records.OfType<RaceTimerEventRecord>());

        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)ts), record.Timestamp);
        Assert.Equal(TimerEventType.RaceStart, record.EventType);
        Assert.Equal(timerValue, record.TimerValue);
    }

    [Fact]
    public void ParsesLinePositionRecord()
    {
        const ulong ts = 1_000_000_000_000UL;
        const byte lineEnd = (byte)LineEndType.Pin;
        const float lat = 53.7f;
        const float lon = 10.0f;

        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write((byte)0x05);
            bw.Write(ts);
            bw.Write(lineEnd);
            bw.Write(lat);
            bw.Write(lon);
        });

        var session = ParseBytes(data);
        var record = Assert.Single(session.Records.OfType<LinePositionRecord>());

        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)ts), record.Timestamp);
        Assert.Equal(LineEndType.Pin, record.LineEnd);
        Assert.Equal(lat, record.Latitude);
        Assert.Equal(lon, record.Longitude);
    }

    [Fact]
    public void ParsesShiftAngleRecord()
    {
        const ulong ts = 1_000_000_000_000UL;
        const byte tackId = 1;  // port
        const byte setBy = 0;   // auto
        const float heading = 45.0f;
        const float sog = 3.5f;

        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write((byte)0x06);
            bw.Write(ts);
            bw.Write(tackId);
            bw.Write(setBy);
            bw.Write(heading);
            bw.Write(sog);
        });

        var session = ParseBytes(data);
        var record = Assert.Single(session.Records.OfType<ShiftAngleRecord>());

        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)ts), record.Timestamp);
        Assert.True(record.IsPort);
        Assert.False(record.IsManual);
        Assert.Equal(heading, record.TrueHeading);
        Assert.Equal(sog, record.SpeedOverGround);
    }

    [Fact]
    public void ParsesDeviceConfigurationRecord()
    {
        const uint bitfield = 0x01; // IsFixedToBodyFrame = true
        const byte loggingRate = 10;

        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write((byte)0x08);
            bw.Write(0UL);      // unused field
            bw.Write(bitfield);
            bw.Write(loggingRate);
        });

        var session = ParseBytes(data);
        var record = Assert.Single(session.Records.OfType<DeviceConfigurationRecord>());

        Assert.True(record.IsFixedToBodyFrame);
        Assert.Equal(loggingRate, record.TelemetryLoggingRate);
    }

    [Fact]
    public void ParsesWindRecord()
    {
        const ulong ts = 1_000_000_000_000UL;
        const float direction = 135.0f;
        const float speed = 7.2f;

        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write((byte)0x0A);
            bw.Write(ts);
            bw.Write(direction);
            bw.Write(speed);
        });

        var session = ParseBytes(data);
        var record = Assert.Single(session.Records.OfType<WindRecord>());

        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)ts), record.Timestamp);
        Assert.Equal(direction, record.WindDirection);
        Assert.Equal(speed, record.WindSpeed);
    }

    [Fact]
    public void ParsesSpeedThroughWaterRecord()
    {
        const ulong ts = 1_000_000_000_000UL;
        const float forward = 3.1f;
        const float horizontal = 0.4f;

        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write((byte)0x0B);
            bw.Write(ts);
            bw.Write(forward);
            bw.Write(horizontal);
        });

        var session = ParseBytes(data);
        var record = Assert.Single(session.Records.OfType<SpeedThroughWaterRecord>());

        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)ts), record.Timestamp);
        Assert.Equal(forward, record.ForwardSpeed);
        Assert.Equal(horizontal, record.HorizontalSpeed);
    }

    [Fact]
    public void ParsesDepthRecord()
    {
        const ulong ts = 1_000_000_000_000UL;
        const float depth = 12.5f;

        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write((byte)0x0C);
            bw.Write(ts);
            bw.Write(depth);
        });

        var session = ParseBytes(data);
        var record = Assert.Single(session.Records.OfType<DepthRecord>());

        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)ts), record.Timestamp);
        Assert.Equal(depth, record.Depth);
    }

    [Fact]
    public void ParsesLoadRecord()
    {
        const ulong ts = 1_000_000_000_000UL;
        const string name = "MAIN";
        const float load = 250.0f;

        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write((byte)0x0F);
            bw.Write(ts);
            bw.Write(Encoding.ASCII.GetBytes(name));
            bw.Write(load);
        });

        var session = ParseBytes(data);
        var record = Assert.Single(session.Records.OfType<LoadRecord>());

        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)ts), record.Timestamp);
        Assert.Equal(name, record.SensorName);
        Assert.Equal(load, record.Load);
    }

    [Fact]
    public void ParsesTemperatureRecord()
    {
        const ulong ts = 1_000_000_000_000UL;
        const float temp = 18.5f;

        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write((byte)0x10);
            bw.Write(ts);
            bw.Write(temp);
        });

        var session = ParseBytes(data);
        var record = Assert.Single(session.Records.OfType<TemperatureRecord>());

        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)ts), record.Timestamp);
        Assert.Equal(temp, record.Temperature);
    }

    // ── Internal / skipped records ────────────────────────────────────────────

    [Theory]
    [InlineData((byte)0x01, 32)]
    [InlineData((byte)0x07, 12)]
    [InlineData((byte)0x0E, 16)]
    [InlineData((byte)0x20, 13)]
    [InlineData((byte)0x21, 52)]
    public void InternalRecords_AreNotAddedToRecordsList(byte key, int payloadSize)
    {
        var data = Build(bw =>
        {
            WritePageHeader(bw);
            bw.Write(key);
            bw.Write(new byte[payloadSize]);
        });

        var session = ParseBytes(data);
        // Only the page header should be in the list; the internal record is discarded.
        Assert.Single(session.Records);
        Assert.IsType<PageHeaderRecord>(session.Records[0]);
    }

    // ── Full VKX 1.4 synthetic integration test ───────────────────────────────

    [Fact]
    public void FullV14Synthetic_ParsesAllRecords()
    {
        const ulong ts = 1_000_000_000_000UL;

        var data = Build(bw =>
        {
            // Page header (VKX 1.4)
            WritePageHeader(bw, VkxFormatVersion.V1_4);

            // Position record (0x02)
            bw.Write((byte)0x02);
            bw.Write(ts);
            bw.Write(537_000_000);  // lat
            bw.Write(100_000_000);  // lon
            bw.Write(2.5f);         // sog
            bw.Write(1.2f);         // cog
            bw.Write(0.0f);         // altitude
            bw.Write(1.0f);         // qw
            bw.Write(0.0f);         // qx
            bw.Write(0.0f);         // qy
            bw.Write(0.0f);         // qz

            // Wind record (0x0A)
            bw.Write((byte)0x0A);
            bw.Write(ts + 1000UL);
            bw.Write(180.0f);
            bw.Write(5.0f);

            // Race timer event (0x04)
            bw.Write((byte)0x04);
            bw.Write(ts + 2000UL);
            bw.Write((byte)TimerEventType.Start);
            bw.Write(-300);

            // Internal record — should be silently skipped (0x01, 32 bytes)
            bw.Write((byte)0x01);
            bw.Write(new byte[32]);

            // Page terminator (0xFE)
            WritePageTerminator(bw, 0);
        });

        var session = ParseBytes(data);

        Assert.Equal(VkxFormatVersion.V1_4, session.FormatVersion);
        Assert.False(session.IsPartial);

        // Header + position + wind + race timer + terminator (internal 0x01 is skipped)
        Assert.Equal(5, session.Records.Count);

        Assert.Single(session.PositionRecords);
        Assert.Single(session.WindRecords);
        Assert.Single(session.RaceTimerEventRecords);

        var pos = session.PositionRecords.First();
        Assert.Equal(53.7, pos.Latitude, precision: 5);
        Assert.Equal(10.0, pos.Longitude, precision: 5);

        var wind = session.WindRecords.First();
        Assert.Equal(180.0f, wind.WindDirection);
        Assert.Equal(5.0f, wind.WindSpeed);

        var timer = session.RaceTimerEventRecords.First();
        Assert.Equal(TimerEventType.Start, timer.EventType);
        Assert.Equal(-300, timer.TimerValue);
    }

    // ── Real file integration test ────────────────────────────────────────────

    [Fact]
    public void RealV14File_ParsesSuccessfully()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .Single(n => n.EndsWith(".vkx", StringComparison.OrdinalIgnoreCase));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        Assert.NotNull(stream);

        var session = VkxParser.Parse(stream);

        Assert.Equal(VkxFormatVersion.V1_4, session.FormatVersion);
        Assert.NotEmpty(session.Records);
        Assert.NotEmpty(session.PositionRecords);
        Assert.False(session.IsPartial);
    }
}
