using MediatR;

namespace SlqStudio.Application.CQRS.LabTask.Queries;

public record GetTaskByIdQuery(int Id) : IRequest<Persistence.Models.LabTask?>;
