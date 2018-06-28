using System.Threading.Tasks;

namespace JeremyTCD.WebUtils.SyntaxHighlighters.HighlightJS
{
    public interface IHighlightJSService
    {
        Task<string> HighlightAsync(string code,
            string languageAlias,
            string classPrefix = "hljs-");

        Task<bool> IsValidLanguageAliasAsync(string languageAlias);
    }
}
