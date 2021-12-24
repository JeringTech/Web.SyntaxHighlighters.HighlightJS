using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Jering.Web.SyntaxHighlighters.HighlightJS.Performance
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private ServiceProvider? _serviceProvider;
        private int _counter;
        private IHighlightJSService? _highlightJSService;

        [GlobalSetup(Target = nameof(HighlightJSService_Highlight))]
        public void HighlightJSService_Highlight_Setup()
        {
            var services = new ServiceCollection();
            services.AddHighlightJS();
            _serviceProvider = services.BuildServiceProvider();
            _highlightJSService = _serviceProvider.GetRequiredService<IHighlightJSService>();
            _counter = 0;
        }

        [Benchmark]
        public async Task<string?> HighlightJSService_Highlight()
        {
            string? result = await _highlightJSService!.HighlightAsync($@"function exampleFunction(arg) {{
    // Example comment
    return arg + '{_counter++}';
}}",
                "javascript").ConfigureAwait(false);
            return result;
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _serviceProvider!.Dispose();
        }
    }
}
