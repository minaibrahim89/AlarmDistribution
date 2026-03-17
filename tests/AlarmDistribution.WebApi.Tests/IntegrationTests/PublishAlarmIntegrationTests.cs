using System.Net;
using System.Net.Http.Json;
using AlarmDistribution.WebApi.Application.Commands.PublishAlarm;
using AlarmDistribution.WebApi.Application.Models;
using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Xunit.Abstractions;

namespace AlarmDistribution.WebApi.Tests.IntegrationTests;

public class PublishAlarmIntegrationTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _httpClient;
    private readonly ITestOutputHelper _output;
    private FakeTimeProvider _timeProvider;

    public PublishAlarmIntegrationTests(TestApplicationFactory appFactory, ITestOutputHelper output)
    {
        _httpClient = appFactory.CreateClient();
        _timeProvider = (FakeTimeProvider)appFactory.Services.GetRequiredService<TimeProvider>();
        _output = output;
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
        if (!response.IsSuccessStatusCode)
            _output.WriteLine(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var primaryNurse = await _httpClient.GetFromJsonAsync<NurseResponse>("/Nurses/1");
        var secondaryNurse = await _httpClient.GetFromJsonAsync<NurseResponse>("/Nurses/2");

        Assert.NotNull(primaryNurse);
        Assert.NotNull(secondaryNurse);
        Assert.Contains(publishAlarmRequest.AlarmId, primaryNurse.PendingAlarms);
        Assert.DoesNotContain(publishAlarmRequest.AlarmId, secondaryNurse.PendingAlarms);
    }

    [Fact]
    public async Task PublishAlarm_WhenNotAckedByPrimaryNurse_NotifiesSecondaryAfterTimeout()
    {
        // Arrange
        var publishAlarmRequest = new PublishAlarmCommand
        {
            AlarmId = 2,
            PatientId = 2,
            AlarmType = AlarmType.HeartRate,
            Timestamp = DateTime.UtcNow
        };

        // Act
        var response = await _httpClient.PostAsync(
            "/alarms",
            JsonContent.Create(publishAlarmRequest));

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var primaryNurse = await _httpClient.GetFromJsonAsync<NurseResponse>("/Nurses/2");
        var secondaryNurse = await _httpClient.GetFromJsonAsync<NurseResponse>("/Nurses/3");

        Assert.NotNull(primaryNurse);
        Assert.NotNull(secondaryNurse);
        Assert.Contains(publishAlarmRequest.AlarmId, primaryNurse.PendingAlarms);
        Assert.DoesNotContain(publishAlarmRequest.AlarmId, secondaryNurse.PendingAlarms);

        _timeProvider.Advance(TimeSpan.FromMinutes(1));
        secondaryNurse = await _httpClient.GetFromJsonAsync<NurseResponse>("/Nurses/2");

        Assert.NotNull(secondaryNurse);
        Assert.Contains(publishAlarmRequest.AlarmId, secondaryNurse.PendingAlarms);
    }
}
