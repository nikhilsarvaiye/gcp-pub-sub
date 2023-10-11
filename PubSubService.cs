using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;

namespace gcp_pub_sub
{
    public class PubSubService : IPubSubService
    {
        private string _projectId;

        private readonly ILogger<PubSubService> _logger;

        public PubSubService(ILogger<PubSubService> logger)
        {
            _logger = logger;
        }

        public void SetProjectId(string projectId)
        {
            _projectId = projectId;
        }

        public async Task<int> PublishMessagesAsync(string topicId, IEnumerable<string> messageTexts)
        {
            TopicName topicName = TopicName.FromProjectTopic(_projectId, topicId);
            PublisherClient publisher = await PublisherClient.CreateAsync(topicName);

            int publishedMessageCount = 0;
            var publishTasks = messageTexts.Select(async text =>
            {
                try
                {
                    string message = await publisher.PublishAsync(text);
                    Console.WriteLine($"Published message {message}");
                    Interlocked.Increment(ref publishedMessageCount);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"An error ocurred when publishing message {text}: {exception.Message}");
                }
            });
            await Task.WhenAll(publishTasks);
            return publishedMessageCount;
        }

        public async Task RunAsync(string topicId, string subscriptionId)
        {
            try
            {
                // var topic = await CreateTopicAsync(topicId);

                // var subscription = await CreateSubscriptionAsync(topicId, subscriptionId);

                // var messageId = await PublishMessageAsync(topicId, "Hi, this is nikhil...");

                // var messages = await PullMessageAsync(subscriptionId);

                await DeleteSubscriptionAsync(subscriptionId);

                await DeleteTopicAsync(topicId);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public async Task<Topic> CreateTopicAsync(string topicId)
        {
            // First create a topic.
            TopicName topicName = new TopicName(_projectId, topicId);

            PublisherServiceApiClient publisherService = await PublisherServiceApiClient.CreateAsync();

            return await publisherService.CreateTopicAsync(topicName);
        }

        public async Task<Topic> GetTopicAsync(string topicId)
        {
            // First create a topic.
            TopicName topicName = new TopicName(_projectId, topicId);

            PublisherServiceApiClient publisherService = await PublisherServiceApiClient.CreateAsync();

            return await publisherService.GetTopicAsync(topicName);
        }

        public async Task<Subscription> CreateSubscriptionAsync(string topicId, string subscriptionId)
        {
            SubscriberServiceApiClient subscriberService = await SubscriberServiceApiClient.CreateAsync();

            return await subscriberService.CreateSubscriptionAsync(
                new SubscriptionName(_projectId, subscriptionId),
                new TopicName(_projectId, topicId),
                pushConfig: null,
                ackDeadlineSeconds: 60
            );
        }

        public async Task<Subscription> GetSubscriptionAsync(string topicId, string subscriptionId)
        {
            SubscriberServiceApiClient subscriberService = await SubscriberServiceApiClient.CreateAsync();

            return await subscriberService.GetSubscriptionAsync(new SubscriptionName(_projectId, topicId));
        }

        public async Task<string> PublishMessageAsync(string topicId, string message)
        {
            // Publish a message to the topic using PublisherClient.
            PublisherClient publisher = await PublisherClient.CreateAsync(new TopicName(_projectId, topicId));

            // PublishAsync() has various overloads. Here we're using the string overload.
            string messageId = await publisher.PublishAsync(message);

            // PublisherClient instance should be shutdown after use.
            // The TimeSpan specifies for how long to attempt to publish locally queued messages.
            await publisher.ShutdownAsync(TimeSpan.FromSeconds(15));

            return messageId;
        }

        public async Task<List<PubsubMessage>> PullMessageAsync(string subscriptionId)
        {
            SubscriptionName subscriptionName = new SubscriptionName(_projectId, subscriptionId);

            // Pull messages from the subscription using SubscriberClient.
            SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName);

            List<PubsubMessage> receivedMessages = new List<PubsubMessage>();

            // Start the subscriber listening for messages.
            await subscriber.StartAsync((msg, cancellationToken) =>
            {
                receivedMessages.Add(msg);

                _logger.LogInformation($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
                _logger.LogInformation($"Text: '{msg.Data.ToStringUtf8()}'");

                // Stop this subscriber after one message is received.
                // This is non-blocking, and the returned Task may be awaited.
                subscriber.StopAsync(TimeSpan.FromSeconds(15));

                // Return Reply.Ack to indicate this message has been handled.
                return Task.FromResult(SubscriberClient.Reply.Ack);
            });

            return receivedMessages;
        }

        public async Task DeleteTopicAsync(string topicId)
        {
            // subscriberService.DeleteSubscription(subscriptionName);
            PublisherServiceApiClient publisherService = await PublisherServiceApiClient.CreateAsync();

            publisherService.DeleteTopic(new TopicName(_projectId, topicId));
        }

        public async Task DeleteSubscriptionAsync(string subscriptionId)
        {
            SubscriberServiceApiClient subscriberService = await SubscriberServiceApiClient.CreateAsync();

            subscriberService.DeleteSubscription(new SubscriptionName(_projectId, subscriptionId));
        }
    }
}
