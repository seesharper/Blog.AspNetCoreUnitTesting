namespace DemoApp.Services
{
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
}
