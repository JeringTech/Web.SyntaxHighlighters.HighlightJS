using Jering.Javascript.NodeJS;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jering.Web.SyntaxHighlighters.HighlightJS.Tests
{
    public class StaticHighlightJSServiceIntegrationTests
    {
        private const int TIMEOUT_MS = 60000;

        [Fact(Timeout = TIMEOUT_MS)]
        public async void Configure_ConfiguresOptions()
        {
            // Act
            // Highlight once to ensure that an initial HighlightJSService is created. The invocation after configuration should properly dispose of this initial instance and create a new one with the
            // specified options.
            await StaticHighlightJSService.IsValidLanguageAliasAsync("csharp").ConfigureAwait(false);
            StaticHighlightJSService.Configure<OutOfProcessNodeJSServiceOptions>(options => options.TimeoutMS = 0);

            // Assert
            // Since we set timeout to 0, the NodeJS invocation is gauranteed to timeout, throwing a ConnectionException.
            await Assert.ThrowsAsync<ConnectionException>(async () => await StaticHighlightJSService.IsValidLanguageAliasAsync("csharp").ConfigureAwait(false)).ConfigureAwait(false);

            // Reset so other tests aren't affected
            StaticHighlightJSService.Configure<OutOfProcessNodeJSServiceOptions>(options => options.TimeoutMS = 60000);
        }

        [Fact(Timeout = TIMEOUT_MS)]
        public async void DisposeServiceProvider_DisposesServiceProvider()
        {
            // Arrange
            StaticHighlightJSService.Configure<OutOfProcessNodeJSServiceOptions>(options => options.TimeoutMS = 0);
            // Highlight once to ensure that an initial HighlightJSService is created.
            try
            {
                await StaticHighlightJSService.IsValidLanguageAliasAsync("csharp").ConfigureAwait(false); // Throws since TimeoutMS == 0
            }
            catch
            {
                // Do nothing 
            }

            // Act
            StaticHighlightJSService.DisposeServiceProvider(); // Dispose, the next call should not be affected by TimeoutMS = 0
            string? result = await StaticHighlightJSService.HighlightAsync(@"public string ExampleFunction(string arg)
{
    // Example comment
    return arg + ""dummyString"";
}", "csharp").ConfigureAwait(false);

            // Assert
            Assert.Equal(@"<span class=""hljs-function""><span class=""hljs-keyword"">public</span> <span class=""hljs-built_in"">string</span> <span class=""hljs-title"">ExampleFunction</span>(<span class=""hljs-params""><span class=""hljs-built_in"">string</span> arg</span>)</span>
{
    <span class=""hljs-comment"">// Example comment</span>
    <span class=""hljs-keyword"">return</span> arg + <span class=""hljs-string"">&quot;dummyString&quot;</span>;
}", result);
        }

        [Theory(Timeout = TIMEOUT_MS)]
        [MemberData(nameof(HighlightAsync_HighlightsCode_Data))]
        public async Task HighlightAsync_HighlightsCode(string dummyCode, string dummyLanguageAlias, string expectedResult)
        {
            // Act
            string? result = await StaticHighlightJSService.HighlightAsync(dummyCode, dummyLanguageAlias).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> HighlightAsync_HighlightsCode_Data()
        {
            return new object[][]
            {
                // javascript
                new object[]
                {
                    @"function exampleFunction(arg) {
    // Example comment
    return arg + 'dummyString';
}",
                    "javascript",
                    @"<span class=""hljs-keyword"">function</span> <span class=""hljs-title function_"">exampleFunction</span>(<span class=""hljs-params"">arg</span>) {
    <span class=""hljs-comment"">// Example comment</span>
    <span class=""hljs-keyword"">return</span> arg + <span class=""hljs-string"">&#x27;dummyString&#x27;</span>;
}"
                },

                // csharp
                new object[]
                {
                    @"public string ExampleFunction(string arg)
{
    // Example comment
    return arg + ""dummyString"";
}",
                    "csharp",
                    @"<span class=""hljs-function""><span class=""hljs-keyword"">public</span> <span class=""hljs-built_in"">string</span> <span class=""hljs-title"">ExampleFunction</span>(<span class=""hljs-params""><span class=""hljs-built_in"">string</span> arg</span>)</span>
{
    <span class=""hljs-comment"">// Example comment</span>
    <span class=""hljs-keyword"">return</span> arg + <span class=""hljs-string"">&quot;dummyString&quot;</span>;
}"
                }
            };
        }

        [Fact(Timeout = TIMEOUT_MS)]
        public void HighlightAsync_IsThreadSafe()
        {
            // Arrange
            const string dummyCode = @"public string ExampleFunction(string arg)
{
    // Example comment
    return arg + ""dummyString"";
}";
            const string dummyLanguageAlias = "csharp";

            // Act
            var results = new ConcurrentQueue<string?>();
            const int numThreads = 5;
            var threads = new List<Thread>();
            for (int i = 0; i < numThreads; i++)
            {
                var thread = new Thread(() => results.Enqueue(StaticHighlightJSService.HighlightAsync(dummyCode, dummyLanguageAlias).GetAwaiter().GetResult()));
                threads.Add(thread);
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            // Assert
            const string expectedResult = @"<span class=""hljs-function""><span class=""hljs-keyword"">public</span> <span class=""hljs-built_in"">string</span> <span class=""hljs-title"">ExampleFunction</span>(<span class=""hljs-params""><span class=""hljs-built_in"">string</span> arg</span>)</span>
{
    <span class=""hljs-comment"">// Example comment</span>
    <span class=""hljs-keyword"">return</span> arg + <span class=""hljs-string"">&quot;dummyString&quot;</span>;
}";
            Assert.Equal(numThreads, results.Count);
            foreach (string? result in results)
            {
                Assert.Equal(expectedResult, result);
            }
        }

        [Theory(Timeout = TIMEOUT_MS)]
        [MemberData(nameof(IsValidLanguageAliasAsync_ChecksIfLanguageAliasIsValid_Data))]
        public async Task IsValidLanguageAliasAsync_ChecksIfLanguageAliasIsValid(string dummyLanguageAlias, bool expectedResult)
        {
            // Act
            bool result = await StaticHighlightJSService.IsValidLanguageAliasAsync(dummyLanguageAlias).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> IsValidLanguageAliasAsync_ChecksIfLanguageAliasIsValid_Data()
        {
            return new object[][]
            {
                new object[]
                {
                    "as", true
                },

                new object[]
                {
                    "actionscript", true
                },

                // Non existent language
                new object[]
                {
                    "non-existent-language", false
                }
            };
        }
    }
}
