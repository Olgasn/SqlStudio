using MediatR;
using SlqStudio.Persistence;

namespace SlqStudio.Application.CQRS.Course.Commands.Handlers;

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand>
{
    private readonly ApplicationDbContext _context;
    public UpdateCourseCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateCourseCommand request, CancellationToken ct)
    {
        var course = await _context.Courses.FindAsync(request.Id);
        if (course != null)
        {
            course.Name = request.Name;
            course.Description = request.Description;
            await _context.SaveChangesAsync(ct);
        }
    }
}
