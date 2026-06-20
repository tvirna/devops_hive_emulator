using DevOpsProject.CommunicationControl.Logic.Services;
using DevOpsProject.CommunicationControl.Logic.Services.Interfaces;
using DevOpsProject.Shared.Clients;
using DevOpsProject.Shared.Configuration;
using DevOpsProject.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DevOpsProject.Tests;

public class CommunicationControlServiceTests
{
    private readonly Mock<ISpatialService> _spatialService = new();
    private readonly Mock<IRedisKeyValueService> _redisService = new();
    private readonly Mock<IPublishService> _messageBus = new();
    private readonly Mock<ILogger<CommunicationControlService>> _logger = new();
    private readonly Mock<IOptionsMonitor<ComControlCommunicationConfiguration>> _commConfig = new();

    private CommunicationControlService CreateService(string hiveKey = "hive", string interferenceKey = "interference")
    {
        var snapshot = new Mock<IOptionsSnapshot<RedisKeys>>();
        snapshot.Setup(s => s.Value).Returns(new RedisKeys { HiveKey = hiveKey, InterferenceKey = interferenceKey });
        _messageBus.Setup(m => m.Publish(It.IsAny<object>())).Returns(Task.CompletedTask);
        var httpClient = new CommunicationControlHttpClient(new HttpClient());
        return new CommunicationControlService(
            _spatialService.Object,
            _redisService.Object,
            snapshot.Object,
            _messageBus.Object,
            httpClient,
            _logger.Object,
            _commConfig.Object);
    }

    [Fact]
    public async Task IsHiveConnected_WhenKeyExists_ReturnsTrue()
    {
        _redisService.Setup(r => r.CheckIfKeyExists("hive:drone-1")).ReturnsAsync(true);
        var result = await CreateService().IsHiveConnected("drone-1");
        Assert.True(result);
    }

    [Fact]
    public async Task IsHiveConnected_WhenKeyMissing_ReturnsFalse()
    {
        _redisService.Setup(r => r.CheckIfKeyExists("hive:drone-1")).ReturnsAsync(false);
        var result = await CreateService().IsHiveConnected("drone-1");
        Assert.False(result);
    }

    [Fact]
    public async Task GetHiveModel_ReturnsModelFromRedis()
    {
        var expected = new HiveModel { HiveID = "drone-1" };
        _redisService.Setup(r => r.GetAsync<HiveModel>("hive:drone-1")).ReturnsAsync(expected);
        var result = await CreateService().GetHiveModel("drone-1");
        Assert.Equal(expected.HiveID, result.HiveID);
    }

    [Fact]
    public async Task DisconnectHive_WhenRedisSucceeds_ReturnsTrue()
    {
        _redisService.Setup(r => r.DeleteAsync("hive:drone-1")).ReturnsAsync(true);
        var result = await CreateService().DisconnectHive("drone-1");
        Assert.True(result);
    }

    [Fact]
    public async Task DisconnectHive_PublishesDisconnectedMessage()
    {
        _redisService.Setup(r => r.DeleteAsync(It.IsAny<string>())).ReturnsAsync(true);
        await CreateService().DisconnectHive("drone-1");
        _messageBus.Verify(m => m.Publish(It.IsAny<object>()), Times.Once);
    }
}
