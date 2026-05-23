namespace Vakaros.Vkx.Parser.NET;

/// <summary>
/// Identifies the VKX specification revision a file was written against.
/// Use <see cref="VkxFormatVersion.ToSpecVersion"/> to map a raw format-version byte to this enum,
/// or read <see cref="VkxSession.SpecVersion"/> directly from a parsed session.
/// </summary>
public enum VkxSpecVersion : byte
{
    /// <summary>The format-version byte does not correspond to any known revision.</summary>
    Unknown = 0xFF,

    /// <summary>VKX 1.0 — initial release (May 2021). Format byte: <c>0x00</c>.</summary>
    V1_0 = 0x00,

    /// <summary>VKX 1.1 — December 2022. Format byte: <c>0x01</c>.</summary>
    V1_1 = 0x01,

    /// <summary>
    /// Unnamed patch to VKX 1.1 (March 2023). Fixes the payload size of the internal
    /// <c>0x0E</c> message. Format byte: <c>0x02</c>.
    /// </summary>
    V1_1_Patch = 0x02,

    /// <summary>VKX 1.2 — April 2023. Temperature message reassigned to ID <c>0x10</c>. Format byte: <c>0x03</c>.</summary>
    V1_2 = 0x03,

    /// <summary>VKX 1.3 — October 2023. Format byte: <c>0x04</c>.</summary>
    V1_3 = 0x04,

    /// <summary>VKX 1.4 — May 2024. Added internal type <c>0x21</c>. Format byte: <c>0x05</c>.</summary>
    V1_4 = 0x05,
}
