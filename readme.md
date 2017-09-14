# AspNetCore Unit Testing

> Disclaimer: No Visual Studio 2017 was ever started during the writing of this post.

This post is going to show how to unit test controllers in AspNetCore.

We are going to do everything using nothing but the dotnet cli and VS Code.

Let's start off by creating a new Web API application.

```shell
dotnet new webapi
```

That's going to set up a minimal Web API application containing a `ValuesController`

> To simplify the example we just return a single string value

Next we are going to implement a service that we will inject into the `ValuesController`

```c#
public interface IService
{
    string GetValue();    
}

public class Service : IService
{
    public string GetValue()
    {
        return "Hello world";
    }
}
```



Next step is to inject our service into the `ValueController`

```c#
[Route("api/[controller]")]
public class ValueController : Controller
{
    private readonly IService _service;

    public ValueController(IService service) => _service = service;

    // GET api/values
    [HttpGet]
    public string Get()
    {
        return _service.GetValue();
    }        
}
```

Finally we need to register our service in the `Startup` class.

```
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IService,Service>();
    services.AddMvc();
}
```



We are now all set and ready for some unit testing. 

## Unit Testing



To test our controller we will create a test project 

```shell
dotnet new xunit
```

We will test our controller using the `TestServer` from the `Microsoft.AspNetCore.TestHost` package

```
dotnet add package Microsoft.AspNetCore.TestHost
```



Let's create our first test

```c#
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

    private TestServer CreateTestServer()
    {
            var builder = new WebHostBuilder()                
            .UseStartup<Startup>();                                    
        return new TestServer(builder);                
    }
}
```



## Configurable Server

Let's imagine that we want to test our controller using a mock implementation of `IService`. 

We need to provide a way to override the default container registration before the server is started.

The first step here is to add the `ConfigureAdditionalServices` method to the `Startup` class.

```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IService,Service>();
    services.AddMvc();
    ConfigureAdditionalServices(services);
}

protected virtual void ConfigureAdditionalServices(IServiceCollection services)
{
}			
```

The `ConfigureAdditionalServices` method gets called after all other services are registered giving us a chance to modify the configuration.

We can now simply inherit from the `Startup` class and make it configurable.

```c#
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
```

While we could mock services right here in this class, we will make it more versatile by just injecting the `configureAtion` delegate allowing this class to be used in different scenarios.

```c#
public class ConfigurableServer : TestServer
{
    public ConfigurableServer(Action<IServiceCollection> configureAction = null) : base(CreateBuilder(configureAction))
    {
    }

    private static IWebHostBuilder CreateBuilder(Action<IServiceCollection> configureAction)
    {
        if (configureAction == null)
        {
            configureAction = (sc) => {};
        }
        var builder = new WebHostBuilder()
            .ConfigureServices(sc => sc.AddSingleton<Action<IServiceCollection>>(configureAction))
            .UseStartup<ConfigurableStartup>();
        return builder;    
    }
}
```

Now this might need a little explanation. First we optionally pass inn the `configureAction` delegate and passes that delegate to the `CreateBuilder` method that creates the `IWebHostBuilder` instance that is again passed to the base constructor. The `IWebHostBuilder` has this `ConfigureServices` method that can be used to register services that is required by the startup class itself. In this case the `ConfigurableStartup` class takes the `configureAction` delegate as a constructor argument and we simply register the delegate as a singleton.

## Mocking

With our new `ConfigurableServer` in place we can start to do some pretty interesting things with regards to mocking services inside our server. 

But first, let's install Moq

```shell
dotnet add package moq
```

We can now use the `configureAction` passed to the `ConfigurableServer` to replace the originally registered service.

```c#
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
```



Want to comment? File an issue [here](https://github.com/seesharper/Blog.AspNetCoreUnitTesting/issues) :)











 











   





