using FluentResults;
using Infrastructure.Adapters.Postgres.Inbox;
using Infrastructure.Adapters.Postgres.Inbox.InputConsumerEvents;
using JetBrains.Annotations;
using JsonNet.ContractResolvers;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Npgsql;
using Quartz;
using Xunit;

namespace IntegrationTests.Inbox;

[TestSubject(typeof(InboxBackgroundJob))]
public class InboxBackgroundJobShould : IntegrationTestBase
{
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        ContractResolver = new PrivateSetterContractResolver()
    };

    private readonly IInputConsumerEvent _event = new VehicleAddedConsumerEvent(Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task MarkProcessedEvents()
    {
        // Arrange
        var inbox = new Infrastructure.Adapters.Postgres.Inbox.Inbox(Context);
        await inbox.Save(_event);
        var jobBuilder = new InboxBackgroundJobBuilder(DataSource);
        var job = jobBuilder.Build();

        // Act
        await job.Execute(new Mock<IJobExecutionContext>().Object);

        // Assert
        var existEvents = Context.Inbox.Take(1).ToList();
        var actualEvent =
            JsonConvert.DeserializeObject<IInputConsumerEvent>(existEvents.First().Content, _jsonSerializerSettings);
        Assert.NotNull(actualEvent);
        Assert.Equivalent(_event, actualEvent);
    }

    [Fact]
    public async Task ReadAndMediatorSendCommandOneTimes()
    {
        // Arrange
        var inbox = new Infrastructure.Adapters.Postgres.Inbox.Inbox(Context);
        await inbox.Save(_event);
        var jobBuilder = new InboxBackgroundJobBuilder(DataSource);
        var job = jobBuilder.Build();

        // Act
        await job.Execute(new Mock<IJobExecutionContext>().Object);

        // Assert
        jobBuilder.VerifyMediatorSendMethodCalls(1);
    }

    private class InboxBackgroundJobBuilder
    {
        private readonly Mock<IMediator> _mediatorMock = new();
        private readonly Mock<ILogger<InboxBackgroundJob>> _loggerMock = new();
        private readonly InboxBackgroundJob _inboxBackgroundJob;

        public InboxBackgroundJobBuilder(NpgsqlDataSource dataSource)
        {
            _mediatorMock.Setup(x => x.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok);
            _inboxBackgroundJob = new InboxBackgroundJob(dataSource, _mediatorMock.Object, _loggerMock.Object);
        }

        public InboxBackgroundJob Build()
        {
            return _inboxBackgroundJob;
        }

        public void VerifyMediatorSendMethodCalls(int times)
        {
            _mediatorMock.Verify(m => m.Send(It.IsAny<IRequest<Result>>(), It.IsAny<CancellationToken>()),
                Times.Exactly(times));
        }
    }
}