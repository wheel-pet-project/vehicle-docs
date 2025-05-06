using FluentResults;
using MediatR;

namespace Infrastructure.Adapters.Postgres.Inbox.InputConsumerEvents;

public interface IConvertibleToCommand
{
    public Guid EventId { get; }

    IRequest<Result> ToCommand();
}