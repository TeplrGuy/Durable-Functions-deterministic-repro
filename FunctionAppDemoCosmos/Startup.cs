using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace FunctionAppDemoCosmos
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonSerializerSettings globalSettings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            JsonConvert.DefaultSettings = () => globalSettings;

            // Add your other service configurations here
        }
    }
}
