using DevOpsProject.CommunicationControl.Logic.Services;
using DevOpsProject.Shared.Models;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DevOpsProject.Tests;

public class SpatialServiceTests
{
    private static SpatialService CreateService(OperationalAreaConfig config)
    {
        var monitor = new Mock<IOptionsMonitor<OperationalAreaConfig>>();
        monitor.Setup(m => m.CurrentValue).Returns(config);
        return new SpatialService(monitor.Object);
    }

    [Fact]
    public void GetHiveOperationalArea_ReturnsRadiusFromConfig()
    {
        var config = new OperationalAreaConfig { Radius_KM = 5.5f };
        var result = CreateService(config).GetHiveOperationalArea(new HiveModel());
        Assert.Equal(config.Radius_KM, result.RadiusKM);
    }

    [Fact]
    public void GetHiveOperationalArea_ReturnsLocationFromConfig()
    {
        var config = new OperationalAreaConfig { Latitude = 50.4f, Longitude = 30.5f };
        var result = CreateService(config).GetHiveOperationalArea(new HiveModel());
        Assert.Equal(config.Latitude, result.InitialLocation.Latitude);
        Assert.Equal(config.Longitude, result.InitialLocation.Longitude);
    }

    [Fact]
    public void GetHiveOperationalArea_ReturnsIntervalsFromConfig()
    {
        var config = new OperationalAreaConfig { TelemetryInterval_MS = 500, PingInterval_MS = 1000 };
        var result = CreateService(config).GetHiveOperationalArea(new HiveModel());
        Assert.Equal(config.TelemetryInterval_MS, result.TelemetryIntervalMs);
        Assert.Equal(config.PingInterval_MS, result.PingIntervalMs);
    }

    [Fact]
    public void GetHiveOperationalArea_ReturnsSpeedAndHeightFromConfig()
    {
        var config = new OperationalAreaConfig { InitialHeight_KM = 1.2f, InitialSpeed_KM = 60f };
        var result = CreateService(config).GetHiveOperationalArea(new HiveModel());
        Assert.Equal(config.InitialHeight_KM, result.InitialHeight);
        Assert.Equal(config.InitialSpeed_KM, result.Speed);
    }
}
