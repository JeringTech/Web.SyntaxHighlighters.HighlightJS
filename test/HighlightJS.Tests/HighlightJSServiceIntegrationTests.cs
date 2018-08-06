using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Jering.Web.SyntaxHighlighters.HighlightJS.Tests
{
    public class HighlightJSServiceIntegrationTests : IDisposable
    {
        private IServiceProvider _serviceProvider;

        [Theory]
        [MemberData(nameof(HighlightAsync_HighlightsCode_Data))]
        public async Task HighlightAsync_HighlightsCode(string dummyCode, string dummyLanguageName, string expectedResult)
        {
            // Arrange 
            IHighlightJSService highlightJSService = CreateHighlightJSService();

            // Act
            string result = await highlightJSService.HighlightAsync(dummyCode, dummyLanguageName).ConfigureAwait(false);

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
        public async Task HighlightAsync_AppendsClassPrefixToClassesIfClassPrefixIsNotNullOtherwiseDoesNotAppendAnything(string dummyClassPrefix, string expectedResult)
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
            string result = await highlightJSService.HighlightAsync(dummyCode, dummyLanguageName, dummyClassPrefix).ConfigureAwait(false);

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
        public async Task IsValidLanguageAliasAsync_ChecksIfLanguageAliasIsValid(string dummyLanguageAlias, bool expectedResult)
        {
            // Arrange
            IHighlightJSService highlightJSService = CreateHighlightJSService();

            // Act
            bool result = await highlightJSService.IsValidLanguageAliasAsync(dummyLanguageAlias).ConfigureAwait(false);

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
            var services = new ServiceCollection();

            services.AddHighlightJS();
            if (Debugger.IsAttached)
            {
                services.Configure<NodeJSProcessOptions>(options => options.NodeAndV8Options = "--inspect-brk");
                services.Configure<OutOfProcessNodeJSServiceOptions>(options => options.TimeoutMS = -1);
            }
            _serviceProvider = services.BuildServiceProvider();

            return _serviceProvider.GetRequiredService<IHighlightJSService>();
        }

        public void Dispose()
        {
            // Ensure that NodeJSService gets disposed
            ((IDisposable)_serviceProvider).Dispose();
        }
    }
}