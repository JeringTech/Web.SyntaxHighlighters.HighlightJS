using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace JeremyTCD.WebUtils.SyntaxHighlighters.HighlightJS.Tests
{
    public class HighlightJSServiceIntegrationTests : IDisposable
    {
        private ServiceProvider _serviceProvider;

        [Theory]
        [MemberData(nameof(Highlight_HighlightsCode_Data))]
        public void Highlight_HighlightsCode(string dummyCode, string dummyLanguageAlias, string expectedResult)
        {
            // Arrange 
            IHighlightJSService highlightJSService = CreateHighlightJSService();

            // Act
            string result = highlightJSService.HighlightAsync(dummyCode, dummyLanguageAlias).Result;

            // Assert
            Assert.Equal(expectedResult, result, ignoreLineEndingDifferences: true);
        }

        public static IEnumerable<object[]> Highlight_HighlightsCode_Data()
        {
            return new object[][]
            {
                // javascript
                new object[]
                {
                    @"  function exampleFunction(arg) {
    // Example comment
    return arg + 'dummyString';
}",
                    "javascript",
                    @"<span class=""hljs-function""><span class=""hljs-keyword"">function</span> <span class=""hljs-title"">exampleFunction</span>(<span class=""hljs-params"">arg</span>) </span>{
    <span class=""hljs-comment"">// Example comment</span>
    <span class=""hljs-keyword"">return</span> arg + <span class=""hljs-string"">'dummyString'</span>;
}"
                },

                // csharp
                new object[]
                {
                    @"  public string ExampleFunction(string arg)
{
    // Example comment
    return arg + ""dummyString"";
}",
                    "csharp",
                    @"<span class=""hljs-function""><span class=""hljs-keyword"">public</span> <span class=""hljs-keyword"">string</span> <span class=""hljs-title"">ExampleFunction</span>(<span class=""hljs-params""><span class=""hljs-keyword"">string</span> arg</span>)
</span>{
    <span class=""hljs-comment"">// Example comment</span>
    <span class=""hljs-keyword"">return</span> arg + <span class=""hljs-string"">""dummyString""</span>;
}"
                }
            };
        }

        [Theory]
        [MemberData(nameof(IsValidLanguageAlias_ChecksIfLanguageAliasIsValid_Data))]
        public void IsValidLanguageAlias_ChecksIfLanguageAliasIsValid(string dummyLanguageAlias, bool expectedResult)
        {
            // Arrange
            IHighlightJSService highlightJSService = CreateHighlightJSService();

            // Act
            bool result = highlightJSService.IsValidLanguageNameOrAliasAsync(dummyLanguageAlias).Result;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> IsValidLanguageAlias_ChecksIfLanguageAliasIsValid_Data()
        {
            return new object[][]
            {
                // Alias
                new object[]
                {
                    "html", true
                },

                // Actual language
                new object[]
                {
                    "css", true
                },

                // Non existent language
                new object[]
                {
                    "non-existent-language", false
                }
            };
        }

        private IHighlightJSService CreateHighlightJSService()
        {
            // Since a new container is created for each test, a new INodeServices instance is created as well.
            // This means that a new node process is started and then disposed of for each test. 
            // It is cleaner to do things this way, but reconsider if performance becomes an issue.
            var services = new ServiceCollection();

            services.AddHighlightJS();
            if (Debugger.IsAttached)
            {
                // Override INodeServices service registered by AddPrism to enable debugging
                services.AddNodeServices(options =>
                {
                    options.LaunchWithDebugging = true;
                    options.InvocationTimeoutMilliseconds = 99999999; // -1 doesn't work, once a js breakpoint is hit, the debugger disconnects
                });

                _serviceProvider = services.BuildServiceProvider();

                // InvokeAsync implicitly starts up a node instance. Adding a break point after InvokeAsync allows
                // chrome to connect to the debugger
                INodeServices nodeServices = _serviceProvider.GetRequiredService<INodeServices>();
                try
                {
                    int dummy = nodeServices.InvokeAsync<int>("").Result;
                }
                catch
                {
                    // Do nothing
                }
            }
            else
            {
                _serviceProvider = services.BuildServiceProvider();
            }

            return _serviceProvider.GetRequiredService<IHighlightJSService>();
        }

        public void Dispose()
        {
            // Ensure that NodeServices gets disposed
            _serviceProvider?.Dispose();
        }
    }
}