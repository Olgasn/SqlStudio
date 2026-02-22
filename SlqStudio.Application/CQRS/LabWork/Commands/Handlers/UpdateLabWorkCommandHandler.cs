using MediatR;
using SlqStudio.Persistence;

namespace SlqStudio.Application.CQRS.LabWork.Commands.Handlers;

public class UpdateLabWorkCommandHandler : IRequestHandler<UpdateLabWorkCommand>
{
    private readonly ApplicationDbContext _context;
    public UpdateLabWorkCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateLabWorkCommand request, CancellationToken ct)
    {
        var labWork = await _context.LabWorks.FindAsync(request.Id);
        if (labWork != null)
        {
            labWork.Name = request.Name;
            labWork.Description = request.Description;
            labWork.Number = request.Number;
            labWork.CourseId = request.CourseId;
            await _context.SaveChangesAsync(ct);
        }
    }
}
