using ATMS.Project.API.Controllers.v1;
using ATMS.Project.Contracts.Models.Dashboard;
using ATMS.Project.Contracts.Requests.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Project.API.Tests;

public sealed class DashboardControllerTest : BaseControllerTest
{
    [Fact]
    public async Task Get_ReturnsCompleteDashboardFromMediator()
    {
        var request = new GetDashboardRequest { ProjectId = Guid.NewGuid(), Period = 7 };
        var dashboard = new DashboardModel
        {
            Period = 7,
            Kpis = [],
            MainChart = new DashboardSeriesChartModel { Labels = [], Series = [] },
            Donuts = [],
            SecondaryChart = new DashboardSecondaryChartModel { Key = "byTicket", Segments = [] },
            Deadlines = [],
            Activities = []
        };
        MediatorMock.Setup(mediator => mediator.Send(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dashboard);
        var controller = new DashboardController(MediatorMock.Object);

        var result = await controller.Get(request, CancellationToken.None);

        Assert.Same(dashboard, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

}
