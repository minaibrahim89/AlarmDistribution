using System.Net;
using System.Net.Http.Json;
using AlarmDistribution.WebApi.Application.Commands.PublishAlarm;
using AlarmDistribution.WebApi.Application.Models;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;

namespace AlarmDistribution.WebApi.Tests.IntegrationTests;

public class PublishAlarmIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _httpClient;

    public PublishAlarmIntegrationTests(TestApplicationFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
    }

    [Fact]
    public async Task PublishAlarm_WhenValid_NotifiesOnlyPrimaryNurseAndReturnsSuccess()
    {
        // Arrange
        var publishAlarmRequest = new PublishAlarmCommand
        {
            AlarmId = 1,
            PatientId = 1,
            AlarmType = AlarmType.HeartRate,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var response = await _httpClient.PostAsync(
            "/alarms",
            JsonContent.Create(publishAlarmRequest));

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var primaryNurse = await _httpClient.GetFromJsonAsync<NurseResponse>("/Nurses/1");
        var secondaryNurse = await _httpClient.GetFromJsonAsync<NurseResponse>("/Nurses/2");

        Assert.NotNull(primaryNurse);
        Assert.NotNull(secondaryNurse);
        Assert.Contains(publishAlarmRequest.AlarmId, primaryNurse.PendingAlarms);
        Assert.DoesNotContain(publishAlarmRequest.AlarmId, secondaryNurse.PendingAlarms);
    }
}
