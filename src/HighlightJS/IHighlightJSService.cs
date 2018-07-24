using Jering.JavascriptUtils.NodeJS;
using System;
using System.Threading.Tasks;

namespace Jering.WebUtils.SyntaxHighlighters.HighlightJS
{
    /// <summary>
    /// A service for performing syntax highlighting using HighlightJS.
    /// </summary>
    public interface IHighlightJSService
    {
        /// <summary>
        /// Highlights code of a specified language.
        /// </summary>
        /// <param name="code">Code to highlight.</param>
        /// <param name="languageAlias">A HighlightJS language alias. Visit http://highlightjs.readthedocs.io/en/latest/css-classes-reference.html#language-names-and-aliases 
        /// for the list of valid language aliases.</param>
        /// <param name="classPrefix">If not null or whitespace, this string will be appended to HighlightJS classes. Defaults to "hljs-".</param>
        /// <returns>Highlighted code.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="code"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="languageAlias"/> is not a valid HighlightJS language alias.</exception>
        /// <exception cref="InvocationException">Thrown if a Node error occurs.</exception>
        Task<string> HighlightAsync(string code,
            string languageAlias,
            string classPrefix = "hljs-");

        /// <summary>
        /// Determines whether a language alias is valid.
        /// </summary>
        /// <param name="languageAlias">Language alias to validate. Visit http://highlightjs.readthedocs.io/en/latest/css-classes-reference.html#language-names-and-aliases 
        /// for the list of valid language aliases.</param>
        /// <returns>true if the specified language alias is a valid HighlightJS language alias. Otherwise, false.</returns>
        /// <exception cref="InvocationException">Thrown if a NodeJS error occurs.</exception>
        ValueTask<bool> IsValidLanguageAliasAsync(string languageAlias);
    }
}
