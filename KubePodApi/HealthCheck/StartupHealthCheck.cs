using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KubePodApi.HealthCheck
{
    public class StartupHealthCheck : IHealthCheck
    {
        static int callsBeforeSuccess;
        static int callsCounter = 0;
        public StartupHealthCheck()
        {
            callsBeforeSuccess = int.Parse(
              Environment.GetEnvironmentVariable("CALLS_BEFORE_SUCCESS") ?? "0");
        }
        public Task<HealthCheckResult> CheckHealthAsync(
          HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            callsCounter++;
            if (callsCounter < callsBeforeSuccess)
            {
                string errorMsg = $"Still starting up... ({callsCounter}/{callsBeforeSuccess})";
                Console.WriteLine(errorMsg);
                return Task.FromResult(HealthCheckResult.Unhealthy(errorMsg));
            }
            string successMsg = "Startup completed.";
            Console.WriteLine(successMsg);
            return Task.FromResult(HealthCheckResult.Healthy(successMsg));
        }
    }

}
