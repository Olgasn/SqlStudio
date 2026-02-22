using MediatR;
using SlqStudio.Persistence;

namespace SlqStudio.Application.CQRS.Course.Commands.Handlers;

public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteCourseCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteCourseCommand request, CancellationToken ct)
    {
        var course = await _context.Courses.FindAsync(request.Id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync(ct);
        }
    }
}
