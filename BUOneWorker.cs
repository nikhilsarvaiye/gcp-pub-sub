using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gcp_pub_sub
{
    public class BUOneWorker : BackgroundService
    {
        private readonly ILogger<BUOneWorker> _logger;
        private readonly IPubSubService _pubSubService;

        private const string ProjectId = "graphical-fort-306505";
        private const string TopicId = "ns-pub-sub";
        private const string SubscriptionId = "projects/graphical-fort-306505/subscriptions/ns-pub-sub-sub";

        public BUOneWorker(ILogger<BUOneWorker> logger, IPubSubService pubSubService)
        {
            _logger = logger;
            _pubSubService = pubSubService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker Running !!!");

                    _pubSubService.SetProjectId(ProjectId);

                    await _pubSubService.RunAsync("ns3-test-topic", "ns3-test-topic-subscription");

                    // await Task.Delay(1000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                // When the stopping token is canceled, for example, a call made from services.msc,
                // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);

                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
        }
    }
}