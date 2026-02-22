using MediatR;

namespace SlqStudio.Application.CQRS.LabWork.Queries;

public record GetLabWorkByIdQuery(int Id) : IRequest<Persistence.Models.LabWork?>;
