using System;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using demoapp;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;
using Moq;
using DemoApp.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DemoApp.Tests
{
    public class ControllerTests
    {
        [Fact]
        public async Task ShouldGetValue()
        {            
            using (var testServer = CreateTestServer())
            {
                var client = testServer.CreateClient();
                var value = await client.GetStringAsync("api/value");
                Assert.Equal("Hello world", value);
            }
        }

        [Fact]
        public async Task ShouldGetMockValue()
        {
            var serviceMock = new Mock<IService>();
            serviceMock.Setup(m => m.GetValue()).Returns("Hello mockworld");
            var serviceDescriptor = new ServiceDescriptor(typeof(IService), serviceMock.Object);

            using (var testServer = new ConfigurableServer(sc => sc.Replace(serviceDescriptor)))
            {
                var client = testServer.CreateClient();
                var value = await client.GetStringAsync("api/value");
                Assert.Equal("Hello mockworld", value);
            }
        }

        private TestServer CreateTestServer()
        {
                var builder = new WebHostBuilder()                
                .UseStartup<Startup>();                                    
            return new TestServer(builder);                
        }
    }
}
