using ATMS.Application.Interfaces;
using ATMS.Application.Localization;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTaskBoard;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTaskBoard;

public sealed class GetWorkTaskCalendarHandler(
    ICurrentUser currentUser,
    IWorkTaskBoardRepository workTaskBoardRepository,
    IMapper mapper) : IRequestHandler<GetWorkTaskCalendarRequest, WorkTaskDeadlineGroupModel[]>
{
    // a day cell fits about six cards; the rest is behind "+ N more"
    private const int CardsPerDeadline = 6;

    // a month grid is 6 weeks at most; a longer range would read every task of it at once
    private static readonly TimeSpan MaxRange = TimeSpan.FromDays(42);

    public async Task<WorkTaskDeadlineGroupModel[]> Handle(GetWorkTaskCalendarRequest request, CancellationToken cancellationToken)
    {
        var range = request.DeadlineTo - request.DeadlineFrom;
        if (range <= TimeSpan.Zero || range > MaxRange)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(request.DeadlineTo), WorkTaskMessages.CalendarRangeInvalid)]);
        }

        var filter = mapper.Map<WorkTaskBoardFilter>(request);
        var criteria = filter
            .And(new WorkTasksOfLiveWorkCriteria())
            .And(new ExceptSuperAdminCriteria<WorkTask>(
                currentUser.RoleId,
                new WorkTasksOfMyProjectsCriteria(currentUser.Id)));

        string[] languages = [CultureHelper.CurrentLanguage, SupportedLanguages.English];

        var result = await workTaskBoardRepository.GetCalendarAsync(criteria, CardsPerDeadline, languages, cancellationToken);

        return result.Groups
            .Select(group => new WorkTaskDeadlineGroupModel
            {
                Deadline = group.Deadline,
                Total = group.Total,
                WorkTasks = group.WorkTasks
                    .Select(task =>
                    {
                        var model = mapper.Map<WorkTaskModel>(task);
                        if (result.SubtaskProgress.TryGetValue(task.Id, out var progress))
                        {
                            model.SubtaskCount = progress.Total;
                            model.DoneSubtaskCount = progress.Done;
                        }

                        return model;
                    })
                    .ToArray()
            })
            .ToArray();
    }
}
