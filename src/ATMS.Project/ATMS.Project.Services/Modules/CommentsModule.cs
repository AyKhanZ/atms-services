using ATMS.Project.Services.Comments;
using ATMS.Project.Services.Comments.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class CommentsModule
{
    public static IServiceCollection AddCommentServices(this IServiceCollection services)
    {
        services.AddScoped<ICommentModelService, CommentModelService>();
        services.AddSingleton<ICommentMentionService, CommentMentionService>();
        return services;
    }
}
