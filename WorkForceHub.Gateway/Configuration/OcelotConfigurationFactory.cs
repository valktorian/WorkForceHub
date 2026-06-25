using Ocelot.Configuration.File;

namespace WorkForceHub.Gateway.Configuration;

public static class OcelotConfigurationFactory
{
    public static FileConfiguration Build(DownstreamOptions d)
    {
        return new FileConfiguration
        {
            GlobalConfiguration = new FileGlobalConfiguration
            {
                RequestIdKey = "X-Correlation-ID"
            },
            Routes =
            [
                Route("/api/auth/login", ["POST"], d.AccountCommand),

                Route("/api/accounts", ["POST"], d.AccountCommand, new FileRateLimitRule
                {
                    EnableRateLimiting = true,
                    Period = "1m",
                    PeriodTimespan = 60,
                    Limit = 100,
                    ClientWhitelist = []
                }),
                Route("/api/accounts", ["GET"], d.AccountQuery),
                Route("/api/accounts/{id}", ["PUT", "DELETE"], d.AccountCommand),
                Route("/api/accounts/{id}/role", ["PATCH"], d.AccountCommand),
                Route("/api/accounts/{id}/password", ["PATCH"], d.AccountCommand),
                Route("/api/accounts/{everything}", ["GET"], d.AccountQuery),

                Route("/api/profiles", ["POST"], d.ProfileCommand),
                Route("/api/profiles/{id}", ["PUT", "DELETE"], d.ProfileCommand),
                Route("/api/profiles/{id}/{everything}", ["POST", "PATCH"], d.ProfileCommand),
                Route("/api/profiles/self/{everything}", ["PATCH"], d.ProfileCommand),
                Route("/api/profiles", ["GET"], d.ProfileQuery),
                Route("/api/profiles/self", ["GET"], d.ProfileQuery),
                Route("/api/profiles/{everything}", ["GET"], d.ProfileQuery),

                Route("/api/time-entries", ["POST"], d.TimeCommand),
                Route("/api/time-entries/{id}", ["PUT", "DELETE"], d.TimeCommand),
                Route("/api/time-entries", ["GET"], d.TimeQuery),
                Route("/api/time-entries/{everything}", ["GET"], d.TimeQuery),

                Route("/api/timesheets", ["POST"], d.TimeCommand),
                Route("/api/timesheets/{id}/{everything}", ["POST"], d.TimeCommand),
                Route("/api/timesheets", ["GET"], d.TimeQuery),
                Route("/api/timesheets/{everything}", ["GET"], d.TimeQuery),

                Route("/api/leave-requests", ["POST"], d.TimeCommand),
                Route("/api/leave-requests/{id}", ["PUT"], d.TimeCommand),
                Route("/api/leave-requests/{id}/{everything}", ["POST"], d.TimeCommand),
                Route("/api/leave-balances/{employeeId}/adjust", ["POST"], d.TimeCommand),
                Route("/api/leave-requests", ["GET"], d.TimeQuery),
                Route("/api/leave-requests/{everything}", ["GET"], d.TimeQuery),
                Route("/api/leave-balances/{everything}", ["GET"], d.TimeQuery),
                Route("/api/holidays", ["GET"], d.TimeQuery),
                Route("/api/leave-types", ["GET"], d.TimeQuery),

                Route("/api/media/images/{category}", ["POST"], d.Media),

                Route("/api/job-movements", ["POST"], d.EvolutionCommand),
                Route("/api/job-movements/{id}", ["PUT", "DELETE"], d.EvolutionCommand),
                Route("/api/job-movements", ["GET"], d.EvolutionQuery),
                Route("/api/job-movements/{everything}", ["GET"], d.EvolutionQuery),

                Route("/api/salary-changes", ["POST"], d.EvolutionCommand),
                Route("/api/salary-changes/{id}", ["PUT", "DELETE"], d.EvolutionCommand),
                Route("/api/salary-changes", ["GET"], d.EvolutionQuery),
                Route("/api/salary-changes/{everything}", ["GET"], d.EvolutionQuery),

                Route("/api/trainings", ["POST"], d.EvolutionCommand),
                Route("/api/trainings/{id}", ["PUT", "DELETE"], d.EvolutionCommand),
                Route("/api/trainings", ["GET"], d.EvolutionQuery),
                Route("/api/trainings/{everything}", ["GET"], d.EvolutionQuery),

                Route("/api/rewards", ["POST"], d.EvolutionCommand),
                Route("/api/rewards/{id}", ["PUT", "DELETE"], d.EvolutionCommand),
                Route("/api/rewards", ["GET"], d.EvolutionQuery),
                Route("/api/rewards/{everything}", ["GET"], d.EvolutionQuery),

                Route("/api/evolution/{everything}", ["GET"], d.EvolutionQuery),
                Route("/api/evolution/{everything}", ["POST", "PUT", "PATCH", "DELETE"], d.EvolutionCommand),
            ]
        };
    }

    private static FileRoute Route(string path, List<string> methods, ServiceEndpoint endpoint, FileRateLimitRule? rateLimit = null)
    {
        var route = new FileRoute
        {
            UpstreamPathTemplate = path,
            UpstreamHttpMethod = methods,
            DownstreamPathTemplate = path,
            DownstreamScheme = "http",
            DownstreamHostAndPorts =
            [
                new FileHostAndPort { Host = endpoint.Host, Port = endpoint.Port }
            ],
            QoSOptions = new FileQoSOptions
            {
                ExceptionsAllowedBeforeBreaking = 3,
                DurationOfBreak = 10000,
                TimeoutValue = 5000
            }
        };

        if (rateLimit is not null)
            route.RateLimitOptions = rateLimit;

        return route;
    }
}
