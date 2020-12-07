using System;

namespace Koasta.Shared.Configuration
{
    internal class Environment : IEnvironment
    {
        public Environment()
        {
            IsProduction = (System.Environment.GetEnvironmentVariable("PUBCRAWL_ENVIRONMENT") ?? "").Equals("live", StringComparison.OrdinalIgnoreCase);
            IsTest = (System.Environment.GetEnvironmentVariable("PUBCRAWL_ENVIRONMENT") ?? "").Equals("test", StringComparison.OrdinalIgnoreCase);
            IsDevelopment = (System.Environment.GetEnvironmentVariable("PUBCRAWL_ENVIRONMENT") ?? "default").Equals("default", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsProduction { get; }

        public bool IsTest { get; }

        public bool IsDevelopment { get; }
    }
}
