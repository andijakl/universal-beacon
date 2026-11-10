using UniversalBeacon.Library.Core.Interop;

namespace UniversalBeacon.Library.Tests;

/// <summary>
/// Helpers to build test payloads and packets.
/// </summary>
internal static class TestData
{
    public static readonly Guid EddystoneServiceUuid = new("0000FEAA-0000-1000-8000-00805F9B34FB");

    /// <summary>
    /// Parse a hex string with optional spaces, e.g. "AA FE 00".
    /// </summary>
    public static byte[] Hex(string hex) => Convert.FromHexString(hex.Replace(" ", string.Empty));

    public static byte[] Concat(params byte[][] parts) => parts.SelectMany(p => p).ToArray();

    public static byte[] Repeat(byte value, int count) => Enumerable.Repeat(value, count).ToArray();

    /// <summary>
    /// Build a raw advertisement structure: [length] [type] [data].
    /// </summary>
    public static byte[] AdStructure(byte type, byte[] data) =>
        Concat([(byte)(data.Length + 1), type], data);

    /// <summary>
    /// Build a complete raw Eddystone advertisement for the given service data payload (starting with AA FE).
    /// </summary>
    public static byte[] RawEddystoneAdvertisement(byte[] serviceData, bool includeUuidList = true) =>
        Concat(
            AdStructure(0x01, [0x06]),
            includeUuidList ? AdStructure(0x03, [0xAA, 0xFE]) : [],
            AdStructure(0x16, serviceData));

    public static BLEAdvertisementPacket Packet(ulong address, byte[] rawAdvertisement, short rssi = -60,
        DateTimeOffset? timestamp = null) =>
        new()
        {
            BluetoothAddress = address,
            RawSignalStrengthInDBm = rssi,
            Timestamp = timestamp ?? DateTimeOffset.Now,
            Advertisement = BLEAdvertisement.Parse(rawAdvertisement)
        };

    // Eddystone-UID: ranging data -18 dBm, namespace EDD1EBEAC04E5DEFA017, instance 0BDB87539B67, 2 RFU bytes
    public const string UidPayload = "AA FE 00 EE EDD1EBEAC04E5DEFA017 0BDB87539B67 0000";

    // Eddystone-URL: ranging data -21 dBm, https://www.google.com/
    public const string UrlPayload = "AA FE 10 EB 01 676F6F676C65 00";

    // Eddystone-TLM: 3000 mV, 24.5 °C, 256 frames, 10 * 0.1 s
    public const string TlmPayload = "AA FE 20 00 0BB8 1880 00000100 0000000A";

    // Eddystone eTLM: 12 bytes encrypted data, salt 0x1234, MIC 0xABCD
    public const string EncryptedTlmPayload = "AA FE 20 01 0102030405060708090A0B0C 1234 ABCD";

    // Eddystone-EID: ranging data -16 dBm, 8-byte ephemeral identifier
    public const string EidPayload = "AA FE 30 F0 1122334455667788";

    // iBeacon manufacturer data (without company ID): UUID, major 1, minor 2, Tx power -59 dBm
    public const string IBeaconUuid = "E2C56DB5-DFFB-48D2-B060-D0F5A71096E0";
    public const string IBeaconManufacturerData = "02 15 E2C56DB5DFFB48D2B060D0F5A71096E0 0001 0002 C5";
}
