using Microsoft.Extensions.Configuration;

namespace SecurityCam
{
    public static class ConfigurationRootExtensions
    {
        public static T Bind<T>(this IConfigurationRoot root)
            where T : new()
        {
            var config = new T();
            root.Bind(config);
            return config;
        }
    }
}