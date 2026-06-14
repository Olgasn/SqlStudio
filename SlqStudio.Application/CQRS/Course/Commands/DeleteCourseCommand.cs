using MediatR;

namespace SlqStudio.Application.CQRS.Course.Commands;

public record DeleteCourseCommand(int Id) : IRequest;