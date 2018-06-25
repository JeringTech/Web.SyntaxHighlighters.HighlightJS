using System.Threading.Tasks;

namespace JeremyTCD.WebUtils.SyntaxHighlighters.HighlightJS
{
    public interface IHighlightJSService
    {
        Task<string> HighlightAsync(string languageNameOrAlias,
            string code,
            bool ignoreIllegals = true,
            string tabReplace = null,
            bool useBR = false,
            string classPrefix = "hljs-");

        Task<bool> IsValidLanguageNameOrAliasAsync(string languageAlias);
    }
}
