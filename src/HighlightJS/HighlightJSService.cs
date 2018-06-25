using Microsoft.AspNetCore.NodeServices;
using Microsoft.AspNetCore.NodeServices.HostingModels;
using System;
using System.Threading.Tasks;

namespace JeremyTCD.WebUtils.SyntaxHighlighters.HighlightJS
{
    public class HighlightJSService : IHighlightJSService, IDisposable
    {
        internal const string BUNDLE = "JeremyTCD.WebUtils.SyntaxHighlighters.HighlightJS.bundle.js";
        private readonly INodeServices _nodeServices;

        /// <summary>
        /// Use <see cref="Lazy{T}"/> for thread safe lazy initialization since invoking a JS method through NodeServices
        /// can take several hundred milliseconds. Wrap in a <see cref="Task{T}"/> for asynchrony.
        /// More information on AsyncLazy - https://blogs.msdn.microsoft.com/pfxteam/2011/01/15/asynclazyt/.
        /// </summary>
        private readonly Lazy<Task<string[]>> _languageNames;

        public HighlightJSService(INodeServices nodeServices)
        {
            _nodeServices = nodeServices;
            _languageNames = new Lazy<Task<string[]>>(LanguageNamesFactoryAsync);
        }

        /// <summary>
        /// Highlights <paramref name="code"/>.
        /// </summary>
        /// <param name="languageNameOrAlias">A HighlightJS language name or alias. Visit http://highlightjs.readthedocs.io/en/latest/css-classes-reference.html#language-names-and-aliases 
        /// for the full list of language names and aliases.</param>
        /// <param name="code">Code to highlight.</param>
        /// <param name="ignoreIllegals">If false, throws an error if code is syntactically invalid. If true, disregards syntactic validity when highlighting.</param>
        /// <param name="tabReplace">If not null, tabs will be replaced with this string.</param>
        /// <param name="useBR">If true, newline characters will be replaced with br elements.</param>
        /// <param name="classPrefix">If not null, this string will be appended to HighlightJS classes.</param>
        /// <returns>Highlighted <paramref name="code"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="code"/> is null.</exception>
        /// <exception cref="NodeInvocationException">Thrown if a Node error occurs.</exception>
        public virtual async Task<string> HighlightAsync(string languageNameOrAlias,
            string code,
            bool ignoreIllegals = true,
            string tabReplace = null,
            bool useBR = false,
            string classPrefix = "hljs-")
        {
            if (code == null)
            {
                throw new ArgumentNullException(Strings.Exception_ParameterCannotBeNull, nameof(code));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                // Nothing to highlight
                return code;
            }

            try
            {
                return await _nodeServices.InvokeExportAsync<string>(BUNDLE,
                    "highlight",
                    code,
                    languageNameOrAlias,
                    ignoreIllegals,
                    tabReplace,
                    useBR,
                    classPrefix).ConfigureAwait(false);
            }
            catch (AggregateException exception)
            {
                if (exception.InnerException is NodeInvocationException)
                {
                    throw exception.InnerException;
                }
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="languageSubset"></param>
        /// <param name="tabReplace"></param>
        /// <param name="useBR"></param>
        /// <param name="classPrefix"></param>
        /// <returns></returns>
        public virtual async Task<HighlightAutoResult> HighlightAutoAsync(string code,
            string[] languageSubset,
            string tabReplace = null,
            bool useBR = false,
            string classPrefix = "hljs-")
        {
            if (code == null || string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(Strings.Exception_ParameterCannotBeNull, nameof(code));
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                // Nothing to highlight
                return new HighlightAutoResult(0, code, null, null);
            }

            try
            {
                return await _nodeServices.InvokeExportAsync<HighlightAutoResult>(BUNDLE,
                    "highlightAuto",
                    code,
                    tabReplace,
                    useBR,
                    classPrefix).ConfigureAwait(false);
            }
            catch (AggregateException exception)
            {
                if (exception.InnerException is NodeInvocationException)
                {
                    throw exception.InnerException;
                }
                throw;
            }
        }

        /// <summary>
        /// Returns a list containing all HighlightJS language names (does not include aliases).
        /// </summary>
        /// <exception cref="NodeInvocationException">Thrown if a Node error occurs.</exception>
        public virtual async Task<string[]> GetLanguageNamesAsync()
        {
            try
            {
                return await _languageNames.Value.ConfigureAwait(false);
            }
            catch (AggregateException exception)
            {
                if (exception.InnerException is NodeInvocationException)
                {
                    throw exception.InnerException;
                }
                throw;
            }
        }

        /// <summary>
        /// Required for lazy initialization.
        /// </summary>
        /// <returns></returns>
        internal virtual Task<string[]> LanguageNamesFactoryAsync()
        {
            return _nodeServices.InvokeExportAsync<string[]>(BUNDLE, "getLanguageNames");
        }

        public void Dispose()
        {
            _nodeServices.Dispose();
        }
    }
}
