using Jering.Javascript.NodeJS;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Jering.Web.SyntaxHighlighters.HighlightJS.Tests
{
    public class StaticHighlightJSServiceIntegrationTests
    {
        private const int _timeoutMS = 60000;

        [Fact(Timeout = _timeoutMS)]
        public async void Configure_ConfiguresOptions()
        {
            // Act
            // Highlight once to ensure that an initial HighlightJSService is created. The invocation after configuration should properly dispose of this initial instance and create a new one with the
            // specified options.
            await StaticHighlightJSService.IsValidLanguageAliasAsync("csharp").ConfigureAwait(false);
            StaticHighlightJSService.Configure<OutOfProcessNodeJSServiceOptions>(options => options.TimeoutMS = 0);

            // Assert
            // Since we set timeout to 0, the NodeJS invocation is gauranteed to timeout. The NodeJS connection attempt is likely to timeout. Both throw an InvocationException.
            await Assert.ThrowsAsync<InvocationException>(async () => await StaticHighlightJSService.IsValidLanguageAliasAsync("csharp").ConfigureAwait(false)).ConfigureAwait(false);

            // Reset so other tests aren't affected
            StaticHighlightJSService.Configure<OutOfProcessNodeJSServiceOptions>(options => options.TimeoutMS = 60000);
        }

        [Theory(Timeout = _timeoutMS)]
        [MemberData(nameof(HighlightAsync_HighlightsCode_Data))]
        public async Task HighlightAsync_HighlightsCode(string dummyCode, string dummyLanguageAlias, string expectedResult)
        {
            // Act
            string result = await StaticHighlightJSService.HighlightAsync(dummyCode, dummyLanguageAlias).ConfigureAwait(false);

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
                    @"<span class=""hljs-function""><span class=""hljs-keyword"">public</span> <span class=""hljs-keyword"">string</span> <span class=""hljs-title"">ExampleFunction</span>(<span class=""hljs-params""><span class=""hljs-keyword"">string</span> arg</span>)</span>
{
    <span class=""hljs-comment"">// Example comment</span>
    <span class=""hljs-keyword"">return</span> arg + <span class=""hljs-string"">""dummyString""</span>;
}"
                }
            };
        }

        [Theory(Timeout = _timeoutMS)]
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