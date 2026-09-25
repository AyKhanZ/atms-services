using ATMS.Application.Models;
using System.Text.Json;
using ATMS.Data.Constants;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Contracts.Models.Dashboard;
using ATMS.Project.Contracts.Requests.Dashboard;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Dashboard;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Dashboard;
using ATMS.Project.Services.Dictionaries.Interfaces;
using ATMS.Project.Services.Handlers.Dashboard;
using ATMS.Project.Services.History.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Project.Services.Tests.Handlers.Dashboard;

public sealed class GetDashboardHandlerTest : BaseHandlerTest
{
    private readonly Mock<IDashboardRepository> _repository = new();
    private readonly Mock<IDictionaryCacheService> _dictionaries = new();
    private readonly Mock<IHistoryValueResolver> _history = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();
    private readonly DefaultHttpContext _httpContext = new();
    private readonly BusinessTimeZone _zone = new(TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"));

    public GetDashboardHandlerTest()
    {
        _httpContextAccessor.Setup(accessor => accessor.HttpContext).Returns(_httpContext);
        CurrentUserMock.Setup(user => user.Id).Returns(Guid.NewGuid());
        CurrentUserMock.Setup(user => user.RoleId).Returns(RoleIds.Employee);
        _repository.Setup(repository => repository.GetAsync(
                It.IsAny<ICriteria<WorkProject>>(), It.IsAny<Guid?>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<double>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmptyData());
        _dictionaries.Setup(service => service.GetWorkTaskStatusesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new DictionaryModel { Id = 1, Code = "New", Name = "New" },
                new DictionaryModel { Id = 2, Code = "InProgress", Name = "In progress" },
                new DictionaryModel { Id = 3, Code = "Done", Name = "Done" }
            ]);
        _dictionaries.Setup(service => service.GetWorkItemPrioritiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new DictionaryModel { Id = 1, Code = "Low", Name = "Low" }]);
        _history.Setup(service => service.ResolveEntriesAsync(
                It.IsAny<IReadOnlyCollection<HistoryEntry>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(29)]
    [InlineData(91)]
    public async Task Handle_UnsupportedPeriod_ReturnsValidationError(int period)
    {
        await Assert.ThrowsAsync<ValidationException>(() => Handler().Handle(
            new GetDashboardRequest { Period = period }, CancellationToken.None));

        _repository.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("period=abc", "Period")]
    [InlineData("projectId=abc&period=7", "ProjectId")]
    public async Task Handle_MalformedQuery_ReturnsValidationError(string query, string field)
    {
        _httpContext.Request.QueryString = new QueryString("?" + query);

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            Handler().Handle(new GetDashboardRequest(), CancellationToken.None));

        Assert.Equal(field, Assert.Single(exception.Errors).PropertyName);
        _repository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_InaccessibleProject_ReturnsNotFound()
    {
        var projectId = Guid.NewGuid();
        _repository.Setup(repository => repository.IsProjectAccessibleAsync(
                It.IsAny<ICriteria<WorkProject>>(), projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<ATMS.Application.Exceptions.Entity.EntityException>(() =>
            Handler().Handle(new GetDashboardRequest { ProjectId = projectId }, CancellationToken.None));

        _repository.Verify(repository => repository.GetAsync(
            It.IsAny<ICriteria<WorkProject>>(), It.IsAny<Guid?>(),
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<double>(),
            It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(ClientRoles))]
    public async Task Handle_Client_HasNoWorkload(Guid roleId)
    {
        CurrentUserMock.Setup(user => user.RoleId).Returns(roleId);

        var result = await Handler().Handle(new GetDashboardRequest(), CancellationToken.None);

        Assert.Null(result.Workload);
        _repository.Verify(repository => repository.GetAsync(
            It.IsAny<ICriteria<WorkProject>>(), It.IsAny<Guid?>(),
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<double>(),
            false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyData_FillsEveryDayAndLeavesDoneChangeNull()
    {
        var result = await Handler().Handle(new GetDashboardRequest { Period = 7 }, CancellationToken.None);

        Assert.Equal(7, result.MainChart.Labels.Length);
        Assert.All(result.MainChart.Series, series => Assert.Equal(new int[7], series.Data));
        var done = Assert.IsType<DashboardDoneKpiModel>(Assert.Single(result.Kpis, kpi => kpi.Key == "done"));
        Assert.Equal(0, done.PreviousValue);
        Assert.Null(done.ChangePercent);
        Assert.Equal("byProject", result.SecondaryChart.Key);

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);
        var kpis = document.RootElement.GetProperty("kpis");
        Assert.False(kpis[0].TryGetProperty("previousValue", out _));
        Assert.True(kpis[3].TryGetProperty("changePercent", out var change));
        Assert.Equal(JsonValueKind.Null, change.ValueKind);
    }

    [Fact]
    public async Task Handle_StatusCounts_BuildsOpenAndOverdueKpis()
    {
        _repository.Setup(repository => repository.GetAsync(
                It.IsAny<ICriteria<WorkProject>>(), It.IsAny<Guid?>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<double>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmptyData(new Dictionary<int, DashboardStatusCount>
            {
                [1] = new(2, 1, 1),
                [2] = new(1, 0, 0)
            }));

        var result = await Handler().Handle(new GetDashboardRequest(), CancellationToken.None);

        Assert.Equal(3, Assert.Single(result.Kpis, kpi => kpi.Key == "open").Value);
        Assert.Equal(1, Assert.Single(result.Kpis, kpi => kpi.Key == "overdue").Value);
        var workload = Assert.IsType<DashboardWorkloadModel>(result.Workload);
        Assert.Equal(1, Assert.Single(workload.Segments, segment => segment.Kind == "unassigned").Value);
    }

    [Fact]
    public async Task Handle_EntityAndPersonRows_KeepDashboardJsonShape()
    {
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        _repository.Setup(repository => repository.GetAsync(
                It.IsAny<ICriteria<WorkProject>>(), It.IsAny<Guid?>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<double>(),
                It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmptyData(
                new Dictionary<int, DashboardStatusCount> { [1] = new(3, 0, 0) },
                [new DashboardWorkloadRow { Id = userId, Name = "Leyla", Surname = "M.", Count = 2 }],
                [new DashboardEntityRow { Id = projectId, Code = "7", Name = "Alpha", Count = 3 }]));

        var result = await Handler().Handle(new GetDashboardRequest(), CancellationToken.None);

        var workload = Assert.IsType<DashboardWorkloadModel>(result.Workload);
        var person = Assert.IsType<HistoryPersonModel>(
            Assert.Single(workload.Segments, item => item.Kind == "user").Person);
        Assert.Equal("Leyla", person.Name);
        Assert.Equal("Alpha", Assert.Single(result.SecondaryChart.Segments).Label);
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var document = JsonDocument.Parse(json);
        var segment = document.RootElement.GetProperty("secondaryChart").GetProperty("segments")[0];
        Assert.Equal("Alpha", segment.GetProperty("label").GetString());
        Assert.False(segment.TryGetProperty("name", out _));
    }

    private GetDashboardHandler Handler() => new(
        CurrentUserMock.Object, _httpContextAccessor.Object, _zone,
        _repository.Object, _dictionaries.Object, _history.Object);

    public static TheoryData<Guid> ClientRoles => new()
    {
        RoleIds.Client,
        RoleIds.ClientManager
    };

    private static DashboardData EmptyData(
        Dictionary<int, DashboardStatusCount>? counts = null,
        DashboardWorkloadRow[]? workload = null,
        DashboardEntityRow[]? secondary = null) => new()
    {
        StatusCounts = counts ?? [],
        PriorityCounts = [],
        CreatedByDay = [],
        DoneByDay = [],
        Workload = workload ?? [],
        Secondary = secondary ?? [],
        Deadlines = [],
        Activities = []
    };
}
