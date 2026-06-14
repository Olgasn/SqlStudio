using MediatR;
using SlqStudio.Persistence;

namespace SlqStudio.Application.CQRS.LabTask.Commands.Handler;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, int>
{
    private readonly ApplicationDbContext _context;
    public CreateTaskCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<int> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        var taskItem = new Persistence.Models.LabTask
        {
            Number = request.Number,
            Title = request.Title,
            Condition = request.Condition,
            SolutionExample = request.SolutionExample,
            LabWorkId = request.LabWorkId
        };

        _context.LabTasks.Add(taskItem);
        await _context.SaveChangesAsync(ct);
        return taskItem.Id;
    }
}