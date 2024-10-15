using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace BytLabs.Api.Configuration
{
    /// <summary>
    /// Provides extension methods for configuration management.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets a strongly-typed configuration section and validates it.
        /// </summary>
        /// <typeparam name="TConfiguration">The type of configuration to retrieve and validate.</typeparam>
        /// <param name="configuration">The configuration manager instance.</param>
        /// <returns>A validated instance of the configuration type.</returns>
        /// <exception cref="Exception">Thrown when the configuration section doesn't exist or validation fails.</exception>
        public static TConfiguration GetConfiguration<TConfiguration>(this ConfigurationManager configuration)
            where TConfiguration : new()
        {
            TConfiguration observabilityConfiguration = new();
            string configurationName = typeof(TConfiguration).Name;
            IConfigurationSection section = configuration.GetRequiredSection(configurationName);

            if (!section.Exists())
            {
                throw new($"Required Configuration Section {configurationName} not exists.");
            }
            section.Bind(observabilityConfiguration);

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(observabilityConfiguration, null, null);
            bool isValid = Validator.TryValidateObject(observabilityConfiguration, validationContext, validationResults, true);

            if (!isValid)
            {
                string errors = string.Join(";", validationResults.Select(vr => vr.ErrorMessage));
                throw new($"Configuration validation error(s) in section '{configurationName}': {errors}");
            }


            return observabilityConfiguration;
        }
    }
}