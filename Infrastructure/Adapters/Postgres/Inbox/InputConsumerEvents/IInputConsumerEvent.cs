using FluentResults;
using MediatR;

namespace Infrastructure.Adapters.Postgres.Inbox.InputConsumerEvents;

public interface IInputConsumerEvent
{
    public Guid EventId { get; }

    IRequest<Result> ToCommand();
}