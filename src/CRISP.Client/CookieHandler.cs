using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;

namespace CRISP.Client
{
    /// <summary>
    /// Handler to ensure cookie credentials are automatically sent over with each request.
    /// </summary>
    public class CookieHandler : DelegatingHandler
    {
        private readonly IJSRuntime? _jsRuntime;
        private readonly NavigationManager _navigation;
        private readonly ILogger<CookieHandler>? _logger;

        public CookieHandler(IJSRuntime jsRuntime, NavigationManager navigationManager, ILogger<CookieHandler> logger)
        {
            _jsRuntime = jsRuntime;
            _navigation = navigationManager;
            _logger = logger;
        }

        /// <summary>
        /// Main method to override for the handler.
        /// </summary>
        /// <param name="request">The original request.</param>
        /// <param name="cancellationToken">The token to handle cancellations.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger?.LogTrace($"CookieHandler: Processing request to {request.RequestUri}");

            // include cookies!
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            request.Headers.Add("X-Requested-With", ["XMLHttpRequest"]);

            _logger?.LogTrace($"CookieHandler: Set BrowserRequestCredentials to Include");

            // Log all headers being sent
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                _logger?.LogTrace($"Request Header: {header.Key} = {string.Join(", ", header.Value)}");
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            _logger?.LogTrace($"CookieHandler: Response status = {response.StatusCode}");

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger?.LogError($"403 Forbidden received for {request.RequestUri}");

                // Log response headers for debugging
                foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
                {
                    _logger?.LogTrace($"Response Header: {header.Key} = {string.Join(", ", header.Value)}");
                }
            }

            return response;
        }
    }
}
