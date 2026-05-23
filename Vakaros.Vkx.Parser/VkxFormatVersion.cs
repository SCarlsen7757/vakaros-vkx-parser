namespace Vakaros.Vkx.Parser;

/// <summary>
/// Named byte constants for every known VKX format version.
/// The version byte is read from each <c>0xFF</c> page header in the file.
/// </summary>
public static class VkxFormatVersion
{
    /// <summary>VKX 1.0 — initial release (May 2021).</summary>
    public const byte V1_0 = 0x00;

    /// <summary>VKX 1.1 — December 2022.</summary>
    public const byte V1_1 = 0x01;

    /// <summary>VKX 1.2 — March 2023 (fixed size of internal 0x0E message).</summary>
    public const byte V1_2 = 0x02;

    /// <summary>VKX 1.3 — April / October 2023.</summary>
    public const byte V1_3 = 0x04;

    /// <summary>VKX 1.4 — May 2024 (added internal 0x21 type).</summary>
    public const byte V1_4 = 0x05;

    /// <summary>The oldest VKX format version this library can parse.</summary>
    public const byte MinimumSupported = V1_4;

    /// <summary>The newest VKX format version this library was written against.</summary>
    public const byte MaxKnown = V1_4;
}
