using Microsoft.Extensions.DependencyInjection;

namespace ApplicationInterface.User
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            return services;
        }
    }
}
