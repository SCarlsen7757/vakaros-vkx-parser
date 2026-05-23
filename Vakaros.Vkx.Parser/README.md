# Vakaros.Vkx.Parser

A .NET library for parsing Vakaros VKX binary log files into strongly-typed objects.

[![NuGet](https://img.shields.io/nuget/v/Vakaros.Vkx.Parser.svg)](https://www.nuget.org/packages/Vakaros.Vkx.Parser)

## Installation

```bash
dotnet add package Vakaros.Vkx.Parser
```

## Quick Start

```csharp
using Vakaros.Vkx.Parser;

// From a file path
VkxSession session = VkxParser.ParseFile("session.vkx");

// From a stream
VkxSession session = VkxParser.Parse(stream);

// From a byte array
VkxSession session = VkxParser.Parse(bytes);
```

## Accessing Records

```csharp
// GPS track
foreach (PositionRecord pos in session.PositionRecords)
    Console.WriteLine($"{pos.Timestamp} lat={pos.Latitude} lon={pos.Longitude} sog={pos.SpeedOverGround} m/s");

// Wind data (only present when a Calypso sensor was attached)
foreach (WindRecord wind in session.WindRecords)
    Console.WriteLine($"{wind.Timestamp} dir={wind.WindDirection}° speed={wind.WindSpeed} m/s");

// Race timer events
foreach (RaceTimerEventRecord evt in session.RaceTimerEventRecords)
    Console.WriteLine($"{evt.Timestamp} {evt.EventType} timer={evt.TimerValue}s");

// All records in file order
foreach (VkxRecord record in session.Records)
    Console.WriteLine(record.Type);
```

## Supported Record Types

| Key    | Type                       | Description                                  |
|--------|----------------------------|----------------------------------------------|
| `0x02` | `PositionRecord`           | GPS position, speed, course, orientation     |
| `0x03` | `DeclinationRecord`        | Magnetic declination                         |
| `0x04` | `RaceTimerEventRecord`     | Race timer events (start, reset, sync, etc.) |
| `0x05` | `LinePositionRecord`       | Start line pin/boat end positions            |
| `0x06` | `ShiftAngleRecord`         | Port/starboard tack shift angles             |
| `0x08` | `DeviceConfigurationRecord`| Device configuration                         |
| `0x0A` | `WindRecord`               | Apparent wind (Calypso sensor)               |
| `0x0B` | `SpeedThroughWaterRecord`  | Speed through water (transducer)             |
| `0x0C` | `DepthRecord`              | Water depth (transducer)                     |
| `0x0F` | `LoadRecord`               | Load cell reading (Cyclops sensor)           |
| `0x10` | `TemperatureRecord`        | Water temperature (transducer)               |

## Notes

- All values use SI units: metres, metres/second, radians, degrees Celsius.
- Records marked "only present when sensor attached" will return empty enumerables if the sensor was not connected.
- The VKX format spec is in [`vkx_format.md`](vkx_format.md).

## Requirements

- .NET 10.0 or later

## License

MIT — see [LICENSE](LICENSE).
