using Jering.Javascript.NodeJS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Jering.Web.SyntaxHighlighters.HighlightJS
{
    /// <summary>
    /// The default implementation of <see cref="IHighlightJSService"/>. This implementation uses <see cref="INodeJSService"/> to send HighlightJS syntax highlighting 
    /// requests to a NodeJS instance.
    /// </summary>
    public class HighlightJSService : IHighlightJSService, IDisposable
    {
        internal static readonly string _moduleCacheIdentifier = typeof(HighlightJSService).Namespace; // This identifier must be unique to this project, so use the namespace
        internal const string BUNDLE_NAME = "bundle.js";

        private readonly INodeJSService _nodeJSService;
        private readonly IEmbeddedResourcesService _embeddedResourcesService;
        private readonly SemaphoreSlim _aliasesSemaphore = new SemaphoreSlim(1, 1); // Semaphore instead of an object (lock) so that we can get aliases asynchronously

        private volatile HashSet<string?>? _aliases; // Volatile since its used in a double checked lock
        private bool _disposed;

        /// <summary>
        /// Creates a <see cref="HighlightJSService"/> instance.
        /// </summary>
        /// <param name="nodeJSService">The service used to invoke HighlightJS in NodeJS.</param>
        /// <param name="embeddedResourcesService">The service used to retrieve embedded resources.</param>
        public HighlightJSService(INodeJSService nodeJSService, IEmbeddedResourcesService embeddedResourcesService)
        {
            _nodeJSService = nodeJSService ?? throw new ArgumentNullException(nameof(nodeJSService));
            _embeddedResourcesService = embeddedResourcesService ?? throw new ArgumentNullException(nameof(embeddedResourcesService));
        }

        /// <inheritdoc />
        public virtual async Task<string?> HighlightAsync(string code,
            string languageAlias,
            string classPrefix = "hljs-",
            CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HighlightJSService));
            }

            if (code == null)
            {
                throw new ArgumentNullException(nameof(code), Strings.Exception_ParameterCannotBeNull);
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                // Nothing to highlight
                return code;
            }

            if (!await IsValidLanguageAliasAsync(languageAlias, cancellationToken).ConfigureAwait(false))
            {
                // languageAlias is invalid
                throw new ArgumentException(string.Format(Strings.Exception_InvalidHighlightJSLanguageAlias, languageAlias));
            }

            object[] args = new object[] { code, languageAlias, string.IsNullOrWhiteSpace(classPrefix) ? "" : classPrefix };
            // Invoke from cache
            (bool success, string? result) = await _nodeJSService.TryInvokeFromCacheAsync<string>(_moduleCacheIdentifier, "highlight", args, cancellationToken).ConfigureAwait(false);
            if (success)
            {
                return result;
            }

            // Invoke from stream since module is not cached
            using Stream moduleStream = _embeddedResourcesService.ReadAsStream(typeof(HighlightJSService).GetTypeInfo().Assembly, BUNDLE_NAME);
            // Invoking from stream is 2+x faster than reading the resource as a string and invoking as string. This is because invoking as string causes almost 
            // 1000x more memory to be allocated, resulting in gen 1+ gcs.
            return await _nodeJSService.InvokeFromStreamAsync<string>(moduleStream, _moduleCacheIdentifier, "highlight", args, cancellationToken).ConfigureAwait(false);
        }

        // Note: Returning a ValueTask here doesn't reduce allocations because the runtime already caches Task<bool> objects - https://blogs.msdn.microsoft.com/dotnet/2018/11/07/understanding-the-whys-whats-and-whens-of-valuetask/.
        /// <inheritdoc />
        public virtual async Task<bool> IsValidLanguageAliasAsync(string languageAlias, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(HighlightJSService));
            }

            if (string.IsNullOrWhiteSpace(languageAlias))
            {
                return false;
            }

            // Double checked lock
            if (_aliases == null)
            {
                await _aliasesSemaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (_aliases == null)
                    {
                        // This should only ever be called once, before any highlighting is done by NodeJS. So take this oppurtunity to 
                        // cache the module.
                        using Stream moduleStream = _embeddedResourcesService.ReadAsStream(typeof(HighlightJSService).GetTypeInfo().Assembly, BUNDLE_NAME);
                        string[]? aliasesArray = await _nodeJSService.InvokeFromStreamAsync<string[]>(moduleStream, _moduleCacheIdentifier, "getAliases", cancellationToken: cancellationToken).ConfigureAwait(false);
                        _aliases = new HashSet<string?>(aliasesArray!); // We know getAliases does not return null
                    }
                }
                finally
                {
                    _aliasesSemaphore.Release();
                }
            }

            return _aliases.Contains(languageAlias);
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the instance. This method is not thread-safe. It should only be called after all other calls to this instance's methods have returned.
        /// </summary>
        /// <param name="disposing">True if the object is disposing or false if it is finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _aliasesSemaphore?.Dispose();
            }

            _nodeJSService?.Dispose(); // _nodeJSService wraps a NodeJS process (an unmanaged resource)
            _disposed = true;
        }

        /// <summary>
        /// Implements the finalization part of the IDisposable pattern by calling Dispose(false).
        /// </summary>
        ~HighlightJSService()
        {
            Dispose(false);
        }
    }
}
