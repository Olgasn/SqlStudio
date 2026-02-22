using MediatR;
using Microsoft.EntityFrameworkCore;
using SlqStudio.Persistence;

namespace SlqStudio.Application.CQRS.Course.Queries.Handlers;

public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, Persistence.Models.Course?>
{
    private readonly ApplicationDbContext _context;
    public GetCourseByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<Persistence.Models.Course?> Handle(GetCourseByIdQuery request, CancellationToken ct)
        => await _context.Courses
            .Include(c => c.LabWorks)
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct);
}
