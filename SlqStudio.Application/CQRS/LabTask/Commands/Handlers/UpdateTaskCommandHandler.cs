using MediatR;
using SlqStudio.Persistence;

namespace SlqStudio.Application.CQRS.LabTask.Commands.Handler;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand>
{
    private readonly ApplicationDbContext _context;
    public UpdateTaskCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task Handle(UpdateTaskCommand request, CancellationToken ct)
    {
        var taskItem = await _context.LabTasks.FindAsync(request.Id);
        if (taskItem != null)
        {
            taskItem.Number = request.Number;
            taskItem.Title = request.Title;
            taskItem.Condition = request.Condition;
            taskItem.SolutionExample = request.SolutionExample;
            taskItem.LabWorkId = request.LabWorkId;
            await _context.SaveChangesAsync(ct);
        }
    }
}
