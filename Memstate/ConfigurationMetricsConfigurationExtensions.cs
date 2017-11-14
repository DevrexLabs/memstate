using System;
using App.Metrics;
using Microsoft.Extensions.Configuration;

namespace Memstate
{
    public static class ConfigurationMetricsConfigurationExtensions
    {
        private const string DefaultSectionName = nameof(MetricsOptions);

        public static IMetricsBuilder ReadFrom(
            this IMetricsConfigurationBuilder configurationBuilder,
            IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            configurationBuilder.ReadFrom(configuration.GetSection(DefaultSectionName));

            return configurationBuilder.Builder;
        }

        public static IMetricsBuilder ReadFrom(
            this IMetricsConfigurationBuilder configurationBuilder,
            IConfigurationSection configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            configurationBuilder.Extend(configuration.AsEnumerable());

            return configurationBuilder.Builder;
        }
    }
}