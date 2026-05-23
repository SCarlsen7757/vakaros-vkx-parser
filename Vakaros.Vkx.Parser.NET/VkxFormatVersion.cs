namespace Vakaros.Vkx.Parser.NET;

/// <summary>
/// Named byte constants for every known VKX format-version byte.
/// The version byte is read from each <c>0xFF</c> page header in the file.
/// Use <see cref="ToSpecVersion"/> to convert a raw byte to the <see cref="VkxSpecVersion"/> enum.
/// </summary>
public static class VkxFormatVersion
{
    /// <summary>VKX 1.0 — initial release (May 2021).</summary>
    public const byte V1_0 = 0x00;

    /// <summary>VKX 1.1 — December 2022.</summary>
    public const byte V1_1 = 0x01;

    /// <summary>
    /// Unnamed patch to VKX 1.1 (March 2023). Fixes the payload size of the internal
    /// <c>0x0E</c> message. Not a named minor version in the spec.
    /// </summary>
    public const byte V1_1_Patch = 0x02;

    /// <summary>VKX 1.2 — April 2023 (temperature message reassigned to ID <c>0x10</c>).</summary>
    public const byte V1_2 = 0x03;

    /// <summary>VKX 1.3 — October 2023.</summary>
    public const byte V1_3 = 0x04;

    /// <summary>VKX 1.4 — May 2024 (added internal <c>0x21</c> type).</summary>
    public const byte V1_4 = 0x05;

    /// <summary>The oldest VKX format version this library can parse.</summary>
    public const byte MinimumSupported = V1_4;

    /// <summary>The newest VKX format version this library was written against.</summary>
    public const byte MaxKnown = V1_4;

    /// <summary>
    /// Maps a raw format-version byte (as read from a <c>0xFF</c> page header) to the
    /// corresponding <see cref="VkxSpecVersion"/> enum value.
    /// Returns <see cref="VkxSpecVersion.Unknown"/> for any byte not in the known revision table.
    /// </summary>
    /// <param name="version">The raw format-version byte.</param>
    public static VkxSpecVersion ToSpecVersion(byte version) => version switch
    {
        V1_0       => VkxSpecVersion.V1_0,
        V1_1       => VkxSpecVersion.V1_1,
        V1_1_Patch => VkxSpecVersion.V1_1_Patch,
        V1_2       => VkxSpecVersion.V1_2,
        V1_3       => VkxSpecVersion.V1_3,
        V1_4       => VkxSpecVersion.V1_4,
        _          => VkxSpecVersion.Unknown,
    };
}
