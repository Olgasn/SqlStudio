using MediatR;
using SlqStudio.Application.CQRS.Course.Commands.Handlers;
using SlqStudio.Application.CQRS.Course.Queries.Handlers;
using SlqStudio.Application.CQRS.LabTask.Commands.Handler;
using SlqStudio.Application.CQRS.LabTask.Queries.Handler;
using SlqStudio.Application.CQRS.LabWork.Commands.Handlers;
using SlqStudio.Application.CQRS.LabWork.Queries.Handlers;

namespace SlqStudio.Extensions;

public static class MediatRExtensions
{
    public static void AddCustomMediatR(this IServiceCollection services)
    {
        var assemblies = new[]
        {
            typeof(CreateCourseCommandHandler).Assembly,
            typeof(UpdateCourseCommandHandler).Assembly,
            typeof(DeleteCourseCommandHandler).Assembly,
            typeof(GetAllCoursesQueryHandler).Assembly,
            typeof(GetCourseByIdQueryHandler).Assembly,
            typeof(CreateLabWorkCommandHandler).Assembly,
            typeof(UpdateLabWorkCommandHandler).Assembly,
            typeof(DeleteLabWorkCommandHandler).Assembly,
            typeof(GetAllLabWorksQueryHandler).Assembly,
            typeof(GetLabWorkByIdQueryHandler).Assembly,
            typeof(CreateTaskCommandHandler).Assembly,
            typeof(UpdateTaskCommandHandler).Assembly,
            typeof(DeleteTaskCommandHandler).Assembly,
            typeof(GetAllTasksQueryHandler).Assembly,
            typeof(GetTaskByIdQueryHandler).Assembly
        };

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
    }
}
