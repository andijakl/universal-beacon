using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;
using static UniversalBeacon.Library.Tests.TestData;

namespace UniversalBeacon.Library.Tests;

public class BeaconManagerTests
{
    private sealed class FakeProvider : IBluetoothPacketProvider
    {
        public event EventHandler<BLEAdvertisementPacketArgs>? AdvertisementPacketReceived;
        public event EventHandler<BTError>? WatcherStopped;

        public bool Started { get; private set; }
        public bool HasSubscribers => AdvertisementPacketReceived != null;

        public void Send(BLEAdvertisementPacket packet) =>
            AdvertisementPacketReceived?.Invoke(this, new BLEAdvertisementPacketArgs(packet));

        public void Start() => Started = true;

        public void Stop()
        {
            Started = false;
            WatcherStopped?.Invoke(this, new BTError(BTError.BluetoothError.Success));
        }
    }

    [Fact]
    public void Packets_AreClusteredByAddress()
    {
        var provider = new FakeProvider();
        using var manager = new BeaconManager(provider);
        var added = new List<Beacon>();
        manager.BeaconAdded += (_, b) => added.Add(b);

        provider.Send(Packet(1, RawEddystoneAdvertisement(Hex(UidPayload))));
        provider.Send(Packet(1, RawEddystoneAdvertisement(Hex(TlmPayload))));
        provider.Send(Packet(2, RawEddystoneAdvertisement(Hex(UrlPayload))));

        Assert.Equal(2, manager.BluetoothBeacons.Count);
        Assert.Equal(2, added.Count);
        Assert.Equal(2, manager.BluetoothBeacons[0].BeaconFrames.Count);
    }

    [Fact]
    public void StartAndStop_AreRelayedToProvider()
    {
        var provider = new FakeProvider();
        using var manager = new BeaconManager(provider);

        manager.Start();
        Assert.True(provider.Started);
        manager.Stop();
        Assert.False(provider.Started);
    }

    [Fact]
    public void InvokeAction_IsUsedForProcessing()
    {
        var provider = new FakeProvider();
        var queued = new List<Action>();
        using var manager = new BeaconManager(provider, queued.Add);

        provider.Send(Packet(1, RawEddystoneAdvertisement(Hex(UidPayload))));
        Assert.Empty(manager.BluetoothBeacons);

        Assert.Single(queued).Invoke();
        Assert.Single(manager.BluetoothBeacons);
    }

    [Fact]
    public async Task ConcurrentPackets_AreProcessedSafely()
    {
        var provider = new FakeProvider();
        using var manager = new BeaconManager(provider);

        await Task.WhenAll(Enumerable.Range(0, 8).Select(t => Task.Run(() =>
        {
            for (ulong i = 0; i < 500; i++)
            {
                provider.Send(Packet(i % 50, RawEddystoneAdvertisement(Hex(TlmPayload))));
            }
        }, TestContext.Current.CancellationToken)));

        Assert.Equal(50, manager.BluetoothBeacons.Count);
        Assert.Equal(50, manager.BluetoothBeacons.Select(b => b.BluetoothAddress).Distinct().Count());
    }

    [Fact]
    public void RemoveStaleBeacons_RemovesOnlyOldBeacons()
    {
        var provider = new FakeProvider();
        using var manager = new BeaconManager(provider);
        provider.Send(Packet(1, [], timestamp: DateTimeOffset.Now - TimeSpan.FromMinutes(5)));
        provider.Send(Packet(2, [], timestamp: DateTimeOffset.Now));

        var removed = manager.RemoveStaleBeacons(TimeSpan.FromMinutes(1));

        Assert.Equal(1, removed);
        Assert.Equal(2UL, Assert.Single(manager.BluetoothBeacons).BluetoothAddress);

        // A removed beacon is added again when it is received again.
        provider.Send(Packet(1, []));
        Assert.Equal(2, manager.BluetoothBeacons.Count);
    }

    [Fact]
    public void Dispose_UnsubscribesFromProvider()
    {
        var provider = new FakeProvider();
        var manager = new BeaconManager(provider);

        manager.Dispose();

        Assert.False(provider.HasSubscribers);
    }
}
