namespace RScore.Consumer.Match.Application.Features.MatchConsumer;

public interface IMatchConsumerHandler
{
    public Task ExecuteAsync(MatchConsumerRequest request, CancellationToken cancellationToken = default);
}