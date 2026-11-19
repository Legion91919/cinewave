using Cinewave.Subscriptions.Domain;

namespace Cinewave.Subscriptions.Application;

internal interface ISubscriptionRepository
{
    Subscription? ForAccount(string accountId);
}
