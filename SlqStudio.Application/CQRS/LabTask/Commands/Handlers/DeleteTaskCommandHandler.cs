using MediatR;
using SlqStudio.Persistence;

namespace SlqStudio.Application.CQRS.LabTask.Commands.Handler;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
{
    private readonly ApplicationDbContext _context;
    public DeleteTaskCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteTaskCommand request, CancellationToken ct)
    {
        var taskItem = await _context.LabTasks.FindAsync(request.Id);
        if (taskItem != null)
        {
            _context.LabTasks.Remove(taskItem);
            await _context.SaveChangesAsync(ct);
        }
    }
}
