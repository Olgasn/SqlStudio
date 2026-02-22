using MediatR;
using Microsoft.EntityFrameworkCore;
using SlqStudio.Persistence;

namespace SlqStudio.Application.CQRS.LabWork.Queries.Handlers;

public class GetLabWorkByIdQueryHandler : IRequestHandler<GetLabWorkByIdQuery, Persistence.Models.LabWork?>
{
    private readonly ApplicationDbContext _context;
    public GetLabWorkByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<Persistence.Models.LabWork?> Handle(GetLabWorkByIdQuery request, CancellationToken ct)
        => await _context.LabWorks
            .Include(lw => lw.Course)
            .Include(lw => lw.Tasks)
            .FirstOrDefaultAsync(lw => lw.Id == request.Id, ct);
}
