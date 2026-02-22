using MediatR;

namespace SlqStudio.Application.CQRS.Course.Queries;

public record GetCourseByIdQuery(int Id) : IRequest<Persistence.Models.Course?>;
