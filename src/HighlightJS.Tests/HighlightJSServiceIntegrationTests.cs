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
        [MemberData(nameof(HighlightAsync_HighlightsCode_Data))]
        public void HighlightAsync_HighlightsCode(string dummyCode, string dummyLanguageName, string expectedResult)
        {
            // Arrange 
            IHighlightJSService highlightJSService = CreateHighlightJSService();

            // Act
            string result = highlightJSService.HighlightAsync(dummyCode, dummyLanguageName).Result;

            // Assert
            Assert.Equal(expectedResult, result, ignoreLineEndingDifferences: true);
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
                    @"<span class=""hljs-function""><span class=""hljs-keyword"">function</span> <span class=""hljs-title"">exampleFunction</span>(<span class=""hljs-params"">arg</span>) </span>{
    <span class=""hljs-comment"">// Example comment</span>
    <span class=""hljs-keyword"">return</span> arg + <span class=""hljs-string"">'dummyString'</span>;
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
                    @"<span class=""hljs-function""><span class=""hljs-keyword"">public</span> <span class=""hljs-keyword"">string</span> <span class=""hljs-title"">ExampleFunction</span>(<span class=""hljs-params""><span class=""hljs-keyword"">string</span> arg</span>)
</span>{
    <span class=""hljs-comment"">// Example comment</span>
    <span class=""hljs-keyword"">return</span> arg + <span class=""hljs-string"">""dummyString""</span>;
}"
                }
            };
        }

        [Theory]
        [MemberData(nameof(HighlightAsync_ReplacesTabsWithTabReplaceIfTabReplaceIsNotNullOtherwiseDoesNotReplaceTabs_Data))]
        public void HighlightAsync_AppendsClassPrefixToClassesIfClassPrefixIsNotNullOtherwiseDoesNotAppendAnything(string dummyClassPrefix, string expectedResult)
        {
            // Arrange
            const string dummyCode = @"public string ExampleFunction(string arg)
{
    // Example comment
    return arg + ""dummyString"";
}";
            const string dummyLanguageName = "csharp";
            IHighlightJSService highlightJSService = CreateHighlightJSService();

            // Act
            string result = highlightJSService.HighlightAsync(dummyCode, dummyLanguageName, dummyClassPrefix).Result;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        public static IEnumerable<object[]> HighlightAsync_ReplacesTabsWithTabReplaceIfTabReplaceIsNotNullOtherwiseDoesNotReplaceTabs_Data()
        {
            return new object[][]
            {
                new object[]{
                    null,
                    @"<span class=""function""><span class=""keyword"">public</span> <span class=""keyword"">string</span> <span class=""title"">ExampleFunction</span>(<span class=""params""><span class=""keyword"">string</span> arg</span>)
</span>{
    <span class=""comment"">// Example comment</span>
    <span class=""keyword"">return</span> arg + <span class=""string"">""dummyString""</span>;
}"
                },
                new object[]{
                    "test-",
                    @"<span class=""test-function""><span class=""test-keyword"">public</span> <span class=""test-keyword"">string</span> <span class=""test-title"">ExampleFunction</span>(<span class=""test-params""><span class=""test-keyword"">string</span> arg</span>)
</span>{
    <span class=""test-comment"">// Example comment</span>
    <span class=""test-keyword"">return</span> arg + <span class=""test-string"">""dummyString""</span>;
}"
                }
            };
        }

        [Theory]
        [MemberData(nameof(IsValidLanguageAliasAsync_ChecksIfLanguageAliasIsValid_Data))]
        public void IsValidLanguageAliasAsync_ChecksIfLanguageAliasIsValid(string dummyLanguageAlias, bool expectedResult)
        {
            // Arrange
            IHighlightJSService highlightJSService = CreateHighlightJSService();

            // Act
            bool result = highlightJSService.IsValidLanguageAliasAsync(dummyLanguageAlias).Result;

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

        private IHighlightJSService CreateHighlightJSService()
        {
            // Since a new container is created for each test, a new INodeServices instance is created as well.
            // This means that a new node process is started and then disposed of for each test. 
            // It is cleaner to do things this way, but reconsider if performance becomes an issue.
            var services = new ServiceCollection();

            services.AddHighlightJS();
            if (Debugger.IsAttached)
            {
                // Override INodeServices service registered by AddHighlightJS to enable debugging
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