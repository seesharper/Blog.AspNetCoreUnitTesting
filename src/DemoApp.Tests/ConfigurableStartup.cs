using System;
using demoapp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace DemoApp.Tests
{
    public class ConfigurableStartup : Startup
    {
        private readonly Action<IServiceCollection> configureAction;

        public ConfigurableStartup(IConfiguration configuration, Action<IServiceCollection> configureAction)
            : base(configuration) => this.configureAction = configureAction;

        protected override void ConfigureAdditionalServices(IServiceCollection services)
        {
            configureAction(services);
        }
    }
}