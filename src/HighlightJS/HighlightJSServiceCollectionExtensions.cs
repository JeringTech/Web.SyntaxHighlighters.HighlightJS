using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JeremyTCD.WebUtils.SyntaxHighlighters.HighlightJS
{
    public static class HighlightJSServiceCollectionExtensions
    {
        public static IServiceCollection AddHighlightJS(this IServiceCollection services)
        {
            services.TryAddSingleton<IHighlightJSService, HighlightJSService>();

            // Third party services
            services.AddNodeServices();

            return services;
        }
    }
}
