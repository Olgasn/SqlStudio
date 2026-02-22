using MediatR;
using SlqStudio.Persistence;

namespace SlqStudio.Application.CQRS.LabWork.Commands.Handlers;

public class DeleteLabWorkCommandHandler : IRequestHandler<DeleteLabWorkCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteLabWorkCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteLabWorkCommand request, CancellationToken ct)
    {
        var labWork = await _context.LabWorks.FindAsync(request.Id);
        if (labWork != null)
        {
            _context.LabWorks.Remove(labWork);
            await _context.SaveChangesAsync(ct);
        }
    }
}
