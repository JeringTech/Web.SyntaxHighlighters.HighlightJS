using Jering.JavascriptUtils.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jering.WebUtils.SyntaxHighlighters.HighlightJS
{
    /// <summary>
    /// Extension methods for setting up HighlightJS in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class HighlightJSServiceCollectionExtensions
    {
        /// <summary>
        /// Adds HighlightJS services to an <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The target <see cref="IServiceCollection"/>.</param>
        public static IServiceCollection AddHighlightJS(this IServiceCollection services)
        {
            services.TryAddSingleton<IHighlightJSService, HighlightJSService>();

            // Third party services
            services.AddNodeJS();

            return services;
        }
    }
}
