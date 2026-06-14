using MediatR;
using Microsoft.EntityFrameworkCore;
using SlqStudio.Persistence;

namespace SlqStudio.Application.CQRS.LabTask.Queries.Handler;

public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, List<Persistence.Models.LabTask>>
{
    private readonly ApplicationDbContext _context;
    public GetAllTasksQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<List<Persistence.Models.LabTask>> Handle(GetAllTasksQuery request, CancellationToken ct)
        => await _context.LabTasks.Include(l => l.LabWork).ToListAsync(ct);
}