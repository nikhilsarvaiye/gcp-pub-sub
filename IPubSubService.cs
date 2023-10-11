using Google.Cloud.PubSub.V1;

namespace gcp_pub_sub
{
    public interface IPubSubService
    {
        void SetProjectId(string projectId);
        
        Task<int> PublishMessagesAsync(string topicId, IEnumerable<string> messageTexts);

        Task RunAsync(string topicId, string subscriptionId);

        Task<Topic> CreateTopicAsync(string topicId);

        Task<Topic> GetTopicAsync(string topicId);

        Task<Subscription> CreateSubscriptionAsync(string topicId, string subscriptionId);

        Task<Subscription> GetSubscriptionAsync(string topicId, string subscriptionId);

        Task<string> PublishMessageAsync(string topicId, string message);

        Task<List<PubsubMessage>> PullMessageAsync(string subscriptionId);

        Task DeleteTopicAsync(string topicId);

        Task DeleteSubscriptionAsync(string subscriptionId);
    }
}
