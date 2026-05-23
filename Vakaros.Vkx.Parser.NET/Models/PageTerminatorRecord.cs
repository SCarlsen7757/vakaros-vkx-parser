namespace Vakaros.Vkx.Parser.NET.Models;

/// <summary>
/// 0xFE — Page terminator record. Carries the byte length of the preceding page (including
/// its header and terminator) to support backwards iteration.
/// </summary>
public record PageTerminatorRecord : VkxRecord
{
    /// <inheritdoc/>
    public override RecordType Type => RecordType.PageTerminator;

    /// <summary>Length of the previous page in bytes, including its header and terminator.</summary>
    public ushort PreviousPageLength { get; init; }
}
